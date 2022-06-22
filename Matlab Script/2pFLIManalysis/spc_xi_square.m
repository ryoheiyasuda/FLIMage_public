function xi = spc_xi_square(single)
global spc gui

if single == 1
    spc_fitexpgauss;
    n_parameters = 4;
elseif single == 2
    spc_fitexp2gauss;
    n_parameters = 6;
end

ch = spc.currentChannel;

range = spc.fit(ch).range;
curve = spc.lifetime(range(1):range(2));
residual = curve - spc.fit(ch).curve;

xi = sum(residual(:).^2./curve(:))/(length(curve) - n_parameters);