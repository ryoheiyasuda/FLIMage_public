function spc_fitWithDoubleExp_triple;
%beta0(1) = tau1
%beta0(2) = peak1
%beta0(3) = peak position
%beta0(4) = tau2
%beta0(5) = peak2

global spc;
global gui;
range = spc.fit.range;
lifetime = spc.lifetime(range(1):1:range(2));
x = [1:1:length(lifetime)];
t = [range(1):1:range(2)];
t = t*spc.datainfo.psPerUnit/1000;

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

[val_max, pos_max] = max(lifetime);

beta0 = [sum(lifetime)/max(lifetime)*1.5, max(lifetime)*0.45, pos_max, ...
    sum(lifetime)/max(lifetime)*0.5, max(lifetime)*0.7];

fit = exp2TripleGauss(beta0, x);
[val_max1, pos_max1] = max(fit);
corr = val_max1/val_max;
corr_pos = pos_max1 - pos_max;

beta0(2) = beta0(2)/corr;
beta0(5) = beta0(5)/corr;
beta0(3) = beta0(3) - corr_pos;

%fit = exp2TripleGauss(beta0, x);
%spc_drawfit (t, fit, lifetime, beta0);
weight = sqrt(lifetime)/sqrt(max(lifetime));
weight(lifetime == 0) = 1;
betahat = spc_nlinfit(x, lifetime, weight, @exp2TripleGauss, beta0);
%betahat = spc_nlinfit(x, lifetime, 1, @exp2TripleGauss, beta0);
%tau = betahat(2)*spc.datainfo.psPerUnit/1000;
%tau2 = betahat(1)*spc.datainfo.psPerUnit/1000;
%Drawing

fit = exp2TripleGauss(betahat, x);

spc_drawfit (t, fit, lifetime, betahat);

tau = betahat(1)*spc.datainfo.psPerUnit/1000;
tau2 = betahat(4)*spc.datainfo.psPerUnit/1000;
peaktime = (betahat(3)+range(1))*spc.datainfo.psPerUnit/1000;

handles = gui.spc.spc_main;
set(handles.beta1, 'String', num2str(betahat(2)));
set(handles.beta2, 'String', num2str(tau));
set(handles.beta3, 'String', num2str(betahat(5)));
set(handles.beta4, 'String', num2str(tau2));
set(handles.beta5, 'String', num2str(peaktime));

pop1 = betahat(2)/(betahat(2)+betahat(5));
pop2 = betahat(5)/(betahat(2)+betahat(5));
set(handles.pop1, 'String', num2str(pop1));
set(handles.pop2, 'String', num2str(pop2));
set(handles.average, 'String', num2str(tau*pop1+tau2*pop2));