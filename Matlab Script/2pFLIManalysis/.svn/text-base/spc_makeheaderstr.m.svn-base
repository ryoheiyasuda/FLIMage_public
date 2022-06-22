function headerstr = spc_makeheaderstr
global spc
global state

headerstr = '';
size = spc.size;
sizestr = ['spc.size=', mat2str(spc.size), ';', 13];

spc.scanHeader = parseHeader(state.headerString);
spc.scanHeader.power = state.init.eom.maxPower(state.init.eom.scanLaserBeam);
spc.scanHeader.motor.position = state.motor.position;
headerstr = [headerstr, sizestr];

str1 = mkstr('spc.datainfo');
str2 = mkstr('spc.fit');
str3 = mkstr('spc.switches');
str4 = mkstr('spc.SPCdata');
str5 = mkstr('spc.scanHeader');

headerstr = [headerstr, str1, str2, str3, str4, str5];

%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

function headerstr = mkstr (arraystr);
global spc;
headerstr = '';

evalc(['fn = fieldnames(', arraystr, ')']);

for i=1:length(fn)
    a = cell2struct(fn(i), 'a', 1);
    fieldstr = [arraystr, '.', a.a];
    eval (['ntr = isnumeric(', fieldstr, ');']);
    eval (['ctr = ischar(', fieldstr, ');']);
    eval (['sttr = isstruct(', fieldstr, ');']);      
    if ntr
        valstr = mat2str(eval (fieldstr));
        exestr = [fieldstr, ' = ', valstr, ';', 13];
    elseif ctr
        strA = eval(fieldstr);
        strA(strfind(strA, '''')) = [];        
        valstr = strcat('''', strA, '''');
        exestr = [fieldstr, ' = ', valstr, ';', 13];
    elseif sttr
        exestr = mkstr(fieldstr); %decode_struct (fieldstr);
    else
        exestr = '';    
    end
    eval(headerstr);
    headerstr = [headerstr, exestr];        
end
