function y = expgauss_wf(beta0, x);
%beta0(1): peak
%beta0(2): exp tau
%beta0(5): center
%beta0(6): gaussian width
% 1/2*erfc[(s^2-tau*x)/{sqrt(2)*s*tau}] * exp[s^2/2/tau^2 - x/tau]

global gui;
global spc;
pulseI=spc.datainfo.pulseInt / spc.datainfo.psPerUnit*1000;

y1 = beta0(1)*exp(beta0(6)^2/2/beta0(2)^2 - (x-beta0(5))/beta0(2));
y2 = erfc((beta0(6)^2-beta0(2)*(x-beta0(5)))/(sqrt(2)*beta0(2)*beta0(6)));
y=y1.*y2;

%"Pre" pulse
y1 = beta0(1)*exp(beta0(6)^2/2/beta0(2)^2 - (x-beta0(5)+pulseI)/beta0(2));
y2 = erfc((beta0(6)^2-beta0(2)*(x-beta0(5)+pulseI))/(sqrt(2)*beta0(2)*beta0(6)));

if spc.switches.imagemode == 1
    roi=get(gui.spc.figure.roi, 'Position');
else
    roi = [1,1,1,1];
end;

y = y1.*y2+y + spc.fit.background*roi(3)*roi(4);
y=y/2;