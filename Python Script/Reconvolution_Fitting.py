# -*- coding: utf-8 -*-
"""
Created on Thu Nov 20 21:21:14 2025

@author: yasudar
"""

import numpy as np
from scipy.optimize import least_squares

class FLIMDecayFitter:
    """
    Reconvolution FLIM decay fitter with:
      - Gaussian IRF
      - 1, 2, or 3 exponential components
      - optional previous-pulse contributions (periodic excitation)
      - weighted least squares (w = 1/sqrt(max(N,1)))
      - ability to fix any subset of parameters

    Parameter order (full vector):
        [A1, tau1, A2, tau2, ..., An, taun, t0, sigma, bg]

    where:
        Ai   : amplitude of component i
        taui : lifetime of component i (same units as t)
        t0   : IRF center
        sigma: IRF width
        bg   : constant background
    """

    def __init__(self, n_exp=2, sync_rate_hz=None, time_unit="ns"):
        assert 1 <= n_exp <= 3, "n_exp must be 1, 2, or 3"
        self.n_exp = n_exp
        self.sync_rate_hz = sync_rate_hz
        self.time_unit = time_unit

        # Build parameter name list
        names = []
        for i in range(1, n_exp+1):
            names += [f"A{i}", f"tau{i}"]
        names += ["t0", "sigma", "bg"]
        self.param_names = names

    # ----------------- basic model pieces ----------------- #
    @staticmethod
    def gaussian_irf(t, t0, sigma):
        """Gaussian IRF, normalized to area ~1."""
        g = np.exp(-0.5 * ((t - t0) / sigma)**2)
        dt = t[1] - t[0]
        area = np.sum(g) * dt
        if area > 0:
            g /= area
        return g

    def exp_kernel(self, t, taus, amps):
        """Sum of exponentials for t >= 0."""
        k = np.zeros_like(t)
        mask = t >= 0
        if np.any(mask):
            tt = t[mask]
            val = np.zeros_like(tt, dtype=float)
            for A, tau in zip(amps, taus):
                val += A * np.exp(-tt / tau)
            k[mask] = val
        return k

    # ----------------- reconvolution with periodic pulses ----------------- #
    def reconvolution_model(self, t, params, n_pulses=None):
        """
        Compute model decay curve for given time axis t and full param vector.

        params: [A1, tau1, A2, tau2, ..., An, taun, t0, sigma, bg]
        If self.sync_rate_hz is None: single-pulse response.
        If not None: explicit sum of contributions from previous pulses.
        """
        t = np.asarray(t, float)
        dt = t[1] - t[0]
        N = len(t)

        # parse parameters
        amps = np.array(params[0:2*self.n_exp:2], dtype=float)
        taus = np.array(params[1:2*self.n_exp:2], dtype=float)
        t0, sigma, bg = params[-3], params[-2], params[-1]

        # ---- single-pulse response on extended time axis ----
        if self.sync_rate_hz is None:
            irf = self.gaussian_irf(t, t0, sigma)
            kernel = self.exp_kernel(t, taus, amps)
            conv = dt * np.convolve(irf, kernel, mode="full")[:N]
            return conv + bg

        # repetition period in same units as t
        T_rep_s = 1.0 / float(self.sync_rate_hz)
        if self.time_unit == "ns":
            T_rep = T_rep_s * 1e9
        elif self.time_unit == "ps":
            T_rep = T_rep_s * 1e12
        else:
            raise ValueError("time_unit must be 'ns' or 'ps'")

        period_bins = int(round(T_rep / dt))
        if period_bins <= 0:
            raise ValueError("period_bins <= 0; check sync_rate_hz and dt")

        # how many pulses to include? (auto if n_pulses is None)
        if n_pulses is None:
            tau_max = np.max(taus)
            # require exp(-m*T_rep/tau_max) < 1e-4
            m_required = int(np.ceil(-tau_max / T_rep * np.log(1e-4)))
            n_pulses = max(1, m_required)
            #print(f'N pulses = {n_pulses}')

        # extended time axis
        N_ext = N + n_pulses * period_bins
        t_ext = np.arange(N_ext) * dt

        irf_ext = self.gaussian_irf(t_ext, t0, sigma)
        kernel_ext = self.exp_kernel(t_ext, taus, amps)
        conv_ext = dt * np.convolve(irf_ext, kernel_ext, mode="full")[:N_ext]

        # periodic summation
        s = conv_ext[:N].copy()
        for k in range(1, n_pulses+1):
            start = k * period_bins
            if start >= len(conv_ext):
                break
            length = min(len(conv_ext) - start, N)
            s[:length] += conv_ext[start:start+length]

        return s + bg

    # ----------------- fitting helpers ----------------- #
    def _default_initial_guess(self, t, y):
        """Heuristic initial guess for [A1, tau1, ..., t0, sigma, bg]."""
        t = np.asarray(t, float)
        y = np.asarray(y, float)
        dt = t[1] - t[0]
        bg0 = max(float(np.min(y)), 0.0)
        A_guess = max(float(np.max(y) - bg0), 1.0)

        cum = np.cumsum(y)
        total = cum[-1]
        if total > 0:
            t50 = t[np.searchsorted(cum, 0.5 * total)]
        else:
            t50 = t[0] + 10*dt
        tau_guess = max(t50 - t[0], dt)

        t0_guess = t[np.argmax(np.gradient(y))]
        sigma_guess = 2 * dt

        # distribute amplitudes among components
        amps = []
        taus = []
        if self.n_exp == 1:
            amps = [A_guess]
            taus = [tau_guess]
        elif self.n_exp == 2:
            amps = [0.7*A_guess, 0.3*A_guess]
            taus = [tau_guess/2, tau_guess*2]
        else:  # 3 exponentials
            amps = [0.5*A_guess, 0.3*A_guess, 0.2*A_guess]
            taus = [tau_guess/3, tau_guess, tau_guess*3]

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
            res.chi2     : chi-square = sum_i ((y_i - m_i)/σ_i)^2
            res.red_chi2 : reduced chi-square = chi2 / dof
        """
        t = np.asarray(t, float)
        y = np.asarray(y, float)
        assert t.ndim == 1 and y.ndim == 1 and t.size == y.size

        # weights: Poisson -> var ~ N => w = 1/sqrt(max(N,1))
        w = 1.0 / np.sqrt(np.clip(y, 1.0, None))

        # initial guess
        if p0 is None:
            p0 = self._default_initial_guess(t, y)
        else:
            p0 = np.asarray(p0, float)
            assert p0.size == len(self.param_names)

        # split into free/fixed
        p0_free, free_idx, fixed = self._split_params(p0, fixed_params)

        # bounds for full vector, then restrict to free
        if bounds is None:
            dt = t[1] - t[0]
            n = self.n_exp
            low = []
            high = []
            for i in range(n):
                low += [0.0, 1e-4]  # A_i >= 0, tau_i >= 1e-4
                high += [np.inf, (t[-1]-t[0])*20]
            low += [t[0]-5*dt, dt/4, 0.0]                   # t0, sigma, bg
            high += [t[-1]+5*dt, (t[-1]-t[0]), np.inf]
            low = np.array(low)
            high = np.array(high)
            bounds = (low[free_idx], high[free_idx])

        def residuals(p_free):
            p_full = self._merge_params(p_free, free_idx, fixed)
            m = self.reconvolution_model(t, p_full)
            return (m - y) * w

        res = least_squares(residuals, p0_free, bounds=bounds)

        # Build full parameter vector
        p_full = self._merge_params(res.x, free_idx, fixed)
        res.x_full = p_full

        # ---------- chi-square calculation ----------
        # These residuals are exactly the weighted ones used in LS:
        r_w = residuals(res.x)          # (m - y)/σ
        chi2 = float(np.sum(r_w**2))    # Σ ((y-m)/σ)^2

        n_points = t.size
        n_free = len(free_idx)
        dof = max(n_points - n_free, 1)

        res.chi2 = chi2
        res.red_chi2 = chi2 / dof
        res.dof = dof

        return res

    # optional helper if you want to recompute χ² later
    def compute_chi2(self, t, y, params):
        """
        Compute chi-square and reduced chi-square for given data and full params.
        """
        t = np.asarray(t, float)
        y = np.asarray(y, float)
        w = 1.0 / np.sqrt(np.clip(y, 1.0, None))
        m = self.reconvolution_model(t, params)
        r_w = (m - y) * w
        chi2 = float(np.sum(r_w**2))
        dof = max(len(t) - len(self.param_names), 1)
        red_chi2 = chi2 / dof
        return chi2, red_chi2




if __name__ == "__main__":
    from FLIMageFileIO import FLIMTiff
    from matplotlib import pyplot as plt
    
    flim_path = r"Series002_t1.flim"
    flim = FLIMTiff()
    flim.read(flim_path)          # uses tifffile, no libtiff
    
    lifetime_range = [2,130]
    # compute maps for page 0, fastZ 0, channel 0
    flim.calculatePage(page=0, fastZpage=0, channel=0,
                        lifetimeRange=lifetime_range,
                        intensityLimit=[0, 5],
                        lifetimeLimit=[1.6, 4.0],
                        lifetimeOffset=0.5)

    sync_rate_hz = float(flim.State.Spc.datainfo.syncRate[0])
    channel = 0
    
    dt_ps = flim.State.Spc.spcData.resolution[channel]
    dt_ns = dt_ps / 1000
    t_full = np.arange(flim.FLIM3D.shape[2]) * dt_ns
    y_full = flim.lifetime
    
    # Extract only fitting range
    start, end = lifetime_range
    t = t_full[start:end]
    y = y_full[start:end]
    
    
#%%    
    n_exp = 2
    fitter = FLIMDecayFitter(n_exp=n_exp, sync_rate_hz=sync_rate_hz, time_unit="ns")

    y = flim.lifetime[lifetime_range[0]:lifetime_range[1]]
    fixed = {} #{"bg": 0.0,}
    res = fitter.fit(t, y) #, fixed_params=fixed)
    
    params = res.x_full
    y_fit = fitter.reconvolution_model(t, params)
   
    if n_exp == 1:
        A1, tau1, t0, sigma, bg = params
        p1 = 1
        print(f'tau1 = {tau1:0.3f} ns ({p1:0.2f})%')
    
    if n_exp == 2:
        A1, tau1, A2, tau2, t0, sigma, bg = params
        p1 = A1 / (A1 + A2) * 100
        p2 = A2 / (A1 + A2) * 100
        
        print(f'tau1 = {tau1:0.3f} ns ({p1:0.2f}%)')
        print(f'tau2 = {tau2:0.3f} ns ({p2:0.2f}%)')

    print(f'Sigma = {sigma:0.3f} ns')
    print(f't0 = {t0:0.3f} ns')
    print(f'bg = {bg}')
    print("chi2:", res.chi2)
    print("reduced chi2:", res.red_chi2)

    plt.semilogy(t, y)
    plt.semilogy(t, y_fit)
    plt.show()
    
    
    