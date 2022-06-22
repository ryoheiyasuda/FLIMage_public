function spc_openAll

[fname,pname] = uigetfile('*.sdt','Select the sdt-file');
files = dir([pname, '*.sdt']);
nfiles = size(files);

for i = 1:nfiles(1)
    fname = files(i).name;
    if exist([pname, fname]) == 2
        spc_opencurves([pname,fname]);
    end
end