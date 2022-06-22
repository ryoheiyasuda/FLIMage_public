function y = cfpExp(beta0, x);
%yield, fret, yield2, fret2, position

global spc;

psPerUnit = spc.datainfo.psPerUnit;

%cfp data 2002/10/1 measured in the caltured
%hippocampal cells.

pop(1) = 0.59309;
tau(1) = 3.4756; %ns
pop(2) = 0.40691;
tau(2) = 1.2686;

tau = tau*1000/psPerUnit;

yield1 = beta0(1);
fret = beta0(2);

%yield2 = beta0(3);
yield2 = 0;
nonfret = beta0(4);
pos = beta0(5);

%NonFret..
beta1 = [tau(1)*(1-yield2), pop(1)*nonfret, pos, tau(2)*(1-yield2), pop(2)*nonfret];
y1 = exp2TripleGauss(beta1, x);

%FRET ..
beta2 = [tau(1)*(1-yield1), pop(1)*fret, pos, tau(2)*(1-yield1), pop(2)*fret];
y2 = exp2TripleGauss(beta2, x);

y = y1+y2;