function y = spc_reconv(beta0, x);
global spc;

range = spc.fit.range;
%background = mean(spc.lifetime(range(1):range(1)+10));

pulse_int = round(spc.datainfo.pulseInt*1000/spc.datainfo.psPerUnit);
%pulse_int = 120;
prf1 = spc.fit.prf(range(1):range(2));
prf = interp1(x, prf1, x+beta0(5), 'linear');
prf(isnan(prf)) = 0;

lenx = length(x);
x = (1:lenx+pulse_int);
x = x(:);

%dt = tshift;
lifetime = beta0(1)*exp(-x/beta0(2));
y1 = conv(lifetime, prf);
y2 (1:lenx) = y1 (pulse_int+1:pulse_int+lenx);
y1 = y1(1:lenx);


y = y1(:) + y2(:);

