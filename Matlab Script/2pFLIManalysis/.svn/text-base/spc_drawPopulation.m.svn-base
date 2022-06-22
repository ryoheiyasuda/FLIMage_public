function rgbimage=spc_drawPopulation(pop);
global spc;
global gui;

if ~nargin
    pop = [1,0];
end

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
    spc_roi = [1,1,spc.size(3)-1, spc.size(2)-1];
end

project = reshape(sum(spc.imageMod, 1), spc.SPCdata.scan_size_x, spc.SPCdata.scan_size_y);
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
% lifetimeMap = spc.lifetimeMap;
% lifetimeMap(bw) = (sumX_project(bw)./sum_project(bw));
spc.lifetimeMap(bw) = (sumX_project(bw)./sum_project(bw))*spc.datainfo.psPerUnit/1000-pos_max2;
lifetimeMap = spc.lifetimeMap / spc.datainfo.psPerUnit* 1000;
if isfield(spc, 'roipoly')
	spc.lifetimeMap(~spc.roipoly) = spc.switches.lifetime_limit(2);
end

population = lifetimeMap;
tauD = spc.fit.beta0(2);
tauAD = spc.fit.beta0(4);
population(bw) = tauD.*(tauD-lifetimeMap(bw))/(tauD-tauAD) ./ (tauD + tauAD -lifetimeMap(bw));

if spc.SPCdata.line_compression > 1
    aa = 1/spc.SPCdata.line_compression;
    %[xi, yi] = meshgrid(aa:aa:spc.SPCdata.scan_size_x, aa:aa:spc.SPCdata.scan_size_y);
    [xi, yi] = meshgrid(1:aa:1-aa+spc.SPCdata.scan_size_x, 1:aa:1-aa+spc.SPCdata.scan_size_y);
    population1 = [population; population(end, :)];
    population2  = [population1, population1(:, end)];
    population = interp2(population2, xi, yi);
    spc.size(2) = spc.SPCdata.scan_size_x * (1/aa);
    spc.size(3) = spc.SPCdata.scan_size_y * (1/aa);           
end
%figure; imagesc(population, [0,1]);

if spc.switches.filter > 1
    filterWindow = ones(spc.switches.filter, spc.switches.filter)/spc.switches.filter/spc.switches.filter;
    population = imfilter(population, filterWindow, 'replicate');
end

rgbimage = spc_im2rgb(population, pop);

try 
    low = spc.switches.lutlim(1);
    high = spc.switches.lutlim(2);
catch
    try
        low = 0;
        high = spc.switches.threshold;
    catch
        maxim = max(max(spc.project, [], 1), [], 2);
        low = 0;
        high = maxim/6;
    end
    spc.switches.lutlim(1) = low;
    spc.switches.lutlim(2) = high;
end



if high-low > 0
    gray = (spc.project-low)/(high - low);
else
    gray = 0;
end
gray(gray > 1) = 1;
gray(gray < 0) = 0;
rgbimage(:,:,1)=rgbimage(:,:,1).*gray;
rgbimage(:,:,2)=rgbimage(:,:,2).*gray;
rgbimage(:,:,3)=rgbimage(:,:,3).*gray;

%figure; image(rgbimage);
%set(gca, 'XTick', [], 'YTick', []);