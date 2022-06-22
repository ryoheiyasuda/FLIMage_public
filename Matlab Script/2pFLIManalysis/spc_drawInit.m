function spc_drawInit
global spc;
global gui;

delete(findobj('Tag', 'spc_analysis'));
%Position files. 
% try
% 	load('flim_gui.mat');
% 	gui.spc.figure.positions = positions;
% 
% 
% end

% gui.spc.figure.projectImageData = zeros(128,128);
% gui.spc.figure.lifetimeMapImageData = zeros(128,128);
% gui.spc.figure.lifetimePlotData = zeros(64, 1);

%     try
%         fig1_pos = gui.spc.figure.positions.fig1;
%         fig2_pos = gui.spc.figure.positions.fig2;
%         fig3_pos = gui.spc.figure.positions.fig3;
%         fig4_pos = gui.spc.figure.positions.fig4;
%     catch

ScSize = get(0, 'ScreenSize');

fig1_pos = [ScSize(3)-400     400        360         300];
fig2_pos = [ScSize(3)-400     50         360         250];
fig3_pos = [ScSize(3)-800     50         360         300];
fig4_pos = [ScSize(3)-800     400        360         300];
%%%%%%%%%%%%%%%%
%Initialization
spc.switches.polyline_radius = 6;
spc.switches.roi = {};
gui.spc.proChannel = 1;
spc.currentChannel = 1;
gui.spc.scanChannel = 2;
spc.switches.noSPC = 0;
spc.switches.maxAve = 0;
spc.switches.redImg = 0;
spc.lifetimeMap = zeros(1,1);

for j = 1:3
    
    spc.fit(j).t_offset = 0;
    spc.fit(j).range = [5, 51];
    spc.fit(j).curve = zeros(spc.fit(j).range(2) - spc.fit(j).range(1) + 1, 1);
    spc.fit(j).beta0 = [1000, 16.42, 1000, 5.56, 7.24,  0.6199];
    spc.fit(j).fixtau = [0,0,0,0,0,0];
    spc.fit(j).lutlim = [10, 30];
    spc.fit(j).lifetime_limit = [1.4, 2.7];
%     spc.fit(j).fixtau1 = 0;
%     spc.fit(j).fixtau2 = 0;
%     spc.fit(j).fix_delta = 0;
%     spc.fit(j).fix_g = 0;
end
%spc.switches.polyline_radius = 6;

%%%%%%%%%%%%%%%%%
%     end
 %Fig1.
    gui.spc.figure.project = figure;
    set(gui.spc.figure.project, 'MenuBar', 'Figure');
    set(gui.spc.figure.project, 'Position', fig1_pos, 'name', 'Projection', 'Tag', 'spc_analysis');
    %set rio callback
%     set(gui.spc.figure.project, 'KeyPressFcn', 'spc_makeRoiA(-1)'); 
%     disp('note: you can press number keys 0-9 in the Project figure to add or edit ROIs');
    %menubar if you want ??
    %f = uimenu('Label','User');
    %uimenu(f, 'Label', 'makeNewRoi', 'Callback', 'spc_makeRoi');
    %uimenu(f, 'Label', 'binning', 'Callback', 'spc_binning');
    roi_context = uicontextmenu;
    gui.spc.figure.projectAxes = axes('Position', [0.1300    0.1100    0.6626    0.8150]);
	gui.spc.figure.projectImage = image(zeros(128,128), 'CDataMapping', 'scaled', 'UIContextMenu', roi_context);
    add_to_menu (roi_context);

    %set axes properties.
    set(gui.spc.figure.projectAxes, 'XTick', [], 'YTick', []);
    %set(gui.spc.figure.projectAxes, 'CLim', [1,spc.switches.threshold]);
    %draw default roi in Fig1.
    roi_pos = [1,1,32, 32];
    gui.spc.figure.roi = rectangle('position', roi_pos, 'ButtonDownFcn', 'spc_dragRoi', 'EdgeColor', [1,1,1]);
    %gui.spc.figure.ptojectColorbar = colorbar;
    colormap('gray');
    gui.spc.figure.projectColorbar = axes('Position', [0.82, 0.11, 0.05, 0.8150]);
    %%%%%%%%%%%%%%%%%%%%%%%%
    scale = 64:-1:1;
    %
    image(scale(:)); colormap('jet');
    set(gui.spc.figure.projectColorbar, 'XTick', [], 'box', 'off', 'Ytick', []);
    set(gui.spc.figure.projectColorbar, 'YAxisLocation', 'right', 'YTickLabel', []);
    gui.spc.figure.projectUpperlimit = uicontrol('Style', 'edit', 'String', '1', ...
                'Unit', 'normalized', 'Position', [0.88, 0.9, 0.1, 0.05], 'Callback', 'spc_redrawSetting(0)');
    gui.spc.figure.projectLowerlimit = uicontrol('Style', 'edit', 'String', '0', ...
                'Unit', 'normalized', 'Position', [0.88, 0.1, 0.1, 0.05], 'Callback', 'spc_redrawSetting(0)');
    gui.spc.figure.projectAuto = uicontrol ('Style', 'checkbox', 'Unit', 'normalized', ...
                'Position', [0.82, 0.02, 0.3, 0.05], 'String', 'Auto', 'Callback', ...
                'spc_redrawSetting', 'BackgroundColor', [0.8,0.8,0.8], 'Value', 1);
    gui.spc.figure.proChannel(1) = uicontrol ('Style', 'radiobutton', 'Unit', 'normalized', ...
                'Position', [0.02, 0.01, 0.2, 0.07], 'String', 'Ch1', 'value', 1, 'Callback', ...
                'global gui;spc.currentChannel=1;spc_switchChannel', 'BackgroundColor', [0.8,0.8,0.8]);
    gui.spc.figure.proChannel(2) = uicontrol ('Style', 'radiobutton', 'Unit', 'normalized', ...
                'Position', [0.2, 0.01, 0.2, 0.07], 'String', 'Ch2', 'value', 0, 'Callback', ...
                'global gui;spc.currentChannel=2;spc_switchChannel', 'BackgroundColor', [0.8,0.8,0.8]);            
    gui.spc.figure.proChannel(3) = uicontrol ('Style', 'radiobutton', 'Unit', 'normalized', ...
                'Position', [0.4, 0.01, 0.2, 0.07], 'String', 'Ch3', 'Callback', ...
                'global gui;spc.currentChannel=3;spc_switchChannel', 'BackgroundColor', [0.8,0.8,0.8]);          
   
%Fig2.
    gui.spc.figure.lifetime = figure;
    set(gui.spc.figure.lifetime, 'Position', fig2_pos, 'name', 'Lifetime', 'Tag', 'spc_analysis');
    gui.spc.figure.lifetimeAxes = axes;
    set(gui.spc.figure.lifetimeAxes, 'Position', [0.15, 0.37, 0.8, 0.57], 'XTick', []);
    gui.spc.figure.lifetimePlot = plot(1:64, zeros(64,1));
    hold on;
    gui.spc.figure.fitPlot = plot(1:64, zeros(64, 1));
    xlabel('');
    ylabel('Photon');
    gui.spc.figure.residual = axes;
    set(gui.spc.figure.residual, 'position', [0.15, 0.15, 0.8, 0.18]);
    gui.spc.figure.residualPlot = plot(1:64, zeros(64, 1));
    xlabel('Lifetime (ns)');
    ylabel('Residuals');

%Fig3.
    gui.spc.figure.lifetimeMap = figure;
    set(gui.spc.figure.lifetimeMap, 'MenuBar', 'Figure');
    set(gui.spc.figure.lifetimeMap, 'Position', fig3_pos, 'name', 'LifetimeMap', 'Tag', 'spc_analysis');
    gui.spc.figure.lifetimeMapAxes = axes('Position', [0.1300    0.1100    0.6626    0.8150]);
    roi_context3 = uicontextmenu;
    gui.spc.figure.lifetimeMapImage = image(zeros(128,128,3), 'CDataMapping', 'scaled', 'UIContextMenu', roi_context3);
    add_to_menu (roi_context3);
    % 	item1 = uimenu(roi_context3, 'Label', 'make new roi', 'Callback', 'spc_makeRoi');
%     item2 = uimenu(roi_context3, 'Label', 'select all', 'Callback', 'spc_selectAll');    
%     item7 = uimenu(roi_context3, 'Label', 'undo', 'Callback', 'spc_undo');
%     item8 = uimenu(roi_context3, 'Label', 'restrict in roi', 'Callback', 'spc_selectRoi');
% 	item9 = uimenu(roi_context3, 'Label', 'log-scale', 'Callback', 'spc_logscale');
%     item10 = uimenu(roi_context3, 'Label', 'poly-lines', 'Callback', 'spc_makepolyLines');
%     item11 = uimenu(roi_context3, 'Label', 'delete poly-lines', 'Callback', 'spc_deletepolyLines');
%     item11 = uimenu(roi_context3, 'Label', 'calculate poly-lines', 'Callback', 'spc_calcpolyLines(1);');
    
    set(gui.spc.figure.lifetimeMapAxes, 'XTick', [], 'YTick', []);
    gui.spc.figure.mapRoi=rectangle('position', roi_pos, 'EdgeColor', [1,1,1], 'ButtonDownFcn', 'spc_dragRoi');
    gui.spc.figure.lifetimeMapColorbar = axes('Position', [0.82, 0.11, 0.05, 0.8150]);
    scale = 64:-1:1;
    imRGB = spc_im2rgb(scale(:), [64, 1], 0);
    gui.spc.figure.lifetimeMapColorbarIm = image(imRGB);
    %colormap(jet);
    set(gui.spc.figure.lifetimeMapColorbar, 'XTick', []);
    set(gui.spc.figure.lifetimeMapColorbar, 'YAxisLocation', 'right', 'YTick', [], 'box', 'off');
    gui.spc.figure.lifetimeUpperlimit = uicontrol('Style', 'edit', 'String', '2.4', ...
                'Unit', 'normalized', 'Position', [0.88, 0.9, 0.1, 0.05], 'Callback', 'spc_redrawSetting(0)');
    gui.spc.figure.lifetimeLowerlimit = uicontrol('Style', 'edit', 'String', '1.7', ...
                'Unit', 'normalized', 'Position', [0.88, 0.1, 0.1, 0.05], 'Callback', 'spc_redrawSetting(0)');
    gui.spc.figure.LUTtext = uicontrol('Style', 'text', 'String', 'LUT', ...
                'Unit', 'normalized', 'Position', [0.88, 0.6, 0.1, 0.05], 'BackgroundColor', [0.8,0.8,0.8]);
    gui.spc.figure.LutLowerlimit = uicontrol('Style', 'edit', 'String', '10', ...
                'Unit', 'normalized', 'Position', [0.88, 0.5, 0.1, 0.05], 'Callback', 'spc_redrawSetting(0)');
    gui.spc.figure.LutUpperlimit = uicontrol('Style', 'edit', 'String', '30', ...
                'Unit', 'normalized', 'Position', [0.88, 0.55, 0.1, 0.05], 'Callback', 'spc_redrawSetting(0)');
    gui.spc.figure.drawPopulation = uicontrol ('Style', 'checkbox', 'Unit', 'normalized', ...
                'Position', [0.05, 0.02, 0.3, 0.05], 'String', 'Draw population', 'Callback', ...
                'spc_redrawSetting(0)', 'BackgroundColor', [0.8,0.8,0.8]);
    gui.spc.figure.filterText = uicontrol ('Style', 'text', 'Unit', 'normalized', ...
                'Position', [0.55, 0.02, 0.3, 0.05], 'String', 'Smooth', 'Callback', ...
                'spc_redrawSetting(1)', 'BackgroundColor', [0.8,0.8,0.8]);            
    gui.spc.figure.filter = uicontrol ('Style', 'edit', 'Unit', 'normalized', ...
                'Position', [0.75, 0.02, 0.1, 0.05], 'String', '1', 'Callback', ...
                'spc_redrawSetting(1)', 'BackgroundColor', [1,1,1]);
 
    gui.spc.figure.FLIMchannel(1) = uicontrol ('Style', 'radiobutton', 'Unit', 'normalized', ...
                'Position', [0.02, 0.01, 0.2, 0.07], 'String', 'Ch1', 'Callback', ...
                'global gui;spc.currentChannel=1;spc_switchChannel', 'BackgroundColor', [0.8,0.8,0.8]);
    gui.spc.figure.FLIMchannel(2) = uicontrol ('Style', 'radiobutton', 'Unit', 'normalized', ...
                'Position', [0.2, 0.01, 0.2, 0.07], 'String', 'Ch2', 'value', 1, 'Callback', ...
                'global gui;spc.currentChannel=2;spc_switchChannel', 'BackgroundColor', [0.8,0.8,0.8]);            
    gui.spc.figure.FLIMchannel(3) = uicontrol ('Style', 'radiobutton', 'Unit', 'normalized', ...
                'Position', [0.4, 0.01, 0.2, 0.07], 'String', 'Ch3', 'Callback', ...
                'global gui;spc.currentChannel=3;spc_switchChannel', 'BackgroundColor', [0.8,0.8,0.8]);        
            
%Fig4.
    gui.spc.figure.scanImgF = figure;
    gui.spc.figure.scanImgA = axes('Position', [0.1300    0.1100    0.6626    0.8150]);
    set(gui.spc.figure.scanImgF, 'MenuBar', 'Figure');
    set(gui.spc.figure.scanImgF, 'Position', fig4_pos, 'name', 'ScanImage', 'Tag', 'spc_analysis');
	gui.spc.figure.scanImg = image(zeros(128,128), 'CDataMapping', 'scaled');
    %set axes properties.
    gui.spc.figure.scanImgA = gca;
    set(gui.spc.figure.scanImgA, 'XTick', [], 'YTick', []);

    gui.spc.figure.redColorbar = axes('Position', [0.82, 0.11, 0.05, 0.8150]);
    scale = 64:-1:1;
    image(scale(:));
    set(gui.spc.figure.redColorbar, 'XTickLabel', []);
    set(gui.spc.figure.redColorbar, 'YAxisLocation', 'right', 'YTickLabel', []);
    gui.spc.figure.redUpperlimit = uicontrol('Style', 'edit', 'String', '1', ...
                'Unit', 'normalized', 'Position', [0.88, 0.9, 0.1, 0.05], 'Callback', 'spc_redrawSetting(0)');
    gui.spc.figure.redLowerlimit = uicontrol('Style', 'edit', 'String', '0', ...
                'Unit', 'normalized', 'Position', [0.88, 0.1, 0.1, 0.05], 'Callback', 'spc_redrawSetting(0)');
    gui.spc.figure.redAuto = uicontrol ('Style', 'checkbox', 'Unit', 'normalized', ...
                'Position', [0.82, 0.02, 0.3, 0.05], 'String', 'Auto', 'Callback', ...
                'spc_redrawSetting(0)', 'BackgroundColor', [0.8,0.8,0.8], 'Value', 1);
    gui.spc.figure.channel(1) = uicontrol ('Style', 'radiobutton', 'Unit', 'normalized', ...
                'Position', [0.02, 0.01, 0.2, 0.07], 'String', 'Ch1', 'Callback', ...
                'global gui;gui.spc.scanChannel=1;spc_redrawSetting(0)', 'BackgroundColor', [0.8,0.8,0.8]);
    gui.spc.figure.channel(2) = uicontrol ('Style', 'radiobutton', 'Unit', 'normalized', ...
                'Position', [0.2, 0.01, 0.2, 0.07], 'String', 'Ch2', 'value', 1, 'Callback', ...
                'global gui;gui.spc.scanChannel=2;spc_redrawSetting(0)', 'BackgroundColor', [0.8,0.8,0.8]);            
    gui.spc.figure.channel(3) = uicontrol ('Style', 'radiobutton', 'Unit', 'normalized', ...
                'Position', [0.4, 0.01, 0.2, 0.07], 'String', 'Ch3', 'Callback', ...
                'global gui;gui.spc.scanChannel=3;spc_redrawSetting(0)', 'BackgroundColor', [0.8,0.8,0.8]);
    colormap('gray'); 
    gui.spc.scanChannel = 2;
spc_main;
 


%backupfiles.
%try
%load('spc_backup.mat');
% if exist(spc_filename) == 2
%     spc_openCurves(spc_filename);
% end
try
    spc_selectAll;
end


if isfield(spc, 'imageMod')
    try
	    spc_redrawSetting(1);
    catch
        disp('Error: no images produced (function spc_drawInit)');
    end
end

% try
% 	if length(spc.lifetime) > 3
%           spc_prfdefault;
%           spc.fit.fixtau1 = 1; 
%           spc.fit.fixtau2 = 1;
%           spc_dispbeta;
%           set(gui.spc.spc_main.beta2, 'String', '2.59');
%           set(gui.spc.spc_main.beta4, 'String', '1.4');
%           set(gui.spc.spc_main.beta1, 'String', '1000');
%           set(gui.spc.spc_main.beta3, 'String', '1000');
%           spc_fitWithDoubleExp;
%           spc_dispbeta;
% 	end
% end
%spc_selectAll;

function add_to_menu(roi_context)
global spc
	item1 = uimenu(roi_context, 'Label', 'make new roi', 'Callback', 'spc_makeRoi');
    item2 = uimenu(roi_context, 'Label', 'select all', 'Callback', 'spc_selectAll');    
	item3 = uimenu(roi_context, 'Label', 'binning 2x2', 'Callback', 'spc_binning');  
    item7 = uimenu(roi_context, 'Label', 'undo', 'Callback', 'spc_undo');
    item8 = uimenu(roi_context, 'Label', 'restrict in roi', 'Callback', 'spc_selectRoi');
	item9 = uimenu(roi_context, 'Label', 'log-scale', 'Callback', 'spc_logscale');
    item10 = uimenu(roi_context, 'Label', 'poly-lines', 'Callback', 'spc_makepolyLines');
    item11 = uimenu(roi_context, 'Label', 'delete poly-lines', 'Callback', 'spc_deletepolyLines');
    item12 = uimenu(roi_context, 'Label', 'calculate poly-lines', 'Callback', 'spc_calcpolyLines(1)');
    dlgstr = 'a = inputdlg(''Input'', '''', 1, {num2str(spc.switches.polyline_radius)});'; 
    dlgstr = [dlgstr, 'global spc; spc.switches.polyline_radius=str2num(a{1});spc_polyDrag(0);'];
    item13 = uimenu(roi_context, 'Label', 'set raidius of poly-line ROIs', 'Callback', dlgstr);
