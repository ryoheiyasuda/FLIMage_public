function y = xexp(beta0, x);

global spc;

%y = beta0(1)*exp(-(x-beta0(2)).^2/2/beta0(3)^2);

y = (x > beta0(2))*beta0(4).*(x-beta0(2)).^(beta0(1))/beta0(3)^(beta0(1)+1) ...
    .*exp(-(x-beta0(2))/beta0(3))+beta0(5);