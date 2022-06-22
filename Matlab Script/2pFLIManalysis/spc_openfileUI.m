function spc_openfileUI;

[fname,pname] = uigetfile('*.sdt','Select the sdt-file');
spc_openfile([fname,pname]);