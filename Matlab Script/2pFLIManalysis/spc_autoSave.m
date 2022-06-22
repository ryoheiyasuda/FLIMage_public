function spc_autoSave
global gui;
global spc;
global state;

try
    name_start=findstr(spc.filename, '\');
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
    eval(['global  ', baseName]);

    save([spc_savePath, baseName, '.mat'], baseName);
    % if strcmp(state.files.baseName, baseName)
    %     eval(['global  ', baseName]);
    %     cd (spc_savePath);
    %     save([spc_savePath, baseName, '.mat'], baseName);
    % else
    % end
end
closereq;