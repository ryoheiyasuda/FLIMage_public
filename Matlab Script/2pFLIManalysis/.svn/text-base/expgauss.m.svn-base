function y = expgauss(beta0, x);
%beta0(1): peak
%beta0(2): exp tau
%beta0(5): center
%beta0(6): gaussian width
% 1/2*erfc[(s^2-tau*x)/{sqrt(2)*s*tau}] * exp[s^2/2/tau^2 - x/tau]

global gui;
global spc;

pulseI=spc.datainfo.pulseInt / spc.datainfo.psPerUnit*1000;
%pulseI = 120;


fix1 = get(gui.spc.spc_main.fixtau1, 'value');
fix2 = get(gui.spc.spc_main.fixtau2, 'value');
fix_g = get(gui.spc.spc_main.fix_g, 'value');
fix_d = get(gui.spc.spc_main.fix_delta, 'value');

if (fix1)
    value = str2double(get(gui.spc.spc_main.beta2, 'string'));
    tau1 = value*1000/spc.datainfo.psPerUnit;
else
    tau1 = beta0(2);
end

if (fix_g)
    value = str2double(get(gui.spc.spc_main.beta6, 'string'));
    tau_g = value * 1000/spc.datainfo.psPerUnit;
else
    tau_g = beta0(6);
end

if (fix_d)
    value =  str2double(get(gui.spc.spc_main.beta5, 'string'));
    tau_d = value * 1000/spc.datainfo.psPerUnit;
else
    tau_d = beta0(5);
end


y1 = beta0(1)*exp(tau_g^2/2/tau1^2 - (x-tau_d)/tau1);
y2 = erfc((tau_g^2-tau1*(x-tau_d))/(sqrt(2)*tau1*tau_g));
y=y1.*y2;

%"Pre" pulse
y1 = beta0(1)*exp(tau_g^2/2/tau1^2 - (x-tau_d+pulseI)/tau1);
y2 = erfc((tau_g^2-tau1*(x-tau_d+pulseI))/(sqrt(2)*tau1*tau_g));


y = y1.*y2+y ;
y=y/2;