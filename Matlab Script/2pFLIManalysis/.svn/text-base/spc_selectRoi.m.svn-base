function spc_selectRoi;

global spc;

index = roipoly;

% siz = size(index);
% index = repmat (index(:), [1,spc.size(1)]);
% index = reshape(index, siz(1), siz(2), spc.size(1));
% index = permute(index, [3,1,2]);

spc.roipoly = index;

%spc.imageMod = reshape(full(index(:) .*  spc.imageMod(:)), spc.size(1), spc.size(2), spc.size(3));
%spc.project = reshape(sum(spc.imageMod, 1), spc.size(2), spc.size(3));
%spc.imageMod = sparse(image);


spc_redrawsetting;
