function spc_fitWithSingleExp
global spc;
global gui;

range = spc.fit.range;
lifetime = spc.lifetime(range(1):1:range(2));
x = [1:1:length(lifetime)];


prf = spc.fit.prf(:);
beta0 = [max(lifetime), sum(lifetime)/max(lifetime), 0, 0, 0];
fit = spc_reconv(beta0,x);
[value, pos] = max(fit);
[value1, pos1] = max(lifetime);
beta0(5) = pos1 - pos;
beta0(6) = 0;
weight = sqrt(lifetime)/sqrt(max(lifetime));
weight(lifetime < 1)=1;
%weight = 1;

%figure(4);
%plot(x, lifetime, x, spc_reconv(beta0,x));
%beta0;

%Fitting !!!
betahat = spc_nlinfit(x, lifetime, weight, @spc_reconv, beta0);
tau = betahat(2)*spc.datainfo.psPerUnit/1000;
peaktime = (betahat(5))*spc.datainfo.psPerUnit/1000;


%Drawing
fit = spc_reconv(betahat, x);
t = [range(1):1:range(2)];
t = t*spc.datainfo.psPerUnit/1000;

%Drawing
spc_drawfit (t, fit, lifetime, betahat);

% try
% 	handles = gui.spc.spc_main;
% 	set(handles.beta1, 'String', num2str(betahat(1)));
% 	set(handles.beta2, 'String', num2str(tau));
% 	set(handles.beta3, 'String', '0');
% 	set(handles.beta4, 'String', '0');
% 	set(handles.beta5, 'String', num2str(peaktime));
% catch
% end