function stack = spc_ROIcalcium;
global spc;
global gui;

firstFrameDark = 1;

if length(findobj('Tag', 'RoiA0')) == 0
    beep;
    errordlg('Set the background ROI (roi 0)!');
    return;
end

[filepath, basename, fn, max1] = spc_AnalyzeFilename(spc.filename);
fpos = [53   192   568   713];
if isfield(gui.spc, 'calcRoi')
     if ishandle(gui.spc.calcRoi)
     else
         gui.spc.calcRoi = figure ('position', fpos);
     end
 else
      gui.spc.calcRoi = figure ('position', fpos);
end

spc_updateMainStrings;
name_start=findstr(spc.filename, '\');
name_start=name_start(end)+1;

cd (filepath);

fname = [basename, '_ROICalcium'];
evalc(['global  ', fname]);
if exist([fname, '.mat'])
    load([fname, '.mat'], fname);
    evalc(['stack = ', fname]);
end

nRoi = length(gui.spc.figure.roiB);


pos_max2 = str2num(get(gui.spc.spc_main.F_offset, 'String'));
if pos_max2 == 0 | isnan (pos_max2)
    pos_max2 = 1.0
end

if firstFrameDark
   g = spc.state.img.greenImg(:,:,1);
   back_g = mean(g(:));
   r = spc.state.img.redImg(:,:,1);
   back_r = mean(r(:));
else
    back_g = 0;
    back_r = 0;
end
for j = 1:nRoi
    if ishandle(gui.spc.figure.roiB(j));
        ROI = get(gui.spc.figure.roiB(j), 'Position');
        tagA = get(gui.spc.figure.roiB(j), 'Tag');
        RoiNstr = tagA(6:end);

        theta = [0:1/20:1]*2*pi;
        xr = ROI(3)/2;
        yr = ROI(4)/2;
        xc = ROI(1) + ROI(3)/2;
        yc = ROI(2) + ROI(4)/2;
        x1 = round(sqrt(xr^2*yr^2./(xr^2*sin(theta).^2 + yr^2*cos(theta).^2)).*cos(theta) + xc);
        y1 = round(sqrt(xr^2*yr^2./(xr^2*sin(theta).^2 + yr^2*cos(theta).^2)).*sin(theta) + yc);
        siz = spc.size;
        ROIreg = roipoly(ones(siz(2), siz(3)), x1, y1);

        nPixel = sum(ROIreg(:));
        
        roiN = j-1;
        if roiN == 0 | roiN == [] %%%%%Background
            stack.bg.time(fn) = datenum(spc.state.internal.triggerTimeString);
            stack.bg.position{fn} = ROI;
            if spc.switches.redImg              
                siz = size(spc.state.img.greenImg);%%%%%%%Calculate stack
                if length(siz) == 3
                    stack.bg.green_nPixel(fn) = nPixel;
                    stack.bg.red_nPixel(fn) = nPixel;
                    for i=1:siz(3)
                        imgi = spc.state.img.greenImg(:,:,i);
                        green_R = imgi(ROIreg);
                        stack.bg.mean_green{fn}(i) = mean(green_R, 1)-back_g;
                        imgi = spc.state.img.redImg(:,:,i);
                        red_R = imgi(ROIreg);
                        stack.bg.mean_red{fn}(i) = mean(red_R, 1)-back_r;
                    end
                end
            end            
        else
            stack.roiData(roiN).time(fn) = datenum(spc.state.internal.triggerTimeString);
            stack.roiData(roiN).time3(fn) = datenum(spc.state.internal.triggerTimeString); 
            stack.roiData(roiN).position{fn} = ROI;
            stack.roiData(roiN).nPixel(fn) = nPixel;
            
            if spc.switches.redImg                
                siz = size(spc.state.img.greenImg);%%%%%%%Calculate stack
                if length(siz) == 3
                    for i=1:siz(3)
                        imgi = spc.state.img.greenImg(:,:,i);
                        green_R = imgi(ROIreg);
                        if firstFrameDark
                            stack.roiData(roiN).mean_green{fn}(i) = mean(green_R, 1)-back_g;
                        else
                            stack.roiData(roiN).mean_green{fn}(i) = mean(stack.bg.mean_green{fn});
                        end
                      

                        imgi = spc.state.img.redImg(:,:,i);
                        red_R = imgi(ROIreg);
                        if firstFrameDark 
                            stack.roiData(roiN).mean_red{fn}(i) = mean(red_R, 1)-back_r;
                        else
                            stack.roiData(roiN).mean_red{fn}(i) = mean(stack.bg.mean_red{fn});
                        end

                    end
                    stack.roiData(roiN).ratio{fn} = stack.roiData(roiN).mean_green{fn}./stack.roiData(roiN).mean_red{fn};
                end
         
            end
            
        end
    end
end

stack(1).filename = fname;
evalc ([fname, ' = stack']);
save(fname, fname);


%Aout = a;
%%%%%%%%%%%%%%%%%%%%%%%%
%Figure
figFormat = [3, 1]; %3 by 1 subplot.
color_a = {[0.7,0.7,0.7], 'red', 'blue', 'green', 'magenta', 'cyan', [1,0.5,0],'black'};
fig_content = {'ratio', 'mean_green', 'mean_red'};
fig_yTitle = {'Ratio', 'Intensity(Green)', 'Intensity(Red)'};    
        
for subP = 1:3  %Three figures
    error = 0;

    figure (gui.spc.calcRoi);
    subplot(figFormat(1), figFormat(2), subP);
    hold off;
    legstr = [];
    for j=1:nRoi-1
        if ishandle(gui.spc.figure.roiB(j+1))
            k = mod(j, length(color_a))+1;
            time1 = stack.roiData(j).time;
            time1 = time1(stack.roiData(j).time > 1000);
            if j==1 && subP == 1
                basetime = min(time1);
            end
            t1 = (time1 - basetime)*24*60;
            evalc(['val = stack.roiData(j).', fig_content{subP}, '{fn}']);
            t = (1:length(val))*spc.state.acq.linesPerFrame * spc.state.acq.msPerLine;
            plot(t, val, '-o', 'color', color_a{k});
            hold on;
            legstr = [legstr; 'ROI', num2str(j)];
        end
    end;
    legend(legstr);
    ylabel(['\fontsize{12} ', fig_yTitle{subP}]);
    xlabel ('\fontsize{12} Time (sec)');

end
