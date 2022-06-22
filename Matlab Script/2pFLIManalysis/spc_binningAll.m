function spc_binningAll
global spcs;
global spc;
global gui;

    if (spc.switches.imagemode == 1)
		for i = (1:spcs.nImages)
            eval(['spcs.spc', num2str(i), ' = spc_calcBinning(spcs.spc', num2str(i), ');']);  
            %eval(['spcs.spc', num2str(i), ' = spc_calcLifetimeMap(spcs.spc', num2str(i), ');']);
            %eval(['spcs.spc', num2str(i), '.rgbLifetime = spc_makeRGBLifetimeMap(spcs.spc', num2str(i), ');']);
		end
		spc = spc_calcBinning(spc);
		
		spc_roi = get(gui.spc.figure.roi, 'Position');
		set(gui.spc.figure.roi, 'Position', round(spc_roi/2));
		
		spc_drawLifetimeMap_All(1);
		spc_drawSetting(1);
    end
    