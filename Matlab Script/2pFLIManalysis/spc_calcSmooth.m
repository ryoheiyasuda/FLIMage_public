function spc = spc_calcSmooth(spc, n);

tw = spc.size(1);
xw = spc.SPCdata.scan_size_x;
yw = spc.SPCdata.scan_size_y;


s = floor((n+1)/2);
imageMod = zeros(spc.nChannels, tw, xw, yw);
%image = reshape(full(spc.image), tw, xw, yw);
for i=1:n
    for j=1:n
        imageMod(:, :, s:xw-n+s, s:yw-n+s)= spc.imageMod(:, :, i:xw+i-n, j:yw+j-n)/n/n + imageMod(:, :, s:xw-n+s, s:yw-n+s);
    end
end
spc.imageMod = imageMod;

imageMod2 = reshape(spc.imageMod(spc.currentChannel, :,:,:), spc.size);
spc.project = reshape(sum(imageMod2, 1), xw, yw);
