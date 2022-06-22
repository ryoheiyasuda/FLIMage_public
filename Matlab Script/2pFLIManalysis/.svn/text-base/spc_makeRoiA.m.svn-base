function spc_makeRoiA(i);
global spc
global gui

i=i+1;
figure(gui.spc.figure.project);
waitforbuttonpress;
figure(gcf);


point1 = get(gca,'CurrentPoint');    % button down detected
finalRect = rbbox;                   % return figure units
point2 = get(gca,'CurrentPoint');    % button up detected
point1 = point1(1,1:2);              % extract x and y
point2 = point2(1,1:2);
p1 = min(point1,point2);             % calculate locations
offset = abs(point1-point2);         % and dimensions
% if offset(1) > offset(2)
%     offset = [offset(1), offset(1)]
% else
%     offset = [offset(2), offset(2)]
% end
spc_roi = round([p1, offset]);

%set(gui.spc.figure.roiA(i), 'Position', spc_roi);
rectstr = ['RoiA', num2str(i-1)];
textstr = ['TextA', num2str(i-1)];
Rois = findobj('Tag', rectstr);
Texts = findobj('Tag', textstr);

for j = 1:length(Rois)
    delete(Rois(j));
end
for j = 1:length(Rois)
    delete(Texts(j));
end

figure(gui.spc.figure.project);
axes(gui.spc.figure.projectAxes);
gui.spc.figure.roiA(i) = rectangle('Position', spc_roi, 'Tag', rectstr, 'EdgeColor', 'cyan', 'Curvature', [1,1], 'ButtonDownFcn', 'spc_dragRoiA');
gui.spc.figure.textA(i) = text(spc_roi(1)-2, spc_roi(2)-2, num2str(i-1), 'color', 'white', 'Tag', textstr, 'ButtonDownFcn', 'spc_deleteRoiA');
%spc_drawLifetime;

figure(gui.spc.figure.lifetimeMap);
axes(gui.spc.figure.lifetimeMapAxes);
gui.spc.figure.roiB(i) = rectangle('Position', spc_roi, 'Tag', rectstr, 'EdgeColor', 'cyan', 'Curvature', [1,1], 'ButtonDownFcn', 'spc_dragRoiA');
gui.spc.figure.textB(i) = text(spc_roi(1)-2, spc_roi(2)-2, num2str(i-1), 'color', 'white', 'Tag', textstr, 'ButtonDownFcn', 'spc_deleteRoiA');

figure(gui.spc.figure.lifetime);

if spc.switches.redImg
    scan_roi = spc_roi; % - [4,0, 0, 0];
    figure(gui.spc.figure.scanImgF);
    axes(gui.spc.figure.scanImgA);
    gui.spc.figure.roiC(i) = rectangle('Position', scan_roi, 'Tag', rectstr, 'EdgeColor', 'cyan', 'Curvature', [1,1], 'ButtonDownFcn', 'spc_dragRoiA');
    gui.spc.figure.textC(i) = text(scan_roi(1)-2, scan_roi(2)-2, num2str(i-1), 'color', 'white', 'Tag', textstr, 'ButtonDownFcn', 'spc_deleteRoiA');
end

spc.fit.spc_roi{i} = get(gui.spc.figure.roiA(i), 'Position');
figure(gui.spc.figure.project);