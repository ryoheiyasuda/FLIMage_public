function a = spc_calcRoi
global spc;
global gui;


if ~spc.switches.noSPC
    nChannels = spc.datainfo.scan_rx;
else
    nChannels = spc.state.acq.numberOfChannelsAcquire;
end

filterwindow = 1; %if neccesary.
if length(findobj('Tag', 'RoiA0')) == 0
    beep;
    errordlg('Set the background ROI (roi 0)!');
    return;
end

[filepath, basename, fn, max1] = spc_AnalyzeFilename(spc.filename);

sSiz = get(0, 'ScreenSize');
fpos = [50   100   500   sSiz(4)-200];

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

if ~isnan(str2double(basename(1)))
    basename = ['A_', basename];
end

fname = [basename, '_ROI2'];
evalc(['global  ', fname]);

if exist([fname, '.mat'], 'file')
    load([fname, '.mat'], fname);
    evalc(['Ch = ', fname]);
end

for channelN = 1:nChannels
        try
            a = Ch(channelN).roiData;
            bg = Ch(channelN).bgData;
        end
        gui.spc.proChannel = channelN;
        spc.currentChannel = channelN;
        spc_switchChannel;
        
        if get(gui.spc.spc_main.fit_eachtime, 'Value')
                try
                    betahat=spc_fitexp2gauss;
                    spc_redrawSetting(1);
                    fit_error = 0;
                catch
                    fit_error = 1;
                end
            else
            fit_error = 1;
        end
        
        pause(0.1);
        
        nRoi = length(gui.spc.figure.roiB);
        if ~spc.switches.noSPC
            range = spc.fit(channelN).range;
        end

        project1 = filter2(ones(filterwindow,filterwindow)/filterwindow^2, spc.project);
        if spc.switches.redImg
            img_greenMax = filter2(ones(filterwindow,filterwindow)/filterwindow^2, spc.state.img.greenMax);
            img_redMax = filter2(ones(filterwindow,filterwindow)/filterwindow^2, spc.state.img.redMax);
        end


        for j = 1:nRoi
            if ishandle(gui.spc.figure.roiB(j));
                siz = spc.size;
                %siz(2) = siz(2) / nChannels;
                siz(2) = size(spc.project, 1);
                siz(3) = size(spc.project, 2);
                goodRoi = 1;
                if strcmp(get(gui.spc.figure.roiB(j), 'Type'), 'rectangle')
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
                    ROIreg = roipoly(ones(siz(2), siz(3)), x1, y1);
                elseif strcmp(get(gui.spc.figure.roiB(j), 'Type'), 'line')
                    xi = get(gui.spc.figure.roiB(j),'XData');
                    yi = get(gui.spc.figure.roiB(j),'YData');
                    ROIreg = roipoly(ones(siz(2), siz(3)), xi, yi);
                    ROI = [xi(:), yi(:)];
                else
                    ROI = 0;
                    goodRoi = 0;
                end
                if goodRoi
                    if j ~= 1
                        if ~spc.switches.noSPC
                            bw = (spc.project >= spc.fit(spc.currentChannel).lutlim(1));        
                            lifetimeMap = spc.lifetimeMap(bw & ROIreg);
                            %lifetimeMap = spc.lifetimeMap(ROIreg);
                            project = spc.project(bw & ROIreg);
                            lifetime = sum(lifetimeMap.*project)/sum(project); 

                            t1 = (1:range(2)-range(1)+1); 
                            t1 = t1(:);
                            tauD = spc.fit(channelN).beta0(2)*spc.datainfo.psPerUnit/1000;
                            tauAD = spc.fit(channelN).beta0(4)*spc.datainfo.psPerUnit/1000;
                            tau_m = lifetime;
                        end
                    end

                    F_int = project1(ROIreg);
                    F_int = F_int(~isnan(F_int));
                    intensity = sum(F_int(:));
                    nPixel = intensity / mean(F_int(:));

                    roiN = j-1;

                    if roiN == 0 || isempty (roiN) %%%%%Background
                        if spc.switches.noSPC
                            bg.time(fn) = datenum(spc.state.internal.triggerTimeString);
                        else
                            bg.time(fn) = datenum([spc.datainfo.date, ',', spc.datainfo.time]);
                        end
                        bg.position{fn} = ROI;
                        bg.mean_int(fn) = mean(F_int(:));
                        bg.nPixel(fn) = nPixel;
                        bg.mean_int(fn) = mean(F_int(:));
                        if spc.switches.redImg
                            green_R = img_greenMax(ROIreg);
                            bg.green_int(fn) = sum(green_R);
                            bg.green_mean(fn) = mean(green_R);
                            bg.green_nPixel(fn) = bg.green_int(fn) / bg.mean_int(fn);             
                            red_R = img_redMax(ROIreg);
                            bg.red_int(fn) = sum(red_R);
                            bg.red_mean(fn) = mean(red_R);
                            bg.red_nPixel(fn) = bg.red_int(fn) / bg.mean_int(fn);
                        end

                    else %not background
                        if spc.switches.noSPC
                            a(roiN).time(fn) = datenum(spc.state.internal.triggerTimeString);
                            a(roiN).time3(fn) = datenum(spc.state.internal.triggerTimeString); 
                        else
                            a(roiN).time(fn) = datenum([spc.datainfo.date, ',', spc.datainfo.time]);
                            a(roiN).time3(fn) = datenum(spc.datainfo.triggerTime);
                            a(roiN).fraction2(fn) = spc_getFraction((tau_m + spc.fit(spc.currentChannel).t_offset)*1000/spc.datainfo.psPerUnit); %tauD*(tauD-tau_m)/(tauD-tauAD) / (tauD + tauAD -tau_m);
                            a(roiN).tauD(fn) = tauD;
                            a(roiN).tauAD(fn) = tauAD;        
                            a(roiN).tau_m(fn) = tau_m;
                        end
                        a(roiN).position{fn} = ROI;
                        a(roiN).nPixel(fn) = nPixel;
                        a(roiN).mean_int(fn) = mean(F_int(:))- bg.mean_int(fn);
                        a(roiN).int_int2(fn) = a(roiN).mean_int(fn)*a(roiN).nPixel(fn);
                        a(roiN).max_int(fn) = max(F_int(:)) - bg.mean_int(fn);

                        if spc.switches.redImg
                            red_R = img_redMax(ROIreg);
                            a(roiN).red_int(fn) = sum(red_R);
                            a(roiN).red_mean(fn) = mean(red_R) - bg.red_mean(fn);
                            a(roiN).red_nPixel(fn) = a(roiN).red_int(fn) / mean(red_R);
                            a(roiN).red_int2(fn) = a(roiN).red_mean(fn)*a(roiN).red_nPixel(fn);
                            a(roiN).red_max(fn) = max(red_R) -  bg.red_mean(fn);
                            green_R = img_greenMax(ROIreg);
                            a(roiN).green_int(fn) = sum(green_R);
                            a(roiN).green_mean(fn) = mean(green_R) - bg.green_mean(fn);
                            a(roiN).green_nPixel(fn) = a(roiN).green_int(fn) / mean(green_R);
                            a(roiN).green_int2(fn) = a(roiN).green_mean(fn)*a(roiN).green_nPixel(fn);
                            a(roiN).green_max(fn) = max(green_R) -  bg.green_mean(fn);
                            a(roiN).ratio = a(roiN).green_mean./a(roiN).red_mean;

            %                 siz = size(spc.state.img.greenImg);%%%%%%%Calculate stack
            %                 if length(siz) == 3
            %                     stack.roiData(roiN).green_nPixel(fn) = bg.green_nPixel(fn);
            %                     stack.roiData(roiN).red_nPixel(fn) = bg.red_nPixel(fn);
            %                     for i=1:siz(3)
            %                         imgi = spc.state.img.greenImg(:,:,i);
            %                         green_R = imgi(ROIreg);
            %                         stack.roiData(roiN).mean_green{fn}(i) = mean(green_R, 1);
            % 
            %                         imgi = spc.state.img.redImg(:,:,i);
            %                         red_R = imgi(ROIreg);
            %                         stack.roiData(roiN).mean_red{fn}(i) = mean(red_R, 1);
            %                     end
            %                 end

                        end %reImg

                    end %Background
                end %%%Good Roi
            end %%%is handle
        end %nRoi

        Ch(channelN).roiData = a;
        Ch(channelN).bgData = bg;
        
        if isfield(gui.spc.figure, 'polyRoi')
                if ishandle(gui.spc.figure.polyRoi{1})
                    nPoly = length(gui.spc.figure.polyRoi);
                else
                    nPoly = 0;
                end
        else
            nPoly = 0;
        end
        if nPoly
            la = spc_calcpolyLines;
            %evalc([fname, '(', num2str(channelN) ,').polyLines{fn} = la']);
            %evalc(['tmp=', fname, '(', num2str(channelN), ').polyLines']);
            Ch(channelN).polyLines{fn} = la;
            tmp = Ch(channelN).polyLines;
            for i=1:length(tmp)
                try
                    dLen(i) = length(tmp{i}.fraction);
                catch
                    dLen(i) = nan;
                end
            end
            len = min(dLen);
            %for j=1:length(tmp{1}.fraction);
                for i=1:length(tmp)
                    try
                        dend(i, 1:len) = tmp{i}.fraction(1:len);
                    catch
                        dend(i, 1:len) = nan(1, len);
                    end
                end
            %end
            %evalc([fname, '(', num2str(channelN), ').Dendrite = dend']);
            Ch(channelN).Dendrite = dend;
        end
end

Ch(1).filename = spc.filename;
Ch(1).roiData(1).filename = fname;
evalc([fname, '= Ch']);
% evalc ([fname, '.roiData = a']);
% evalc ([fname, '.bgData = bg']);
%evalc ([fname, '.stack = stack']);
save(fname, fname);
%Aout = a;
%%%%%%%%%%%%%%%%%%%%%%%%
%Figure

color_a = {[0.7,0.7,0.7], 'red', 'blue', 'green', 'magenta', 'cyan', [1,0.5,0],'black'};
fig_contentA = {'fraction2', 'tau_m', 'int_int2', 'red_int2', 'ratio'};
fig_yTitleA = {'Fraction', 'Tau_m', 'Intensity(FLIM)', 'Intensity(R)', 'ratio'};
fc(1) = get(gui.spc.spc_main.fracCheck, 'Value');
fc(2) = get(gui.spc.spc_main.tauCheck, 'Value');
fc(3) = get(gui.spc.spc_main.greenCheck, 'Value');
fc(4) = get(gui.spc.spc_main.redCheck, 'Value');
fc(5) = get(gui.spc.spc_main.RatioCheck, 'Value');

if spc.switches.noSPC
        fc(1) = 0;
        fc(2) = 0;
        set(gui.spc.spc_main.fracCheck, 'Value', 0);
        set(gui.spc.spc_main.tauCheck, 'Value', 0);
end

k = 0;
for j = 1:length(fc)
    if fc(j)
        k = k+1;
        fig_content{k} = fig_contentA{j};
        fig_yTitle{k} = fig_yTitleA{j};
    end
end
if ~k
    return;
end

panelN = k;
figFormat = [panelN, 1]; %panelN by 1 subplot.

basetime = 0;
for subP = 1:panelN  %Three figures
    error = 0;

    figure (gui.spc.calcRoi);
    subplot(figFormat(1), figFormat(2), subP);
    hold off;
    legstr = [];
    for channelN = 1:nChannels
        for j=1:nRoi-1
            if ishandle(gui.spc.figure.roiB(j+1)) 
                if (strcmp(get(gui.spc.figure.roiB(j+1), 'Type'), 'rectangle') || strcmp(get(gui.spc.figure.roiB(j+1), 'Type'), 'line'))
                    k = mod(j, length(color_a))+1;
                    time1 = a(j).time3;
                    time1 = time1(a(j).time > 1000);
                    if basetime == 0
                        basetime = min(time1);
                    end
                    t = (time1 - basetime)*24*60;
                    evalc(['val = Ch(channelN).roiData(j).', fig_content{subP}]);
                    val = val(a(j).time > 1000);
                    if length(t) == length(val)
                        plot(t, val, '-o', 'color', color_a{k}, 'linewidth', channelN);
                    else
                        plot(val, '-o', 'color', color_a{k}, 'linewidth', channelN);
                        error = 1;
                    end
                    hold on;
                    str1 = sprintf('Ch%01d,ROI%02d', channelN, j);
                    legstr = [legstr; str1];
                end
            end
        end;
    end
    if subP == panelN
        hl = legend(legstr);
        pos = get(hl, 'position');
        set(hl, 'position', [0, 0, pos(3), pos(4)]);
    end
    ylabel(['\fontsize{12} ', fig_yTitle{subP}]);

    if ~error
        xlabel ('\fontsize{12} Time (min)');
    else
        xlabel ('\fontsize{12} ERROR');
    end
end
