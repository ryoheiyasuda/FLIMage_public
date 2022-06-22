function rgbimage = im2_ratio (hl, ll);

[fname,pname] = uigetfile('*.tif','Select tif-file');
image1 = double(imread([pname, fname], 1));
image2 = double(imread([pname, fname], 2));

[fname,pname] = uigetfile('*.tif','Select Background file');
back1 = double(imread([pname, fname], 1));
back2 = double(imread([pname, fname], 2));

b1 = mean(mean(back1, 1), 2);
b2 = mean(mean(back2, 1), 2);

disp(['backgroud = ', num2str(b1), ',', num2str(b2)]);

image1 = image1 - b1;
image2 = image2 - b2;

image1 = im2_smooth (image1, 3);
image2 = im2_smooth (image2, 3);

immax = max(max(image1, [], 1), [], 2);
bw = (image1 > 0);
sz = size(image1);
ratio = zeros(sz(1), sz(2));
ratio(bw) = image2(bw) ./ image1(bw);

ratiomax = max(max(ratio, [], 1), [], 2);


rgbimage = spc_im2rgb(ratio, [hl, ll]);

low = 0;
high = immax/10;

gray = (image1-low)/(high - low);
gray(gray > 1) = 1;
gray(gray < 0) = 0;
rgbimage(:,:,1)=rgbimage(:,:,1).*gray;
rgbimage(:,:,2)=rgbimage(:,:,2).*gray;
rgbimage(:,:,3)=rgbimage(:,:,3).*gray;

figure; image(rgbimage);
set(gca, 'XTick', []);
set(gca, 'YTick', []);
colorbar;
set(gca, 'XTick', []);
set(gca, 'YTick', []);