%function readFisiologyData(fileName)
fileName = 'C:\Users\yasudar\Documents\Data\AnanT\10292019\New shortcut001.txt';

[pn, fn, ext] = fileparts(fileName);
currentFileN = str2double(fn(end-2:end));

fid = fopen(fileName, 'r');
y = 0;
tline = fgetl(fid);
mode = 'None';
ch = 1;
while ischar(tline)
    if contains(tline, ':')
        mode = tline(1:end-1);
    elseif contains(tline, ',') && strcmp(mode, 'Data')
        evalc(['data{ch} = [', tline, ']']);
        ch = ch+1;
    elseif contains(tline, 'PulseName')
        val = strsplit(tline, '=');
        PulseName = val{2};
    elseif contains(tline, 'PulseSet=')
        val = strsplit(tline, '=');
        PulseSet = val{2};
    elseif  contains(tline, '=')
        try
            val = strsplit(tline, '=');
            if strcmp(mode, 'MC700Parameters')
                evalc(['MC700Parameters.', val{1}, '= [' lower(val{2}), ']']);
            elseif strcmp(mode, 'StimParameters')
                evalc(['StimParameters.', PulseSet, '.', val{1}, '=' lower(val{2})]);
            end
        end
    end
    
    tline = fgetl(fid);
    y = y + 1;
end

disp(y);