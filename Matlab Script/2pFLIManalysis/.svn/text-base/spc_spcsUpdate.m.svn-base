function spc_spcsUpdate
global spcs;
global spc;

 if (spc.switches.imagemode ~= 1)
     return;
 end
eval(['spcs.spc', num2str(spcs.current), '.switches = spc.switches;'], ['spcN = spcs.current;']);
eval(['spcs.spc', num2str(spcs.current), '.fit = spc.fit;'], ['spcN = spcs.current;']);
eval(['spcs.spc', num2str(spcs.current), '.rgbLifetime = spc.rgbLifetime;'], ['spcN = spcs.current;']);
eval(['spcs.spc', num2str(spcs.current), '.imageMod = spc.imageMod;'], ['spcN = spcs.current;']);