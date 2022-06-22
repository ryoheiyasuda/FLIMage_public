function spc_fitWithSingleExp_triple;

global spc

try
   triple = spc.fit.tripleGauss;
catch
    triple = [0.0006
    1.0000
   -0.0343
    0.0830
    0.0156
    0.5527
   -0.4447
    0.5842
    0.0682
    0.1103];
    spc.fit.tripleGauss = triple;
end;

range = spc.fit.range;
lifetime = spc.lifetime(range(1):1:range(2));
x = [1:1:length(lifetime)];

[val_max, pos_max] = max(lifetime);

beta0 = [max(lifetime), sum(lifetime)/max(lifetime), 0,0,pos_max];
weight = sqrt(lifetime)/sqrt(max(lifetime));
weight (weight <1) = 1;

betahat = spc_nlinfit(x, lifetime, weight, @expTripleGauss, beta0);

tau = betahat(2)*spc.datainfo.psPerUnit/1000;
%tau2 = betahat(1)*spc.datainfo.psPerUnit/1000;
%Drawing

fit = expTripleGauss(betahat, x);
t = [range(1):1:range(2)];
t = t*spc.datainfo.psPerUnit/1000;

spc_drawfit (t, fit, lifetime, betahat);