function spc_opennext

global spc

try
    [filepath, basename, filenumber, max] = spc_AnalyzeFilename(spc.filename);
catch
    error('Current filename not specified');
end

next_filenumber_str = '000';
next_filenumber_str ((end+1-length(num2str(filenumber+1))):end) = num2str(filenumber+1);

% if max == 0
    next_filename = [filepath, basename, next_filenumber_str, '.tif'];
% else
%     next_filename = [filepath, basename, next_filenumber_str, '_max.tif'];
% end

if exist(next_filename)
    spc_openCurves (next_filename);
else
    disp([next_filename, ' not exist!']);
end

spc_updateMainStrings;
%update the ROIs, too. 
%spc_recoverRois;