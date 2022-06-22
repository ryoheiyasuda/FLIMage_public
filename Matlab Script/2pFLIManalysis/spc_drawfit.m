function spc_drawfit (t, fit, lifetime, channel)
global spc;
global gui;

%figure(gui.spc.figure.lifetime);
%lifetime = spc.lifetime(range(1):1:range(2));

residual = lifetime(:) - fit(:);

%axes(gui.spc.figure.lifetimeaxes);

set(gui.spc.figure.lifetimePlot, 'XData', t, 'YData', lifetime(:));
set(gui.spc.figure.fitPlot, 'XData', t, 'YData', fit);
set(gui.spc.figure.fitPlot, 'Color', 'red');
if (spc.switches.logscale == 0)
    set(gui.spc.figure.lifetimeAxes, 'YScale', 'linear');
else
    set(gui.spc.figure.lifetimeAxes, 'YScale', 'log');
end;

set(gui.spc.figure.lifetimeAxes, 'XTick', []);

try
    res = residual;
catch
    res = 0;
end

if (res ~= 0 & length(t) == length(res))
    set(gui.spc.figure.residualPlot, 'XData', t, 'YData', res);
end

spc.fit(channel).residual = residual;
%spc.fit(channel).curve = fit;
%spc.fit.beta0 = beta0;