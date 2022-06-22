function rgbimage=spc_im2rgb(gray, limit);

gray = (gray-limit(1))/(limit(2)-limit(1));
gray = 1-gray;

part0 = (0 > gray);
part1 = (0 <= gray & gray < 1/3);
part2 = (1/3 <= gray & gray < 2/3);
part3 = (2/3 <= gray & gray <= 1);
part4 = (gray > 1);

blue = part1 + part2.*(-3*gray+2)+part0;
green = part1.*(3*gray) + part2 + part3.*(-3*gray+3);
red = part2.*(gray*3 - 1)+part3+part4;

rgbimage = [red(:), green(:), blue(:)];

siz = size(gray);
rgbimage = reshape(rgbimage, siz(1), siz(2), 3); 


%%%%%%%%%%%%%%Gray Scale
% gray = gray(:);
% gray(gray > 1)=1;
% gray(gray < 0)=0;
% gray = [gray, gray, gray];
% rgbimage = gray;
% rgbimage = reshape(rgbimage, siz(1), siz(2), 3); 
%%%%%%%%%%%%%%%%%