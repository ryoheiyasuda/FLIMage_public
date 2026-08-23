% Build PIPE/pipe_Client first, then load the DLL from this script's folder.
NET.addAssembly(fullfile(fileparts(mfilename('fullpath')), 'pipe_Client.dll'));
flim = FLIM_Com();

