function spc_colorbar
global spc

b = spc.fit(spc.currentChannel).lifetime_limit;

axes('Position', [0.95,0.1,0.02,0.4]);
scale = 56:-1:9; image(scale(:));

set(gca,'XTickLabel', [], 'YTick', [1,48], 'YTickLabel', b);

set(gcf, 'PaperPositionMode', 'auto');
set(gcf, 'PaperOrientation', 'landscape');