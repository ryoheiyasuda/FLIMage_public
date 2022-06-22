function imageMod = im2_smooth (image1, n)

s = floor((n+1)/2);
sz = size(image1);
xw = sz(1);
yw = sz(2);
imageMod = zeros(sz);

for i=1:n
    for j=1:n
        imageMod(s:xw-n+s, s:yw-n+s)= image1(i:xw+i-n, j:yw+j-n) + imageMod(s:xw-n+s, s:yw-n+s);
    end
end
