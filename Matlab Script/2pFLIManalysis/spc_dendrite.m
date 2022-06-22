function a = spc_dendrite(varargin)
global spc;
global gui;

%%%For x15 zoom. %%%
%%% 16 um / 128pixel = 1 um / 8 pixel 

va = varargin;
ns = length(va); %number of sample.

diff_lifetime = 0; %1;
green = 1;
smoothTWindow = 2; %Required when sample size is small.
errorbar1 = 0; %When the number of data is sufficient...
dist0 = 10;
dist1 = 40; %Pixel from the stimulated spine
nSpine = 3; %Stimulated spine, near spine (closer than dist1), distal spine
plotSpine = 2;
colorj = {'red','blue','green','cyan','magenta','black',[1,0.5,0],[0.5,0.5,0.5], 'yellow'};
dendStep = 16; %Dendritic segment from 1:dendStep, Dendstep+1:2*DendStep....
range = 1:7; %Normalization time.
xrange = 1:100;
pixelPerMicron = 8;
timepoint = [1,2,4,8];
%%%%Lend for spine%%%%%
str{1} = 'Stimulated spines';
str{2} = ['Adjacent spines(<', num2str(dist1/pixelPerMicron), ' um)'];
str{3} = ['Distal spines(>', num2str(dist1/pixelPerMicron), ' um)'];

FnStr = [];
for i=1:ns
    ta = va{i}.roiData(1).time;
    time1{i} = (ta - ta(i))*24*60;
    if i==1
        FnStr = [FnStr, va{i}.roiData(1).filename];
    else
        FnStr = [FnStr, ',', va{i}.roiData(1).filename];
    end
end

for i=1:ns
    siz = size(va{i}.Dendrite);
    sizT (i) = siz(1);    
    sizVcL(i) = siz(2);
end

%Find miniumu length for average.
TL = min(sizT);
VcL = min(sizVcL);

ave_dend = zeros(TL, VcL);
for i=1:nSpine
    spineMat{i} = [];
    intMat{i} = [];
end

t = zeros(1, TL);
for i=1:ns
    if diff_lifetime
        for j = 1:VcL
            ave_dend(:, j) = ave_dend(:, j) + (va{i}.Dendrite(1:TL, j) - mean(va{i}.Dendrite(range, j)))/ns;
        end
    else
        ave_dend = ave_dend + (va{i}.Dendrite(1:TL, 1:VcL))/ns;
    end
    if diff_lifetime
        lifetime = va{i}.roiData(1).fraction2(1:TL) - mean(va{i}.roiData(1).fraction2(range));
    else
        lifetime = va{i}.roiData(1).fraction2(1:TL);
    end
    spineMat{1} = [spineMat{1}; lifetime];
    if green
        intensity = va{i}.roiData(1).int_int2(1:TL)/mean(va{i}.roiData(1).int_int2(range));
    else
        intensity = va{i}.roiData(1).red_int2(1:TL)/mean(va{i}.roiData(1).red_int2(range));
    end
    intMat{1} = [intMat{1}; intensity];
    t = t + time1{i}(1:TL)/ns;
    for j=1:nSpine
        spineA{j} = [];
        intA{j} = [];
    end
    for j=2:length(va{i}.roiData)
        if length (va{i}.roiData(j).fraction2) >= TL
            ds = va{i}.roiData(j).position{1} - va{i}.roiData(1).position{1}; %Distance from the first spine
            ds_spine = sqrt(ds(1).^2+ds(2).^2);
            if green
                intensity = va{i}.roiData(j).int_int2(1:TL)/mean(va{i}.roiData(j).int_int2(range));
            else
                intensity = va{i}.roiData(j).red_int2(1:TL)/mean(va{i}.roiData(j).red_int2(range));
            end
            if diff_lifetime
                lifetime = va{i}.roiData(j).fraction2(1:TL) - mean(va{i}.roiData(j).fraction2(range));
            else
                lifetime = va{i}.roiData(j).fraction2(1:TL);
            end
%             if j == 0
%                     spineA{2} = [spineA{2}; lifetime];
%                     intA{2} = [intA{2}; intensity];
%             else
%                     spineA{3} = [spineA{3}; lifetime];
%                      intA{3} = [intA{3}; intensity]; 
%             end
            if ds_spine <= dist0
            elseif ds_spine <= dist1
                    spineA{2} = [spineA{2}; lifetime];
                    intA{2} = [intA{2}; intensity];
            else
                    spineA{3} = [spineA{3}; lifetime];
                    intA{3} = [intA{3}; intensity];                    
            end
        end %length check
    end % for ROI.
    for j=2:nSpine
        siz = size(spineA{j});
        if siz(1)>1
            spineB = mean(spineA{j});
            intB = mean(intA{j});
        elseif siz(1) == 1
            spineB = spineA{j};
            intB = intA{j};
        else
            spineB = nan(1,TL);
            intB = nan(1, TL);
        end
        if ~isnan(spineB)
            spineMat{j} = [spineMat{j}; spineB];
            intMat{j} = [intMat{j}; intB];
        end
    end
end


%%%%Filtering
t = t(smoothTWindow:end);
ave_dend = filter(ones(smoothTWindow, 1)/smoothTWindow, 1, ave_dend);
ave_dend = ave_dend(smoothTWindow:end, :);

%%%Averaging spines.
for i=1:nSpine
    ave_spine{i} = mean(spineMat{i}, 1);
    siz = size(spineMat{i});
    count_spine{i} = siz(1);
    sem_spine{i} = std(spineMat{i}, 0, 1)/sqrt(count_spine{i});
    ave_int{i} = mean(intMat{i}, 1);
    sem_int{i} = std(intMat{i}, 0, 1)/sqrt(count_spine{i});
end
%%%%Filtering spines
for i=1:nSpine
    ave_spine{i} = filter(ones(1,smoothTWindow)/smoothTWindow, 1, ave_spine{i});
    ave_int{i} = filter(ones(1,smoothTWindow)/smoothTWindow, 1, ave_int{i});
    
    ave_spine{i} = ave_spine{i}(smoothTWindow:end);
    sem_spine{i} = sem_spine{i}(smoothTWindow:end);
    ave_int{i} = ave_int{i}(smoothTWindow:end);
    sem_int{i} = sem_int{i}(smoothTWindow:end);
end

%%%%%%%%%%%%%%%

a.time = t;
a.dend = ave_dend;
a.spine = ave_spine;
a.sem_spine = sem_spine;
a.int = ave_int;
a.sem_int = sem_int;
a.count_spine = count_spine
a.filenames = FnStr;
a.intMat = intMat;
a.spineMat = spineMat;

figure; 
subplot(2,2,1);
hold on;
alldata = [];
for i=1:plotSpine
    if errorbar1
        plot1 = errorbar(a.time, a.spine{i}, a.sem_spine{i}, '-o', 'color', colorj{i});
    else
        if length(a.time) == length(a.spine{i})
            plot1 = plot(a.time, a.spine{i}, '-o', 'color', colorj{i});
        end
    end
    alldata = [alldata; a.spine{i}];
    if i==1
        set(plot1, 'Linewidth', 2);
    end
end
ax1 = gca;
title(['Spine: ', FnStr], 'Interpreter', 'none')

Legstr = [];
for i=1:plotSpine
    Leg = '                            '; %28 char
    Leg(1:length(str{i})) = str{i};
    Legstr = [Legstr; Leg];
end
Legend(Legstr);

subplot(2,2,2);
hold on;
if errorbar1
    plot1 = errorbar(a.time, a.spine{1}, a.sem_spine{1}, 'Linewidth', 2);
else
    if length(a.time) == length(a.spine{i})
        plot(a.time, a.spine{1}, '-o', 'color', colorj{1}, 'Linewidth', 2);
    end
end
j=2;
Legstr = ['Stimulated spines    '];
%length(Legstr)
dendF = round(dendStep/2);
adend = mean(a.dend(:,1:dendF-1), 2);
plot(a.time(:), adend(:), '-', 'color', colorj{j});
j=j+1;
str1 = sprintf('Dendrite %04.1f-%04.1f um', 0, dendF/pixelPerMicron);
%dif = length(Legstr) - length(str1)
Legstr = [Legstr; str1];
alldata = [alldata(:); adend(:)];
for i=dendF:dendStep:VcL-dendStep
    adend = mean(a.dend(:,i+1:i+dendStep), 2);
    plot(a.time(:), adend(:), '-', 'color', colorj{j});
    j=j+1;
    str1 = sprintf('Dendrite %04.1f-%04.1f um', i/pixelPerMicron, (i+dendStep)/pixelPerMicron);
    Legstr = [Legstr; str1];
    alldata = [alldata(:); adend(:)];
end
ylim1 = [min(alldata(:))-0.01, max(alldata(:))+0.01];
set(gca, 'Ylim', ylim1);
Legend(Legstr);
title(['Dendrite: ', FnStr], 'Interpreter', 'none')
set(ax1, 'Ylim', ylim1);

subplot(2,2,3);
hold on;
alldata = [];
for i=1:plotSpine
    if errorbar1
        errorbar(a.time, a.int{i}, a.sem_int{i}, '-o', 'color', colorj{i});
    else
        if length(a.time) == length(a.spine{i})
            plot1 = plot(a.time, a.int{i}, '-o', 'color', colorj{i});
        end
    end
    alldata = [alldata; a.int{i}];
    if i==1
        set(plot1, 'Linewidth', 2);
    end
end
ax1 = gca;
title(['Spine: ', FnStr], 'Interpreter', 'none')

Legstr = [];
for i=1:plotSpine
    Leg = '                            '; %28 char
    Leg(1:length(str{i})) = str{i};
    Legstr = [Legstr; Leg];
end
Legend(Legstr);
xlabel('Time(min)');
if green
    ylabel('Green fluorescence (normalized)');
else
    ylabel('Red fluorescence (normalized)');
end

subplot(2,2,4);
hold on;
baseD = mean(a.dend(range, xrange), 1);
x = [0, xrange/pixelPerMicron];
j = 1;
spine0 = mean(a.spine{1}(range))
baseD = [spine0, baseD];
plot(x, baseD, 'color', colorj{j});
Legstr = sprintf('%02d min', 0);
for i=timepoint
    j = j+1;
    dend1 = mean(a.dend(14+i, xrange), 1);
    spine1 = a.spine{1}(14+i);
    dend1 = [spine1, dend1];
    plot(x, dend1, 'color', colorj{j});
    Legstr = [Legstr; sprintf('%02d min', i)];
end
plot(0, spine0, 'o', 'MarkerFaceColor', colorj{j}, 'MarkerEdgeColor', colorj{1});
j = 2;
for i = [1,2,4,8]
    plot(0, a.spine{1}(14+i), 'o', 'MarkerFaceColor', colorj{j}, 'MarkerEdgeColor', colorj{j});
    j = j+1;
end
Legend(Legstr);
xlabel('Time(min)');
ylabel('Binding fraction');



