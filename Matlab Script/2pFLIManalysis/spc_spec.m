function [prf, background, pulse_int] = spc_spec(x, move);
global spc

range = spc.fit.range;

pulse_int = round(spc.datainfo.pulseInt*1000/spc.datainfo.psPerUnit);

%--------------------
%PRF calculation
%prf = spc.fit.prf(range(1):1:range(2))/sum(spc.fit.prf);
%prf = spc.fit.prf/sum(spc.fit.prf);

%prf = spc.fit.prf(:, 2);
%t = spc.fit.prf(:, 1);
%mag = spc.datainfo.psPerUnit/(t(2)-t(1));

%prf = interp1(t, prf, t*mag+move, 'linear');
%prf(isnan(prf))=0;
%prf = prf(1:length(x));
prf = spc.fit.prf;

