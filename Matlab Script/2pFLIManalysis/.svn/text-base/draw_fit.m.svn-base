function draw_fit (t, fit, lifetime)
global gui;
global spc;

figure(gui.spc.figure.lifetime);
%lifetime = spc.lifetime(range(1):1:range(2));

residual = lifetime - fit;
if (spc.switches.logscale == 0)
    %plot(t, spc.lifetime, 'ButtonDownFcn', 'spc_logscale');
    plot(t, lifetime, t, fit);
else
    %semilogy(t, spc.lifetime, 'ButtonDownFcn', 'spc_logscale');
    semilogy(t, lifetime, t, fit);
end;
eval ('res = residual;', 'res = 0;');
if (res ~= 0 & length(t) == length(res))
    set (gca, 'Position', [0.15, 0.3, 0.8, 0.6]);
    gui.spc.figure.residual = axes ('position', [0.15, 0.1, 0.8, 0.18]);
    plot(t, res);
end
xlabel('Lifetime (ns)');
ylabel('Photon');