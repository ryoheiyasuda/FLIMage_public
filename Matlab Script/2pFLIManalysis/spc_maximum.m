function spc_maximum (header, startN, endN);
global spc
global spcs
%Usage spc_maximum('header name', start, end)


[fname,pname] = uigetfile('*.sdt','Select sdt path');

i = startN;
stri = num2str(i);
while (length(stri) < 3)
     stri = ['0', stri];
end

fname = [header, stri, '.sdt'];

if exist([pname, fname]) == 2
        spc_readdata([pname,fname]);
end
    
for (i = startN+1:1:endN)
    
    spc1 = spc;
    
    stri = num2str(i);
    while (length(stri) < 3)
         stri = ['0', stri];
    end
    fname = [header, stri, '.sdt'];
    disp(fname);

    if exist([pname, fname]) == 2
        spc_readdata([pname,fname]);
    end
    index = (spc.project <= spc1.project);
	siz = size(index);
	
	index = repmat (index(:), [1,spc.size(1)]);
	index = reshape(index, siz(1), siz(2), spc.size(1));
	index = permute(index, [3,1,2]);
    index = index(:);
    
    image1 = full(spc1.image);
    image0 = full(spc.image);
	image = index .*  image1(:) + (1-index) .* image0(:);
    
    image = reshape(image, spc.size(1), spc.size(2), spc.size(3));
    spc.project = reshape(sum(image, 1), spc.size(2), spc.size(3));
 
    spc.image = sparse(reshape(image, spc.size(1), spc.size(2)*spc.size(3)));
    spc.imageMod = spc.image;
    %spc_drawSetting(1);
end

spc_drawSetting(1);

eval('spcs.nImages = spcs.nImages + 1;', 'spcs.nImages = 1;');
eval(['spcs.spc', num2str(spcs.nImages), '=spc;'], ''); 
spcs.current = spcs.nImages;