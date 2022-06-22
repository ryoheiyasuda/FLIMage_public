function fraction = spc_getFraction(tau_m)
%Check that exp2gauss does not have any offset!!
global spc gui

beta1 = spc.fit(spc.currentChannel).beta0;
range = spc.fit(spc.currentChannel).range;

%figure; hold on;
%tau_m 
t1 = 1:range(2)-range(1)+1;
t1 = t1;
for i=1:110
    fraction1(i) = i-11;    
    beta1(1) = 100*(100-fraction1(i));
    beta1(3) = 100*fraction1(i);
    curve1 = exp2gauss(beta1, t1);
    tau1(i) = sum(t1.*curve1)/sum(curve1); %*spc.datainfo.psPerUnit/1000;
    %plot(t1, curve1/max(curve1)); 
end

%plot(t1, spc.lifetime(range(1):range(2))/max(spc.lifetime), '-r', 'linewidth', 2);
%set(gca, 'Yscale', 'log');

fraction = interp1(tau1, fraction1, tau_m)/100;