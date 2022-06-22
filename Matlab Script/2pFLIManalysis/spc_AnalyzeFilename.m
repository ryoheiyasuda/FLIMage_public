function [filepath, basename, filenumber, max1, spc1, ext] = spc_AnalyzeFilename(filename)

[filepath, filename1, ext] = fileparts(filename);

if isempty(filepath)
    filepath = [pwd, filesep];
else
    filepath = [filepath, filesep];
end

filename = [filename1, ext];

if strcmp(filename1(end-3:end), '_max')
    max1 = 1;
    fn = filename1(1:end-4);
elseif strcmp(filename1(end-2:end), 'max')
    max1 = 2;
    fn = filename1(1:end-3);
else 
    max1 = 0;
    fn = filename1;
end

filenumber = str2double(fn(end-2:end));
basename = fn(1:end-3);

if isempty(filenumber)
    basename = fn;
end

if isempty(strfind(filepath, 'spc'))
    spc1 = 0;
else
    spc1 = 1;
end