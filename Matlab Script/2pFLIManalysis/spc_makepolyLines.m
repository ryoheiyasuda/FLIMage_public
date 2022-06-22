function spc_makepolyLines
global spc;
global gui;

delete(findobj('Tag', 'poly'));
radius =  spc.switches.polyline_radius;

[x, y] = getline;
gui.spc.figure.polyRoi = {};

% xx = [x(1):0.25:x(end)];
% yy = spline(x, y, xx);
[xx, yy] = spc_spline(x', y');
axes(gui.spc.figure.projectAxes);
gui.spc.figure.polyLine = line(xx, yy, 'Tag', 'poly', 'color', 'cyan');
for i=1:length(x)
   roiPos = [x(i)-radius, y(i)-radius, radius*2, radius*2];
   gui.spc.figure.polyRoi{i} = rectangle('Position', roiPos, 'Tag', 'poly', 'EdgeColor', 'cyan', ...
       'ButtonDownFcn', 'spc_polyDrag', 'Curvature', [1,1]);
end

axes(gui.spc.figure.lifetimeMapAxes);
gui.spc.figure.polyLineB = line(xx, yy, 'Tag', 'poly', 'color', 'cyan');
for i=1:length(x)
   roiPos = [x(i)-radius, y(i)-radius, radius*2, radius*2];
   gui.spc.figure.polyRoiB{i} = rectangle('Position', roiPos, 'Tag', 'poly', 'EdgeColor', 'cyan', ...
       'ButtonDownFcn', 'spc_polyDrag', 'Curvature', [1,1]);
end