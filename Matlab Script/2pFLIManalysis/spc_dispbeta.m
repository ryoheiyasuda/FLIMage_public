function spc_dispbeta
global gui;
global spc;

if isfield(spc.fit(spc.currentChannel), 'beta0')
    
	handles = gui.spc.spc_main;
	betahat = spc.fit(spc.currentChannel).beta0;
	
	tau = betahat(2)*spc.datainfo.psPerUnit/1000;
	tau2 = betahat(4)*spc.datainfo.psPerUnit/1000;
	%peaktime = (betahat(5)+range(1))*spc.datainfo.psPerUnit/1000;
    peaktime = (betahat(5))*spc.datainfo.psPerUnit/1000;
    if length(betahat) >= 6
        tau_g = betahat(6)*spc.datainfo.psPerUnit/1000;
    end
	
%     fix1 = spc.fit(spc.currentChannel).fixtau1; % get(gui.spc.spc_main.fixtau1, 'value');
%     fix2 = spc.fit(spc.currentChannel).fixtau2; %get(gui.spc.spc_main.fixtau2, 'value');
%     fix_g = spc.fit(spc.currentChannel).fix_delta; %get(gui.spc.spc_main.fix_g, 'value');
%     fix_d = spc.fit(spc.currentChannel).fix_g; %get(gui.spc.spc_main.fix_delta, 'value');

    if isfield(spc.fit(spc.currentChannel), 'fixtau')
        fixtau = spc.fit(spc.currentChannel).fixtau;
    else
        spc.fit(spc.currentChannel) = zeros(1,6);
        fixtau = zeros(1,6);
    end
        
    set(handles.fixtau1, 'Value', fixtau(2));
    set(handles.fixtau2, 'Value', fixtau(4));
    set(handles.fix_g, 'Value', fixtau(5));
    set(handles.fix_delta, 'Value', fixtau(6));
    
    set(handles.beta1, 'String', num2str(betahat(1)));
    set(handles.beta3, 'String', num2str(betahat(3)));
    set(handles.beta2, 'String', num2str(tau));
    set(handles.beta4, 'String', num2str(tau2));
        

	set(handles.beta5, 'String', num2str(peaktime));
    set(handles.beta6, 'String', num2str(tau_g));
	
	pop1 = betahat(1)/(betahat(3)+betahat(1));
	pop2 = betahat(3)/(betahat(3)+betahat(1));
	set(handles.pop1, 'String', num2str(pop1));
	set(handles.pop2, 'String', num2str(pop2));
    %mean_tau = (tau*tau*pop1+tau2*tau2*pop2)/(tau*pop1 + tau2*pop2);
    if strcmp(spc.fit(spc.currentChannel).fittype, 'singleExp')
        mean_tau= tau;
    else
        mean_tau = (tau*tau*pop1+tau2*tau2*pop2)/(tau*pop1 + tau2*pop2);
    end
	set(handles.average, 'String', num2str(mean_tau));

end

try
    set(handles.F_offset, 'String', num2str(spc.fit(spc.currentChannel).t_offset));
catch
    set(handles.F_offset, 'String', 'NAN');
end

range1 = round(spc.fit(spc.currentChannel).range.*spc.datainfo.psPerUnit/100)/10;
set(handles.spc_fitstart, 'String', num2str(range1(1)));
set(handles.spc_fitend, 'String', num2str(range1(2)));
