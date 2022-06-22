function betahat=spc_fitexp2gauss
global spc gui

if isfield(spc.fit(spc.currentChannel), 'fixtau')
    fixtau = spc.fit(spc.currentChannel).fixtau;
else
    spc.fit(spc.currentChannel).fixtau = zeros(1,6);
    fixtau = zeros(1,6);
end
 
range = spc.fit(spc.currentChannel).range;

lifetime = spc.lifetime(range(1):1:range(2));
x = 1:1:length(lifetime);

[val_max, pos_max] = max(lifetime);

beta0 = spc_initialValue_double;
    
weight = 1./sqrt(lifetime); %/sqrt(max(lifetime));
weight(lifetime < 1)=1; %/sqrt(max(lifetime));

try
    %betahat = spc_nlinfit(x, lifetime+1, weight, @exp2gauss, beta0);
    betahat = spc_nlinfit(x, lifetime, weight, @exp2gauss, beta0);
end

for j = [2, 4, 5, 6]
    if fixtau(j)
        betahat(j) = beta0(j);
    end
end

spc.fit(spc.currentChannel).beta0 = betahat;
spc.fit(spc.currentChannel).curve = exp2gauss(betahat, x);
spc.fit(spc.currentChannel).fittype = 'doubleExp';

%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
tauD = spc.fit(spc.currentChannel).beta0(2)*spc.datainfo.psPerUnit/1000;
tauAD = spc.fit(spc.currentChannel).beta0(4)*spc.datainfo.psPerUnit/1000;

pop2 = betahat(3) / (betahat(1) + betahat(3)); %spc_getFraction(sum(lifetime.*x)/sum(lifetime));
pop1 = 1 - pop2;

tau_m = (tauD*tauD*pop1+tauAD*tauAD*pop2)/(tauD*pop1 + tauAD*pop2);
tau_m2 = sum(lifetime.*x)/sum(lifetime)*spc.datainfo.psPerUnit/1000;
shift1 = tau_m2 - tau_m;
spc.fit(spc.currentChannel).t_offset = shift1;