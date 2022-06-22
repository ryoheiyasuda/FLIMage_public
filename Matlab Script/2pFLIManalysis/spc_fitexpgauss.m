function betahat=spc_fitexpgauss
global spc;
global gui;


range = spc.fit(spc.currentChannel).range;
lifetime = spc.lifetime(range(1):1:range(2));
fixtau = spc.fit(spc.currentChannel).fixtau;
x = [1:1:length(lifetime)];

[val_max, pos_max] = max(lifetime);

%beta0 = [max(lifetime), sum(lifetime)/max(lifetime), 0, 0, pos_max, tau_g];
try
	handles = gui.spc.spc_main;
	beta0(1)=str2num(get(handles.beta1, 'String'));
	beta0(2)=str2num(get(handles.beta2, 'String'));
	beta0(3)=str2num(get(handles.beta3, 'String'));
	beta0(4)=str2num(get(handles.beta4, 'String'));
	beta0(5)=str2num(get(handles.beta5, 'String'));
    beta0(6)=str2num(get(handles.beta6, 'String'));
	beta0(2) = beta0(2)*1000/spc.datainfo.psPerUnit;
	beta0(4) = beta0(4)*1000/spc.datainfo.psPerUnit;
	beta0(5) = beta0(5)*1000/spc.datainfo.psPerUnit;
    beta0(6) = beta0(6)*1000/spc.datainfo.psPerUnit;
catch
    beta0 = [0, 0, 0, 0, 0, 0];
end
if beta0(1) <= 100 || beta0(2) <= 0.5*1000/spc.datainfo.psPerUnit || beta0(2) >=5*1000/spc.datainfo.psPerUnit
    beta0(1) = max(spc.lifetime(range(1):1:range(2)));
    beta0(2) = sum(spc.lifetime(range(1):1:range(2)))/beta0(1);
end
%beta0(3) = 0;
%beta0(4) = 0;
if beta0(5) <= 0*1000/spc.datainfo.psPerUnit || beta0(5) >= 6*1000/spc.datainfo.psPerUnit 
    beta0(5) = 1000/spc.datainfo.psPerUnit;
end
if beta0(6) <= 0.05*1000/spc.datainfo.psPerUnit || beta0(6) >= 1.0*1000/spc.datainfo.psPerUnit 
    beta0(6) = 0.15*1000/spc.datainfo.psPerUnit;
end

%fix1 = get(gui.spc.spc_main.fixtau1, 'value');
% set(gui.spc.spc_main.fixtau2, 'value', 0); --why? 
betahat(4) = beta0(4);

weight = 1./sqrt(lifetime); %/sqrt(max(lifetime));
weight(lifetime < 1)=1; %/sqrt(max(lifetime));

%betahat = nlinfit(x, lifetime, @expgauss, beta0, 'Weights', weight);
betahat = spc_nlinfit(x, lifetime, weight, @expgauss, beta0);

for j = [2, 5, 6]
    if fixtau(j)
        betahat(j) = beta0(j);
    end
end

%Drawing
spc.fit(spc.currentChannel).beta0 = betahat;
spc.fit(spc.currentChannel).curve = expgauss(betahat, x);
spc.fit(spc.currentChannel).fittype = 'singleExp';

tauD = spc.fit(spc.currentChannel).beta0(2)*spc.datainfo.psPerUnit/1000;
tau_m = tauD;
tau_m2 = sum(lifetime.*x)/sum(lifetime)*spc.datainfo.psPerUnit/1000;
shift1 = tau_m2 - tau_m;
spc.fit(spc.currentChannel).t_offset = shift1;

% t = [range(1):1:range(2)];
% t = t*spc.datainfo.psPerUnit/1000;
% 
% spc_drawfit (t, fit, lifetime, betahat);