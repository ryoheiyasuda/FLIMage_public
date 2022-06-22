function spc_makeMovieFromFrames
global spc gui
%global tmp


DialogTitle = 'Save Movie as ...';
[path1, file1] = fileparts(spc.filename);
DefaultName = [path1, filesep, file1, '_movie.tif'];
FilterSpec = '*.tif';

[FileName,PathName,FilterIndex] = uiputfile(FilterSpec,DialogTitle,DefaultName);
fname = [PathName, filesep, FileName];
    
spc.page = 1;
set(gui.spc.spc_main.spc_page, 'String', num2str(spc.page));
spc_redrawSetting(1);
saveProject = spc.project;

imwrite(spc.rgbLifetime, fname, 'compression', 'none', 'writemode', 'overwrite');

for i=2:spc.stack.nStack
    set(gui.spc.spc_main.spc_page, 'String', num2str(i));
    spc_redrawSetting(1);
    imwrite(spc.rgbLifetime, fname, 'compression', 'none', 'writemode', 'append');
    pause(0.02);
    
end


end

