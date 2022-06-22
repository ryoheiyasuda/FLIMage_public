function spc_averageMultipleImages
global spc gui

% prompt1 = {'number of images'};
% def1 = {'1'};
% dlg_title1 = 'Average';
% num_lines1 = 1;
% answer1 = inputdlg(prompt1,dlg_title1,num_lines1,def1);
% nImages = str2num(answer1{1});

nImages = 8;
for i=1:nImages
    prompt{i} = ['Enter files to average', num2str(i), ':'];
    if i == 1
        def{i} = [num2str(1+5*(i-1)), ':', num2str(5*i)];
    else
        def{i} = '';
    end
end
dlg_title = 'Average';
num_lines = 1;

answer = inputdlg(prompt,dlg_title,num_lines,def);

num = 0;
for i = 1:nImages
    if ~isempty(answer{i}) 
        num = num+1;
        imageN{num} = str2num(answer{i});
    end
end

[filePath, baseName, filenumber, max1, spc1] = spc_AnalyzeFilename(spc.filename);

for j = 1:num
    for i=1:length(imageN{j})
        spc_openCurves(imageN{j}(i));
        if i==1
            sumA3 = spc.imageMod;
            if spc.state.img.greenImg
                sumG3 = spc.state.img.greenMax;
            end
            if spc.state.img.redImg
                sumR3 = spc.state.img.redMax;
            end
            saveImage = spc.project;
        else
            C = xcorr2(saveImage, spc.project);
            [value1, index1] = max(C(:));
            [siz_x, siz_y] = size(C);
            index_x = floor(index1 / siz_y) + 1;
            index_y = index1 - (index_x-1) * siz_y;
            shift_x = index_x - (siz_x + 1)/2;
            shift_y = index_y - (siz_y + 1)/2;
            spc_shiftSPC([shift_y, shift_x]);
            sumA3 = sumA3 + spc.imageMod;
            if spc.state.img.greenImg
                sumG3 = sumG3 + spc.state.img.greenMax;
            end
            if spc.state.img.redImg
                sumR3 = sumR3 + spc.state.img.redMax;
            end
        end
    end


    spc.imageMod = sumA3;
    if spc.state.img.greenImg
        spc.state.img.greenMax = sumG3;
    end
    if spc.state.img.redImg
        spc.state.img.redMax = sumR3;
    end
    set(gui.spc.spc_main.spc_page, 'String', '1');
    spc.stack.image1{1}=spc.imageMod/length(imageN);
    if spc.state.img.greenImg
        spc.state.img.redImg = spc.state.img.redMax/length(imageN);
    end
    if spc.state.img.redImg
        spc.state.img.greenImg = spc.state.img.greenMax/length(imageN);
    end
    spc_redrawSetting(1);

    rgbLifetime{j} = spc.rgbLifetime;
    
    numstr = ['_', num2str(imageN{j}(1)), '-', num2str(imageN{j}(end))];
    
    imwrite(spc.rgbLifetime, [filePath, baseName, '_lifetime_average', numstr, '.tif'], 'compression', 'none');
    if spc.state.img.greenImg
        imwrite(uint16(spc.project), [filePath, baseName, '_greenImage_average', numstr, '.tif'], 'compression', 'none');
    end
    if spc.state.img.redImg
        imwrite(uint16(sumR3), [filePath, baseName, '_redImage_average', numstr, '.tif'], 'compression', 'none');
    end
end

figure;
for j = 1:num
    subplot(1, num, j);
    imagesc(rgbLifetime{j});
    set(gca,'XTickLabel', [], 'YTickLabel', []);
end
spc_colorbar;