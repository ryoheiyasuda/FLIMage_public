function [Aout, header] = im2_opentif(filename)

global spc gui

largeFile = 100;

fileInfo = imfinfo(filename);
frames = length(fileInfo);
imWidth = fileInfo(1).Width;
imHeight = fileInfo(1).Height;
state.imageProc.colorMap = 0;
header = fileInfo(1).ImageDescription;
Aout = ones(imHeight, imWidth, frames);

if frames > largeFile
    gui.spc.waitbar = waitbar(0,'Opening Tif image...', 'Name', 'Open TIF Image', 'Pointer', 'watch');
end

for i = 1:frames
        Aout(:,:,i) = imread(filename, i);
        if frames > largeFile
            waitbar(i/frames, gui.spc.waitbar);
        end
end

if frames > largeFile
    close(gui.spc.waitbar);
end
% delim = strfind(header, ';');
% evalc(header(1:delim(1)));
% for j=1:length(delim)-1
%     try
%         evalc(header(delim(j)+1:delim(j+1)));
%     catch
%         header(delim(j)+1:delim(j+1));
%     end
% end
evalc(header);

if ~isfield (state.acq, 'numAvgFramesSave')
    state.acq.numAvgFramesSave = state.acq.numberOfFrames;
end

nStack = state.acq.numberOfZSlices; %frames / nF / state.acq.numberOfChannelsSave;
if nStack == frames*state.acq.numberOfChannelsSave
        nF = 1; %nF: number of frames per channels that is needed for averaging off line.
elseif nStack <= frames*state.acq.numberOfChannelsSave
    nF = ceil(frames / nStack / state.acq.numberOfChannelsSave);
else
    nF = 1;
end

if nStack < 1
    nStack = 1;
    nF = 1;
end

if nF < 1
    nF = 1;
end

state.img.greenImg = zeros(size(Aout, 1), size(Aout, 2),  nStack);
state.img.redImg = zeros(size(Aout, 1), size(Aout, 2),  nStack);

if ~spc.switches.noSPC
    pixelshift = 0;
else
    pixelshift = 0;
end
spc.switches.pixelshift = pixelshift;
if state.acq.numberOfChannelsSave == 2
    state.img.greenImgF = Aout(:,:,1:2:frames-1);
    state.img.greenImgF(:, pixelshift+1:end, :) = state.img.greenImgF(:, 1:end-pixelshift, :);
    state.img.redImgF = Aout (:,:,2:2:frames);
    state.img.redImgF(:, pixelshift+1:end, :) = state.img.redImgF(:, 1:end-pixelshift, :);
    
    if nF > 1
        for j = 1:nStack
            state.img.greenImg(:, :, j) = mean(state.img.greenImgF(:, :, (j-1)*nF+1:j*nF), 3);
            state.img.redImg(:, :, j) = mean(state.img.redImgF(:, :, (j-1)*nF+1:j*nF), 3);
        end
    else
        state.img.greenImg = state.img.greenImgF;
        state.img.redImg = state.img.redImgF;
    end
    
    state.img.greenMax = max(state.img.greenImg, [], 3);
    state.img.redMax = max(state.img.redImg, [], 3);
    
%     state.img.greenMax(:, pixelshift :end) = state.img.greenMax(:, 1:end-pixelshift+1);
%     %state.img.greenMax = medfilt2(state.img.greenMax, [3,3]);
%     state.img.redMax (:, pixelshift :end) = state.img.redMax(:, 1:end-pixelshift+1);  %Shift by !
%     %state.img.redMax = medfilt2(state.img.redMax, [3,3]);  %Median for red fluorescence;

else
    state.img.greenImgF = Aout;
    state.img.greenImgF(:, pixelshift+1:end, :) = state.img.greenImgF(:, 1:end-pixelshift, :);
    state.img.redImgF = Aout;
    state.img.redImgF(:, pixelshift+1:end, :) = state.img.redImgF(:, 1:end-pixelshift, :);
    
    if nF > 1
        for j = 1:nStack
            state.img.greenImg(:, :, 1) = mean(state.img.greenImgF(:, :, (j-1)*nF+1:j*nF), 3);
            state.img.redImg(:, :, 1) = mean(state.img.redImgF(:, :, (j-1)*nF+1:j*nF), 3);
        end
    else
        state.img.greenImg = state.img.greenImgF;
        state.img.redImg = state.img.redImgF;
    end
    
    state.img.greenMax = max(state.img.greenImg, [], 3);
    state.img.redMax = max(state.img.redImg, [], 3);
    %state.img.redMax = medfilt2(state.img.redMax, [3,3]);  %Median for red fluorescence;
end



if spc.switches.noSPC
    state.img.greenImg = state.img.greenImgF;
    state.img.redImg = state.img.redImgF;
    spc.stack.project = state.img.greenImg;
    
    spc.stack.nStack = size(state.img.greenImg, 3);
    set(gui.spc.spc_main.slider1, 'max', spc.stack.nStack);
    set(gui.spc.spc_main.slider1, 'min', 1);
    set(gui.spc.spc_main.minSlider, 'max', spc.stack.nStack);
    set(gui.spc.spc_main.minSlider, 'min', 1);
    if spc.stack.nStack > 1
        set(gui.spc.spc_main.slider1, 'sliderstep', [1/(spc.stack.nStack-1), 1/(spc.stack.nStack-1)]);
        set(gui.spc.spc_main.minSlider, 'sliderstep', [1/(spc.stack.nStack-1), 1/(spc.stack.nStack-1)]);
        set(gui.spc.spc_main.slider1, 'value', spc.stack.nStack);
        set(gui.spc.spc_main.minSlider, 'value', 1);
    else
        set(gui.spc.spc_main.slider1, 'sliderstep', [1,1]);
        set(gui.spc.spc_main.minSlider, 'sliderstep', [1,1]);
        set(gui.spc.spc_main.slider1, 'value', 1);
        set(gui.spc.spc_main.minSlider, 'value', 1);
    end
    set(gui.spc.spc_main.spc_page, 'String', num2str([1:spc.stack.nStack]));
    spc.page = [1:spc.stack.nStack];
end

state.header = header;
spc.state = state;


