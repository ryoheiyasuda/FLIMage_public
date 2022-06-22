function spc_drawSetting (flag);

global spc;
global spcs;
global gui;

    gui.spc.figure.project = 101;
    gui.spc.figure.lifetime = 102;
    gui.spc.figure.lifetimeMap = 103;
    
%spc.lifetime = spc.page1.lifetime;
if spc.switches.imagemode == 0
    figure(gui.spc.figure.lifetime);
    %set(gui.spc.figure.lifetime, 'Position', [410, 50, 360, 300], 'name', 'Lifetime');
    eval('axes(gui.spc.figure.lifetimeaxes);', 'gui.spc.figure.lifetimeaxes = axes;');
    set(gui.spc.figure.lifetimeaxes, 'Position', [0.15, 0.37, 0.8, 0.57], 'xtick', []);
    ylabel('Photon');
    eval('axes(gui.spc.figure.residual);', 'gui.spc.figure.residual = axes');
    set(gui.spc.figure.residual, 'position', [0.15, 0.15, 0.8, 0.18]);

%%%%%%%%%%%%%%
else
    eval('roi_pos = get(gui.spc.figure.roi, ''Position'');', 'roi_pos = round([spc.size(2)/5,spc.size(3)/5,spc.size(2)/2,spc.size(3)/2]);'); 

 %Fig1.
    figure(gui.spc.figure.project);
    set(gui.spc.figure.project, 'MenuBar', 'none');
    set(gui.spc.figure.project, 'Position', [20,50,360,300], 'name', 'Projection');
    %menubar if you want ??
    %f = uimenu('Label','User');
    %uimenu(f, 'Label', 'makeNewRoi', 'Callback', 'spc_makeRoi');
    %uimenu(f, 'Label', 'binning', 'Callback', 'spc_binning');
    roi_context = uicontextmenu;
	image(spc.project, 'CDataMapping', 'scaled', 'UIContextMenu', roi_context);
	item1 = uimenu(roi_context, 'Label', 'make new roi', 'Callback', 'spc_makeRoi');
    item2 = uimenu(roi_context, 'Label', 'select all', 'Callback', 'spc_selectAll');    
	item3 = uimenu(roi_context, 'Label', 'binning 2x2', 'Callback', 'spc_binning');
    item4 = uimenu(roi_context, 'Label', 'smoothing 2x2', 'Callback', 'spc_smooth(2)');
    item5 = uimenu(roi_context, 'Label', 'smoothing 3x3', 'Callback', 'spc_smooth(3)');
    item6 = uimenu(roi_context, 'Label', 'smoothing 4x4', 'Callback', 'spc_smooth(4)');    
    item7 = uimenu(roi_context, 'Label', 'undo', 'Callback', 'spc_undo');
    item8 = uimenu(roi_context, 'Label', 'restrict in roi', 'Callback', 'spc_selectRoi');
	item9 = uimenu(roi_context, 'Label', 'log-scale', 'Callback', 'spc_logscale');

    %set axes properties.
    gui.spc.figure.projectAxes = gca;
    set(gui.spc.figure.projectAxes, 'XTick', [], 'YTick', []);
    %set(gui.spc.figure.projectAxes, 'CLim', [1,spc.switches.threshold]);
    %draw default roi in Fig1.
    gui.spc.figure.roi = rectangle('position', roi_pos, 'ButtonDownFcn', 'spc_dragRoi', 'EdgeColor', [1,1,1]);
    colorbar;

%Fig2.
    figure(gui.spc.figure.lifetime);
    set(gui.spc.figure.lifetime, 'Position', [410, 50, 360, 300], 'name', 'Lifetime');
    eval('axes(gui.spc.figure.lifetimeaxes);','gui.spc.figure.lifetimeaxes = axes');
    set(gui.spc.figure.lifetimeaxes, 'Position', [0.15, 0.37, 0.8, 0.57], 'XTick', []);
    ylabel('Photon');
    eval('axes(gui.spc.figure.residual);', 'gui.spc.figure.residual = axes');
    set(gui.spc.figure.residual, 'position', [0.15, 0.15, 0.8, 0.18]);
%Fig3.
    figure(gui.spc.figure.lifetimeMap);
    set(gui.spc.figure.lifetimeMap, 'MenuBar', 'none');
    set(gui.spc.figure.lifetimeMap, 'Position', [20, 400, 360, 300], 'name', 'LifetimeMap');
    try
        axes(gui.spc.figure.lifetimeMapImage);
    catch
        gui.spc.figure.lifetimeMapImage = axes('Position', [0.1300    0.1100    0.6626    0.8150]);
    end
    gui.spc.figure.mapRoi=rectangle('position', roi_pos, 'EdgeColor', [1,1,1], 'ButtonDownFcn', 'spc_dragRoi');
    try
        axes(gui.spc.figure.lifetimeMapColorbar); 
    catch
        gui.spc.figure.lifetimeMapColorbar = axes('Position', [0.88, 0.11, 0.05, 0.8150]);
    end
    scale = 56:-1:9;
    image(scale(:));
    colormap(jet);
    set(gui.spc.figure.lifetimeMapColorbar, 'XTickLabel', []);
    set(gui.spc.figure.lifetimeMapColorbar, 'YAxisLocation', 'right', 'YTickLabel', []);
end

if nargin == 0
    spc_drawAll(1);
else
    spc_drawAll(flag);
end

%spc_spcsUpdate;