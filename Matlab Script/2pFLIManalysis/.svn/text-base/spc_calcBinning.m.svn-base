function spcout = spc_calcBinning(spc)

xw = spc.size(2);
yw = spc.size(3);
%imageMod = reshape(full(spc.imageMod), spc.size(1), xw, yw);
spc.imageMod = spc.imageMod(:,1:2:xw,1:2:yw) + spc.imageMod(:,2:2:xw,1:2:yw)...
    + spc.imageMod(:,1:2:xw,2:2:yw) + spc.imageMod(:,2:2:xw,2:2:yw);
spc.size(2)=round(spc.size(2)/2);
spc.size(3)=round(spc.size(3)/2);
%spc.imageMod = sparse(reshape(imageMod, spc.size(1), spc.size(2)*spc.size(3)));
spc.project = reshape(sum(spc.imageMod, 1), spc.size(2), spc.size(3));

spcout = spc;