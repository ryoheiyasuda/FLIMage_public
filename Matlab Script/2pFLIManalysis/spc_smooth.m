function spc_smooth(n);
global spc;
global gui;

if ~nargin
    n = 3;
end
if (spc.switches.imagemode == 0)
    return;
end;
spc = spc_calcSmooth(spc, n);

spc_redrawSetting(1);