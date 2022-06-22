function [beta,r,J] = spc_nlinfit(X,y,weight,model,beta0)
%Modified by Ryohei.
% [BETA,R,J] = NLINFIT(X,Y,WEIGHT,FUN,BETA0)
% WEIGHT is weight for fitting. For fitting to photo-measurement,
% you should use sqrt(y).
%
%NLINFIT Nonlinear least-squares data fitting by the Gauss-Newton method.
%   NLINFIT(X,Y,FUN,BETA0) estimates the coefficients of a nonlinear
%   function.  Y is a vector.  X is a vector or matrix with the same
%   number of rows as Y.  FUN is a function that accepts two arguments,
%   a coefficient vector and an array of X values, and returns a vector
%   of fitted Y values.  BETA0 is a vector containing initial guesses for
%   the coefficients.
%
%   [BETA,R,J] = NLINFIT(X,Y,FUN,BETA0) returns the fitted coefficients
%   BETA, the residuals R, and the Jacobian J.  You can use these outputs
%   with NLPREDCI to produce error estimates on predictions, and with
%   NLPARCI to produce error estimates on the estimated coefficients.
%
%   Examples
%   --------
%   FUN can be specified using @:
%      nlintool(x, y, @myfun, b0)
%   where MYFUN is a MATLAB function such as:
%      function yhat = myfun(beta, x)
%      b1 = beta(1);
%      b2 = beta(2);
%      yhat = 1 ./ (1 + exp(b1 + b2*x));
%
%   FUN can also be an inline object:
%      fun = inline('1 ./ (1 + exp(b(1) + b(2)*x))', 'b', 'x')
%      nlintool(x, y, fun, b0)
%
%   See also NLPARCI, NLPREDCI, NLINTOOL.

%   B.A. Jones 12-06-94.
%   Copyright 1993-2000 The MathWorks, Inc. 
% $Revision: 2.20 $  $Date: 2000/05/26 18:53:20 $

%if (nargin<4), error('NLINFIT requires four arguments.'); end
if (nargin<5), error('NLINFIT requires five arguments.'); end

if min(size(y)) ~= 1
   error('Requires a vector second input argument.');
end
y = y(:);
weight = weight(:); %Added by RYohei
%weight = 1;
if size(X,1) == 1 % turn a row vector into a column vector.
   X = X(:);
end

wasnan = (isnan(y) | any(isnan(X),2));
if (any(wasnan))
   y(wasnan) = [];
   X(wasnan,:) = [];
end
n = length(y);

p = length(beta0);
beta0 = beta0(:);

J = zeros(n,p);
beta = beta0;
betanew = beta + 1;
maxiter = 100; %default = 100
iter = 0;
betatol = 0.5E-6;  %default = 1e-4
rtol = 0.5E-6; %default = 1e-4
sse = 1;
sseold = sse;
seps = sqrt(eps);
zbeta = zeros(size(beta));
s10 = sqrt(10);
eyep = eye(p);
zerosp = zeros(p,1);
while (norm((betanew-beta)./(beta+seps)) > betatol | abs(sseold-sse)/(sse+seps) > rtol) & iter < maxiter
   if iter > 0, 
      beta = betanew;
   end

   iter = iter + 1;
   yfit = feval(model,beta,X);
   r = y - yfit;
   r = r./weight;  %added by Ryohei
   sseold = r'*r;

   for k = 1:p,
      delta = zbeta;
      if (beta(k) == 0)
         nb = sqrt(norm(beta));
         delta(k) = seps * (nb + (nb==0));
      else
         delta(k) = seps*beta(k);
      end
      yplus = feval(model,beta+delta,X);
      J(:,k) = (yplus - yfit)/delta(k);
   end

   Jplus = [J;(1.0E-2)*eyep];
   rplus = [r;zerosp];

   % Levenberg-Marquardt type adjustment 
   % Gauss-Newton step -> J\r
   % LM step -> inv(J'*J+constant*eye(p))*J'*r
   step = Jplus\rplus;
   
   betanew = beta + step;
   yfitnew = feval(model,betanew,X);
   rnew = y - yfitnew;
   rnew = rnew./weight; %Added by RYohei
   sse = rnew'*rnew;
   iter1 = 0;
   while sse > sseold & iter1 < 12
      step = step/s10;
      betanew = beta + step;
      yfitnew = feval(model,betanew,X);
      rnew = y - yfitnew;
      rnew = rnew./weight; %Added by Ryohei
      sse = rnew'*rnew; 
      iter1 = iter1 + 1;
   end
end
if iter == maxiter
    beep;
    beep;
   disp('NLINFIT did NOT converge. Returning results from last iteration.');
end
%disp('Iteration number =');
%disp(iter);