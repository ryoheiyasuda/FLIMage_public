function betahat=spc_fitgauss;
global spc
range = spc.fit.range;
lifetime = spc.lifetime(range(1):1:range(2));
x = [1:1:length(lifetime)];

%[maxcount, index]=sort(spc.lifetime(range(1):1:range(2)));
%pos_max=index(length(index));
[val_max, pos_max] = max(lifetime);

beta0 = [0, max(lifetime), pos_max, sum(lifetime)/max(lifetime)];

betahat = spc_nlinfit(x, lifetime, sqrt(lifetime)/sqrt(max(lifetime)), @gauss, beta0);
%tau = betahat(2)*spc.datainfo.psPerUnit/1000;
%tau2 = betahat(1)*spc.datainfo.psPerUnit/1000;
%Drawing

fit = gauss(betahat, x);
t = [range(1):1:range(2)];
t = t*spc.datainfo.psPerUnit/1000;

spc_drawfit (t, fit, lifetime, betahat);