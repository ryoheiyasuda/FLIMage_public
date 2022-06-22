function spci = spc_doSameAll;
global spc;
%global spci;
%spci = [];

pname = 'C:\MATLAB6p1\work\r130\spc\';
header = 'r130c';
istart = 6;
iend = 50;
ind = [1,3,6:14];

nImages = 0;
for i = ind
    nstr = num2str(i);
    while length(nstr) < 3
        nstr = ['0', nstr];
    end
    fname = [pname, header, nstr, '.sdt'];
    if exist(fname) == 2
        nImages = nImages + 1;
        spc_readdata(fname);
        spc_drawSetting;
        spc_smooth(4);
        [spc_prop, ratio2]  = spc_ratio (2, -0.25, 0.5);
        nstr2 = num2str(nImages);
        %eval(['spci.im', nstr2, '= spc.rgbLifetime;']);
        eval(['spci.im', nstr2, '= spc_prop;']);
    end
end

%nImages = iend - istart + 1;
figure(105);

%if nImages > 3
%    nrow = 2;
%else
%    nrow = 1;
%end
nrow = ceil (nImages/6);

ncol = ceil(nImages/nrow);
fillF = 0.975;
drawArea = 1;

set(gcf, 'Position', [50,50, 200*ncol/drawArea, 200*nrow]);
set(gcf, 'PaperPositionMode', 'auto');
for i = 1:nImages
    yi = floor((i-1)/ncol);
    xi = i-yi*ncol-1;
    w = (1/ncol*fillF)*drawArea;
    h = 1/nrow*fillF;
    x = (xi/ncol + 1/ncol*(1-fillF)/2)*drawArea;
    y = yi/nrow + 1/nrow*(1-fillF)/2;
    position = [x,1-y-1/nrow,w,h];
    %subplot (nrow, ncol, i);
    subplot('Position', position);
    eval(['image(spci.im', num2str(i), ');']);
    set(gca, 'XTick', [], 'YTick', []);
end