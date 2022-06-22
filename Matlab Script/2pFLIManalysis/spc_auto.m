function spc_auto(flag)

global spc;
global state;
global gui;

% pos_max2 = str2num(get(gui.spc.spc_main.F_offset, 'String'));
% if pos_max2 == 0 || isnan (pos_max2)
%     pos_max2 = 1.0;
% end

if nargin == 0
    flag = 0;
end

spc_updateMainStrings;
name_start=strfind(spc.filename, filesep);
name_start=name_start(end)+1;

if strfind(spc.filename, 'max')
    baseName_end=length(spc.filename)-11;
elseif strfind(spc.filename, 'glu')
    baseName_end=length(spc.filename)-11;
else
    baseName_end=length(spc.filename)-7;
end
baseName=spc.filename(name_start:baseName_end);
graph_savePath=spc.filename(1:name_start-5);
spc_savePath=spc.filename(1:name_start-1);

if ~isnan(str2double(baseName(1)))
    baseName = ['A_', baseName];
end

eval(['global  ', baseName]);
cd (spc_savePath);

if exist([baseName, '.mat'], 'file')
    load([baseName, '.mat'], baseName);
    evalc(['tmp = ', baseName]);
    try
        str1 = tmp(1).textbox;
    catch
        str1 = '';
    end
else
    str1 = '';
    %disp('no such file');
    %button = questdlg('Do you want to make new files?');
    %return;
end

graphfile = [graph_savePath, baseName, '_tau_all.fig'];

if isfield(gui.spc, 'online')
     if ishandle(gui.spc.online)
        if strcmp(get(gui.spc.online, 'Tag'), 'Online_fig')
             fname_graph = get(gui.spc.online, 'filename');
             [G_pathstr,G_name,G_ext] = fileparts(fname_graph);
             if strcmp(G_name, [baseName, '_tau_all'])
                saveas(gui.spc.online, graphfile);
             end
             %close(gui.spc.online);
        else
             mkfigure (str1);
        end
     else
         mkfigure(str1);
     end
 else
      mkfigure (str1);
 end


evalc(['tmp = ', baseName]);

nChannels = spc.datainfo.scan_rx;
if length(tmp) == nChannels
    a = tmp;
else
    a = [];
end

try
    a(1).textbox = get(gui.spc.textbox, 'String');
end

if flag == 0
    fc = state.files.fileCounter;
else
    fc = str2double(spc.filename(baseName_end+1:baseName_end+3));
end


nChannels = spc.nChannels;

saveChannel = spc.currentChannel;
for channelN = 1:nChannels
    gui.spc.proChannel = channelN;
    spc.currentChannel = channelN;
    if flag
        spc_redrawSetting(1);
    else
        spc_redrawSetting(0, 1);
    end
    
    if get(gui.spc.spc_main.fit_eachtime, 'Value')
        try
            betahat=spc_fitexp2gauss;
            spc_redrawSetting(1);
            fit_error = 0;
        catch
            fit_error = 1;
        end
    else
        fit_error = 1;
    end
    
    pause(0.1);
    
    p1 = spc.fit(spc.currentChannel).beta0(1);
    p2 = spc.fit(spc.currentChannel).beta0(3);
    tau1 = spc.fit(spc.currentChannel).beta0(2)*spc.datainfo.psPerUnit/1000;
    tau2 = spc.fit(spc.currentChannel).beta0(4)*spc.datainfo.psPerUnit/1000;
    f = p2/(p1+p2);

    a(channelN).fraction(fc) = f;
    a(channelN).tau1(fc) = tau1;
    a(channelN).tau2(fc) = tau2;
    a(channelN).tau_m(fc) = sum(spc.lifetime)/max(spc.lifetime)*spc.datainfo.psPerUnit/1000;
    range = spc.fit(gui.spc.proChannel).range;
    t1 = (1:range(2)-range(1)+1); t1 = t1(:);
    lifetime = spc.lifetime(range(1):range(2)); 
    lifetime = lifetime(:);
    
    %pos_max2 = spc.fit(gui.spc.proChannel).beta0(5)*spc.datainfo.psPerUnit/1000;
    %a(channelN).tau_m2(fc) = sum(lifetime.*t1)/sum(lifetime)*spc.datainfo.psPerUnit/1000 - pos_max2;
   
    %a(channelN).time(fc) = datenum(spc.state.internal.triggerTimeString); % 
    a(channelN).time(fc) = datenum(spc.datainfo.triggerTime); %datenum([spc.datainfo.date, ',', spc.datainfo.time]);

    tauD = spc.fit(spc.currentChannel).beta0(2)*spc.datainfo.psPerUnit/1000;
    tauAD = spc.fit(spc.currentChannel).beta0(4)*spc.datainfo.psPerUnit/1000;
    
    %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
    %a(channelN).fraction2(fc) = spc_getFraction(sum(lifetime.*t1)/sum(lifetime));
    %pop2 = a(channelN).fraction2(fc);
    %pop1 = 1 - pop2;
    %a(channelN).tau_m2(fc) = (tauD*tauD*pop1+tauAD*tauAD*pop2)/(tauD*pop1 + tauAD*pop2);
    
    a(channelN).fraction2(fc) = tauD*(tauD-a(channelN).tau_m2(fc))/(tauD-tauAD) / (tauD + tauAD -a(channelN).tau_m2(fc));
    a(channelN).tau_m2(fc) = sum(lifetime.*t1)/sum(lifetime)*spc.datainfo.psPerUnit/1000 - spc.fit(gui.spc.proChannel).t_offset;
    
    %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
    a(channelN).time(a(channelN).time<7.3e5) = a(channelN).time(fc);
    a(channelN).fraction(a(channelN).fraction > 1) = 1;
    a(channelN).fraction(a(channelN).fraction < -0.2) = -0.2;
    a(channelN).fraction2(a(channelN).fraction2 > 1) = 1;
    a(channelN).fraction2(a(channelN).fraction2 < -0.2) = -0.2;

    if flag == 0
        a(channelN).time(state.files.fileCounter) = datenum(now);
    end
    %a(channelN).time = a(channelN).time - min (a(channelN).time);
    if isfield(spc, 'scanHeader')
        if isfield(spc.scanHeader.motor, 'position')
            a(channelN).position(fc) = spc.scanHeader.motor.position;
        else    
            a(channelN).position(fc) = state.motor.position;
            spc.scanHeader.motor.position = state.motor.position;
        end
    end
end

gui.spc.proChannel = saveChannel;
spc.currentChannel = saveChannel;
spc_redrawSetting(1);

figure (gui.spc.online);
    subplot(2,3,3);
    cstr = {'red', 'green', 'blue', 'green', 'magenta', 'cyan', 'black'};
    
    fl1 = 0;
    for channelN = 1:nChannels
            ptime = (a(channelN).time - min(a(channelN).time))*60*24 ;
            pfrac = a(channelN).fraction;
            if length(ptime) > 0
                plot(ptime, pfrac, '-o', 'Color', cstr{channelN});
                if fl1 == 0; hold on; fl1 = 1; end
            end
    end
    hold off;
   ylabel('Fraction (tau2): Fitting'); 
   xlabel ('Time (min)');

    subplot(2,3,[1,2]);
   
    fl1 = 0;
    for channelN = 1:nChannels
            ptime = (a(channelN).time- min(a(channelN).time))*60*24 ;
            pfrac2 = a(channelN).fraction2;
            if length(ptime) > 0
                plot(ptime, pfrac2, '-o', 'Color', cstr{channelN});
                if fl1 == 0;  hold on; fl1 = 1; end
            end
    end
    hold off;
ylabel('Fraction (tau2)');
xlabel ('Time (min)');

subplot(2,3,[4,5]);
    
    fl1 = 0;
    for channelN = 1:nChannels
            ptime = (a(channelN).time - min(a(channelN).time))*60*24 ;
            ptau = a(channelN).tau_m2;
            ptau(ptau < 0.1) = 2;
            if length(ptime) > 0
                plot(ptime, ptau, '-o', 'Color', cstr{channelN});
                if fl1 == 0; hold on; fl1 = 1; end
            end
    end
    hold off;    
    
    
ylabel('mean photon emission time');
xlabel ('Time (min)');




%ylim([0, 0.5])
eval([baseName, '= a;']);
save([spc_savePath, baseName, '.mat'], baseName);
%saveas(gui.spc.online, graphfile);

if isfield(state, 'acq')
    try
        save_autoSetting;
    catch
        disp('Error in saving Autosetting');
    end
end


function mkfigure (str1)
global gui;
gui.spc.online = figure;
gui.spc.textbox = uicontrol('Style', 'edit', 'Unit', 'Normalized', ...
  'Position',[0.6607 0.1167 0.2893 0.3357], 'Max', 100, 'Min', 1, 'String', str1);
set(gui.spc.online, 'Tag', 'Online_fig', 'CloseRequestFcn', 'spc_autoSave');
set(gui.spc.textbox, 'BackgroundColor', 'White');
set(gui.spc.textbox, 'HorizontalAlignment', 'left')