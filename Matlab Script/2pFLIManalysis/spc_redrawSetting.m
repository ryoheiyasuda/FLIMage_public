function spc_redrawSetting (recalc, calcmax)
%Recalc: re-calculation of Maxmum projection and lifetimeMap. 
%Not required for recalc of smooth or color.

global spc;
global gui;

%
if ~nargin
    recalc = 1;
end

if recalc == 1
    spc_maxProc_offLine;
end

try 
    roi_pos = get(gui.spc.figure.roi, 'Position');
catch
    roi_pos = [1, 1, spc.size(3)-1, spc.size(2)-1];
end

if ~spc.switches.noSPC
    if (spc.currentChannel > spc.nChannels)
        spc.currentChannel = spc.nChannels;
    end
end

for j = 1:length(gui.spc.figure.proChannel)
    set(gui.spc.figure.proChannel(j), 'Value', (spc.currentChannel == j));
    set(gui.spc.figure.FLIMchannel(j), 'Value', (spc.currentChannel == j));
    %disp(['set(gui.spc.spc_main.Ch', num2str(j), ', ''Value'', (spc.currentChannel == j))']);
    evalc(['set(gui.spc.spc_main.Ch', num2str(j), ', ''Value'', (spc.currentChannel == j))']);
end

%if flag
    if spc.switches.noSPC
        if spc.currentChannel == 1
            spc.project = spc.state.img.greenMax;
        else
            spc.project = spc.state.img.redMax;
        end
        if isfield(gui.spc.figure, 'proChannel')
            for i=1:length(gui.spc.figure.proChannel)
                val = get(gui.spc.figure.proChannel(i), 'Value');
                if val ~= (i==spc.currentChannel)
                    set(gui.spc.figure.proChannel(i), 'Value', (i==spc.currentChannel));
                    set(gui.spc.figure.projectAuto, 'Value', 1);
                end
            end
            spc.currentChannel = gui.spc.figure.proChannel;
        end
    else
        imageMod = spc.imageMod(spc.currentChannel, :,:,:);
        spc.project = reshape(sum(imageMod, 2), spc.size(2), spc.size(3));
        
        if spc.SPCdata.line_compression > 1
            aa = 1/spc.SPCdata.line_compression;
            %[xi, yi] = meshgrid(aa:aa:spc.SPCdata.scan_size_x, aa:aa:spc.SPCdata.scan_size_y);
            [yi, xi] = meshgrid(1:aa:1-aa+spc.size(2), 1:aa:1-aa+spc.size(3));
            project1 = [spc.project; spc.project(end, :)];
            project2 = [project1, project1(:, end)];
            spc.project = interp2(project2, yi, xi)*aa*aa;
        end
    end
    
    spc.switches.filter = str2double(get(gui.spc.figure.filter, 'String'));
    if spc.switches.filter > 1
       filterWindow = ones(spc.switches.filter, spc.switches.filter)/spc.switches.filter/spc.switches.filter;
        spc.project(1:end-1, 2:end) = imfilter(spc.project(1:end-1, 2:end), filterWindow, 'replicate');
    end
%end %{flag}

set(gui.spc.figure.projectImage, 'CData', spc.project);
autoLUT = get(gui.spc.figure.projectAuto, 'Value');
if autoLUT
    %set(gui.spc.figure.projectImage, 'CDataMapping', 'direct');
    uplimit = round(max(spc.project(:)));
    lowlimit = round(min(spc.project(:)));
else
    uplimit = str2double(get(gui.spc.figure.projectUpperlimit, 'String'));
    lowlimit = str2double(get(gui.spc.figure.projectLowerlimit, 'String'));
end
set(gui.spc.figure.projectUpperlimit, 'String', num2str(uplimit));
set(gui.spc.figure.projectLowerlimit, 'String', num2str(lowlimit));
try
    set(gui.spc.figure.projectAxes, 'Clim', [lowlimit, uplimit]);
end
set(gui.spc.figure.projectAuto, 'Value', 0);
siz = size(spc.project);

%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

set(gui.spc.figure.projectAxes, 'Xlim', [0.5,siz(2)+0.5], 'Ylim', [0.5,siz(1)+0.5]);
set(gui.spc.figure.lifetimeMapAxes, 'Xlim', [0.5,siz(2)+0.5], 'Ylim', [0.5,siz(1)+0.5]);
set(gui.spc.figure.scanImgA, 'Xlim', [0.5,siz(1)+0.5], 'Ylim', [0.5,siz(2)+0.5]);

%gui.spc.figure.roi = rectangle ('position', roi_pos, 'ButtonDownFcn', 'spc_dragRoi', 'EdgeColor', [1,1,1]);
if ~spc.switches.noSPC
    fitstart = round(str2double(get(gui.spc.spc_main.spc_fitstart, 'String'))*1000/spc.datainfo.psPerUnit);
    fitend = round(str2double(get(gui.spc.spc_main.spc_fitend, 'String'))*1000/spc.datainfo.psPerUnit);
    if fitstart < 1
        fitstart = 1;
    end
    if fitend <0
        fitend = spc.size(1);
    end
    if fitend < fitstart
        fitend = spc.size(1);
        fitstart = 1;
    end
    if fitend > spc.size(1)
        fitend = spc.size(1);
    end
    spc.fit(spc.currentChannel).range = [fitstart, fitend];
    range = round(spc.fit(spc.currentChannel).range.*spc.datainfo.psPerUnit/100)/10;
    set(gui.spc.spc_main.spc_fitstart, 'String', num2str(range(1)));
    set(gui.spc.spc_main.spc_fitend, 'String', num2str(range(2)));

    

    spc_drawAll;
end

%%%%%%%%%%%%%%%%%%%%%%
if spc.switches.redImg
    %if flag == 1
        if isfield(gui.spc.figure, 'channel')
            for i=1:length(gui.spc.figure.channel)
                val = get(gui.spc.figure.channel(i), 'Value');
                if val ~= (i==gui.spc.scanChannel)
                    set(gui.spc.figure.channel(i), 'Value', (i==gui.spc.scanChannel));
                    set(gui.spc.figure.redAuto, 'Value', 1);
                end
            end
        end
        
        if gui.spc.scanChannel == 1
            scanImg = spc.state.img.greenMax;
        else
            scanImg = spc.state.img.redMax;
            gui.spc.scanChannel = 2;
        end

        for i = 1:length(gui.spc.figure.channel)
              set(gui.spc.figure.channel(i), 'Value', (i==gui.spc.scanChannel));
        end
        
        if spc.switches.filter > 1
            scanImg(1:end-1, 2:end) = imfilter(scanImg(1:end-1, 2:end), filterWindow, 'replicate');
        else
            scanImg = scanImg;
        end
        set(gui.spc.figure.scanImg, 'CData', scanImg);
        set(gui.spc.figure.scanImgA, 'XTick', [], 'YTick', []);
        autoLUT = get(gui.spc.figure.redAuto, 'Value');
        if autoLUT
            %set(gui.spc.figure.projectImage, 'CDataMapping', 'direct');
            uplimit = round(max(scanImg(:)));
            lowlimit = round(min(scanImg(:)));
        else
            uplimit = str2double(get(gui.spc.figure.redUpperlimit, 'String'));
            lowlimit = str2double(get(gui.spc.figure.redLowerlimit, 'String'));
        end
        set(gui.spc.figure.redUpperlimit, 'String', num2str(uplimit));
        set(gui.spc.figure.redLowerlimit, 'String', num2str(lowlimit));
        try
            set(gui.spc.figure.scanImgA , 'Clim', [lowlimit, uplimit]);
        end
        set(gui.spc.figure.redAuto, 'Value', 0);    
    %end
end

%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%%%%%%%%%%%%%%%%Display fitting%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
if ~spc.switches.noSPC




    if recalc
        if isfield (spc, 'filename')
            set(gui.spc.spc_main.filename, 'String', spc.filename);
        end
    end
    %
    range = spc.fit(spc.currentChannel).range;
    t = [range(1):1:range(2)];
    lifetime = spc.lifetime(t);
    t = t*spc.datainfo.psPerUnit/1000;
    betahat = spc.fit(spc.currentChannel).beta0;
    try
        spc_drawfit (t,  spc.fit(spc.currentChannel).curve, lifetime, spc.currentChannel);   
    end
    spc_dispbeta;
end



