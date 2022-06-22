function spc_saveAsTiff (fname, append);
global spc;

if ~append
    disp([datestr(clock), 'Spc file =', fname]);
end

if ~nargin
    append = 0;
end

siz = spc.size;

saveimage = uint16(reshape(spc.imageMod, siz(1), siz(2)*siz(3)));

if append
    imwrite(saveimage, fname, 'WriteMode', 'append');
else
    spc_headerstr = spc_makeheaderstr;
    imwrite(saveimage, fname, 'WriteMode', 'overwrite', 'Description', spc_headerstr);
end

spc.filename = fname;


% image = reshape(full(spc.image), spc.size(1), spc.size(2), spc.size(3));
% 
% for i = 1:size(1)
%     saveimage = reshape(image(i, :, :),size(2),size(3));
%     if i == 1
%         imwrite(saveimage, fname, 'tiff', 'WriteMode', 'overwrite', 'Description', ['hohoho', 13 ...
%             , 'hohoho']);
%     else
%         imwrite(saveimage, fname, 'tiff', 'WriteMode', 'append');
%     end
% end