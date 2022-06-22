function spc_maxProc_offLine
global spc;
global gui;

page = str2double(get(gui.spc.spc_main.spc_page, 'String'));
if isempty(page)
    page = 1;
end
spc.page = page;

%Calculate just current Channel.
stack_project = [];
for i = 1:length(spc.stack.image1)
    stack_project = [stack_project, reshape(sum(spc.stack.image1{i}, 2), [spc.nChannels, spc.size(2), spc.size(3)])];
end
spc.stack.project = reshape(stack_project, [spc.nChannels, spc.size(2), spc.size(3), length(spc.stack.image1)]);

if ~spc.switches.noSPC
    if all(page <= size(spc.stack.project, 4))
        [maxP, index_max] = max(spc.stack.project(:,:,:,page), [], 4);
    else
        page = 1:size(spc.stack.project, 4);
        spc.page = page;
        [maxP, index_max] = max(spc.stack.project(:,:,:,1:end), [], 3);
        set(gui.spc.spc_main.n_of_pages, 'string', num2str(spc.stack.nStack));
        set(gui.spc.spc_main.spc_page, 'String', num2str(page));
    end
    
    siz = size(spc.stack.image1{1});
    stack_max = zeros(siz);
    stack_max = stack_max(:);

    for i=1:length(page)
        %index = (index_max == page(i));
        if ~spc.switches.maxAve
            index = (index_max == i);
            siz_index = size(index);
            index = repmat (index(:), [1,siz(2)]); %%siz1 = channel, siz2 = time.
            index = reshape(index, [siz_index(1), siz_index(2), siz_index(3), siz(2)]);
            index = permute(index, [1,4,2,3]);
            index = index(:);
            stack_max = stack_max + index .* double(spc.stack.image1{page(i)}(:));
        else
            stack_max = stack_max + double(spc.stack.image1{page(i)}(:));
        end
    end

    image1 = reshape(stack_max, siz);
    if spc.switches.maxAve
        image1 = image1 / length(page);
    end

    spc.imageMod = reshape(image1, siz);
    imageMod2 = reshape(spc.imageMod(spc.currentChannel, :,:,:), spc.size);
    spc.project = reshape(sum(imageMod2, 1), siz(3), siz(4));
end
%spc_redrawSetting;

try
    if ~spc.switches.maxAve
        if spc.switches.redImg
            spc.state.img.greenMax = max(spc.state.img.greenImg(:,:,page), [], 3);
            spc.state.img.redMax = max(spc.state.img.redImg(:,:,page), [], 3);
        end
    else
        if spc.switches.redImg
            spc.state.img.greenMax = mean(spc.state.img.greenImg(:,:,page), 3);
            spc.state.img.redMax = mean(spc.state.img.redImg(:,:,page), 3);
        end        
        
    end
end