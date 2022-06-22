function spc_changeCurrent(spcN)
global spc;
global spcs;

if spcN < 0 | spcN >  spcs.nImages
    spcN = spcs.current;
else
    spc_spcsUpdate;
    spcs.current = spcN;
    eval(['spc = spcs.spc', num2str(spcN), ';'], 'spcN = spcs.current;');
    spc_drawSetting(1);
end


spcs.current = spcN;

    
disp('Current file is');
eval(['str = spcs.spc', num2str(spcs.current), '.filename;']);
disp([num2str(spcN), '.  ', str]);
