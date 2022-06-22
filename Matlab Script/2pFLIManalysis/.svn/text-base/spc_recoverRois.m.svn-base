function spc_recoverRois;
global spc
global gui



[filepath, basename, fn, max] = spc_AnalyzeFilename(spc.filename);
if isfield(gui.spc, 'calcRoi')
     if ishandle(gui.spc.calcRoi)
     else
         gui.spc.calcRoi = figure;;
     end
 else
      gui.spc.calcRoi = figure;;
end

spc_updateMainStrings;
name_start=findstr(spc.filename, '\');
name_start=name_start(end)+1;


cd (filepath);
fname = [basename, '_ROI2'];
evalc(['global  ', fname]);
if exist([fname, '.mat'])
    load([fname, '.mat'], fname);
    try
        evalc(['a = ', fname, '.roiData']);
    catch
        a = 0;
    end
    try
        evalc(['bg = ', fname, '.bgData']);
    catch
        bg = 0;
    end
    try
        evalc(['la = ', fname, '.polyLines']);
    catch
        la = 0;
    end
end

if isstruct(a)
    nRoi = length(a);
    if nRoi > 0 || length(bg.position) >= fn
        for i = 0:nRoi
            %i
            rectstr = ['RoiA', num2str(i)];
            textstr = ['TextA', num2str(i)];
            Rois = findobj('Tag', rectstr);
            Texts = findobj('Tag', textstr);
            for j = 1:length(Rois)
                delete(Rois(j));
                delete(Texts(j));
            end
            try
                if i == 0
                    spc_roi = bg.position{fn};
                else
                    spc_roi = a(i).position{fn};
                end
            catch
                siz = 0;
            end
            siz = size(spc_roi);
            if siz(2) == 4
                figure(gui.spc.figure.project);
                axes(gui.spc.figure.projectAxes);
                gui.spc.figure.roiA(i+1) = rectangle('Position', spc_roi, 'Tag', rectstr, 'EdgeColor', 'cyan', 'Curvature', [1,1], 'ButtonDownFcn', 'spc_dragRoiB');
                gui.spc.figure.textA(i+1) = text(spc_roi(1)-2, spc_roi(2)-2, num2str(i), 'color', 'white', 'Tag', textstr, 'ButtonDownFcn', 'spc_deleteRoiA');
                %spc_drawLifetime;
            
                figure(gui.spc.figure.lifetimeMap);
                axes(gui.spc.figure.lifetimeMapAxes);
                gui.spc.figure.roiB(i+1) = rectangle('Position', spc_roi, 'Tag', rectstr, 'EdgeColor', 'cyan', 'Curvature', [1,1], 'ButtonDownFcn', 'spc_dragRoiB');
                gui.spc.figure.textB(i+1) = text(spc_roi(1)-2, spc_roi(2)-2, num2str(i), 'color', 'white', 'Tag', textstr, 'ButtonDownFcn', 'spc_deleteRoiA');

                if spc.switches.redImg
                    scan_roi = spc_roi; % - [4,0, 0, 0];
                    figure(gui.spc.figure.scanImgF);
                    axes(gui.spc.figure.scanImgA);
                    gui.spc.figure.roiC(i+1) = rectangle('Position', scan_roi, 'Tag', rectstr, 'EdgeColor', 'cyan', 'Curvature', [1,1], 'ButtonDownFcn', 'spc_dragRoiB');
                    gui.spc.figure.textC(i+1) = text(scan_roi(1)-2, scan_roi(2)-2, num2str(i), 'color', 'white', 'Tag', textstr, 'ButtonDownFcn', 'spc_deleteRoiA');
                end

                spc.fit.spc_roi{i+1} = get(gui.spc.figure.roiA(i+1), 'Position');
            elseif siz(2) == 2
                figure(gui.spc.figure.project);
                axes(gui.spc.figure.projectAxes);
                xi = spc_roi(:,1);
                yi = spc_roi(:,2);
                gui.spc.figure.roiA(i+1) = line(xi, yi, 'Tag', rectstr, 'color', 'cyan', 'ButtonDownFcn', 'spc_dragRoiB');
                gui.spc.figure.textA(i+1) = text(xi(1)-2, yi(1)-2, num2str(i), 'color', 'white', 'Tag', textstr, 'ButtonDownFcn', 'spc_deleteRoiA');
                figure(gui.spc.figure.lifetimeMap);
                axes(gui.spc.figure.lifetimeMapAxes);
                gui.spc.figure.roiB(i+1) = line(xi, yi, 'Tag', rectstr, 'color', 'cyan', 'ButtonDownFcn', 'spc_dragRoiB');
                gui.spc.figure.textB(i+1) = text(xi(1)-2, yi(1)-2, num2str(i), 'color', 'white', 'Tag', textstr, 'ButtonDownFcn', 'spc_deleteRoiA');
                if spc.switches.redImg
                    figure(gui.spc.figure.scanImgF);
                    axes(gui.spc.figure.scanImgA);
                    gui.spc.figure.roiC(i+1) = line(xi, yi, 'Tag', rectstr, 'color', 'cyan', 'ButtonDownFcn', 'spc_dragRoiB');
                    gui.spc.figure.textC(i+1) = text(xi(1)-2, yi(1)-2, num2str(i), 'color', 'white', 'Tag', textstr, 'ButtonDownFcn', 'spc_deleteRoiA');
                end
                spc.fit.spc_roi{i+1} = [xi(:), yi(:)];
            else %%%weired Roi
                
            end
        end
    end
end

if iscell(la)
    if isstruct(la{fn})
        spc.switches.polyline_radius = la{fn}.radius;
        delete(findobj('Tag', 'poly'));
        gui.spc.figure.polyRoi = {};
        x = la{fn}.coordinateX;
        y = la{fn}.coordinateY;
        [xx, yy] = spc_spline(x, y);
        axes(gui.spc.figure.projectAxes);
        gui.spc.figure.polyLine = line(xx, yy, 'Tag', 'poly', 'color', 'cyan');
        for i=1:length(la{fn}.roiPos)
           %roiPos = [x(i)-radius, y(i)-radius, radius*2, radius*2];
           roiPos = la{fn}.roiPos{i};
           gui.spc.figure.polyRoi{i} = rectangle('Position', roiPos, 'Tag', 'poly', 'EdgeColor', 'cyan', ...
               'ButtonDownFcn', 'spc_polyDrag', 'Curvature', [1,1]);
        end

        axes(gui.spc.figure.lifetimeMapAxes);
        gui.spc.figure.polyLineB = line(xx, yy, 'Tag', 'poly', 'color', 'cyan');
        for i=1:length(la{fn}.roiPos)
           roiPos = la{fn}.roiPos{i};
           gui.spc.figure.polyRoiB{i} = rectangle('Position', roiPos, 'Tag', 'poly', 'EdgeColor', 'cyan', ...
               'ButtonDownFcn', 'spc_polyDrag', 'Curvature', [1,1]);
        end
    end
end