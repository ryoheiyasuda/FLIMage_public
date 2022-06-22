function y = exp2gauss(beta0, x)
global gui;
global spc;


pulseI=spc.datainfo.pulseInt / spc.datainfo.psPerUnit*1000;

fix1 = get(gui.spc.spc_main.fixtau1, 'value');
fix2 = get(gui.spc.spc_main.fixtau2, 'value');
fix_g = get(gui.spc.spc_main.fix_g, 'value');
fix_d = get(gui.spc.spc_main.fix_delta, 'value');

if (fix1)
    value = str2double(get(gui.spc.spc_main.beta2, 'string'));
    tau1 = value*1000/spc.datainfo.psPerUnit;
    beta0(2) = tau1;
else
    tau1 = beta0(2);
end

if (fix2)
    value = str2double(get(gui.spc.spc_main.beta4, 'string'));
    tau2 = value*1000/spc.datainfo.psPerUnit;
    beta0(4) = tau2;
else
    tau2 = beta0(4);
end

if (fix_d)
    value =  str2double(get(gui.spc.spc_main.beta5, 'string'));
    tau_d = value * 1000/spc.datainfo.psPerUnit;
else
    tau_d = beta0(5);
end

if (fix_g)
    value = str2double(get(gui.spc.spc_main.beta6, 'string'));
    tau_g = value * 1000/spc.datainfo.psPerUnit;
else
    tau_g = beta0(6);
end



y1 = beta0(1)*exp(tau_g^2/2/tau1^2 - (x-tau_d)/tau1);
y2 = erfc((tau_g^2-tau1*(x-tau_d))/(sqrt(2)*tau1*tau_g));
y=y1.*y2;

%"Pre" pulse
y1 = beta0(1)*exp(tau_g^2/2/tau1^2 - (x-tau_d+pulseI)/tau1);
y2 = erfc((tau_g^2-tau1*(x-tau_d+pulseI))/(sqrt(2)*tau1*tau_g));

ya = y1.*y2+y;
ya = ya/2;

y1 = beta0(3)*exp(tau_g^2/2/tau2^2 - (x-tau_d)/tau2);
y2 = erfc((tau_g^2-tau2*(x-tau_d))/(sqrt(2)*tau2*tau_g));
y=y1.*y2;

y1 = beta0(3)*exp(tau_g^2/2/tau2^2 - (x-tau_d+pulseI)/tau2);
y2 = erfc((tau_g^2-tau2*(x-tau_d+pulseI))/(sqrt(2)*tau2*tau_g));

yb = y1.*y2+y;
yb = yb/2;

y=ya+yb;
