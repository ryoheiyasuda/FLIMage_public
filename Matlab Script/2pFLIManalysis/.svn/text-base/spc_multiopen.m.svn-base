function spc_multiopen (header, startN, endN);
global spc
global spcs
%Usage spc_maximum('header name', start, end)


[fname,pname] = uigetfile('*.sdt','Select sdt path');


for (i = startN:1:endN)    
    stri = num2str(i);
    while (length(stri) < 3)
         stri = ['0', stri];
    end
    fname = [header, stri, '.sdt'];
    disp(fname);

    if exist([pname, fname]) == 2
        spc_readdata([pname,fname]);
    end

    spc_drawSetting(1);
    eval('spcs.nImages = spcs.nImages + 1;', 'spcs.nImages = 1;');
    eval(['spcs.spc', num2str(spcs.nImages), '=spc;'], ''); 
    spcs.current = spcs.nImages;
end

spc_drawSetting(1);

