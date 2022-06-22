function spc_selectAll;
global spc
global gui

spc_roi = [1, 1, spc.size(3), spc.size(2)];
set(gui.spc.figure.roi, 'Position', spc_roi);
set(gui.spc.figure.mapRoi, 'Position', spc_roi);
spc.roipoly = ones(spc.size(2), spc.size(3));
spc_redrawSetting(0);