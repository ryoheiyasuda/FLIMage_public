function spc_selectAll
global spc
global gui


Xlim1 = get(gui.spc.figure.projectAxes, 'Xlim');
Ylim1 = get(gui.spc.figure.projectAxes, 'Ylim');
siz2 = Ylim1(2) - Ylim1(1);
siz3 = Xlim1(2) - Xlim1(1);

spc_roi = [1, 1, siz3, siz2];
set(gui.spc.figure.roi, 'Position', spc_roi);
set(gui.spc.figure.mapRoi, 'Position', spc_roi);
%spc.roipoly = ones(spc.size(2), spc.size(3));
spc_redrawSetting(1);