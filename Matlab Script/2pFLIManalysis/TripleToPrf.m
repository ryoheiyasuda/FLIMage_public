function TripleToPrf;
global spc;

beta0 = spc.fit.beta0;
%1.. offset ---ignore
%258.. peak1
%369.. position
%470.. width

[val, pos] = max(spc.fit.curve);
psPerUnit = spc.datainfo.psPerUnit;

%gauss1
b2 = beta0(2);
beta0(1) = beta0(1)/b2;
beta0(2) = 1;
beta0(3) = (beta0(3)-pos)/1000*psPerUnit;
beta0(4) = beta0(4)/1000*psPerUnit;

%gauss2
beta0(5) = beta0(5)/b2;
beta0(6) = (beta0(6)-pos)/1000*psPerUnit;
beta0(7) = beta0(7)/1000*psPerUnit;

%gauss2
beta0(8) = beta0(8)/b2;
beta0(9) = (beta0(9)-pos)/1000*psPerUnit;
beta0(10) = beta0(10)/1000*psPerUnit;

spc.fit.tripleGauss = beta0;