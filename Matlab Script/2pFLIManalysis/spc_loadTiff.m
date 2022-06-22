function error = spc_loadTiff (fname)
global spc;
global gui;


error = 0;
finfo = imfinfo (fname);
header = finfo(1).ImageDescription;
pages = 1:length(finfo);
spc.stack.image1 = {};
stack_project = [];

FLIMimageData = false;
if contains(header, 'FLIMimage')
    header = spc_processHeaderForFLIMimage(header);
    FLIMimageData = true;
end

if contains(header, 'spc')
    lines = splitlines(header);
    spc.format = 'None';
    for i = 1:length(lines)
        try
            if startsWith(lines{i}, 'Acquired_Time') || startsWith(lines{i}, 'Format')
                line1 = strrep(lines{i}, ' ', '');
                line1 = strrep(line1, ';', '');
                tmp = split(line1, '=');
                if startsWith(tmp{1}, 'Acquired_Time')
                    spc.acquired_time = tmp{2};
                elseif (startsWith(tmp{1}, 'Format'))
                    spc.format = tmp{2};
                end
            else
                evalc(lines{i});
            end
        catch
            disp(lines{i});
        end
    end
    %evalc(header);
    
    if FLIMimageData
        %spc = state.spc;
        spc.switches.noSPC = 0;
        spc.datainfo.scan_x = state.acq.pixelsPerLine;
        spc.datainfo.scan_y = state.acq.linesPerFrame; %*state.acq.nChannels;
        spc.SPCdata.scan_rout_x = state.acq.nChannels;
        spc.SPCdata.scan_rout_y = 1;
        spc.datainfo.scan_rx = state.acq.nChannels;
        spc.datainfo.scan_ry = 1;
        spc.size = [state.spc.SPCdata.n_dataPoint, spc.datainfo.scan_y, spc.datainfo.scan_x];
        spc.switches.maxAve = 0;
        spc.datainfo.psPerUnit = state.spc.SPCdata.resolution;
        spc.switches.imagemode = 2;
        spc.SPCdata.line_compression = 1;
        spc.switches.logscale = 1;
        spc.datainfo.psPerUnit = spc.datainfo.psPerUnit(1);
        try
            spc.datainfo.pulseInt = 1e9/state.spc.datainfo.syncRate;
        catch
            spc.datainfo.pulseInt = 1e9/80249160;
        end
        for i = 1:state.acq.nChannels
            evalc(['spc.fit(', num2str(i), ').beta0 = state.spc.analysis.fit_param', num2str(i)]);
            spc.fit(i).t_offset = state.spc.analysis.offset(i);
            spc.fit(i).fittype = 'single';
        end
    else
        spc.nChannels = spc.SPCdata.scan_rout_x;
        spc.switches.noSPC = 0;
        spc.datainfo.scan_y = spc.SPCdata.scan_size_y; %*spc.SPCdata.scan_rout_x;
        spc.datainfo.scan_rx = spc.SPCdata.scan_rout_x;
        try
            spc.datainfo.scan_ry = spc.SPCdata.scan_rout_y;
        catch
            spc.datainfo.scan_ry = 1;
        end
        spc.size = [spc.size(1), spc.datainfo.scan_y, spc.datainfo.scan_x];
    end
    
    for i=1:length(pages)
        image1 = double(imread(fname, pages(i)));
        
        if (FLIMimageData)
            spc.state = state;
            spc.nChannels = state.acq.nChannels;
            if strcmp(spc.format, 'None')
                 if finfo(1).Width == spc.size(1)
                     spc.format = 'ChYX_Time';
                 elseif finfo(1).Width == spc.size(2) * spc.size(3) * spc.nChannels
                     spc.format = 'Time_ChYX';
                 elseif (finfo(1).Width == prodc(spc.size) * spc.nChannels)
                     spc.format = 'Linear';
                 end
            end
          
            image2 = zeros([spc.nChannels, spc.size(1), spc.size(2), spc.size(3)]);
            if strcmp(spc.format, 'Linear')
               offset = 1;
              for ch = 1:spc.nChannels
                  offset2 = offset + prod(spc.size) - 1;
                  image2(ch,:,:,:) = reshape(image1(offset:offset2), spc.size);
                  offset = offset2 + 1;
              end
            elseif strcmp(spc.format, 'ChYX_Time') %Very original format.
                image1 = reshape(image1, [spc.size(1), spc.size(2) * spc.nChannels, spc.size(3)]);
                for ch = 1:spc.nChannels
                    image2(ch,:,:,:) = image1(:, (ch-1)*spc.size(2)+1:ch*spc.size(2), :);
                end
            elseif strcmp(spc.format, 'ChTime_YX')
                image1 = permute(image1, [2,1]);
                image1 = reshape(image1, [spc.size(1) * spc.nChannels, spc.size(2), spc.size(3)]);
                for ch = 1:spc.nChannels
                    image2(ch,:,:,:) = image1((ch-1)*spc.size(1)+1:ch*spc.size(1), :, :);
                end
            elseif strcmp(spc.format,   'Time_ChYX')
                image1 = permute(image1, [2,1]); %Now X=time, Y=ChYX
                image1 = reshape(image1, [spc.size(1), spc.size(2) * spc.nChannels, spc.size(3)]);
                for ch = 1:spc.nChannels
                    image2(ch,:,:,:) = image1(:, (ch-1)*spc.size(2)+1:ch*spc.size(2), :);
                end
            else %Version 1.XXX.
                if finfo(1).Width == spc.size(1) %Time_ChYX....
                    image1 = reshape(image1, [spc.size(1), spc.size(2) * spc.nChannels, spc.size(3)]);
                    for ch = 1:spc.nChannels
                        image2(ch,:,:,:) = image1(:, (ch-1)*spc.size(2)+1:ch*spc.size(2), :);
                    end
                else %???
                    image1 = reshape(image1, [spc.size(2), spc.size(3), spc.size(1)]); %%ChY, X, T.
                    image2 = permute(image1, [3, 1, 2]);
                end
            end
            
            spc.imageMod = image1;           
            
        else
            image1 = reshape(image1, [spc.size(1), spc.size(2) * spc.nChannels, spc.size(3)]);
            image2 = zeros([spc.nChannels, spc.size(1), spc.size(2), spc.size(3)]);
            for ch = 1:spc.nChannels
                image2(ch,:,:,:) = image1(:, (ch-1)*spc.size(2)+1:ch*spc.size(2), :);
            end
        end
        spc.stack.image1{i} = uint16(image2);
        
        stack_project = [stack_project, reshape(sum(image2, 2), [spc.nChannels, spc.size(2), spc.size(3)])];
    end %pages
    
    spc.stack.project = reshape(stack_project, [spc.nChannels, spc.size(2), spc.size(3), length(pages)]);
    spc.stack.nStack = length(pages);
    if spc.stack.nStack > 1
        set(gui.spc.spc_main.slider1, 'enable', 'on');
        set(gui.spc.spc_main.minSlider, 'enable', 'on');
        set(gui.spc.spc_main.slider1, 'min', 1, 'max', spc.stack.nStack);
        set(gui.spc.spc_main.minSlider, 'min', 1, 'max', spc.stack.nStack);
        set(gui.spc.spc_main.slider1, 'sliderstep', [1/(spc.stack.nStack-1), 1/(spc.stack.nStack-1)]);
        set(gui.spc.spc_main.minSlider, 'sliderstep', [1/(spc.stack.nStack-1), 1/(spc.stack.nStack-1)]);
        set(gui.spc.spc_main.slider1, 'value', spc.stack.nStack);
        set(gui.spc.spc_main.minSlider, 'value', 1);
    else
        set(gui.spc.spc_main.slider1, 'enable', 'off');
        set(gui.spc.spc_main.minSlider, 'enable', 'off');
        set(gui.spc.spc_main.slider1, 'min', 0, 'max', 1);
        set(gui.spc.spc_main.minSlider, 'min', 0, 'max', 1);
        set(gui.spc.spc_main.slider1, 'sliderstep', [1,1]);
        set(gui.spc.spc_main.minSlider, 'sliderstep', [1,1]);
    end
    set(gui.spc.spc_main.spc_page, 'String', num2str(1:spc.stack.nStack));
    
    spc.currentChannel = 1;
    
    spc.switches.redImg = 0;
    
else
    disp('This is not a spc file !!');
    error = 2;
    return;
end
spc.filename = fname;
%spc.size = [spc.size(1), spc.datainfo.scan_y, spc.datainfo.scan_x];

spc.page = 1:spc.stack.nStack;


