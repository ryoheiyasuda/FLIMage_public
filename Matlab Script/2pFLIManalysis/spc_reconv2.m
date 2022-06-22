function y = spc_reconv2(beta0, x)
global gui;
global spc;

range = spc.fit.range;
background = mean(spc.lifetime(range(1):range(1)+10));
%background = 0;
%backprf = mean(spc.fit.prf(range(1):range(1)+10));
% fix1 = spc.fit.fixtau1;
% fix2 = spc.fit.fixtau2;
% fixd = spc.fit.fix_delta;
fix1 = get(gui.spc.spc_main.fixtau1, 'Value');
fix2 = get(gui.spc.spc_main.fixtau2, 'Value');
fixd = get(gui.spc.spc_main.fix_delta, 'Value');
if fixd
    value = str2double(get(gui.spc.spc_main.beta5, 'string'));
    deltapeak = value*1000/spc.datainfo.psPerUnit;
else
    deltapeak=beta0(5);
end

pulse_int = round(spc.datainfo.pulseInt*1000/spc.datainfo.psPerUnit);
prf1 = spc.fit.prf(range(1):range(2));
prf = interp1(x, prf1, x+deltapeak, 'linear');
prf(isnan(prf)) = 0;

lenx = length(x);
x = (1:lenx+pulse_int);
x = x(:);

if (fix1)
    value = str2double(get(gui.spc.spc_main.beta2, 'string'));
    tau1 = value*1000/spc.datainfo.psPerUnit;
else
    tau1 = beta0(2);
end

if (fix2)
    value = str2double(get(gui.spc.spc_main.beta4, 'string'));
    tau2 = value*1000/spc.datainfo.psPerUnit;
else
    tau2 = beta0(4);
end
%tau2 = 0.9*1000/spc.datainfo.psPerUnit;
%tau1 = beta0(2);
%tau2 = beta0(4);
beta0(1) = abs(beta0(1));
beta0(3) = abs(beta0(3));

lifetime = beta0(1)*exp(-x/tau1)+beta0(3)*exp(-x/tau2);
y1 = conv(lifetime, prf);

y2 (1:lenx) = y1 (pulse_int+1:pulse_int+lenx);
y1 = y1(1:lenx);


y = y1(:) + y2(:);