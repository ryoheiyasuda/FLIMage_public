# -*- coding: utf-8 -*-
"""
Created on Fri Jun 26 2026

@author: yasudar
"""

import numpy as np
from scipy.optimize import least_squares
from scipy.special import erfc


class FLIMDecayFitter:
    """
    Analytical reconvolution FLIM decay fitter using the same ExpGauss model
    form as the C# implementation.

    The public Python parameter order is kept the same as Reconvolution_Fitting.py:
        [A1, tau1, A2, tau2, ..., An, taun, t0, sigma, bg]

    where:
        Ai    : amplitude of component i
        taui  : lifetime of component i, in the same units as t
        t0    : IRF center
        sigma : Gaussian IRF width
        bg    : constant background

    Internally, each lifetime is converted to the C# rate form:
        k = 1 / tau

    The single-component response is:
        0.5 * A * exp(sigma^2*k^2/2 - (x-t0)*k)
            * erfc((sigma^2*k - (x-t0)) / (sqrt(2)*sigma))
    """

    def __init__(self, n_exp=2, sync_rate_hz=None, time_unit="ns"):
        assert 1 <= n_exp <= 3, "n_exp must be 1, 2, or 3"
        self.n_exp = n_exp
        self.sync_rate_hz = sync_rate_hz
        self.time_unit = time_unit

        names = []
        for i in range(1, n_exp + 1):
            names += [f"A{i}", f"tau{i}"]
        names += ["t0", "sigma", "bg"]
        self.param_names = names

    # ----------------- C#-style analytical model pieces ----------------- #
    @staticmethod
    def exp_gauss(t, amplitude, tau, sigma, t0):
        """
        Analytical convolution of a Gaussian IRF and a single exponential.

        This matches FLIMage/MathLibrary/MatrixCalc.cs:
            ExpGauss([pop, k, tauG, t0], x)
        with k = 1 / tau.
        """
        t = np.asarray(t, dtype=float)
        tau = float(tau)
        sigma = float(sigma)

        if tau <= 0.0 or sigma <= 0.0:
            return np.full_like(t, np.nan, dtype=float)

        k = 1.0 / tau
        x_shift = t - t0
        exponent = sigma * sigma * k * k / 2.0 - x_shift * k
        erfc_arg = (sigma * sigma * k - x_shift) / (np.sqrt(2.0) * sigma)

        with np.errstate(over="ignore", invalid="ignore"):
            y = 0.5 * amplitude * np.exp(exponent) * erfc(erfc_arg)

        return y

    @staticmethod
    def ExpGauss(beta0, x):
        """
        C#-style single-pulse ExpGauss.

        beta0 follows MatrixCalc.ExpGauss:
            [pop, k, tauG, t0]
        where k is the decay rate, not the lifetime.
        """
        beta0 = np.asarray(beta0, dtype=float)
        pop1 = beta0[0]
        k1 = beta0[1]
        tauG = beta0[2]
        t0 = beta0[3]

        x = np.asarray(x, dtype=float)
        if k1 <= 0.0 or tauG <= 0.0:
            return np.full_like(x, np.nan, dtype=float)

        exponent = tauG * tauG * k1 * k1 / 2.0 - (x - t0) * k1
        erfc_arg = (tauG * tauG * k1 - (x - t0)) / (np.sqrt(2.0) * tauG)

        with np.errstate(over="ignore", invalid="ignore"):
            y = pop1 * np.exp(exponent) * erfc(erfc_arg) / 2.0

        return y

    @staticmethod
    def ExpGaussPeriodic(beta0, x, pulseI, n_pulses):
        """
        C#-style previous-pulse sum for one component.

        beta0 follows [pop, k, tauG, t0], and pulseI is in the same units as x.
        """
        beta0 = np.asarray(beta0, dtype=float)
        y = np.zeros_like(np.asarray(x, dtype=float), dtype=float)
        beta_shift = beta0.copy()

        for m in range(int(n_pulses) + 1):
            beta_shift[3] = beta0[3] - m * pulseI
            y += FLIMDecayFitter.ExpGauss(beta_shift, x)

        return y

    @staticmethod
    def ExpGaussArray(beta0, x, pulseI):
        """
        C#-style one-exponential array model.

        beta0 follows ImageProcessing.ExpGaussArray:
            [pop, k, tauG, t0, baseline]
        """
        beta0 = np.asarray(beta0, dtype=float)
        k1 = beta0[1]
        if k1 <= 0.0:
            return np.full_like(np.asarray(x, dtype=float), np.nan, dtype=float)

        m_required = int(np.ceil(-1.0 / k1 / pulseI * np.log(1e-4)))
        n_pulses = max(1, m_required)
        y = FLIMDecayFitter.ExpGaussPeriodic(beta0[:4], x, pulseI, n_pulses)
        return y + beta0[4]

    @staticmethod
    def Exp2GaussArray(beta0, x, pulseI):
        """
        C#-style two-exponential array model.

        beta0 follows ImageProcessing.Exp2GaussArray:
            [pop1, k1, pop2, k2, tauG, t0, baseline]
        """
        beta0 = np.asarray(beta0, dtype=float)
        x = np.asarray(x, dtype=float)
        y = np.zeros_like(x, dtype=float)
        tauG = beta0[4]
        t0 = beta0[5]
        baseline = beta0[6]

        for m in range(2):
            ai = beta0[2 * m]
            ki = beta0[2 * m + 1]
            beta_single = np.array([ai, ki, tauG, t0], dtype=float)
            if ki <= 0.0:
                return np.full_like(x, np.nan, dtype=float)

            m_required = int(np.ceil(-1.0 / ki / pulseI * np.log(1e-4)))
            n_pulses = max(1, m_required)
            y += FLIMDecayFitter.ExpGaussPeriodic(beta_single, x, pulseI, n_pulses)

        return y + baseline

    @staticmethod
    def _period_from_sync_rate(sync_rate_hz, time_unit):
        T_rep_s = 1.0 / float(sync_rate_hz)
        if time_unit == "ns":
            return T_rep_s * 1e9
        if time_unit == "ps":
            return T_rep_s * 1e12
        raise ValueError("time_unit must be 'ns' or 'ps'")

    # ----------------- reconvolution with periodic pulses ----------------- #
    def reconvolution_model(self, t, params, n_pulses=None):
        """
        Compute model decay curve for given time axis t and full param vector.

        params: [A1, tau1, A2, tau2, ..., An, taun, t0, sigma, bg]
        If self.sync_rate_hz is None: single-pulse response.
        If not None: analytical sum of previous-pulse contributions.
        """
        t = np.asarray(t, dtype=float)

        amps = np.array(params[0:2 * self.n_exp:2], dtype=float)
        taus = np.array(params[1:2 * self.n_exp:2], dtype=float)
        t0, sigma, bg = params[-3], params[-2], params[-1]

        y = np.zeros_like(t, dtype=float)

        if self.sync_rate_hz is None:
            for A, tau in zip(amps, taus):
                y += self.ExpGauss([A, 1.0 / tau, sigma, t0], t)
            return y + bg

        T_rep = self._period_from_sync_rate(self.sync_rate_hz, self.time_unit)
        if T_rep <= 0.0:
            raise ValueError("repetition period <= 0; check sync_rate_hz")

        for A, tau in zip(amps, taus):
            if n_pulses is None:
                m_required = int(np.ceil(-float(tau) / T_rep * np.log(1e-4)))
                n_component_pulses = max(1, m_required)
            else:
                n_component_pulses = int(n_pulses)

            beta_single = [A, 1.0 / tau, sigma, t0]
            y += self.ExpGaussPeriodic(beta_single, t, T_rep, n_component_pulses)

        return y + bg

    # ----------------- fitting helpers ----------------- #
    def _default_initial_guess(self, t, y):
        """Heuristic initial guess for [A1, tau1, ..., t0, sigma, bg]."""
        t = np.asarray(t, dtype=float)
        y = np.asarray(y, dtype=float)
        dt = t[1] - t[0]
        bg0 = max(float(np.min(y)), 0.0)
        A_guess = max(float(np.max(y) - bg0), 1.0)

        cum = np.cumsum(y)
        total = cum[-1]
        if total > 0:
            t50 = t[np.searchsorted(cum, 0.5 * total)]
        else:
            t50 = t[0] + 10 * dt
        tau_guess = max(t50 - t[0], dt)

        t0_guess = t[np.argmax(np.gradient(y))]
        sigma_guess = 2 * dt

        if self.n_exp == 1:
            amps = [A_guess]
            taus = [tau_guess]
        elif self.n_exp == 2:
            amps = [0.7 * A_guess, 0.3 * A_guess]
            taus = [tau_guess / 2, tau_guess * 2]
        else:
            amps = [0.5 * A_guess, 0.3 * A_guess, 0.2 * A_guess]
            taus = [tau_guess / 3, tau_guess, tau_guess * 3]

        params = []
        for A, tau in zip(amps, taus):
            params += [A, tau]
        params += [t0_guess, sigma_guess, bg0]
        return np.array(params, dtype=float)

    def _split_params(self, full_params, fixed_params):
        """Split full param vector into free subset + index map."""
        if fixed_params is None:
            fixed_params = {}

        free = []
        free_idx = []
        fixed = {}

        for i, name in enumerate(self.param_names):
            if name in fixed_params:
                fixed[i] = float(fixed_params[name])
            else:
                free_idx.append(i)
                free.append(full_params[i])

        return np.array(free, dtype=float), free_idx, fixed

    def _merge_params(self, free_values, free_idx, fixed):
        """Rebuild full parameter vector from free and fixed pieces."""
        full = np.zeros(len(self.param_names), dtype=float)
        free_iter = iter(free_values)
        for i in range(len(self.param_names)):
            if i in fixed:
                full[i] = fixed[i]
            else:
                full[i] = next(free_iter)
        return full

    # ----------------- public fit API ----------------- #
    def fit(self, t, y, p0=None, bounds=None, fixed_params=None):
        """
        Fit decay using weighted least squares.

        After fitting, result has:
            res.x_full   : full parameter vector
            res.chi2     : chi-square = sum_i ((y_i - m_i)/sigma_i)^2
            res.red_chi2 : reduced chi-square = chi2 / dof
        """
        t = np.asarray(t, dtype=float)
        y = np.asarray(y, dtype=float)
        assert t.ndim == 1 and y.ndim == 1 and t.size == y.size

        w = 1.0 / np.sqrt(np.clip(y, 1.0, None))

        if p0 is None:
            p0 = self._default_initial_guess(t, y)
        else:
            p0 = np.asarray(p0, dtype=float)
            assert p0.size == len(self.param_names)

        p0_free, free_idx, fixed = self._split_params(p0, fixed_params)

        if bounds is None:
            dt = t[1] - t[0]
            low = []
            high = []
            for _ in range(self.n_exp):
                low += [0.0, 1e-4]
                high += [np.inf, (t[-1] - t[0]) * 20]
            low += [t[0] - 5 * dt, dt / 4, 0.0]
            high += [t[-1] + 5 * dt, (t[-1] - t[0]), np.inf]
            low = np.array(low)
            high = np.array(high)
            bounds = (low[free_idx], high[free_idx])

        def residuals(p_free):
            p_full = self._merge_params(p_free, free_idx, fixed)
            m = self.reconvolution_model(t, p_full)
            return (m - y) * w

        res = least_squares(residuals, p0_free, bounds=bounds)

        p_full = self._merge_params(res.x, free_idx, fixed)
        res.x_full = p_full

        r_w = residuals(res.x)
        chi2 = float(np.sum(r_w ** 2))

        n_points = t.size
        n_free = len(free_idx)
        dof = max(n_points - n_free, 1)

        res.chi2 = chi2
        res.red_chi2 = chi2 / dof
        res.dof = dof

        return res

    def compute_chi2(self, t, y, params):
        """
        Compute chi-square and reduced chi-square for given data and full params.
        """
        t = np.asarray(t, dtype=float)
        y = np.asarray(y, dtype=float)
        w = 1.0 / np.sqrt(np.clip(y, 1.0, None))
        m = self.reconvolution_model(t, params)
        r_w = (m - y) * w
        chi2 = float(np.sum(r_w ** 2))
        dof = max(len(t) - len(self.param_names), 1)
        red_chi2 = chi2 / dof
        return chi2, red_chi2


if __name__ == "__main__":
    from FLIMageFileIO import FLIMTiff
    from matplotlib import pyplot as plt

    flim_path = r"Series002_t1.flim"
    flim = FLIMTiff()
    flim.read(flim_path)

    lifetime_range = [2, 130]
    flim.calculatePage(
        page=0,
        fastZpage=0,
        channel=0,
        lifetimeRange=lifetime_range,
        intensityLimit=[0, 5],
        lifetimeLimit=[1.6, 4.0],
        lifetimeOffset=0.5,
    )

    sync_rate_hz = float(flim.State.Spc.datainfo.syncRate[0])
    channel = 0

    dt_ps = flim.State.Spc.spcData.resolution[channel]
    dt_ns = dt_ps / 1000
    t_full = np.arange(flim.FLIM3D.shape[2]) * dt_ns

    start, end = lifetime_range
    t = t_full[start:end]
    y = flim.lifetime[start:end]

    n_exp = 2
    fitter = FLIMDecayFitter(n_exp=n_exp, sync_rate_hz=sync_rate_hz, time_unit="ns")
    res = fitter.fit(t, y)

    params = res.x_full
    y_fit = fitter.reconvolution_model(t, params)

    if n_exp == 1:
        A1, tau1, t0, sigma, bg = params
        p1 = 1
        print(f"tau1 = {tau1:0.3f} ns ({p1:0.2f})%")

    if n_exp == 2:
        A1, tau1, A2, tau2, t0, sigma, bg = params
        p1 = A1 / (A1 + A2) * 100
        p2 = A2 / (A1 + A2) * 100

        print(f"tau1 = {tau1:0.3f} ns ({p1:0.2f}%)")
        print(f"tau2 = {tau2:0.3f} ns ({p2:0.2f}%)")

    print(f"Sigma = {sigma:0.3f} ns")
    print(f"t0 = {t0:0.3f} ns")
    print(f"bg = {bg}")
    print("chi2:", res.chi2)
    print("reduced chi2:", res.red_chi2)

    plt.semilogy(t, y)
    plt.semilogy(t, y_fit)
    plt.show()
