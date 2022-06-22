function spc_filterFrames
global spc gui
%global tmp

prompt = {'Filter Window Size'};
dlg_title = 'Input';
num_lines = 1;
def = {'4'};
answer = inputdlg(prompt, dlg_title, num_lines, def);
if isempty(answer)
    return;
end
fw = str2num(answer{1});

spc.page = 1;
set(gui.spc.spc_main.spc_page, 'String', num2str(spc.page));
spc_redrawSetting(1);

for i=1:spc.stack.nStack - fw + 1
    for j = i+1:i+fw-1
        spc.stack.image1{i} = spc.stack.image1{i}+spc.stack.image1{j};
    end
end

spc.stack.nStack = spc.stack.nStack - fw + 1;

spc.page = 1;
set(gui.spc.spc_main.spc_page, 'String', num2str(spc.page));
spc_redrawSetting(1);


end


