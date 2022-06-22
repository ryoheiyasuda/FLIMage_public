function spc_drawLifetimeMap_All (flag)
global spc
global spcs

 if (spc.switches.imagemode ~= 1)
     return;
 end

range = spc.switches.lifetime_limit;
thresh = spc.switches.threshold;

if nargin == 0 | flag == 1
    for i = 1:spcs.nImages
        range1 = spc.fit.range(1);
        range2 = spc.fit.range(2);
        eval(['spcs.spc', num2str(i), '.fit.range(1) = range1;']);
        eval(['spcs.spc', num2str(i), '.fit.range(2) = range2;']);
        eval(['spcs.spc', num2str(i), ' = spc_calcLifetimeMap(spcs.spc', num2str(i), ');']);
        eval(['spcs.spc', num2str(i), '.rgbLifetime = spc_makeRGBLifetimeMap(spcs.spc', num2str(i), ');']);
        eval(['spcs.spc', num2str(i), '.switches.lifetime_limit (1) = range(1);']);
        eval(['spcs.spc', num2str(i), '.switches.lifetime_limit (2) = range(2);']);
        eval(['spcs.spc', num2str(i), '.switches.threshold = thresh;']);
        %eval(['spcs.spc', num2str(i), '.rgbLifetime = spc_makeRGBLifetimeMap(spcs.spc', num2str(i), ');']);
        %spc_drawSetting(1);
    end
end


figure(104);

if spcs.nImages > 3
    nrow = 2;
else
    nrow = 1;
end
ncol = ceil(spcs.nImages/nrow);
fillF = 0.975;
drawArea = 0.90;

set(gcf, 'Position', [50,50, 200*ncol/drawArea, 200*nrow]);
set(gcf, 'PaperPositionMode', 'auto');

for i = 1:spcs.nImages
    yi = floor((i-1)/ncol);
    xi = i-yi*ncol-1;
    w = (1/ncol*fillF)*drawArea;
    h = 1/nrow*fillF;
    x = (xi/ncol + 1/ncol*(1-fillF)/2)*drawArea;
    y = yi/nrow + 1/nrow*(1-fillF)/2;
    position = [x,1-y-1/nrow,w,h];
    %subplot (nrow, ncol, i);
    subplot('Position', position);
    eval(['image(spcs.spc', num2str(i), '.rgbLifetime);']);
    set(gca, 'XTick', [], 'YTick', []);
end

range = spc.switches.lifetime_limit;

subplot('Position', [0.9125, 0.2/nrow, 0.015, 0.5/nrow]);
scale = 56:-1:9;
image(scale(:));
colormap(jet);
set(gca, 'XTickLabel', []);
set(gca, 'YAxisLocation', 'right', 'YTickLabel', []);
set(gca, 'YTick', [1,48], 'YTickLabel', [range(1), range(2)]);