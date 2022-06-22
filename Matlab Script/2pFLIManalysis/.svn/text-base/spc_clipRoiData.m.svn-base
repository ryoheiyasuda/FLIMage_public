function va1 = spc_clipRoiData(va, t)
global spc;
global gui;


va1.Dendrite = va.Dendrite(t, :);

for tt = 1:length(t)
    va1.polyLines{tt} = va.polyLines{t(tt)};
end

for i=1:length(va.roiData)
    names = fieldnames(va.roiData(i));
    for j=1:length(names)
        if strcmp(names{j}, 'filename')
            va1.roiData(i).filename = va.roiData(i).filename;
        elseif strcmp(names{j}, 'positions')
            evalc(['va1.roiData(i).', names(j), '= va.roiData(i).', names(j), '{t}']);
        else
            evalc(['va1.roiData(i).', names{j}, '= va.roiData(i).', names{j}, '(t)']);
        end
    end
end

names = fieldnames(va.bgData);
for j=1:length(names)
    if strcmp(names{j}, 'filename')
    elseif strcmp(names{j}, 'positions')
        evalc(['va1.bgData.', names(j), '= va.bgData.', names(j), '{t}']);
    else
        evalc(['va1.bgData.', names{j}, '= va.bgData.', names{j}, '(t)']);
    end
end
