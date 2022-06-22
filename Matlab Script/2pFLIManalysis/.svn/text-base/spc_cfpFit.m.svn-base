function spc_cfpFit;

global spc;
global gui;

range = spc.fit.range;
lifetime = spc.lifetime(range(1):1:range(2));
x = [1:1:length(lifetime)];
t = [range(1):1:range(2)];
t = t*spc.datainfo.psPerUnit/1000;

[val_max, pos_max] = max(lifetime);

yield1 = 0.6;
yield2 = 0; %Not active.
%beta0 = [yield, val_max/2, val_max/2, pos_max];
beta0 = [yield1, val_max/4, yield2, val_max/2, pos_max];

fit = cfpExp(beta0, x);
[val_max1, pos_max1] = max(fit);
corr = val_max1/val_max;
corr_pos = pos_max1 - pos_max;

beta0(2) = beta0(2)/corr;
beta0(4) = beta0(4)/corr;
beta0(5) = beta0(5) - corr_pos;
fit = cfpExp(beta0, x);
spc_drawfit (t, fit, lifetime, beta0);

betahat = spc_nlinfit(x, lifetime, sqrt(lifetime)/sqrt(max(lifetime)), @cfpExp, beta0);
%betahat = spc_nlinfit(x, lifetime, 1, @exp2TripleGauss, beta0);
%tau = betahat(2)*spc.datainfo.psPerUnit/1000;
%tau2 = betahat(1)*spc.datainfo.psPerUnit/1000;
%Drawing

fit = cfpExp(betahat, x);


spc_drawfit (t, fit, lifetime, betahat);

tau = (1-betahat(1))*1.2686;
tau2 = (1-betahat(1))*3.4756;
peaktime = (betahat(5)+range(1))*spc.datainfo.psPerUnit/1000;

handles = gui.spc.spc_main;
set(handles.beta1, 'String', num2str(betahat(2)));
set(handles.beta2, 'String', num2str(tau));
set(handles.beta3, 'String', num2str(betahat(4)));
set(handles.beta4, 'String', num2str(tau2));
set(handles.beta5, 'String', num2str(peaktime));

pop1 = betahat(2)/(betahat(2)+betahat(4));
pop2 = betahat(4)/(betahat(2)+betahat(4));
set(handles.pop1, 'String', num2str(pop1));
set(handles.pop2, 'String', num2str(pop2));