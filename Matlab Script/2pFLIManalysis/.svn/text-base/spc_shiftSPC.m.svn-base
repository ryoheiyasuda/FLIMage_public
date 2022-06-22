function spc_shiftSPC(shift)

global spc

% %imageMod = spc.imageMod;
% if shift(1) > 0
%     spc.imageMod(:, 1+shift(1):spc.size(2), :) = spc.imageMod(:, 1:spc.size(2)-shift(1), :);
% else
%     shift(1) = -shift(1);
%     spc.imageMod(:, 1:spc.size(2)-shift(1), :) = spc.imageMod(:, 1+shift(1):spc.size(2), :);
% end
% 
% 
% if shift(2) > 0
%     spc.imageMod(:, :, 1+shift(2):spc.size(3)) = spc.imageMod(:, :, 1:spc.size(3)-shift(2));
% else
%     shift(2) = -shift(2);
%     spc.imageMod(:, :, 1:spc.size(3)-shift(2), :) = spc.imageMod(:, :, 1+shift(2):spc.size(3));
% end
if shift(1) > 0
    spc.project(1+shift(1):spc.size(2), :) = spc.project(1:spc.size(2) - shift(1), :);
    spc.lifetimeMap(1+shift(1):spc.size(2), :)=spc.lifetimeMap(1:spc.size(2) - shift(1), :);
else
    shift(1) = -shift(1);
    spc.project(1:spc.size(2)-shift(1), :) = spc.project(1+shift(1):spc.size(2), :);
    spc.lifetimeMap(1:spc.size(2)-shift(1), :) = spc.lifetimeMap(1+shift(1):spc.size(2), :);
end

if shift(2) > 0
    spc.project(:, 1+shift(2):spc.size(2)) = spc.project(:, 1:spc.size(2) - shift(2));
    spc.lifetimeMap(:, 1+shift(2):spc.size(2))=spc.lifetimeMap(:, 1:spc.size(2) - shift(2));
else
    shift(2) = -shift(2);
    spc.project(:, 1:spc.size(2)-shift(2)) = spc.project(:, 1+shift(2):spc.size(2));
    spc.lifetimeMap(:, 1:spc.size(2)-shift(2)) = spc.lifetimeMap(:, 1+shift(2):spc.size(2));
end

spc_redrawSetting(0);