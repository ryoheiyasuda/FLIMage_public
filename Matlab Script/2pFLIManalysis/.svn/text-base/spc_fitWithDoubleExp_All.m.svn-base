function spc_fitWithDoubleExp_All;
global spc;
global gui;

spc.lifetimeMap2 = [];
range = spc.fit.range;
lifetime = spc.lifetime(range(1):1:range(2));
x = [1:1:length(lifetime)];
t = [range(1):1:range(2)];
t = t*spc.datainfo.psPerUnit/1000;

try
   triple = spc.fit.tripleGauss;
catch
    triple = [0.0006
    1.0000
   -0.0343
    0.0830
    0.0156
    0.5527
   -0.4447
    0.5842
    0.0682
    0.1103];
    spc.fit.tripleGauss = triple;
end;

for xi=1:spc.size(2)
    for yi = 1:spc.size(3)
        curve = spc.image(:,xi*yi);
        curve = curve(range(1):range(2));
        sum1 = sum(curve(:));
        peak1 = max(curve);
        
        %if (sum1 > spc.switches.threshold)
            beta0 = [peak1, peak1];
            weight = sqrt(lifetime)/sqrt(max(lifetime));
            weight(weight == 0) = 1;
            betahat = spc_nlinfit(x, curve, weight, @exp2TripleGauss_tauFix, beta0);
            spc.lifetimeMap2(xi, yi) = 100*betahat(2)/(betahat(1)+betahat(2));
            %else
            %spc.lifetimeMap2(xi, yi) = 0;
            %end
        
    end
end
%Pixel-by-Pixel fitting.
limit = [0, 100];
map2 = 100-spc.lifetimeMap2;
rgbimage = spc_im2rgb(map2, limit);
gray = spc.project/spc.switches.threshold;
gray(gray > 1) = 1;
%%gray = gray.^(0.5);
rgbimage(:,:,1)=rgbimage(:,:,1).*gray;
rgbimage(:,:,2)=rgbimage(:,:,2).*gray;
rgbimage(:,:,3)=rgbimage(:,:,3).*gray;
spc.rgbLifetime2 = rgbimage;

figure(4);
ih = axes('Position', [0.1300    0.1100    0.6626    0.8150]);
image(spc.rgbLifetime2);

%draw colormap.
barh = axes('Position', [0.88, 0.11, 0.05, 0.8150]);
scale = 56:-1:9;
%scale = 9:56;
image(scale(:));
colormap(jet);
set(barh, 'XTickLabel', []);
th = text('string', 'ns');
set(barh, 'YAxisLocation', 'right', 'YTick', [1,48], 'YTicKLabel', [100, 0] ...
    , 'YLabel', th);
