function spc_calc_timeCourseFromStack (slices)
global spc gui

if ~nargin
    slices = 1:spc.stack.nStack;
end

%%%%%%%%%%%%%%
[PATHSTR,fileNAME,EXT] = fileparts(spc.filename);

cd(PATHSTR);

fname = [fileNAME, '_ROI2'];
evalc(['global  ', fname]);

if ~spc.switches.noSPC
    nChannels = spc.datainfo.scan_rx;
else
    nChannels = 1; %spc.state.acq.numberOfChannelsAcquire;
end

filename = [PATHSTR, filesep, fname, '.mat'];
if exist(filename, 'file')
    load(filename);
    evalc(['Ch = ', fname]);
end

for channelN = 1:nChannels
    try
        a = Ch(channelN).roiData;
        bg = Ch(channelN).bgData;
    end
end

%%%%%%%%%%%%%%%%%%%%%%%%%%%%

nRoi = length(gui.spc.figure.roiB);
if ~spc.switches.noSPC
    range = spc.fit(spc.currentChannel).range;
end
pos_max2 = str2num(get(gui.spc.spc_main.F_offset, 'String'));
if pos_max2 == 0 || isnan (pos_max2)
    pos_max2 = 1.0;
end

if spc.switches.redImg
    img_greenMax = spc.state.img.greenMax;
    img_redMax = spc.state.img.redMax;
end

for j=1:nRoi
    a(j).time = spc.scanHeader.acq.linesPerFrame*spc.scanHeader.acq.msPerLine*[1:spc.stack.nStack];
    a(j).time = [1:spc.stack.nStack];
end


spc_switchChannel;
for fn=slices
    spc.page = fn;
    spc.switches.currentPage = spc.page;
    set(gui.spc.spc_main.spc_page, 'String', num2str(spc.page));
    
    spc_redrawSetting;
    pause(0.01);
    
    for j = 1:nRoi
        if ishandle(gui.spc.figure.roiB(j))
            siz = size(spc.project);
            siz(2) = siz(2) / nChannels;
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
                ROIreg = roipoly(ones(siz(1), siz(2)), x1, y1);
            elseif strcmp(get(gui.spc.figure.roiB(j), 'Type'), 'line')
                xi = get(gui.spc.figure.roiB(j),'XData');
                yi = get(gui.spc.figure.roiB(j),'YData');
                ROIreg = roipoly(ones(siz(1), siz(2)), xi, yi);
                ROI = [xi(:), yi(:)];
            else
                ROI = 0;
                goodRoi = 0;
            end
            if goodRoi
                if j ~= 1
                    if ~spc.switches.noSPC
                        bw = (spc.project >= spc.fit(gui.spc.proChannel).lutlim(1));
                        lifetimeMap = spc.lifetimeMap(bw & ROIreg);
                        lifetimeMap = spc.lifetimeMap(ROIreg);
                        project = spc.project(bw & ROIreg);
                        lifetime = sum(lifetimeMap.*project)/sum(project);
                        
                        t1 = (1:range(2)-range(1)+1);
                        t1 = t1(:);
                        tauD = spc.fit(gui.spc.proChannel).beta0(2)*spc.datainfo.psPerUnit/1000;
                        tauAD = spc.fit(gui.spc.proChannel).beta0(4)*spc.datainfo.psPerUnit/1000;
                        tau_m = lifetime;
                    end
                end
                
                F_int = spc.project(ROIreg);
                F_int = F_int(~isnan(F_int));
                intensity = sum(F_int(:));
                nPixel = intensity / mean(F_int(:));
                
                roiN = j-1;
                
                if roiN == 0 || isempty(roiN) %%%%%Background
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
                        img_greenMax = spc.state.img.greenImg(:,:,fn);
                        green_R = img_greenMax(ROIreg);
                        bg.green_int(fn) = sum(green_R);
                        bg.green_mean(fn) = mean(green_R);
                        bg.green_nPixel(fn) = bg.green_int(fn) / bg.green_mean(fn);
                        img_redMax = spc.state.img.redImg(:,:,fn);
                        red_R = img_redMax(ROIreg);
                        bg.red_int(fn) = sum(red_R);
                        bg.red_mean(fn) = mean(red_R);
                        bg.red_nPixel(fn) = bg.red_int(fn) / bg.mean_int(fn);
                    end
                    
                else %No background
                    if spc.switches.noSPC
                    else
                        a(roiN).fraction2(fn) = tauD*(tauD-tau_m)/(tauD-tauAD) / (tauD + tauAD -tau_m);
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
                        img_redMax = spc.state.img.redImg(:,:,fn);
                        red_R = img_redMax(ROIreg);
                        a(roiN).red_int(fn) = sum(red_R);
                        a(roiN).red_mean_raw(fn) = mean(red_R);
                        a(roiN).red_mean(fn) = mean(red_R) - bg.red_mean(fn);
                        a(roiN).red_nPixel(fn) = a(roiN).red_int(fn) / mean(red_R);
                        a(roiN).red_int2(fn) = a(roiN).red_mean(fn)*a(roiN).red_nPixel(fn);
                        a(roiN).red_max(fn) = max(red_R) -  bg.red_mean(fn);
                        img_greenMax = spc.state.img.greenImg(:,:,fn);
                        green_R = img_greenMax(ROIreg);
                        a(roiN).green_int(fn) = sum(green_R);
                        a(roiN).green_mean_raw(fn) = mean(green_R);
                        a(roiN).green_mean(fn) = mean(green_R) - bg.green_mean(fn);
                        a(roiN).green_nPixel(fn) = a(roiN).green_int(fn) / mean(green_R);
                        a(roiN).green_int2(fn) = a(roiN).green_mean(fn)*a(roiN).green_nPixel(fn);
                        a(roiN).green_max(fn) = max(green_R) -  bg.green_mean(fn);
                        a(roiN).ratio = a(roiN).green_mean./a(roiN).red_mean;
                        
                    end %reImg
                    
                end %Background
            end %%%Good Roi
        end %%%is handle
    end
end

Ch(channelN).roiData = a;
Ch(channelN).bgData = bg;
end%Channel.

%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

Ch(1).filename = spc.filename;
Ch(1).roiData(1).filename = fname;
evalc([fname, '= Ch']);
evalc ([fname, '.stack = stack']);

save(fname, fname);

%%%%%%%%%%%%%%%%%%%%%%%
Figure
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

color_a = {[0.7,0.7,0.7], 'red', 'blue', 'green', 'magenta', 'cyan', [1,0.5,0],'black'};
if ~spc.switches.noSPC
    fig_content = {'tau_m', 'int_int2'};
    fig_yTitle = {'tau_m', 'Intensity(Green)'};
else
    fig_content = {'ratio', 'int_int2', 'red_int2'};
    fig_yTitle = {'Ratio', 'Intensity(Green)', 'Intensity(Red)'};
end

figFormat = [length(fig_content), 1];

sSiz = get(0, 'ScreenSize');
fpos = [50   100   500   sSiz(4)-200];
if isfield(gui.spc.figure, 'fastFramePlot')
    if ishandle(gui.spc.figure.fastFramePlot)
        figure(gui.spc.figure.fastFramePlot);
    else
        gui.spc.figure.fastFramePlot = figure('position', fpos);
    end
else
    gui.spc.figure.fastFramePlot = figure('position', fpos);
end


for subP = 1:prod(figFormat)  %Three figures
    error = 0;
    
    
    subplot(figFormat(1), figFormat(2), subP);
    hold off;
    legstr = [];
    for channelN = 1:nChannels
        for j=1:nRoi-1
            if ishandle(gui.spc.figure.roiB(j+1))
                k = mod(j, length(color_a))+1;
                time1 = a(j).time;
                if j==1 && subP == 1
                    basetime = min(time1);
                end
                t = (time1 - basetime);
                
                try
                    evalc(['val = Ch(channelN).roiData(j).', fig_content{subP}]);
                    
                    if length(t) == length(val)
                        plot(t, val, '-', 'color', color_a{k}, 'linewidth', channelN);
                    else
                        plot(val, '-', 'color', color_a{k}, 'linewidth', channelN);
                        error = 1;
                    end
                end
                hold on;
                str1 = sprintf('Ch%01d,ROI%02d', channelN, j);
                legstr = [legstr; str1];
            end
        end
    end
    legend(legstr);
    ylabel(['\fontsize{12} ', fig_yTitle{subP}]);
    
    if ~error
        xlabel ('\fontsize{12} Frame');
    else
        xlabel ('\fontsize{12} ERROR');
    end
end

