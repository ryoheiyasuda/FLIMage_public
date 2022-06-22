function varargout = spc_main(varargin)
% SPC_MAIN Application M-file for spc_main.fig
%    FIG = SPC_MAIN launch spc_main GUI.
%    SPC_MAIN('callback_name', ...) invoke the named callback.

% Last Modified by GUIDE v2.5 28-Jan-2015 12:02:52
global gui;
global spc;

if nargin == 0  % LAUNCH GUI

	fig = openfig(mfilename,'reuse');

	% Generate a structure of handles to pass to callbacks, and store it. 
	handles = guihandles(fig);
    gui.spc.spc_main = handles;
	guidata(fig, handles);

	if nargout > 0
		varargout{1} = fig;
	end
    
    try
        range = round(spc.fit(spc.currentChannel).range.*spc.datainfo.psPerUnit/100)/10;
        set(handles.spc_fitstart, 'String', num2str(range(1)));
        set(handles.spc_fitend, 'String', num2str(range(2)));
        set(handles.back_value, 'String', num2str(spc.fit(spc.currentChannel).background));
        set(handles.filename, 'String', spc.filename);
        set(handles.spc_page, 'String', num2str(spc.switches.currentPage));
        set(handles.slider1, 'Value', (spc.switches.currentPage-1)/100);
        spc_dispbeta;
    end
    try
        set(handles.fracCheck, 'Value', 1);
        set(handles.tauCheck, 'Value', 1);
        set(handles.greenCheck, 'Value', 1);
        set(handles.redCheck, 'Value', 1);
        set(handles.RatioCheck, 'Value', 0);
    end
       

elseif ischar(varargin{1}) % INVOKE NAMED SUBFUNCTION OR CALLBACK

	try
		if (nargout)
			[varargout{1:nargout}] = feval(varargin{:}); % FEVAL switchyard
		else
			feval(varargin{:}); % FEVAL switchyard
		end
	catch
		disp(lasterr);
	end

end


%| ABOUT CALLBACKS:
%| GUIDE automatically appends subfunction prototypes to this file, and 
%| sets objects' callback properties to call them through the FEVAL 
%| switchyard above. This comment describes that mechanism.
%|
%| Each callback subfunction declaration has the following form:
%| <SUBFUNCTION_NAME>(H, EVENTDATA, HANDLES, VARARGIN)
%|
%| The subfunction name is composed using the object's Tag and the 
%| callback type separated by '_', e.g. 'slider2_Callback',
%| 'figure1_CloseRequestFcn', 'axis1_ButtondownFcn'.
%|
%| H is the callback object's handle (obtained using GCBO).
%|
%| EVENTDATA is empty, but reserved for future use.
%|
%| HANDLES is a structure containing handles of components in GUI using
%| tags as fieldnames, e.g. handles.figure1, handles.slider2. This
%| structure is created at GUI startup using GUIHANDLES and stored in
%| the figure's application data using GUIDATA. A copy of the structure
%| is passed to each callback.  You can store additional information in
%| this structure at GUI startup, and you can change the structure
%| during callbacks.  Call guidata(h, handles) after changing your
%| copy to replace the stored original so that subsequent callbacks see
%| the updates. Type "help guihandles" and "help guidata" for more
%| information.
%|
%| VARARGIN contains any extra arguments you have passed to the
%| callback. Specify the extra arguments by editing the callback
%| property in the inspector. By default, GUIDE sets the property to:
%| <MFILENAME>('<SUBFUNCTION_NAME>', gcbo, [], guidata(gcbo))
%| Add any extra arguments after the last argument, before the final
%| closing parenthesis.



%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%Menu bar
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%File menu
% --------------------------------------------------------------------
function varargout = spc_file_Callback(h, eventdata, handles, varargin)
% --------------------------------------------------------------------
function varargout = spc_open_Callback(h, eventdata, handles, varargin)
global spc;
global gui;
%spc.fit.range(1) = 	str2num(get(handles.spc_fitstart, 'String'))/spc.datainfo.psPerUnit*1000;
%spc.fit.range(2) =	str2num(get(handles.spc_fitend, 'String'))/spc.datainfo.psPerUnit*1000;
%page = str2num(get(handles.spc_page, 'String'));

[fname,pname] = uigetfile('*.sdt;*.mat;*.tif;*.flim','Select spc-file');
cd (pname);
filestr = [pname, fname];
if exist(filestr) == 2
        spc_openCurves(filestr);
end
%spc_putIntoSPCS;
spc_updateMainStrings;

%%%Put into spc array if there is no state


% --------------------------------------------------------------------
function varargout = openall_Callback(h, eventdata, handles, varargin)
global spc;
%spc.fit.range(1) = 	str2num(get(handles.spc_fitstart, 'String'))/spc.datainfo.psPerUnit*1000;
%spc.fit.range(2) =	str2num(get(handles.spc_fitend, 'String'))/spc.datainfo.psPerUnit*1000;
%spc_openAll;
spc_updateMainStrings;
% --------------------------------------------------------------------
function varargout = spc_loadPrf_Callback(h, eventdata, handles, varargin)
global spc;
[fname,pname] = uigetfile('*.mat','Select mat-file');
if exist([pname, fname]) == 2
    load ([pname,fname], 'prf');
end
spc.fit.prf = prf;

% --------------------------------------------------------------------
function varargout = spc_savePrf_Callback(h, eventdata, handles, varargin)
global spc;
[fname,pname] = uiputfile('*.mat','Select the mat-file');
prf = spc.fit.prf;
%if (pname == 7) & (fname ~= '')
    save ([pname,fname], 'prf');
%end

%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%Fitting menu
% --------------------------------------------------------------------
function varargout = spc_fitting_Callback(h, eventdata, handles, varargin)
% --------------------------------------------------------------------
function varargout = spc_exps_Callback(h, eventdata, handles, varargin)
global spc
range = spc.fit.range;
lifetime = spc.lifetime(range(1):1:range(2));
x = [1:1:length(lifetime)];
beta0 = [max(lifetime), sum(lifetime)/max(lifetime)];
betahat = spc_nlinfit(x, lifetime, sqrt(lifetime)/sqrt(max(lifetime)), @expfit, beta0);
tau = betahat(2)*spc.datainfo.psPerUnit/1000;
set(handles.beta1, 'String', num2str(betahat(1)));
set(handles.beta2, 'String', num2str(tau));
set(handles.beta3, 'String', '0');
set(handles.beta4, 'String', '0');
set(handles.beta5, 'String', '0');
set(handles.pop1, 'String', '100');
set(handles.pop2, 'String', '0');
set(handles.average, 'String', num2str(tau));

%Drawing
fit = expfit(betahat, x);
t = [range(1):1:range(2)];
t = t*spc.datainfo.psPerUnit/1000;
spc_drawfit (t, fit, lifetime, betahat);


function y=expfit(beta0, x);
global spc;
if spc.switches.imagemode == 1
    spc_roi = get(spc.figure.roi, 'Position');
else
    spc_roi = [1,1,1,1];
end;
y=exp(-x/beta0(2))*beta0(1);

% --------------------------------------------------------------------
function varargout = spc_expgauss_Callback(h, eventdata, handles, varargin)
betahat=spc_fitexp2gauss;
spc_redrawSetting(1);
% --------------------------------------------------------------------
function varargout = spc_exp2gauss_Callback(h, eventdata, handles, varargin)
betahat=spc_fitexp2gauss;
spc_redrawSetting(1);

% --------------------------------------------------------------------
function varargout = expgauss_triple_Callback(h, eventdata, handles, varargin)
spc_fitWithSingleExp_triple;

% --------------------------------------------------------------------
function varargout = Double_expgauss_triple_Callback(h, eventdata, handles, varargin)
spc_fitWithDoubleExp_triple;
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%Drawing menu
% --------------------------------------------------------------------
function varargout = spc_drawing_Callback(h, eventdata, handles, varargin)
% --------------------------------------------------------------------
function varargout = spc_drawall_Callback(h, eventdata, handles, varargin)
%spc_spcsUpdate;
spc_redrawSetting(1);

% --------------------------------------------------------------------
function varargout = logscale_Callback(h, eventdata, handles, varargin)
spc_logscale;
% --------------------------------------------------------------------
function varargout = lifetime_map_Callback(h, eventdata, handles, varargin)
twodialog;
% --------------------------------------------------------------------
function varargout = show_all_Callback(h, eventdata, handles, varargin)
for j = 1:4
    figure(j)
end

% --------------------------------------------------------------------
function varargout = redraw_all_Callback(h, eventdata, handles, varargin)
spc_redrawSetting(1);

%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%Analysis menu
% --------------------------------------------------------------------
function varargout = analysis_Callback(h, eventdata, handles, varargin)
% --------------------------------------------------------------------
function varargout = smoothing_Callback(h, eventdata, handles, varargin)
spc_smooth;
% --------------------------------------------------------------------
function varargout = binning_Callback(h, eventdata, handles, varargin)
spc_binning;

% --------------------------------------------------------------------
function varargout = smooth_all_Callback(h, eventdata, handles, varargin)
spc_smoothAll;

% --------------------------------------------------------------------
function varargout = binning_all_Callback(h, eventdata, handles, varargin)
spc_binningAll;

% --------------------------------------------------------------------
function varargout = undoall_Callback(h, eventdata, handles, varargin)
spc_undoAll;
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%Buttons
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
% --------------------------------------------------------------------
function varargout = spc_fit1_Callback(h, eventdata, handles, varargin)
global spc
betahat=spc_fitexpgauss;
%spc_dispbeta;
spc_redrawSetting(1);

% --------------------------------------------------------------------
function varargout = spc_fit2_Callback(h, eventdata, handles, varargin)
global spc
betahat=spc_fitexp2gauss;
% spc_dispbeta;
spc_redrawSetting(1);

% --------------------------------------------------------------------
function varargout = spc_look_Callback(h, eventdata, handles, varargin)
global spc gui
range = spc.fit(spc.currentChannel).range;
lifetime = spc.lifetime(range(1):1:range(2));
x = [1:1:length(lifetime)];
beta0 = spc_initialValue_double;

% pop1 = beta0(1)/(beta0(1)+beta0(3));
% pop2 = beta0(3)/(beta0(1)+beta0(3));
% set(handles.pop1, 'String', num2str(pop1));
% set(handles.pop2, 'String', num2str(pop2));

%Drawing
fit = exp2gauss(beta0, x);
t = [range(1):1:range(2)];
t = t*spc.datainfo.psPerUnit/1000;

%Drawing
spc_drawfit (t, fit, lifetime, spc.currentChannel);
figure(2);
spc_dispbeta;

% --------------------------------------------------------------------
function varargout = spc_redraw_Callback(h, eventdata, handles, varargin)
global spc gui

spc_redrawSetting(1);
for j = 1:4
    figure(j);
end
set(gui.spc.spc_main.spc_main, 'color',  [1, 1, 0.75])




%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%Beta windows
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
% --------------------------------------------------------------------
function varargout = beta1_Callback(h, eventdata, handles, varargin)
global spc
val1 = str2double(get(h, 'String'));
spc.fit(spc.currentChannel).beta0(1) = val1; %*1000/spc.datainfo.psPerUnit;
spc_dispbeta;

% --------------------------------------------------------------------
function varargout = beta2_Callback(h, eventdata, handles, varargin)
global spc gui
val1 = str2double(get(h, 'String'));
spc.fit(spc.currentChannel).beta0(2) = val1*1000/spc.datainfo.psPerUnit;
spc_dispbeta;

% --------------------------------------------------------------------
function varargout = beta3_Callback(h, eventdata, handles, varargin)
global spc gui
val1 = str2double(get(h, 'String'));
spc.fit(spc.currentChannel).beta0(3) = val1; %*1000/spc.datainfo.psPerUnit;
spc_dispbeta;

% --------------------------------------------------------------------
function varargout = beta4_Callback(h, eventdata, handles, varargin)
global spc gui
val1 = str2double(get(h, 'String'));
spc.fit(spc.currentChannel).beta0(4) = val1*1000/spc.datainfo.psPerUnit;
spc_dispbeta;

% --------------------------------------------------------------------
function varargout = beta5_Callback(h, eventdata, handles, varargin)
global spc gui
val1 = str2double(get(h, 'String'));
spc.fit(spc.currentChannel).beta0(5) = val1*1000/spc.datainfo.psPerUnit;
spc_dispbeta;

% --------------------------------------------------------------------
function varargout = beta6_Callback(hObject, eventdata, handles)
global spc gui
val1 = str2double(get(h, 'String'));
spc.fit(spc.currentChannel).beta0(6) = val1*1000/spc.datainfo.psPerUnit;
spc_dispbeta;

% --------------------------------------------------------------------
function varargout = spc_fitstart_Callback(h, eventdata, handles, varargin)
%spc_redrawSetting(1);
spc_redrawSetting(1);

% --------------------------------------------------------------------
function varargout = spc_fitend_Callback(h, eventdata, handles, varargin)
% --------------------------------------------------------------------
%spc_redrawSetting(1);
spc_redrawSetting(1);

%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%Spc, page control
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function varargout = spc_page_Callback(h, eventdata, handles, varargin)
global spc;
global gui;

current = spc.page;
spc.page = str2num(get(gui.spc.spc_main.spc_page, 'String'));

if any(spc.page > length(spc.stack.image1))
    spc.page = current;
end
if any(spc.page < 1)
    spc.page = current;
end
spc.switches.currentPage = spc.page;


%if all(current ~= spc.page)
    %spc_maxProc_offLine;
    spc_redrawSetting(1);
    set(gui.spc.spc_main.spc_page, 'String', num2str(spc.page));
%end
% 
spc_updateMainStrings;



% --------------------------------------------------------------------
function varargout = slider1_Callback(h, eventdata, handles, varargin)
global spc
global gui

current = spc.page;
slider_value = get(handles.slider1, 'Value');
slider_step = get(handles.slider1, 'sliderstep');
page = round(slider_value);


if page > spc.stack.nStack
    page = spc.stack.nStack;
end
if page < 1
    page = 1;
end
if page < min(spc.page)
    page = min(spc.page);
end
spc.page = min(current):page;
spc.switches.currentPage = spc.page;
set(gui.spc.spc_main.spc_page, 'String', num2str(spc.page));
%spc_maxProc_offLine;
spc_redrawSetting(1);

spc_updateMainStrings;

%
% --- Executes on slider movement.
function minSlider_Callback(hObject, eventdata, handles)
global spc;
global gui;

current = spc.page;
slider_value = get(handles.minSlider, 'Value');
slider_step = get(handles.minSlider, 'sliderstep');
page = round(slider_value);


if page > spc.stack.nStack
    page = spc.stack.nStack;
end
if page > max(spc.page)
    page = max(spc.page);
end
if page < 1
    page = 1;
end
spc.page = page:max(current);
spc.switches.currentPage = spc.page;
set(gui.spc.spc_main.spc_page, 'String', num2str(spc.page));
%spc_maxProc_offLine;
spc_redrawSetting(1);


spc_updateMainStrings;


% --- Executes during object creation, after setting all properties.
function minSlider_CreateFcn(hObject, eventdata, handles)
% hObject    handle to minSlider (see GCBO)
% eventdata  reserved - to be defined in a future version of MATLAB
% handles    empty - handles not created until after all CreateFcns called

% Hint: slider controls usually have a light gray background.
if isequal(get(hObject,'BackgroundColor'), get(0,'defaultUicontrolBackgroundColor'))
    set(hObject,'BackgroundColor',[.9 .9 .9]);
end
% --------------------------------------------------------------------
function varargout = spcN_Callback(h, eventdata, handles, varargin)
%global spcs;
global spc;

spcN = str2num(get(handles.spcN, 'String'));

spc_changeCurrent(spcN);

%set(handles.spcN, 'String', num2str(spcs.current));
%set(handles.slider2, 'Value', (spcs.current-1)/100);
%set(handles.filename, 'String', num2str(spc.filename));
spc_updateMainStrings;

% --------------------------------------------------------------------
function varargout = slider2_Callback(h, eventdata, handles, varargin)

slider_value = get(handles.slider2, 'Value');
slider_step = get(handles.slider2, 'sliderstep');
spcN = slider_value*100+1;

spc_changeCurrent(spcN);
    
%set(handles.spcN, 'String', num2str(spcs.current));
%set(handles.slider2, 'Value', (spcs.current-1)/100);
%set(handles.filename, 'String', num2str(spc.filename));
spc_updateMainStrings;


% --------------------------------------------------------------------
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%Utilities
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function varargout = fixtau1_Callback(h, eventdata, handles, varargin)
global spc gui
spc.fit(spc.currentChannel).fixtau(2) = get(h, 'Value');

function varargout = fixtau2_Callback(h, eventdata, handles, varargin)
global spc gui
spc.fit(spc.currentChannel).fixtau(4) = get(h, 'Value');

% --- Executes on button press in fix_g.
function fix_g_Callback(hObject, eventdata, handles)
global spc gui
spc.fit(spc.currentChannel).fixtau(5) = get(hObject, 'Value');

% --- Executes on button press in fix_delta.
function fix_delta_Callback(hObject, eventdata, handles)
global spc gui
spc.fit(spc.currentChannel).fixtau(6) = get(hObject, 'Value');

% --- Executes on button press in pushbutton14.
function pushbutton14_Callback(hObject, eventdata, handles)
% hObject    handle to pushbutton14 (see GCBO)
% eventdata  reserved - to be defined in a future version of MATLAB
% handles    structure with handles and user data (see GUIDATA)
%timecourse.

spc_auto(1);





% --- Executes during object creation, after setting all properties.
function beta6_CreateFcn(hObject, eventdata, handles)
% hObject    handle to beta6 (see GCBO)
% eventdata  reserved - to be defined in a future version of MATLAB
% handles    empty - handles not created until after all CreateFcns called

% Hint: edit controls usually have a white background on Windows.
%       See ISPC and COMPUTER.
if ispc
    set(hObject,'BackgroundColor','white');
else
    set(hObject,'BackgroundColor',get(0,'defaultUicontrolBackgroundColor'));
end


% --- Executes on button press in pushbutton15.
function pushbutton15_Callback(hObject, eventdata, handles)
% hObject    handle to pushbutton15 (see GCBO)
% eventdata  reserved - to be defined in a future version of MATLAB
% handles    structure with handles and user data (see GUIDATA)


% --- Executes on button press in spc_opennext. Open the next file.
function spc_opennext_Callback(hObject, eventdata, handles)
% hObject    handle to spc_opennext (see GCBO)
% eventdata  reserved - to be defined in a future version of MATLAB
% handles    structure with handles and user data (see GUIDATA)
spc_opennext;


% --- Executes on button press in spc_openprevious. Open the Previous file.
function spc_openprevious_Callback(hObject, eventdata, handles)
% hObject    handle to spc_openprevious (see GCBO)
% eventdata  reserved - to be defined in a future version of MATLAB
% handles    structure with handles and user data (see GUIDATA)
spc_openprevious;


% --------------------------------------------------------------------
function Roi_Callback(hObject, eventdata, handles)
% hObject    handle to Roi (see GCBO)
% eventdata  reserved - to be defined in a future version of MATLAB
% handles    structure with handles and user data (see GUIDATA)

% --------------------------------------------------------------------
function Roi0_Callback(hObject, eventdata, handles)
spc_makeRoiA(0);
% --------------------------------------------------------------------
function Roi1_Callback(hObject, eventdata, handles)
spc_makeRoiA(1);
% --------------------------------------------------------------------
function Roi2_Callback(hObject, eventdata, handles)
spc_makeRoiA(2);
% --------------------------------------------------------------------
function Roi3_Callback(hObject, eventdata, handles)
spc_makeRoiA(3);


% --------------------------------------------------------------------
function Roi4_Callback(hObject, eventdata, handles)
spc_makeRoiA(4);

% --------------------------------------------------------------------
function Roi5_Callback(hObject, eventdata, handles)
spc_makeRoiA(5);

% --------------------------------------------------------------------
function RoiMore_Callback(hObject, eventdata, handles)
prompt = 'Roi Number:';
dlg_title = 'Roi';
num_lines= 1;
def     = {'6'};
answer  = inputdlg(prompt,dlg_title,num_lines,def);

spc_makeRoiA(str2num(answer{1}));
% --------------------------------------------------------------------
function asRoi_Callback(hObject, eventdata, handles)
% hObject    handle to asRoi (see GCBO)
% eventdata  reserved - to be defined in a future version of MATLAB
% handles    structure with handles and user data (see GUIDATA)
prompt = 'Roi Number:';
dlg_title = 'Roi';
num_lines= 1;
def     = {'1'};
answer  = inputdlg(prompt,dlg_title,num_lines,def);

spc_makeRoiB(str2num(answer{1}));



function File_N_Callback(hObject, eventdata, handles)
% hObject    handle to File_N (see GCBO)
% eventdata  reserved - to be defined in a future version of MATLAB
% handles    structure with handles and user data (see GUIDATA)

% Hints: get(hObject,'String') returns contents of File_N as text
%        str2double(get(hObject,'String')) returns contents of File_N as a double

global spc;
try
    [filepath, basename, filenumber, max] = spc_AnalyzeFilename(spc.filename);
    fileN = get(handles.File_N, 'String');
    next_filenumber_str = '000';
    next_filenumber_str ((end+1-length(fileN)):end) = num2str(fileN);
    if max == 0
        next_filename = [filepath, basename, next_filenumber_str, '.tif'];
    else
        next_filename = [filepath, basename, next_filenumber_str, '_max.tif'];
    end
    if exist(next_filename)
        spc_openCurves (next_filename);
    else
        disp([next_filename, ' not exist!']);
    end
    spc_updateMainStrings;
end

% --- Executes during object creation, after setting all properties.
function File_N_CreateFcn(hObject, eventdata, handles)
% hObject    handle to File_N (see GCBO)
% eventdata  reserved - to be defined in a future version of MATLAB
% handles    empty - handles not created until after all CreateFcns called

% Hint: edit controls usually have a white background on Windows.
%       See ISPC and COMPUTER.
if ispc && isequal(get(hObject,'BackgroundColor'), get(0,'defaultUicontrolBackgroundColor'))
    set(hObject,'BackgroundColor','white');
end



function edit14_Callback(hObject, eventdata, handles)
% hObject    handle to edit14 (see GCBO)
% eventdata  reserved - to be defined in a future version of MATLAB
% handles    structure with handles and user data (see GUIDATA)

% Hints: get(hObject,'String') returns contents of edit14 as text
%        str2double(get(hObject,'String')) returns contents of edit14 as a double


% --- Executes during object creation, after setting all properties.
function edit14_CreateFcn(hObject, eventdata, handles)
% hObject    handle to edit14 (see GCBO)
% eventdata  reserved - to be defined in a future version of MATLAB
% handles    empty - handles not created until after all CreateFcns called

% Hint: edit controls usually have a white background on Windows.
%       See ISPC and COMPUTER.
if ispc && isequal(get(hObject,'BackgroundColor'), get(0,'defaultUicontrolBackgroundColor'))
    set(hObject,'BackgroundColor','white');
end


% --- Executes on button press in auto_A.
function auto_A_Callback(hObject, eventdata, handles)
% hObject    handle to auto_A (see GCBO)
% eventdata  reserved - to be defined in a future version of MATLAB
% handles    structure with handles and user data (see GUIDATA)

spc_adjustTauOffset;

function F_offset_Callback(hObject, eventdata, handles)
% hObject    handle to F_offset (see GCBO)
% eventdata  reserved - to be defined in a future version of MATLAB
% handles    structure with handles and user data (see GUIDATA)

% Hints: get(hObject,'String') returns contents of F_offset as text
%        str2double(get(hObject,'String')) returns contents of F_offset as a double

spc_redrawSetting (1);

% --- Executes during object creation, after setting all properties.
function F_offset_CreateFcn(hObject, eventdata, handles)
% hObject    handle to F_offset (see GCBO)
% eventdata  reserved - to be defined in a future version of MATLAB
% handles    empty - handles not created until after all CreateFcns called

% Hint: edit controls usually have a white background on Windows.
%       See ISPC and COMPUTER.
if ispc && isequal(get(hObject,'BackgroundColor'), get(0,'defaultUicontrolBackgroundColor'))
    set(hObject,'BackgroundColor','white');
end

% --------------------------------------------------------------------
function fit_single_prf_Callback(hObject, eventdata, handles)
% hObject    handle to fit_double_prf (see GCBO)
% eventdata  reserved - to be defined in a future version of MATLAB
% handles    structure with handles and user data (see GUIDATA)
global spc

if ~isfield(spc.fit, 'prf');
    spc_prfdefault; 
end
if length(spc.fit.prf) ~= spc.lifetime
    spc_prfdefault;
end

spc_fitWithSingleExp;
spc_dispbeta;
% --------------------------------------------------------------------
function fit_double_prf_Callback(hObject, eventdata, handles)
% hObject    handle to fit_double_prf (see GCBO)
% eventdata  reserved - to be defined in a future version of MATLAB
% handles    structure with handles and user data (see GUIDATA)
global spc

if ~isfield(spc.fit, 'prf');
    spc_prfdefault; 
end
if length(spc.fit.prf) ~= spc.lifetime
    spc_prfdefault;
end

spc_fitWithDoubleExp;
spc_dispbeta;


% --- Executes on button press in fit_eachtime.
function fit_eachtime_Callback(hObject, eventdata, handles)
% hObject    handle to fit_eachtime (see GCBO)
% eventdata  reserved - to be defined in a future version of MATLAB
% handles    structure with handles and user data (see GUIDATA)

% Hint: get(hObject,'Value') returns toggle state of fit_eachtime


% --- Executes on button press in calcRoi.
function calcRoi_Callback(hObject, eventdata, handles)
% hObject    handle to calcRoi (see GCBO)
% eventdata  reserved - to be defined in a future version of MATLAB
% handles    structure with handles and user data (see GUIDATA)
spc_calcRoi;


% --------------------------------------------------------------------
function RecRoi_Callback(hObject, eventdata, handles)
% hObject    handle to RecRoi (see GCBO)
% eventdata  reserved - to be defined in a future version of MATLAB
% handles    structure with handles and user data (see GUIDATA)
spc_recoverRois;


% --- If Enable == 'on', executes on mouse press in 5 pixel border.
% --- Otherwise, executes on mouse press in 5 pixel border or over spc_opennext.
function spc_opennext_ButtonDownFcn(hObject, eventdata, handles)
% hObject    handle to spc_opennext (see GCBO)
% eventdata  reserved - to be defined in a future version of MATLAB
% handles    structure with handles and user data (see GUIDATA)


% --------------------------------------------------------------------
function Untitled_1_Callback(hObject, eventdata, handles)
% hObject    handle to Untitled_1 (see GCBO)
% eventdata  reserved - to be defined in a future version of MATLAB
% handles    structure with handles and user data (see GUIDATA)


% --------------------------------------------------------------------
function s_profile_Callback(hObject, eventdata, handles)
% hObject    handle to s_profile (see GCBO)
% eventdata  reserved - to be defined in a future version of MATLAB
% handles    structure with handles and user data (see GUIDATA)
global gui
figure(gui.spc.figure.project);

spc_makepolyLines;

% --------------------------------------------------------------------
function polylines_Callback(hObject, eventdata, handles)
% hObject    handle to polylines (see GCBO)
% eventdata  reserved - to be defined in a future version of MATLAB
% handles    structure with handles and user data (see GUIDATA)
global spc
fn = spc.filename;
fn = [fn, '_ROI2'];
spc_DendriteAnalysis(fn);


% --------------------------------------------------------------------
function spc_averageMultipleImages_Callback(hObject, eventdata, handles)
% hObject    handle to spc_averageMultipleImages (see GCBO)
% eventdata  reserved - to be defined in a future version of MATLAB
% handles    structure with handles and user data (see GUIDATA)

spc_averageMultipleImages;

function calcRoiTo_CreateFcn(hObject, eventdata, handles)

function calcRoiFrom_CreateFcn(hObject, eventdata, handles)

% --- Executes on button press in calcRoiBatch.
function calcRoiBatch_Callback(hObject, eventdata, handles)
% hObject    handle to calcRoiBatch (see GCBO)
% eventdata  reserved - to be defined in a future version of MATLAB
% handles    structure with handles and user data (see GUIDATA)

fromVal = str2double(get(handles.calcRoiFrom, 'String'));
toVal = str2double(get(handles.calcRoiTo, 'String'));
for i = fromVal : toVal
    spc_openCurves(i);
    pause(0.05);
    if length(findobj('Tag', 'RoiA0')) ~= 0
        spc_calcRoi;
    end
    spc_auto(1);
end


% --- Executes on button press in tauCheck.
function tauCheck_Callback(hObject, eventdata, handles)
% hObject    handle to tauCheck (see GCBO)
% eventdata  reserved - to be defined in a future version of MATLAB
% handles    structure with handles and user data (see GUIDATA)

% Hint: get(hObject,'Value') returns toggle state of tauCheck


% --- Executes on button press in fracCheck.
function fracCheck_Callback(hObject, eventdata, handles)
% hObject    handle to fracCheck (see GCBO)
% eventdata  reserved - to be defined in a future version of MATLAB
% handles    structure with handles and user data (see GUIDATA)

% Hint: get(hObject,'Value') returns toggle state of fracCheck


% --- Executes on button press in redCheck.
function redCheck_Callback(hObject, eventdata, handles)
% hObject    handle to redCheck (see GCBO)
% eventdata  reserved - to be defined in a future version of MATLAB
% handles    structure with handles and user data (see GUIDATA)

% Hint: get(hObject,'Value') returns toggle state of redCheck


% --- Executes on button press in greenCheck.
function greenCheck_Callback(hObject, eventdata, handles)
% hObject    handle to greenCheck (see GCBO)
% eventdata  reserved - to be defined in a future version of MATLAB
% handles    structure with handles and user data (see GUIDATA)

% Hint: get(hObject,'Value') returns toggle state of greenCheck


% --- Executes on button press in RatioCheck.
function RatioCheck_Callback(hObject, eventdata, handles)
% hObject    handle to RatioCheck (see GCBO)
% eventdata  reserved - to be defined in a future version of MATLAB
% handles    structure with handles and user data (see GUIDATA)

% Hint: get(hObject,'Value') returns toggle state of RatioCheck


% --- Executes on button press in Ch1.
function Ch1_Callback(hObject, eventdata, handles)
% hObject    handle to Ch1 (see GCBO)
% eventdata  reserved - to be defined in a future version of MATLAB
% handles    structure with handles and user data (see GUIDATA)

% Hint: get(hObject,'Value') returns toggle state of Ch1

global gui
gui.spc.proChannel = 1;
spc_switchChannel;



% --- Executes on button press in Ch2.
function Ch2_Callback(hObject, eventdata, handles)
% hObject    handle to Ch2 (see GCBO)
% eventdata  reserved - to be defined in a future version of MATLAB
% handles    structure with handles and user data (see GUIDATA)

% Hint: get(hObject,'Value') returns toggle state of Ch2

global gui
gui.spc.proChannel = 2;
spc_switchChannel;

% --- Executes on button press in Ch3.
function Ch3_Callback(hObject, eventdata, handles)
% hObject    handle to Ch3 (see GCBO)
% eventdata  reserved - to be defined in a future version of MATLAB
% handles    structure with handles and user data (see GUIDATA)

% Hint: get(hObject,'Value') returns toggle state of Ch3


global gui
gui.spc.proChannel = 3;
spc_switchChannel;


% --------------------------------------------------------------------
function fastAnalysis_Callback(hObject, eventdata, handles)
% hObject    handle to fastAnalysis (see GCBO)
% eventdata  reserved - to be defined in a future version of MATLAB
% handles    structure with handles and user data (see GUIDATA)
spc_calc_timeCourseFromStack;


% --- Executes on button press in radiobutton4.
function radiobutton4_Callback(hObject, eventdata, handles)
% hObject    handle to radiobutton4 (see GCBO)
% eventdata  reserved - to be defined in a future version of MATLAB
% handles    structure with handles and user data (see GUIDATA)

% Hint: get(hObject,'Value') returns toggle state of radiobutton4
global spc gui

spc.switches.maxAve = get(hObject, 'value');
set(gui.spc.figure.redAuto, 'Value', 1);
spc_redrawSetting(1);


% --- Executes on button press in onePageRight.
function onePageRight_Callback(hObject, eventdata, handles)
% hObject    handle to onePageRight (see GCBO)
% eventdata  reserved - to be defined in a future version of MATLAB
% handles    structure with handles and user data (see GUIDATA)
global spc gui

if length(spc.page) == spc.stack.nStack
    spc.page = 1;
else
    spc.page = sort(spc.page+1);
end

if spc.page(end) > spc.stack.nStack
    if length(spc.page) > 1
        spc.page = spc.page(1:end-1);
    else
        spc.page =spc.stack.nStack;
    end
end

set(gui.spc.spc_main.spc_page, 'String', num2str(spc.page));
spc_redrawSetting(1);

spc_updateMainStrings;

% --- Executes on button press in onePageLeft.
function onePageLeft_Callback(hObject, eventdata, handles)
% hObject    handle to onePageLeft (see GCBO)
% eventdata  reserved - to be defined in a future version of MATLAB
% handles    structure with handles and user data (see GUIDATA)
global spc gui

if length(spc.page) == spc.stack.nStack
    spc.page = 1;
else
    spc.page = sort(spc.page-1);
end

if spc.page(1) < 1
    if length(spc.page) > 1
        for i=1:length(spc.page)-1
            spc.page(i) = spc.page(i+1);
        end

        spc.page = spc.page(1:end-1);
    else
        spc.page = 1;
    end
end


set(gui.spc.spc_main.spc_page, 'String', num2str(spc.page));
spc_redrawSetting(1);

spc_updateMainStrings;


% --- Executes on button press in tenPageLeft.
function tenPageLeft_Callback(hObject, eventdata, handles)
% hObject    handle to tenPageLeft (see GCBO)
% eventdata  reserved - to be defined in a future version of MATLAB
% handles    structure with handles and user data (see GUIDATA)
global spc gui

if length(spc.page) == spc.stack.nStack
    spc.page = 1;
else
    spc.page = sort(spc.page-10);
end

if spc.page(1) < 1
    if length(spc.page) > 1
        for i=1:length(spc.page)-1
            spc.page(i) = spc.page(i+1);
        end

        spc.page = spc.page(1:end-1);
    else
        spc.page = 1;
    end
end


set(gui.spc.spc_main.spc_page, 'String', num2str(spc.page));
spc_redrawSetting(1);

spc_updateMainStrings;

% --- Executes on button press in tenPageRight.
function tenPageRight_Callback(hObject, eventdata, handles)
% hObject    handle to tenPageRight (see GCBO)
% eventdata  reserved - to be defined in a future version of MATLAB
% handles    structure with handles and user data (see GUIDATA)
global spc gui

if length(spc.page) == spc.stack.nStack
    spc.page = 1;
else
    spc.page = sort(spc.page+10);
end


if spc.page(end) > spc.stack.nStack
    if length(spc.page) > 1
        spc.page = spc.page(1:end-1);
    else
        spc.page =spc.stack.nStack;
    end
end

set(gui.spc.spc_main.spc_page, 'String', num2str(spc.page));
spc_redrawSetting(1);

spc_updateMainStrings;


% --------------------------------------------------------------------
function alignFrames_Callback(hObject, eventdata, handles)
% hObject    handle to alignFrames (see GCBO)
% eventdata  reserved - to be defined in a future version of MATLAB
% handles    structure with handles and user data (see GUIDATA)
global gui

slicesS = get(gui.spc.spc_main.spc_page, 'String');
spc_alignFrames;
set(gui.spc.spc_main.spc_page, 'String', slicesS);
spc_redrawSetting(1);
spc_updateMainStrings;

% --------------------------------------------------------------------
function frameCurrentAnal_Callback(hObject, eventdata, handles)
% hObject    handle to frameCurrentAnal (see GCBO)
% eventdata  reserved - to be defined in a future version of MATLAB
% handles    structure with handles and user data (see GUIDATA)
global gui

slicesS = get(gui.spc.spc_main.spc_page, 'String');
slices = str2num(slicesS);
spc_calc_timeCourseFromStack(slices);
set(gui.spc.spc_main.spc_page, 'String', slicesS);
spc_redrawSetting(1);
spc_updateMainStrings;


% --------------------------------------------------------------------
function Frames_Callback(hObject, eventdata, handles)
% hObject    handle to Frames (see GCBO)
% eventdata  reserved - to be defined in a future version of MATLAB
% handles    structure with handles and user data (see GUIDATA)


% --------------------------------------------------------------------
function saveMovie_Callback(hObject, eventdata, handles)
% hObject    handle to saveMovie (see GCBO)
% eventdata  reserved - to be defined in a future version of MATLAB
% handles    structure with handles and user data (see GUIDATA)

spc_makeMovieFromFrames;


% --------------------------------------------------------------------
function TFilter_Callback(hObject, eventdata, handles)
% hObject    handle to TFilter (see GCBO)
% eventdata  reserved - to be defined in a future version of MATLAB
% handles    structure with handles and user data (see GUIDATA)

spc_filterFrames;
