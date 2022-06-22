function [spc_prop, ratio2] = spc_ratio (a, lowerlim, upperlim);

global spc;
global gui;

a = a / spc.datainfo.psPerUnit * 1000;
range = spc.fit.range;
lifetime = spc.lifetime(range(1):1:range(2));
lenx = length(lifetime);
x = [1:lenx];
%thresh = a - range(1) + 1;
handles = gui.spc.spc_main;
tau1=str2num(get(handles.beta2, 'String'));
tau2=str2num(get(handles.beta4, 'String'));
tau1 = tau1 / spc.datainfo.psPerUnit * 1000;
tau2 = tau2 / spc.datainfo.psPerUnit * 1000;


error = 0; try; spc.fit.prf(3); catch; spc_prfdefault; end
%spc_selectAll;
%spc_fitWithDoubleExp;
%offset = spc.fit.beta0(5);
offset = 0;
[value,pos]=max(spc.fit.prf);
thresh = round(offset + pos + a - range(1) + 1);
Ndiv = 200;
for i=1:Ndiv+1
    pop1 = (i-1)/Ndiv;
    pop2 = 1 - pop1;
    beta0 = [pop1, tau1, pop2, tau2, offset];
    fit = spc_reconv2(beta0, x);
    ratio(i) = sum(fit(1:thresh))/sum(fit(1:lenx));
    ratio1(i) = sum(fit(thresh:lenx))/sum(fit(1:lenx));
    ratio2(i) = ratio1(i)/ratio(i);
end
ratio2 = ratio2(:);
ratiomax = max(ratio2);
ratiomin = min(ratio2);

ratio2 = round((ratio2 - ratiomin)/(ratiomax - ratiomin)*100);
rindex = zeros(101, 1);
for i=1:101
    rindex(i) = (mean(find (ratio2 == i-1))-1)/Ndiv * 100;
end

imageMod = reshape(spc.imageMod, spc.size(1), spc.size(2), spc.size(3));

sum1 = sum(imageMod(range(1):thresh+range(1)-1,:,:), 1);
sum2 = sum(imageMod(thresh+range(1)-1:range(2),:,:), 1);
sum1 = sum1(:);
sum2 = sum2(:);

spc_ratio = sum1.*0;
bw = (sum1 > 0);

spc_ratio(bw) = sum2(bw)./sum1(bw);

x = [0:0.01:1];
bw = (spc.project > spc.switches.lutlim(1));
spc_ratio(bw) = round((spc_ratio(bw) - ratiomin) /(ratiomax - ratiomin) * 100);
spc_ratio(spc_ratio > 100) = 100;
spc_ratio (spc_ratio < 0) = 0;

size = spc.size;
spc_prop  = zeros(size(2), size(3));
spc_prop = spc_prop(:);

spc_prop(bw) = (100 - rindex(spc_ratio(bw)+1))/100;
%spc_prop (bw) = 1 - interp1(ratio2, x, spc_ratio(bw), 'nearest');

spc_prop = reshape(spc_prop, size(2), size(3));
%spc_prop = spc_ratio;
rgbimage =spc_im2rgb(spc_prop, [upperlim, lowerlim]);

lut = spc.switches.lutlim;
gray = (spc.project- lut(1))/(lut(2)-lut(1));
gray(gray > 1) = 1;
gray(gray < 0) = 0;

rgbimage(:,:,1)=rgbimage(:,:,1).*gray;
rgbimage(:,:,2)=rgbimage(:,:,2).*gray;
rgbimage(:,:,3)=rgbimage(:,:,3).*gray;

spc_prop = rgbimage;