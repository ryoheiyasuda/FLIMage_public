function spc_updateMainStrings
global spc;
global gui;
%global spcs;

if isfield(gui, 'spc')
    if isfield(gui.spc, 'spc_main')
		handles = gui.spc.spc_main;
        if ~spc.switches.noSPC
            range = round(spc.fit(spc.currentChannel).range.*spc.datainfo.psPerUnit/100)/10;
            set(handles.spc_fitstart, 'String', num2str(range(1)));
            set(handles.spc_fitend, 'String', num2str(range(2)));
        end
        set(handles.filename, 'String', spc.filename);
        try
            set(handles.spc_page, 'String', spc_num2str(spc.page));
		    set(handles.slider1, 'Value', max(spc.page));
            set(handles.minSlider, 'Value', min(spc.page));
            %
        end
%         try
% 		    set(handles.spcN, 'String', num2str(spcs.current));    
% 		    set(handles.slider2, 'Value', (spcs.current-1)/100);
%         end
        try
            [filepath, basename, filenumber, max1] = spc_AnalyzeFilename(spc.filename);
            set(handles.File_N, 'String', num2str(filenumber));
        end
        try
            set(gui.spc.spc_main.n_of_pages, 'string', num2str(spc.stack.nStack))
        end
    end
    
    if isfield(gui.spc, 'lifetimerange')
        try
            hg = gui.spc.lifetimerange;
            set(hg.upper, 'String', num2str(spc.switches.lifetime_limit(2)));
            set(hg.lower, 'String', num2str(spc.switches.lifetime_limit(1)));
        catch
        end
        try %for the compatibility with old files.
            set(hg.spc_thresh, 'String', num2str(spc.switches.lutlim(2)));
            set(hg.spc_lowthresh, 'String', num2str(spc.switches.lutlim(1)));
        catch
            try
                set(hg.spc_thresh, 'String', num2str(spc.switches.threshold));
                set(hg.spc_lowthresh, 'String', '0');
            catch
            end
        end
    end
end

try
 spc_dispbeta;
catch
  %disp('Error in spc_dispbeta !! main strings are not updated'); 
end