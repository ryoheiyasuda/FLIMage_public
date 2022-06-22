function spc_fitWithDoubleExp
global spc;
global gui;

%Make new PRF
prf = spc.fit.prf(:);

range = spc.fit.range;
lifetime = spc.lifetime(range(1):1:range(2));
x = [1:1:length(lifetime)];

try
	handles = gui.spc.spc_main;
	beta0(1)=str2num(get(handles.beta1, 'String'));
	beta0(2)=str2num(get(handles.beta2, 'String'));
	beta0(3)=str2num(get(handles.beta3, 'String'));
	beta0(4)=str2num(get(handles.beta4, 'String'));
	beta0(5)=str2num(get(handles.beta5, 'String'));
	beta0(2) = beta0(2)*1000/spc.datainfo.psPerUnit;
	beta0(4) = beta0(4)*1000/spc.datainfo.psPerUnit;
	beta0(5) = beta0(5)*1000/spc.datainfo.psPerUnit;
catch
end
%*1000/spc.datainfo.psPerUnit;
beta0(6) = 1;
if beta0(1) == 0
    beta0(1) = max(spc.lifetime(range(1):1:range(2)));
end
if beta0(2) == 0
    beta0(2) = sum(spc.lifetime(range(1):1:range(2)))/beta0(1);
    %set (handles.fixtau1, 'value', 0);
    %set (handles.fixtau2, 'value', 0);
%     spc.fit.fixtau1 = 0;
%     spc.fit.fixtau2 = 0;
end
if beta0(4) == 0
    b2 = beta0(2);
    beta0(2) = b2*1.3;
    beta0(4) = b2/2;
    %set (handles.fixtau1, 'value', 0);
    %set (handles.fixtau2, 'value', 0);
%     spc.fit.fixtau1 = 0;
%     spc.fit.fixtau2 = 0;
end
if beta0(3) == 0
    b1 = beta0(1);
    beta0(1) = b1*0.4;
    beta0(3) = b1*0.2;
end

weight = sqrt(lifetime)/sqrt(max(lifetime));
warning off MATLAB:divideByZero;
warning off MATLAB:Rank deficient;
weight(lifetime < 1)=1;
%weight = 1;

%figure(4)
%plot(x, lifetime, x, spc_reconv2(beta0,x));

%fitting !
betahat = spc_nlinfit(x, lifetime, weight, @spc_reconv2, beta0);
betahat(1) = abs(betahat(1));
betahat(3) = abs(betahat(3));

tau = betahat(2)*spc.datainfo.psPerUnit/1000;
tau2 = betahat(4)*spc.datainfo.psPerUnit/1000;
peaktime = betahat(5)*spc.datainfo.psPerUnit/1000;

pop1 = betahat(1)/(betahat(1)+betahat(3));
pop2 = betahat(3)/(betahat(1)+betahat(3));

spc_dispbeta;


fit = spc_reconv2(betahat, x);
t = [range(1):1:range(2)];
t = t*spc.datainfo.psPerUnit/1000;

%Drawing
spc_drawfit (t, fit, lifetime, betahat);
