function spc_fit2gauss(beta0);
global spc
range = spc.fit.range;
lifetime = spc.lifetime(range(1):1:range(2));
x = [1:1:length(lifetime)];

%[maxcount, index]=sort(spc.lifetime(range(1):1:range(2)));
%pos_max=index(length(index));
[val_max, pos_max] = max(lifetime);

%beta0 = [0, max(lifetime)*0.8, pos_max, 30, ...
%            max(lifetime)*0.3, pos_max+15, 30];
 
betahat = spc_nlinfit(x, lifetime, 1, @Twogauss, beta0);
%tau = betahat(2)*spc.datainfo.psPerUnit/1000;
%tau2 = betahat(1)*spc.datainfo.psPerUnit/1000;
%Drawing

fit = Twogauss(betahat, x);
t = [range(1):1:range(2)];
t = t*spc.datainfo.psPerUnit/1000;

spc_drawfit (t, fit, lifetime, betahat);