function spc_undoAll
global spcs;
global spc;
global gui;

if (spc.switches.imagemode == 1)
    for i = (1:spcs.nImages)
        eval(['spc_opencurves(spcs.spc', num2str(i), '.filename)']);
        eval(['spcs.spc', num2str(i), '.size=spc.size;'], ''); 
		eval(['spcs.spc', num2str(i), '.datainfo=spc.datainfo;'], ''); 
		eval(['spcs.spc', num2str(i), '.switches=spc.switches;'], '');
		eval(['spcs.spc', num2str(i), '.filename=spc.filename;'], '');
		eval(['spcs.spc', num2str(i), '.project=spc.project;'], ''); 
		eval(['spcs.spc', num2str(i), '.lifetime=spc.lifetime;'], '');
		eval(['spcs.spc', num2str(i), '.lifetimeMap=spc.lifetimeMap;'], ''); 
		eval(['spcs.spc', num2str(i), '.rgbLifetime=spc.rgbLifetime;'], ''); 
		evalc(['spcs.spc', num2str(i), '.imageMod = sparse(reshape(full(spc.image), spc.size(1), spc.size(2)*spc.size(3)))']);
    end

    %save settings
    spc_roi = get(gui.spc.figure.roi, 'Position');
    saveSize = spc.size;
    %%%%%%%%%%%%%%%%%%

    spc_drawLifetimeMap_All(1);
    spc_drawSetting(1);
end
