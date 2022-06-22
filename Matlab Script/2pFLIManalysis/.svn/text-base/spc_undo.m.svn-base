function spc_undo
global gui;
global spc;

if (spc.switches.imagemode == 1)
    %save settings
    spc_roi = get(gui.spc.figure.roi, 'Position');
    saveSize = spc.size;
    %%%%%%%%%%%%%%%%%%
    %spc.imageMod = spc.image;
    spc_openCurves(spc.filename);
    %try; spc.size = spc.sizeOrg; catch; end
    %image = reshape(full(spc.imageMod), spc.size(1), spc.size(2), spc.size(3));
    %spc.project = reshape(sum(spc.imageMod, 1), spc.size(2), spc.size(3));
    spc_selectall;
end

spc_redrawSetting(1);