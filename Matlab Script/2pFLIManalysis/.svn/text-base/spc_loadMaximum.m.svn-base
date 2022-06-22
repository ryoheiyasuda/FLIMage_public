function spc_loadMaximum(s_start, s_end);

global spc;

[fname,pname] = uigetfile('*.tif','Select spc-file');
cd (pname);
filestr = [pname, fname];

finfo = imfinfo (fname);
header = finfo(1).ImageDescription;

if findstr(header, 'spc')
    evalc(header);
    image = double(imread(fname));
else
    disp('This is not a spc file !!');
end

spc.filename = fname;

frames = length(imfinfo(fname));

if ~nargin
    s_start = 1;
    s_end = frames;
end

if s_end > frames
    disp('error: slice end has to be smaller than number of frames !!');
    s_end = frames;
end

for i = s_start:s_end
    disp(['Reading frame ', num2str(i)]);
    image = double(imread(fname, i));
    image = reshape(image, spc.size);
    project = reshape(sum(image, 1), spc.size(2), spc.size(3));
    if (i == s_start)
        image_max = image;
        project_max = project;
    else
        index = (project_max >= project);
        siz = size(index);
        
        index = repmat (index(:), [1,spc.size(1)]);
		index = reshape(index, siz(1), siz(2), spc.size(1));
		index = permute(index, [3,1,2]);
        index = index(:);
        
		image_max = index .*  image_max(:) + (1-index) .* image(:);
        
        image_max = reshape(image_max, spc.size(1), spc.size(2), spc.size(3));
        project_max = reshape(sum(image_max, 1), spc.size(2), spc.size(3));
    end    
end

 spc.project = reshape(sum(image_max, 1), spc.size(2), spc.size(3));
 spc.image = sparse(reshape(image_max, spc.size(1), spc.size(2)*spc.size(3)));
 spc.imageMod = spc.image;

spc_redrawsetting;