function spc_polyDrag(drag);
global spc;
global gui;

if ~nargin
    drag = 1;
end

if drag
    point1 = get(gca,'CurrentPoint'); % button down detected
    point1 = point1(1,1:2);              % extract x and y

    RoiRect = get(gco, 'Position');
    rectFigure = get(gcf, 'Position');
    rectAxes = get(gca, 'Position');

    xmag = (rectFigure(3)*rectAxes(3))/spc.size(3);  %pixel/screenpixel
    xoffset =rectAxes(1)*rectFigure(3);
    ymag = (rectFigure(4)*rectAxes(4))/spc.size(2);
    yoffset = rectAxes(2)*rectFigure(4);

    rect1 = [xmag*RoiRect(1)+xoffset+0.5, ymag*(spc.size(2)-RoiRect(2)-RoiRect(4))+yoffset+.5, xmag*RoiRect(3), ymag*RoiRect(4)];
    rect2 = dragrect(rect1);
    roiPos = [round((rect2(1)-xoffset)/xmag), round(spc.size(2)-RoiRect(4)-(rect2(2)-yoffset)/ymag), RoiRect(3), RoiRect(4)];
    
    shift = roiPos(1:2)-RoiRect(1:2);
    if strcmp(get(gcf, 'SelectionType'), 'normal')
        set(gco, 'position', roiPos);
    end
end

x = zeros(1,length(gui.spc.figure.polyRoi));
y = zeros(1,length(gui.spc.figure.polyRoi));
for i=1:length(gui.spc.figure.polyRoi)
    if (gca == gui.spc.figure.lifetimeMapAxes)
         roiPos = get(gui.spc.figure.polyRoiB{i}, 'Position');         
    else
         roiPos = get(gui.spc.figure.polyRoi{i}, 'Position');      
    end
    roiPos([1,2]) = roiPos([1,2])+roiPos([3,4])/2-[spc.switches.polyline_radius, spc.switches.polyline_radius];    
    roiPos([3,4]) = [spc.switches.polyline_radius*2, spc.switches.polyline_radius*2];
    if strcmp(get(gcf, 'SelectionType'), 'extend')
        roiPos([1,2]) = roiPos([1,2])+shift;
    end
    set(gui.spc.figure.polyRoi{i}, 'Position', roiPos);
    set(gui.spc.figure.polyRoiB{i}, 'Position', roiPos);
        x(i) = roiPos(1)+roiPos(3)/2;
        y(i) = roiPos(2)+roiPos(4)/2;
       
end


[xx, yy] = spc_spline(x, y);

set(gui.spc.figure.polyLine, 'XData', xx, 'YData', yy);
set(gui.spc.figure.polyLineB, 'XData', xx, 'YData', yy);
