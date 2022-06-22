function y = gauss(beta0, x);

global spc;

y = beta0(2)*exp(-(x-beta0(3)).^2/2/beta0(4)^2)+beta0(1);
