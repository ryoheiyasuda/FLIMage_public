function spc_mkanim (animfile)

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

animfile = [animfile, '.tif'];
imwrite(spcs.spc1.rgbLifetime, animfile, 'Compression', 'none');
for i = 2:spcs.nImages
    eval(['imwrite(spcs.spc', num2str(i), '.rgbLifetime, animfile, ''Compression'', ''none'', ''WriteMode'', ''append'')']);
end
