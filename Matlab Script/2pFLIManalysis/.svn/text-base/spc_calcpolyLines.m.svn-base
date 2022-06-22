function a = spc_calcpolyLines(graph);
global spc;
global gui;


if ~nargin
    graph = 0;
end
stepsize = 1;
%radius = 6;

roiP = get(gui.spc.figure.polyRoi{1}, 'Position');
radius = roiP(3)/2;
xx = get(gui.spc.figure.polyLine, 'XData');
yy = get(gui.spc.figure.polyLine, 'YData');
xx_shift = [xx(2:end), xx(end)];
yy_shift = [yy(2:end), yy(end)];
d_rr = sqrt((xx_shift - xx).^2+(yy_shift - yy).^2);
rr = cumsum(d_rr);
%rr = sqrt((xx-xx(1)).^2+(yy-yy(1)).^2);
rstep = [0:stepsize:rr(end)];
xx1 = 1:length(rstep);
yy1 = 1:length(rstep);
polyLifetime = 1:length(rstep);

j=1;
for i=0:stepsize:rr(end)
    xx1(j) = mean(xx(i == round(rr)));
    yy1(j) = mean(yy(i == round(rr)));
    j = j+1;
end

tauD = spc.fit.beta0(2)*spc.datainfo.psPerUnit/1000;
tauAD = spc.fit.beta0(4)*spc.datainfo.psPerUnit/1000;
for i=1:length(xx1)
    theta = [0:1/20:1]*2*pi;
    xr = radius;
    yr = radius;
    xc = xx1(i);
    yc = yy1(i);
    x1 = round(sqrt(xr^2*yr^2./(xr^2*sin(theta).^2 + yr^2*cos(theta).^2)).*cos(theta) + xc);
    y1 = round(sqrt(xr^2*yr^2./(xr^2*sin(theta).^2 + yr^2*cos(theta).^2)).*sin(theta) + yc);
    siz = spc.size;
    ROIreg = roipoly(ones(siz(2), siz(3)), x1, y1);
    bw = (spc.project >= spc.switches.lutlim(1));
    if sum(spc.project(ROIreg & bw)) > 0
        polyLifetime(i) = sum(spc.lifetimeMap(ROIreg & bw).*spc.project(ROIreg &bw))/sum(spc.project(ROIreg & bw));
        tau_m = polyLifetime(i);
        BF(i) = tauD*(tauD-tau_m)/(tauD-tauAD) / (tauD + tauAD -tau_m);
    else
        polyLifetime(i) = nan;
        BF(i) = nan;
    end
end

for i=1:length(gui.spc.figure.polyRoi)
    roiPos{i} = get(gui.spc.figure.polyRoi{i}, 'Position'); 
end

a.fraction = BF;
a.lifetime = polyLifetime;
a.coordinateX = xx1;
a.coordinateY = yy1;
a.contourL = rstep;
a.radius = radius;
a.roiPos = roiPos

if graph
    figure; plot(rstep, BF);
end