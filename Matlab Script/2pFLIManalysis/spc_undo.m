function spc_undo
global spc;

if (spc.switches.imagemode == 1)
    spc.roipoly = ones(spc.size(2:3));
    spc_openCurves(spc.filename);
end

spc_redrawSetting(1);