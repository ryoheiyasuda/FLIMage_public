function spc_calcLifetimeMap;
global spc;
global gui;

pos_max2 = str2num(get(gui.spc.spc_main.F_offset, 'String'));
if pos_max2 == 0 | isnan (pos_max2)
    pos_max2 = 1.0
end
set(gui.spc.spc_main.F_offset, 'String', num2str(pos_max2));

spc.fit.range = round(spc.fit.range);
range = spc.fit.range;
try
    spc_roi = get(gui.spc.figure.roi, 'Position');
catch
    spc_roi = [1,1,spc.size(3), spc.size(2)];
end

project = reshape(sum(spc.imageMod, 1),spc.SPCdata.scan_size_x, spc.SPCdata.scan_size_y);
spc.lifetimeAll = reshape(sum(sum(spc.imageMod, 2), 3), spc.size(1), 1);

[maxcount, pos_max] = max(spc.lifetimeAll(range(1):1:range(2)));
pos_max = pos_max+range(1)-1;

x_project = 1:length(range(1):range(2));
x_project2 = repmat(x_project, [1,spc.SPCdata.scan_size_x*spc.SPCdata.scan_size_y]);
x_project2 = reshape(x_project2, length(x_project), spc.SPCdata.scan_size_x, spc.SPCdata.scan_size_y);
sumX_project = spc.imageMod(range(1):range(2),:,:).*x_project2;
sumX_project = sum(sumX_project, 1);

sum_project = sum(spc.imageMod(range(1):range(2),:,:), 1);
sum_project = reshape(sum_project, spc.SPCdata.scan_size_x, spc.SPCdata.scan_size_y); 

spc.lifetimeMap = zeros(spc.SPCdata.scan_size_x, spc.SPCdata.scan_size_y);
bw = (sum_project > 0);
spc.lifetimeMap(bw) = (sumX_project(bw)./sum_project(bw))*spc.datainfo.psPerUnit/1000-pos_max2;

if spc.SPCdata.line_compression > 1
    aa = 1/spc.SPCdata.line_compression;
    %[xi, yi] = meshgrid(aa:aa:spc.SPCdata.scan_size_x, aa:aa:spc.SPCdata.scan_size_y);
    [xi, yi] = meshgrid(1:aa:1-aa+spc.SPCdata.scan_size_x, 1:aa:1-aa+spc.SPCdata.scan_size_y);
    lifetimeMap1 = [spc.lifetimeMap; spc.lifetimeMap(end, :)];
    lifetimeMap2 = [lifetimeMap1, lifetimeMap1(:, end)];
    spc.lifetimeMap = interp2(lifetimeMap2, xi, yi);
    spc.size(2) = spc.SPCdata.scan_size_x * (1/aa);
    spc.size(3) = spc.SPCdata.scan_size_y * (1/aa);           
end

if spc.switches.filter > 1
    filterWindow = ones(spc.switches.filter, spc.switches.filter)/spc.switches.filter/spc.switches.filter;
    spc.lifetimeMap = imfilter(spc.lifetimeMap, filterWindow, 'replicate');
end


if isfield(spc, 'roipoly')
	spc.lifetimeMap(~spc.roipoly) = spc.switches.lifetime_limit(2);
end
