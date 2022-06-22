function spc_smoothAll
global spcs;
global spc;

if (spc.switches.imagemode == 1)
	for i = (1:spcs.nImages)
        eval(['spcs.spc', num2str(i), ' = spc_calcSmooth(spcs.spc', num2str(i), ', 3);']);  
	end
	spc = spc_calcSmooth(spc, 3);
	spc_drawLifetimeMap_All(1);
	spc_drawLifetimeMap(1);
end
