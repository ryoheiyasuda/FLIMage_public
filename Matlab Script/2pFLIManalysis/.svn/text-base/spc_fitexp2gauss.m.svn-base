function betahat=spc_fitexp2gauss;
global spc;
global gui;
range = spc.fit.range;
lifetime = spc.lifetime(range(1):1:range(2));
x = [1:1:length(lifetime)];

[val_max, pos_max] = max(lifetime);

beta0 = spc_initialValue_double;
    
weight = sqrt(lifetime)/sqrt(max(lifetime));
weight(lifetime < 1)=1;

betahat = spc_nlinfit(x, lifetime, weight, @exp2gauss, beta0);

fit = exp2gauss(betahat, x);
t = [range(1):1:range(2)];
t = t*spc.datainfo.psPerUnit/1000;

spc_drawfit (t, fit, lifetime, betahat);