function spc_makeRGBLifetimeMap;

global spc;
%Drawing
rgbimage = spc_im2rgb(spc.lifetimeMap, spc.switches.lifetime_limit);
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

spc.rgbLifetime = rgbimage;
