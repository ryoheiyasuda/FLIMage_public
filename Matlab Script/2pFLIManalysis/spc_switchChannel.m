function spc_switchChannel
global spc gui

if ~spc.switches.noSPC
    if (gui.spc.proChannel > spc.nChannels)
        gui.spc.proChannel = spc.nChannels;
    end
end

set(gui.spc.figure.lifetimeUpperlimit, 'String', num2str(spc.fit(gui.spc.proChannel).lifetime_limit(1)));
set(gui.spc.figure.lifetimeLowerlimit, 'String', num2str(spc.fit(gui.spc.proChannel).lifetime_limit(2)));
set(gui.spc.figure.LutUpperlimit, 'String', num2str(spc.fit(gui.spc.proChannel).lutlim(2)));
set(gui.spc.figure.LutLowerlimit, 'String', num2str(spc.fit(gui.spc.proChannel).lutlim(1)));

spc.currentChannel = gui.spc.proChannel;

spc_redrawSetting (1);
