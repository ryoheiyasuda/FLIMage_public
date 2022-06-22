function y = exp2TripleGauss(beta0, x);
%exp2TripleGauss;
%beta0(1) = tau1
%beta0(2) = peak1
%beta0(3) = peak position
%beta0(4) = tau2
%beta0(5) = peak2


global spc;



triple = spc.fit.tripleGauss;


psPerUnit = spc.datainfo.psPerUnit;
beta1(1:3) = beta0(1:3);
y1 = expTripleGauss(beta1, x);

beta1(1:2) = beta0(4:5);
y2 = expTripleGauss(beta1, x);

y = y1+y2;