function header = spc_processHeaderForFLIMimage(header)
header = strrep(header, 'FLIMimage parameters', '');
header = strrep(header, 'True', 'true');
header = strrep(header, 'False', 'false');
header = strrep(header, '.Spc.', '.spc.');
header = strrep(header, '.Acq.', '.acq.');
header = strrep(header, 'State.', 'state.');
header = strrep(header, 'spcData.', 'SPCdata.');

