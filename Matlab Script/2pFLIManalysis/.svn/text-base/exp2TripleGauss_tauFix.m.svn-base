function y = exp2TripleGauss_tauFix(beta0, x);
%beta0 = pop1
%beta1 = pop2

global spc;


beta1 = spc.fit.beta0;

%exp2TripleGauss;
%beta0(1) = tau1
%beta0(2) = peak1
%beta0(3) = peak position
%beta0(4) = tau2
%beta0(5) = peak2

beta1(2) = beta0(1);
beta1(5) = beta0(2);

y = exp2TripleGauss(beta1, x);