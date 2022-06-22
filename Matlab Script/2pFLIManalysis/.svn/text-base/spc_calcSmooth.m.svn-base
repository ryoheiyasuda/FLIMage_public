function spc = spc_calcSmooth(spc, n);

tw = spc.size(1);
xw = spc.SPCdata.scan_size_x;
yw = spc.SPCdata.scan_size_y;


s = floor((n+1)/2);
imageMod = zeros(tw, xw, yw);
%image = reshape(full(spc.image), tw, xw, yw);
for i=1:n
    for j=1:n
        imageMod(:, s:xw-n+s, s:yw-n+s)= spc.imageMod(:, i:xw+i-n, j:yw+j-n)/n/n + imageMod(:, s:xw-n+s, s:yw-n+s);
    end
end
spc.imageMod = imageMod;
%spc.imageMod = sparse(reshape(imageMod, spc.size(1), xw*yw));

spc.project = reshape(sum(spc.imageMod, 1), xw, yw);
