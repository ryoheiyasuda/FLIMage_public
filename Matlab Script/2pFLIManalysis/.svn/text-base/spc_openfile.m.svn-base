function spc = spc_openfile(spc_filename);
global spc
spc = [];
spc_filename;
spc.filename = spc_filename;
fid = fopen(spc_filename, 'r');
spc.header.spc_revision = fread(fid, 1, 'int16');
spc.header.info_offset = fread(fid, 1, 'int32');
spc.header.info_length = fread(fid, 1, 'int16');
spc.header.setup_offset = fread(fid, 1, 'int32');
spc.header.setup_length = fread(fid, 1, 'int16');
spc.header.data_block_offset = fread(fid, 1, 'int32');
spc.header.no_of_data_blocks_old = fread(fid, 1, 'int16');
spc.header.data_block_length = fread(fid, 1, 'int32');
spc.header.meas_desc_block_offset = fread(fid, 1, 'int32');
spc.header.no_of_meas_desc_blocks = fread(fid, 1, 'int16');
spc.header.meas_desc_block_length = fread(fid, 1, 'int16');
spc.header.header_valid = fread(fid, 1, 'uint16');
spc.header.no_of_data_blocks = fread(fid, 1, 'uint32');
spc.header.reserve2 = fread(fid, 1, 'uint16');
spc.header.checksum = fread(fid, 1, 'uint16');

infoident = fgetl(fid);
spc.info.ID = fgetl(fid);
spc.info.title = fgetl(fid);
spc.info.version = fgetl(fid);
spc.info.revision = fgetl(fid);
spc.info.time = fgetl(fid);
spc.info.author = fgetl(fid);
spc.info.company = fgetl(fid);
spc.info.contents = fgetl(fid);
spc.info.contents2 = fgetl(fid);
infoend = fgetl(fid);

fseek (fid, spc.header.data_block_offset, 'bof');
spc.dataheader.block_no = fread(fid, 1, 'int16');
spc.dataheader.data_offset = fread(fid, 1, 'int32');
spc.dataheader.next_block_offset = fread(fid, 1, 'int32');
spc.dataheader.block_type = fread(fid, 1, 'uint16');
spc.dataheader.meas_desc_block_no = fread(fid, 1, 'int16');
spc.dataheader.lblock_no = fread(fid, 1, 'uint32');
spc.dataheader.block_length = fread(fid, 1, 'uint32');

fseek (fid, spc.header.meas_desc_block_offset, 'bof');
spc.datainfo.time = reshape(char(fread(fid, 9, 'schar')), 1, 9);
spc.datainfo.date = reshape(char(fread(fid, 11, 'schar')), 1, 11);
spc.datainfo.mod_ser_no = reshape(char(fread(fid, 16, 'schar')), 1, 16);
spc.datainfo.meas_mode = fread(fid, 1, 'int16');
spc.datainfo.cfd_ll = fread(fid, 1, 'float32');
spc.datainfo.cfd_lh = fread(fid, 1, 'float32');
spc.datainfo.cfd_zc = fread(fid, 1, 'float32');
spc.datainfo.cfd_hf = fread(fid, 1, 'float32');
spc.datainfo.syn_zc = fread(fid, 1, 'float32');
spc.datainfo.syn_fd = fread(fid, 1, 'int16');
spc.datainfo.syn_hf = fread(fid, 1, 'float32');
spc.datainfo.tac_r = fread(fid, 1, 'float32');
spc.datainfo.tac_g = fread(fid, 1, 'int16');
spc.datainfo.tac_of = fread(fid, 1, 'float32');
spc.datainfo.tac_ll = fread(fid, 1, 'float32');
spc.datainfo.taclh = fread(fid, 1, 'float32');
spc.datainfo.adc_re = fread(fid, 1, 'int16');
spc.datainfo.eal_de = fread(fid, 1, 'int16');
spc.datainfo.ncx = fread(fid, 1, 'int16');
spc.datainfo.ncy = fread(fid, 1, 'int16');
spc.datainfo.page = fread(fid, 1, 'uint16');
spc.datainfo.col_t = fread(fid, 1, 'float32');
spc.datainfo.rep_t = fread(fid, 1, 'float32');
spc.datainfo.stopt = fread(fid, 1, 'int16');
spc.datainfo.overfl = char(fread(fid, 1, 'schar'));
spc.datainfo.use_motor = fread(fid, 1, 'int16');
spc.datainfo.steps = fread(fid, 1, 'int16');
spc.datainfo.offset = fread(fid, 1, 'float32');
spc.datainfo.dither = fread(fid, 1, 'int16');
spc.datainfo.incr = fread(fid, 1, 'int16');
spc.datainfo.mem_bank = fread(fid, 1, 'int16');
spc.datainfo.mod_type = reshape(char(fread(fid, 16, 'schar')), 1, 16);
spc.datainfo.syn_th = fread(fid, 1, 'float32');
spc.datainfo.dead_time_comp = fread(fid, 1, 'int16');
spc.datainfo.polarity_l = fread(fid, 1, 'int16');
spc.datainfo.polarity_f = fread(fid, 1, 'int16');
spc.datainfo.polarity_p = fread(fid, 1, 'int16');
spc.datainfo.linediv = fread(fid, 1, 'int16');
spc.datainfo.accumulate = fread(fid, 1, 'int16');
spc.datainfo.flbck_y = fread(fid, 1, 'int32');
spc.datainfo.flbck_x = fread(fid, 1, 'int32');
spc.datainfo.bord_u = fread(fid, 1, 'int32');
spc.datainfo.bord_l = fread(fid, 1, 'int32');
spc.datainfo.pix_time = fread(fid, 1, 'float32');
spc.datainfo.pix_clk = fread(fid, 1, 'int16');
spc.datainfo.pix_clk = fread(fid, 1, 'int16');
spc.datainfo.scan_x = fread(fid, 1, 'int32');
spc.datainfo.scan_y = fread(fid, 1, 'int32');
spc.datainfo.scan_rx = fread(fid, 1, 'int32');
spc.datainfo.scan_ry = fread(fid, 1, 'int32');
spc.datainfo.fifo_type = fread(fid, 1, 'int16');
spc.datainfo.epx_div = fread(fid, 1, 'int16');
spc.datainfo.mod_type_code = fread(fid, 1, 'int16');

fseek (fid, spc.dataheader.data_offset, 'bof');
width = 2^(round(log2(spc.datainfo.scan_x-1)+0.5));
height = 2^(round(log2(spc.datainfo.scan_y-1)+0.5));
spc.size = [spc.datainfo.adc_re, width, height];
n_curves = spc.datainfo.adc_re*height*width;
spc.image = fread(fid, n_curves, 'uint16');
fclose(fid);
%%%%%%%%%%%%%
spc.datainfo.pulseInt = 13.16 %(ns)
spc.datainfo.psPerUnit = spc.datainfo.tac_r/spc.datainfo.tac_g/spc.size(1)*1e12;
spc.image = reshape(spc.image, spc.size(1), width, height)/spc.datainfo.incr;
spc.switches.peak = [-1, 4];
spc.switches.lifetime_limit = [0.5, 5];
spc.switches.logscale = 1;
spc.fit.range = [1, spc.size(1)];
spc.switches.threshold = 300;
eval('background = spc.fit.background', 'spc.fit.background=0');
%%%%%%%%%%%%%%
close(1);
spc.figure.project = 1;
spc.figure.lifetime = 2;
spc.figure.lifetimeMap = 3;
%Fig1.
figure(spc.figure.project);
set(spc.figure.project, 'Position', [20,50,360,300], 'name', 'Projection');
%menubar if you want ??
f = uimenu('Label','User');
uimenu(f, 'Label', 'makeNewRoi', 'Callback', 'spc_makeRoi');
uimenu(f, 'Label', 'binning', 'Callback', 'spc_binning');

%Fig2.
figure(spc.figure.lifetime);
set(spc.figure.lifetime, 'Position', [410, 50, 360, 300], 'name', 'Lifetime');

%Fig3.
figure(spc.figure.lifetimeMap);
set(spc.figure.lifetimeMap, 'Position', [20, 450, 360, 300], 'name', 'LifetimeMap');
spc.figure.mapRoi=rectangle('position', [20,20,20,20], 'EdgeColor', [1,1,1]);

spc_drawAll;