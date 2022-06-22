function spc_binning;
global spc;
global gui;

if (spc.switches.imagemode == 0)
    return;
end;
spc.sizeOrg = spc.size;
spc = spc_calcBinning(spc);

%spc_roi = get(gui.spc.figure.roi, 'Position');
%set(gui.spc.figure.roi, 'Position', ceil(spc_roi/2));

spc_selectall;
spc_redrawSetting(1);
spc_spcsUpdate;