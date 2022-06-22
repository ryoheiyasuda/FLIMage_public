function spc_adjustTauOffset
global spc;
global gui;

handles = gui.spc.spc_main;
if ~(get(gui.spc.spc_main.fixtau1, 'Value') & get(gui.spc.spc_main.fixtau2, 'Value'))
    errordlg ('Fix tau1 and tau2 !');
    return;
end
spc_fitexp2gauss;
range = spc.fit.range;
t1 = (1:length([range(1):range(2)]));
t1 = t1(:);
lifetime = spc.lifetime(range(1):range(2));
lifetime = lifetime(:);
a = sum(lifetime.*t1)/sum(lifetime)*spc.datainfo.psPerUnit/1000;
offset1 = a - str2num(get(gui.spc.spc_main.average, 'String'));
set(handles.F_offset, 'String', num2str(offset1));
spc_redrawSetting;

range = spc.fit.range;
lifetime = spc.lifetime(range(1):1:range(2));
x = [1:1:length(lifetime)];
beta0 = spc_initialValue_double;
fit = exp2gauss(beta0, x);
t = [range(1):1:range(2)];
t = t*spc.datainfo.psPerUnit/1000;
spc_drawfit (t, fit, lifetime, beta0);
spc_dispbeta;