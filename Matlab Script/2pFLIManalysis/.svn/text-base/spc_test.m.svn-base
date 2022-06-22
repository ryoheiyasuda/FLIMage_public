function spc_test

background = 50;
offset = 50;

x = [1:1:150];
beta2 = [2000, 10, 1000, 24, offset];
y = poissrnd(spc_reconv2(beta2, x));

beta1 = [max(y), (sum(y)-background*length(y))/max(y), offset];
[betahat1, r1, J1] = spc_nlinfit(x, y, sqrt(y)/sqrt(max(y)), @spc_reconv, beta1);

beta2 = [1700, 12, 820, 23, offset];
[betahat2,r2,J2] = spc_nlinfit(x, y, sqrt(y)/sqrt(max(y)), @spc_reconv2, beta2);
betahat2


figure(1);
plot(x, y, x, spc_reconv2(betahat2, x), ...
    x, spc_reconv(betahat1,x));
figure(2);
semilogy(x, y, x, spc_reconv2(betahat2, x),...
    x, spc_reconv(betahat1, x));
sum=sum(y)