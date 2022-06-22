function error = spc_readdata(spc_filename);
global spc

error = 0;
%Reading file.
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


for count = 1:spc.header.no_of_data_blocks;
    error = 0;
    if count == 1
        fseek (fid, spc.header.data_block_offset, 'bof');
    else
        fseek (fid, tmp.dataheader.next_block_offset, 'bof');
    end;
    
    tmp.dataheader.block_no = fread(fid, 1, 'int16');
    tmp.dataheader.data_offset = fread(fid, 1, 'int32');
    tmp.dataheader.next_block_offset = fread(fid, 1, 'int32');
    tmp.dataheader.block_type = fread(fid, 1, 'uint16');
    tmp.dataheader.meas_desc_block_no = fread(fid, 1, 'int16');
    tmp.dataheader.lblock_no = fread(fid, 1, 'uint32');
    tmp.dataheader.block_length = fread(fid, 1, 'uint32');
    
    position = spc.header.meas_desc_block_offset+(tmp.dataheader.meas_desc_block_no)*...
        spc.header.meas_desc_block_length;
    fseek (fid, position, 'bof');
    tmp.datainfo.time = reshape(char(fread(fid, 9, 'schar')), 1, 9);
    tmp.datainfo.date = reshape(char(fread(fid, 11, 'schar')), 1, 11);
    tmp.datainfo.mod_ser_no = reshape(char(fread(fid, 16, 'schar')), 1, 16);
    tmp.datainfo.meas_mode = fread(fid, 1, 'int16');
    tmp.datainfo.cfd_ll = fread(fid, 1, 'float32');
    tmp.datainfo.cfd_lh = fread(fid, 1, 'float32');
    tmp.datainfo.cfd_zc = fread(fid, 1, 'float32');
    tmp.datainfo.cfd_hf = fread(fid, 1, 'float32');
    tmp.datainfo.syn_zc = fread(fid, 1, 'float32');
    tmp.datainfo.syn_fd = fread(fid, 1, 'int16');
    tmp.datainfo.syn_hf = fread(fid, 1, 'float32');
    tmp.datainfo.tac_r = fread(fid, 1, 'float32');
    tmp.datainfo.tac_g = fread(fid, 1, 'int16');
    tmp.datainfo.tac_of = fread(fid, 1, 'float32');
    tmp.datainfo.tac_ll = fread(fid, 1, 'float32');
    tmp.datainfo.taclh = fread(fid, 1, 'float32');
    tmp.datainfo.adc_re = fread(fid, 1, 'int16');
    tmp.datainfo.eal_de = fread(fid, 1, 'int16');
    tmp.datainfo.ncx = fread(fid, 1, 'int16');
    tmp.datainfo.ncy = fread(fid, 1, 'int16');
    tmp.datainfo.page = fread(fid, 1, 'uint16');
    tmp.datainfo.col_t = fread(fid, 1, 'float32');
    tmp.datainfo.rep_t = fread(fid, 1, 'float32');
    tmp.datainfo.stopt = fread(fid, 1, 'int16');
    tmp.datainfo.overfl = char(fread(fid, 1, 'schar'));
    tmp.datainfo.use_motor = fread(fid, 1, 'int16');
    tmp.datainfo.steps = fread(fid, 1, 'int16');
    tmp.datainfo.offset = fread(fid, 1, 'float32');
    tmp.datainfo.dither = fread(fid, 1, 'int16');
    tmp.datainfo.incr = fread(fid, 1, 'int16');
    tmp.datainfo.mem_bank = fread(fid, 1, 'int16');
    tmp.datainfo.mod_type = reshape(char(fread(fid, 16, 'schar')), 1, 16);
    tmp.datainfo.syn_th = fread(fid, 1, 'float32');
    tmp.datainfo.dead_time_comp = fread(fid, 1, 'int16');
    tmp.datainfo.polarity_l = fread(fid, 1, 'int16');
    tmp.datainfo.polarity_f = fread(fid, 1, 'int16');
    tmp.datainfo.polarity_p = fread(fid, 1, 'int16');
    tmp.datainfo.linediv = fread(fid, 1, 'int16');
    tmp.datainfo.accumulate = fread(fid, 1, 'int16');
    tmp.datainfo.flbck_y = fread(fid, 1, 'int32');
    tmp.datainfo.flbck_x = fread(fid, 1, 'int32');
    tmp.datainfo.bord_u = fread(fid, 1, 'int32');
    tmp.datainfo.bord_l = fread(fid, 1, 'int32');
    tmp.datainfo.pix_time = fread(fid, 1, 'float32');
    tmp.datainfo.pix_clk = fread(fid, 1, 'int16');
    tmp.datainfo.pix_clk = fread(fid, 1, 'int16');
    tmp.datainfo.scan_x = fread(fid, 1, 'int32');
    tmp.datainfo.scan_y = fread(fid, 1, 'int32');
    tmp.datainfo.scan_rx = fread(fid, 1, 'int32');
    tmp.datainfo.scan_ry = fread(fid, 1, 'int32');
    tmp.datainfo.fifo_type = fread(fid, 1, 'int16');
    tmp.datainfo.epx_div = fread(fid, 1, 'int16');
    tmp.datainfo.mod_type_code = fread(fid, 1, 'int16');

    fseek (fid, tmp.dataheader.data_offset, 'bof');
    %width = 2^(round(log2(tmp.datainfo.scan_x-1)+0.5));
    %height = 2^(round(log2(tmp.datainfo.scan_y-1)+0.5));
    %tmp.size = [tmp.datainfo.adc_re, width, height];
    %n_curves = tmp.datainfo.adc_re*height*width;
    %tmp.curves = fread(fid, n_curves, 'uint16');
    tmp.datainfo.pulseInt = 13.16; %(ns)
    tmp.datainfo.psPerUnit = tmp.datainfo.tac_r/tmp.datainfo.tac_g/tmp.datainfo.adc_re*1e12;
    %eval('savepage = page;', 'savepage = 1;');
    page = tmp.datainfo.page;
        

    width = 2^(round(log2(tmp.datainfo.scan_x-1)+0.5));
    hight = 2^(round(log2(tmp.datainfo.scan_y-1)+0.5));
    tmp.size = [tmp.datainfo.adc_re, hight, width];
    npoints = prod(tmp.size);
    tmp.curve = fread(fid, npoints, 'uint16')/tmp.datainfo.incr;
    if prod(size(tmp.curve)) == npoints
        tmp.image = reshape(tmp.curve, tmp.size(1), tmp.size(2), tmp.size(3));
        eval(['spc.page', num2str(page), '.size = tmp.size;']);
        eval(['spc.page', num2str(page), '.image = tmp.image;']);
    else
        error = 1;
    end

    
end;

fclose(fid);
%%%%%%%%%%%%%
    spc.datainfo = tmp.datainfo;


%%%%%%%%%%%%%%
    spc.size = tmp.size; %[tmp.datainfo.adc_re, spc.datainfo.scan_y, spc.datainfo.scan_x];
    spc.imageMod = tmp.image;
    spc.project = reshape(sum(spc.imageMod, 1), spc.size(2), spc.size(3));
    spc.stack.nStack = 1;
    spc.stack.image1 = {};
    spc.stack.image1{1} = spc.imageMod;
    spc.stack.project = spc.project;
    spc.switches.noSPC = 0;
    spc.datainfo.psPerUnit = spc.datainfo.tac_r/spc.datainfo.tac_g*1e9*1000/spc.size(1);

    spc.SPCdata.line_compression = 1;
    spc.switches.imagemode = 1;
    spc.switches.logscale = 1;
    spc.datainfo.triggerTime = spc.datainfo.time;