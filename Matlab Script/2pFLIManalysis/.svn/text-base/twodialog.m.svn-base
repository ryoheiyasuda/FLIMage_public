function varargout = twodialog(varargin)
% TWODIALOG Application M-file for twodialog.fig
%    FIG = TWODIALOG launch twodialog GUI.
%    TWODIALOG('callback_name', ...) invoke the named callback.

% Last Modified by GUIDE v2.0 11-Jul-2003 00:17:24
global spc
global gui

if nargin == 0  % LAUNCH GUI

	fig = openfig(mfilename,'reuse');

	% Use system color scheme for figure:
	set(fig,'Color',get(0,'defaultUicontrolBackgroundColor'));

	% Generate a structure of handles to pass to callbacks, and store it. 
	handles = guihandles(fig);
	guidata(fig, handles);
    gui.spc.lifetimerange = handles;
    spc_updateMainStrings;

	if nargout > 0
		varargout{1} = fig;
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



% --------------------------------------------------------------------
function varargout = upper_Callback(h, eventdata, handles, varargin)
global spc


% --------------------------------------------------------------------
function varargout = lower_Callback(h, eventdata, handles, varargin)


% --------------------------------------------------------------------
function varargout = spc_thresh_Callback(h, eventdata, handles, varargin)


% --------------------------------------------------------------------
function varargout = spc_lowthresh_Callback(h, eventdata, handles, varargin)


% --------------------------------------------------------------------
function varargout = ok_Callback(h, eventdata, handles, varargin)
global spc
global gui
up = str2num(get(handles.upper, 'String'));
lw = str2num(get(handles.lower, 'String'));
thresh = str2num(get(handles.spc_thresh, 'String'));
thresh2 = str2num(get(handles.spc_lowthresh, 'String'));
range = [lw, up];
spc.switches.lifetime_limit=range;
spc.switches.threshold = thresh;
spc.switches.lutlim = [thresh2, thresh];
figure(gui.spc.figure.lifetimeMap);
%handle = gca;
%set(handle, 'CLimMode', 'manual', 'CLim', spc.switches.lifetime_limit);
spc_drawLifetimeMap(0);
%set(spc.figure.projectAxes, 'CLim', [1,spc.switches.threshold])
spc_redrawSetting;

% --------------------------------------------------------------------
function varargout = cancel_Callback(h, eventdata, handles, varargin)
global spc
set(handles.upper, 'String', num2str(spc.switches.lifetime_limit(2)));
set(handles.lower, 'String', num2str(spc.switches.lifetime_limit(1)));

% --------------------------------------------------------------------
function varargout = pushbutton3_Callback(h, eventdata, handles, varargin)
spc_adjustRange_All;





