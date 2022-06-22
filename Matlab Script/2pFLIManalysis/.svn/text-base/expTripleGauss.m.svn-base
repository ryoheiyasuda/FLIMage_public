function y = expTripleGauss(beta0, x);
%beta0(1) : tau
%beta0(2) : peak value
%beta0(3) : peak position


global spc;
global gui;

triple = spc.fit.tripleGauss;
psPerUnit = spc.datainfo.psPerUnit;

%tau, peak, pos
width(1:3) = abs(triple(4:3:10))*1000/psPerUnit;
center(1:3) = triple(3:3:9)*1000/psPerUnit;
%normf = sum(triple(2:3:8).*width*sqrt(2*pi));
normf=1;
peak(1:3) = triple(2:3:8)/normf;

beta1(6) = width(1);
beta1(2) = beta0(2);  %tau
beta1(1) = beta0(1)*peak(1);  %peak
beta1(5) = beta0(5)+center(1);  %center
y1 = expgauss(beta1, x);

beta1(6) = width(2);
beta1(2) = beta0(2);  %tau
beta1(1) = beta0(1)*peak(2);  %peak
beta1(5) = beta0(5)+center(2);  %center
y2 = expgauss(beta1, x);

beta1(6) = width(3);
beta1(2) = beta0(2);  %tau
beta1(1) = beta0(1)*peak(3);  %peak
beta1(5) = beta0(5)+center(3);  %center
y3 = expgauss(beta1, x);

y = y1+y2+y3;