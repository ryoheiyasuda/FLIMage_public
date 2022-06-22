function spc_DendriteAnalysis(fn)

% DendriteMatrix: row = timepoint, column = pixel
% DendriteMatrix1: DendriteMatrix subtracted by baseline time point

if ~nargin
    a = dir('*_ROI2.mat');
    fn = a(1).name;
    fn = fn(1:end-4);
end


load(fn);
evalc(['data = ', fn]);

pixelPerMicronAt20Zoom = 10.666; %%%%Measure the value
%baseLine = 1:6; %%%%%%%Baseline frames.


prompt = {'Enter baseline:', 'Enter peak:'};
dlg_title = 'Baseline and Peak frames';
num_lines = 1;
def = {'1:6', '10:14'};
answer = inputdlg(prompt,dlg_title,num_lines,def);

baseLine = str2num(answer{1});
basal = baseLine;
peak = str2num(answer{2});

%fn = data.roiData(1).filename;
numStr = sprintf('%03d', baseLine(1));
imfile = [fn(1: 6), numStr, '.tif'];
infof = imfinfo(imfile);





header=infof(1).ImageDescription;
evalc(header);
zoom1 = spc.scanHeader.acq.zoomhundreds*100+spc.scanHeader.acq.zoomtens*10+spc.scanHeader.acq.zoomones;
pixelPerMicron = pixelPerMicronAt20Zoom * 20 / zoom1;


% make a convienience variable
pl = data.polyLines; 
nacq = size(pl,2); 

sustain = [nacq-4: nacq];

% for now we assume there is only one polyline - only one dendrite. 
% we want to plot the binding fraction for all images over the length of
% the polyline. 

%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
figure; 
subplot(2,3, 1);
maxstart = 0; 
minfin = 1e6;  
for j = 1:nacq
    maxstart = max(maxstart, min(pl{j}.contourL)); 
    minfin = min(minfin, max(pl{j}.contourL)); 
end

% in order to get the plots to be different colors, you should put them
% into a matrix, and then plot that. 
m = zeros(nacq, minfin - maxstart + 1); 
for j = 1:nacq
    sta = find(pl{j}.contourL == maxstart);
    fin = find(pl{j}.contourL == minfin); 
    m(j,:) = pl{j}.fraction(sta:fin); 
end
time = data.roiData(1,1).time3; 
time = time - time(baseLine(end));
% time appears to be in fractions of a day. 
% convert it to minutes, which is slightly more convenient. 
time = time * 60 * 24; 
plot(time, m'); %plot plots lines along columns.
title('all lifetime data for polylines')
xlabel('time, minutes'); 
ylabel('binding fraction'); 

%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
subplot(2,3,2)
plot(m'); %plot plots lines along columns.
title('all lifetime data for polylines')
xlabel('pixels'); 
ylabel('binding fraction'); 




% now: convert this to distance from uncaging point
%uncaging point is the point on the polyline closest to ROI1. 
% therefore, search through all points for the closest. 
totlen = size(m,2);

distances = zeros(nacq, totlen); 
Dend_Binned = [];
for j = 1:nacq
	p = data.roiData(1,1).position{j}; 
	if numel(p) == 4 
		pos = p;
	end
	roi1x = pos(1)+pos(3)/2; 
	roi1y = pos(2)+pos(4)/2; 
	% disp(['roi1 ' num2str(roi1x) ' ' num2str(roi1y)]); 
	sta = find(pl{j}.contourL == maxstart);
	fin = find(pl{j}.contourL == minfin);
	xv = pl{j}.coordinateX(sta:fin); 
	yv = pl{j}.coordinateY(sta:fin);
	% iterate over all points on the polyline to select the closest.

    dist =  sqrt((roi1x - xv).^2 + (roi1y - yv).^2);
    [val, closestk] = min(dist);
    
	% okay, now we know the closest pixel - measure all distances relative
	% to this one. 
	% there are more efficient vectoral ways of doing this, but it may be
	% harder to understand.

    dist_fromB = cumsum([0, sqrt(diff(xv).^2 + diff(yv).^2)])/pixelPerMicron;
    distances(j,:) = dist_fromB;
    dendValues = m(j, :);
    for l = 0:round(max(dist_fromB))
        Dend_Binned_each(l+1) = mean(dendValues(dist_fromB >= l-0.5 & dist_fromB < l+0.5));
    end
    Dend_Binned = [Dend_Binned; Dend_Binned_each];
end



%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

subplot(2, 3, 3);
hold on
x = 0:length(Dend_Binned(1, :))-1;
plot(x, mean(Dend_Binned(basal, :), 1), '-ro');
plot(x, mean(Dend_Binned(peak, :), 1), '-bo');
plot(x, mean(Dend_Binned(sustain, :), 1), '-ko');

hold off
xlabel('distance from uncaging point, um'); 
ylabel('binding fraction per pixel'); 
title('scatterplot of BF vs distance from uncaging'); 
legend({'basal','peak','sustain'}); 



%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
subplot(2, 3, 4);
hold on
Dend_BinnedMean = mean(Dend_Binned(baseLine, :), 1);
Dend_Binned2 =Dend_Binned - repmat(Dend_BinnedMean, [nacq, 1]);

x = 0:length(Dend_Binned2(1, :))-1;
plot(x, mean(Dend_Binned2(basal, :), 1), '-ro');
plot(x, mean(Dend_Binned2(peak, :), 1), '-bo');
plot(x, mean(Dend_Binned2(sustain, :), 1), '-ko');

hold off
xlabel('distance from uncaging point, um'); 
ylabel('binding fraction per pixel'); 
title('scatterplot of BF vs distance from uncaging'); 
legend({'basal','peak','sustain'}); 

%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
subplot(2, 3, 5);
% plot time along the x axis
% binding fraction on the y axis
% organize traces by distance from uncaging point. 
% the plots will be: 0-1um, 1-4um, 4-10um. 
d = distances; % a convenience variable. 
for j = 1:nacq
	mm = m(j,d(j,:) < 1); % the 1 here is in microns and sets the first interval
	mm = mm(isnan(mm) == 0); 
	p(j,1) = mean(mm); 
	pstd(j,1) = std(mm); 
	mm = m(j,(d(j,:) >= 1 & d(j,:) < 4)); %these set the limits for the second interval
	mm = mm(isnan(mm) == 0); 
	p(j,2) = mean(mm); 
	pstd(j,2) = std(mm); 
	mm = m(j,(d(j,:) >= 4 & d(j,:) < 10)); %and this the limits for the 3rd interval.
	mm = mm(isnan(mm) == 0); 
	p(j,3) = mean(mm); 
	pstd(j,3) = std(mm); 
end
hold on
h(1) = plot(time, p(:,1), 'ro-');
h(2) = plot(time, p(:,2), 'bo-'); 
h(3) = plot(time, p(:,3), 'go-'); 

% h(1) = plot(time, p(:,1), 'ro-'); 
% h(1) = errorbar(time, p(:,1), pstd(:,1), 'ro-');
% h(2) = errorbar(time, p(:,2), pstd(:,2), 'bo-'); 
% h(3) = errorbar(time, p(:,3), pstd(:,3), 'go-'); 
% % adjust the error bar widths to be smaller and less obtrusive. 
% for k = 1:3
% 	errorbar_tick(h(k), 200); 
% end
hold off
legend(h, 'd < 1\mum','1\mum < d < 4\mum', '4\mum < d < 10\mum'); 
xlabel('time, minutes');


%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
m1 = mean(m(baseLine, :), 1);
DendriteMatrix1 = m - repmat(m1, [nacq, 1]);
DendriteMatrix = m;

%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
fn = [fn, '_Dendrite'];
a.Dend_Binned = Dend_Binned;
a.Dend_Binned2 = Dend_Binned2;
a.baseLine = baseLine;

evalc([fn, '=a']);
save(fn, fn);
