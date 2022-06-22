function rgbimage=spc_im2rgb(gray, limit, colormap1)

if nargin < 3
    colormap1 = 1;
end
%0: gray scale, 1: Rainbow, 2: Red and blue, 3: modified rainbow, 4: modified rainbow 2

gray = (gray-limit(1))/(limit(2)-limit(1));
gray = 1-gray;
gray(1 < gray) = 1;
gray(0 > gray) = 0;


switch colormap1
    case {1}
            part1 = (0 <= gray & gray < 1/3);
            part2 = (1/3 <= gray & gray < 2/3);
            part3 = (2/3 <= gray & gray <= 1);
            blue = part1 + part2.*(-3*gray+2);
            green = part1.*(3*gray) + part2 + part3.*(-3*gray+3);
            red = part2.*(gray*3 - 1)+part3;
    case {2}
        %%%Red and Blue scheme%%%%%%%%%%%%%%%%
            part1 = (0 <= gray & gray < 1/2);
            part2 = (1/2 <= gray & gray <= 1);

            blue = part1 + part2.*(-2*gray+2);
            green = part1.*(2*gray) + part2.*(-2*gray+2);
            red = part1.*(2*gray) + part2;
    case {3}       
            gray = gray*0.85+0.15;
            part1 = (0 <= gray & gray < 1/3);
            part2 = (1/3 <= gray & gray < 2/3);
            part3 = (2/3 <= gray & gray <= 1);
            blue = part1 + part2.*(-3*gray+2);
            green = part1.*(3*gray) + part2 + part3.*(-3*gray+3);
            red = part2.*(gray*3 - 1)+part3;
    case {4}
        %%%%Modified jet scheme3%%%%%%%%%%%%%%%%
            gray = gray;
            part1 = (0 <= gray & gray < 1/4);
            part2 = (1/4 <= gray & gray < 2/4);
            part3 = (2/4 <= gray & gray < 3/4);
            part4 = (3/4 <= gray & gray <= 1);
            blue = part1 + part2.*(-4*gray + 2) + part4.*(4*gray-3);
            green = part1.*(4*gray) + part2 + part3.*(-4*gray+3)+part4.*(4*gray-3);
            red = part2.*(gray*4 - 1)+part3+part4;
    case {0}
        %%%%Gray scale
            gray(gray > 1)=1;
            gray(gray < 0)=0;
            blue = gray;
            green = gray;
            red = gray;
    otherwise
end

%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
rgbimage = [red(:), green(:), blue(:)];

siz = size(gray);
rgbimage = reshape(rgbimage, siz(1), siz(2), 3); 

%%%%%%%%%%%%%%%%%