function spc_shiftSPC(shift)

global spc

sizR = size(spc.state.img.redMax);
siz = size(spc.imageMod);
if shift(1) > 0
    spc.imageMod(:, :, 1+shift(1):siz(2), :) = spc.imageMod(:, :, 1:siz(2)-shift(1), :);
    if (spc.state.img.redImg)
        spc.state.img.redMax(1+shift(1):sizR(1), :) = spc.state.img.redMax(1:sizR(1)-shift(1), :);
    end
else
    shift(1) = -shift(1);
    spc.imageMod(:, :, 1:siz(2)-shift(1), :) = spc.imageMod(:, :, 1+shift(1):siz(2), :);
    if (spc.state.img.redImg)
        spc.state.img.redMax(1:sizR(1)-shift(1), :) = spc.state.img.redMax(1+shift(1):sizR(1), :);
    end
end


if shift(2) > 0
    spc.imageMod(:, :, :, 1+shift(2):siz(3)) = spc.imageMod(:, :, :, 1:siz(3)-shift(2));
    if (spc.state.img.redImg)
        spc.state.img.redMax(:, 1+shift(2):sizR(2)) = spc.state.img.redMax(:, 1:sizR(2)-shift(2));
    end
else
    shift(2) = -shift(2);
    spc.imageMod(:, :, :, 1:siz(3)-shift(2)) = spc.imageMod(:, :, :, 1+shift(2):siz(3));
    if (spc.state.img.redImg)
        spc.state.img.redMax(:, 1:sizR(2)-shift(2)) = spc.state.img.redMax(:, 1+shift(2):sizR(2));
    end
end
