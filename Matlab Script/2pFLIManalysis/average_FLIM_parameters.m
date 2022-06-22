function output = average_FLIM_parameters(dname, nFrame, freq, pulses_to_average, threshold, threshold2)


%% Select the directory where you put your files
%Name of variables
%tau_m_Array
%n_tau_mArray: normalized tau_m, all data
%fraction2_Array
%n_fraction2_Array: normalized fraction2, all data
%int_int2_Array: intensity: all data
%n_int_Array: normalized int, all data
%pulseAverage_tau_m_Array: pulse average, all data.

%
%mean_XXXX: (like mean_n_tau_m)
%sem_XXXX: sem


if ~nargin
    dname = uigetdir(pwd);

    % setting
    % errbar1 = input('Error bar,true[1]/false[0]? [1]:');
    % if isempty(errbar1); errbar1 = 1; end
    % 
    % if errbar1 == 0
    %     put_errorbar = false;
    % elseif errbar1 == 1
    %     put_errorbar = true;
    % else
    %     put_errorbar = true;
    % end

    nFrame = input('Base line frames [30]:');
    if isempty(nFrame); nFrame = 30; end
    

    freq = input('Pulse intervals in frames [16]:');
    if isempty(freq); freq = 16; end

    pulses_to_average = input('Pulses to average [1:30]:');
    if isempty(pulses_to_average); pulses_to_average = 1:30; end

    
    % NRoi = input('# of Roi [1]:'); %Number of ROI
    % if isempty(NRoi); NRoi = 1; end

    threshold = input('remove data with baseline fluctuation (std of tau_m in Roi1) > [0.1] ns:');
    if isempty(threshold); threshold = 0.1; end

    threshold2 = input('remove data points exceed +/-[0.5] ns from the baseline:');
    if isempty(threshold2); threshold2 = 0.5; end
end
NRoi = 10;
baselineFrames = 1:nFrame;


%%
colorj = {'red', 'green', 'blue'};

a=dir([dname, '\*_ROI2.mat']);
nfile = length(a);

%% Initialize arrays
for j = 1:NRoi
    n_fraction2_Array{j} = [];
    fraction2_Array{j} = [];
    n_int_Array{j} = [];
    int_int2_Array{j} = [];
    tau_m_Array{j} = [];
    n_tau_m_Array{j} = [];
    fileCounter(j) = 0;
end

%% Read and put into arrays
for i=1:nfile
    load([dname, '\', a(i).name]); %Reading file
    fn{i} = a(i).name(1:end-4);
    evalc(['roiData = ', fn{i}, '.roiData']);
    time1 = roiData(1).time;
    
    noisy_data = 0;
    %%%%
    for j = 1:NRoi
        if j <= length(roiData) && (length(roiData(j).fraction2(:)) > 2)
            n_fraction2 = roiData(j).fraction2(:) - mean(roiData(j).fraction2(baselineFrames));
            n_int = roiData(j).int_int2(:) / mean(roiData(j).int_int2(baselineFrames));
            n_tau_m = roiData(j).tau_m(:) - mean(roiData(j).tau_m(baselineFrames));
            fraction2 = roiData(j).fraction2(:);
            int_int2 = roiData(j).int_int2(:);
            tau_m = roiData(j).tau_m(:);
            
            index_exceed = abs(n_tau_m) > threshold2;
            fraction2(index_exceed) = nan;
            int_int2(index_exceed) = nan;
            tau_m(index_exceed) = nan;
            
            %Reevaluate
            base1 = tau_m(baselineFrames);
            n_tau_m = tau_m - mean(base1(~isnan(base1)));
            base1 = int_int2(baselineFrames);
            n_int = int_int2 / mean(base1(~isnan(base1)));
            base1 = fraction2(baselineFrames);
            n_fraction2 = fraction2 - mean(base1(~isnan(base1)));
            
            noisy_data = std(n_tau_m(baselineFrames)) > threshold;
        else
%             nans = nan(1, length(roiData(1).fraction2));
%             n_fraction2 = nans(:);
%             n_int = nans(:);
%             n_tau_m = nans(:);
%             fraction2 = nans(:);
%             int_int2 = nans(:);
%             tau_m = nans(:);
            
            noisy_data = 1;
        end
        
        if noisy_data
            nans = nan(1, length(roiData(1).fraction2));
            n_fraction2 = nans(:);
            n_int = nans(:);
            n_tau_m = nans(:);
            fraction2 = nans(:);
            int_int2 = nans(:);
            tau_m = nans(:);
        end
            
        %if j == 1
            
            
           % if ~noisy_data
                fileCounter(j) = fileCounter(j)  + 1;
                fileNames{fileCounter(j), j} = a(i).name;
                
                if noisy_data
                    fileNames{fileCounter(j), j} = [a(i).name, '_removed'];
                end
          %  end
        %end
        
       % if ~noisy_data
            fraction2_Array{j} = [fraction2_Array{j}, fraction2(:)];
            n_fraction2_Array{j} = [n_fraction2_Array{j}, n_fraction2(:)];
            int_int2_Array{j} = [int_int2_Array{j}, int_int2(:)];
            n_int_Array{j} = [n_int_Array{j}, n_int(:)];
            tau_m_Array{j} = [tau_m_Array{j}, tau_m];
            n_tau_m_Array{j} = [n_tau_m_Array{j}, n_tau_m(:)];
     %   end

    end
end

for j = 1:NRoi
    data_exist = any(~isnan(fraction2_Array{j}(:)));
    if ~data_exist
        break;
    else     
    end
end

NRoi = j - 1;


%% Calculate mean and sem.
names = {'fraction2', 'n_fraction2', 'tau_m', 'n_tau_m', 'int_int2', 'n_int'};

for j = 1:NRoi
    for k = 1:length(names)
        evalc(['data1 = ', names{k},'_Array{j}']);
        evalc([names{k}, '_Array1{j} = data1']);
        %data1(isnan(data1)) = 0;
        index1 = (~isnan(data1));
        data1(~index1) = 0;
        mean1 = sum(data1, 2)./sum(index1, 2);
        mean2 = repmat(mean1, [1, size(data1, 2)]);
        mean2 (~index1) = 0;
        if nfile >= 3
            sem1 = sqrt(sum((data1-mean2).^2, 2)./sum(index1, 2)./(sum(index1, 2)-1));
        else
            sem1 = mean1*0;
        end
        
        evalc(['mean_', names{k}, '{j} = mean1']);
        evalc(['sem_', names{k}, '{j} = sem1']);
    end
end


%% Calculate spike triggered average
names2 = {'fraction2', 'n_fraction2', 'tau_m', 'n_tau_m'};

for j = 1:NRoi
    for k = 1:length(names2)
        
        evalc(['data2 = ', names2{k},'_Array{', num2str(j), '}']);
        
        data3 = zeros(freq, size(data2, 2), length(pulses_to_average));
        %data1(isnan(data1)) = 0;
        for pulse = pulses_to_average - 1
            data3(:,:,pulse+1) = data2(nFrame+1+pulse*freq : nFrame+(pulse+1)*freq, :);            
        end
        
        index3 = (~isnan(data3));
        data3(~index3) = 0;
        pulseAverage = sum(data3, 3)./sum(index3, 3);
        
        index4 = (~isnan(pulseAverage));
        pulseAverage1 = pulseAverage;
        pulseAverage(~index4) = 0;
        mean4 = sum(pulseAverage, 2) ./ sum(index4, 2);
        mean_array = repmat(mean4, [1, size(pulseAverage, 2)]);
        sem4 = sqrt(sum((pulseAverage-mean_array).^2, 2)./sum(index4, 2)./(sum(index4, 2)-1));
             
        evalc(['pulseAverage_', names2{k}, '_Array{j} = pulseAverage1']);
        evalc(['mean_pulseAverage_', names2{k}, '{j}= mean4']);
        evalc(['sem_pulseAverage_', names2{k}, '{j} = sem4']);
        %evalc(['sem_', names{k}, '{j} = sem1']);
    end
end

%%
%Saving!!
    for k = 1:length(names)
        evalc(['output.', names{k},'_Array = ', names{k},'_Array1']);
        evalc(['output.mean_', names{k}, ' = mean_', names{k}]);
        evalc(['output.sem_',  names{k}, ' = sem_', names{k}]);
    end
    
    for k=1:length(names2)
        evalc(['output.pulseAverage_', names2{k}, '_Array = pulseAverage_', names{k}, '_Array']);
        evalc(['output.mean_pulseAverage_', names2{k}, ' = mean_pulseAverage_', names{k}]);
        evalc(['output.sem_pulseAverage_', names2{k}, ' = sem_pulseAverage_', names{k}]);
    end
output.fileNames = fileNames;

ROI2_average = output;

save('ROI2_average', 'ROI2_average');

%% Graphing.
figure;
roi = 1;
for j = 1:fileCounter(roi)
    file_names{j} = fileNames{j, roi};
end
file_names{fileCounter(roi)+1} = 'Average';
time1 = time1/1000;

subplot(1,3,1);
hold on;
plot(time1, n_tau_m_Array{1});
plot(time1, mean_n_tau_m{1}, '-r', 'linewidth', 3);
legend(file_names, 'interpreter', 'none');
xlabel('Time (s)');
ylabel('delta tau_m (ns)');

subplot(1,3,2);
hold on;
plot(time1, n_int_Array{1});
plot(time1, mean_n_int{1}, '-r', 'linewidth', 3);
xlabel('Time (s)');
ylabel('normalized volume');

subplot(1,3,3);
hold on;
plot(pulseAverage_n_tau_m_Array{1});
plot(mean_pulseAverage_n_tau_m{1}, '-r', 'linewidth', 3);
xlabel('Frame');
ylabel('delta tau_m (ns)');


