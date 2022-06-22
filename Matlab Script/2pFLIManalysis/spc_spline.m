function [xx, yy]=spc_spline(x, y)

step = 0.1;

theta = atan2(y(end)-y(1), x(end)-x(1));
xrot = [cos(theta), sin(theta); -sin(theta), cos(theta)]*[x; y]; 
xrotx = xrot(1,:);
yroty = xrot(2,:);
xxrot = [xrotx(1):step:xrotx(end)];

yyrot = interp1(xrotx, yroty, xxrot, 'pchip');

xxrotx = [cos(theta), -sin(theta); sin(theta), cos(theta)]*[xxrot; yyrot]; 
xx = xxrotx(1,:);
yy = xxrotx(2, :);
