function spc_fileList

global spcs


for i=(1:spcs.nImages)
    if i == spcs.current
        eval(['str = spcs.spc', num2str(i), '.filename;']);
        disp(['+', num2str(i), '.  ', str]);
    else
        eval(['str = spcs.spc', num2str(i), '.filename;']);
        disp([' ', num2str(i), '.  ', str]);
    end
end
