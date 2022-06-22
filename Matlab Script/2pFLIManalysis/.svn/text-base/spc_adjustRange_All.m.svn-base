function spc_adjustRange_All
global gui;
global spc;
global spcs;

handles = gui.spc.lifetimerange;

up = str2num(get(handles.upper, 'String'));
lw = str2num(get(handles.lower, 'String'));
lut(2) = str2num(get(handles.spc_thresh, 'String'));
lut(1) = str2num(get(handles.spc_lowthresh, 'String'));
range = [lw, up];
spc.switches.lifetime_limit=range;
spc.switches.lutlim = lut;

for i = (1:spcs.nImages)
    eval(['spcs.spc', num2str(i), '.switches.lifetime_limit (1) = range(1);']);
    eval(['spcs.spc', num2str(i), '.switches.lifetime_limit (2) = range(2);']);
    eval(['spcs.spc', num2str(i), '.switches.lutlim= lut;']);
    eval(['spcs.spc', num2str(i), '.rgbLifetime = spc_makeRGBLifetimeMap(spcs.spc', num2str(i), ');']);
end
spc_drawLifetimeMap_All(0);