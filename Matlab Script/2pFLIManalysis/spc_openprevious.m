function spc_openprevious

global spc;

try
    [filepath, basename, filenumber, max] = spc_AnalyzeFilename(spc.filename);
catch
    error('Current filename not specified');
end

previous_filenumber_str = '000';
previous_filenumber_str ((end+1-length(num2str(filenumber-1))):end) = num2str(filenumber-1);

if max == 0
    previous_filename = [filepath, basename, previous_filenumber_str, '.tif'];
else
    previous_filename = [filepath, basename, previous_filenumber_str, '_max.tif'];
end

if exist(previous_filename)
    spc_openCurves (previous_filename);
else
    disp([previous_filename, ' not exist!']);
end

spc_updateMainStrings;
%update the ROIs, too. 
%spc_recoverRois;