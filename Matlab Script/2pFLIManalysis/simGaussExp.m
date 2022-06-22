function [y, tau, sum1] = simGaussExp(beta0, x, N);
global gui;
global spc;


tau1 = beta0(2);
tau2 = beta0(4);
tau_g = beta0(6);
tau_d = beta0(5);

y1 = beta0(1)*exp(tau_g^2/2/tau1^2 - (x-tau_d)/tau1);
y2 = erfc((tau_g^2-tau1*(x-tau_d))/(sqrt(2)*tau1*tau_g));
ya=y1.*y2/2;


y1 = beta0(3)*exp(tau_g^2/2/tau2^2 - (x-tau_d)/tau2);
y2 = erfc((tau_g^2-tau2*(x-tau_d))/(sqrt(2)*tau2*tau_g));
yb=y1.*y2/2;

y=ya+yb;
%Integ = sum(y);

y = y * N; %/ sum(y);
y = poissrnd(y);

tau = sum(y.*x)/sum(y)-beta0(5);
sum1 = sum(y);
