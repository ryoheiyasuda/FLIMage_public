function simFRET
YFRET = 0.5;
tc = 30;
N_init = 1500;
alpha = 1.5;
alphaGR = 0.4
%
RBT = 0.4;
RBTGR = 0.1;
%
ADirect = 0.03;
PAD1 = 0.2;
PAD0 = 0.1;

PAD = [PAD0*ones(1,tc), PAD1*ones(1,tc)];
PD = 1-PAD;
ND1 = PAD*(1-YFRET)+PD;
ND = N_init*(ND1/ND1(1));
ND0 = N_init/ND1(1);

tauD = 2.5;
tauAD = tauD*(1-YFRET);



x = 0:0.2:64*0.2;
tau_theory = (PD*tauD*tauD+PAD*tauAD*tauAD)./(PD*tauD+PAD*tauAD);

xt = 1:length(PAD);
figure; 
subplot(3,2,2)
hold on;
for i = xt
    beta0 = [PD(i),tauD,PAD(i),tauAD,1,0.1];
    [y{i}, tau(i), sum1(i)] = simGaussExp(beta0, x, ND(i));
    if mod(i, 5) == 0
        if i<=tc
            plot(x, y{i});
        else
            plot(x, y{i}, 'color', 'red');
        end
    end
end
ND_ex = poissrnd(ND);
NDGR_ex = poissrnd(ND);


NA = ND0*PAD*YFRET*alpha+ND*RBT+ND0*0.03;
NA_ex = poissrnd(NA);

NAGR = ND0*PAD*YFRET*alphaGR+ND*RBTGR+ND0*0.03;
NAGR_ex = poissrnd(NAGR);
set(gca, 'Box', 'Off')
xlabel('Time (ns)');
ylabel('Number of photons')

%figure; plot(x, y);
subplot(3,2,4)
plot(xt, tau, '-o', 'color', 'blue', 'linewidth', 1, 'MarkerFaceColor','blue');
set(gca, 'Box', 'Off')
ylim([1.9, 2.8]);
xlabel('Trial');
ylabel('Fluorescence lifetime (ns)')
%%%%%
subplot(3,2,1)
plot(xt, ND_ex, '-o', 'color', 'cyan', 'linewidth', 1, 'MarkerFaceColor','cyan');
hold on;
plot(xt, NA_ex, '-o', 'color', [1,0.5,0], 'linewidth', 1, 'MarkerFaceColor',[1,0.5,0]);
set(gca, 'Box', 'Off')
ylim([0,N_init*1.25]);
xlabel('Trial');
ylabel('Number of photons')
%%%%%%
subplot(3,2,3)
plot(xt, NDGR_ex, '-o', 'color', 'green', 'MarkerFaceColor','green');
hold on;
plot(xt, NAGR_ex, '-o', 'color', 'red', 'linewidth', 1, 'MarkerFaceColor','red');
set(gca, 'Box', 'Off')
ylim([0, N_init*1.25]);
xlabel('Trial');
ylabel('Number of photons')
%%%%%
subplot(3,2,5)
plot(xt, NA_ex./ND_ex, '-o', 'color', 'black', 'linewidth', 1, 'MarkerFaceColor','black');
hold on;
plot(xt, NAGR_ex./ND_ex, '-o', 'color', [0.5,0.5,0.5], 'linewidth', 1, 'MarkerFaceColor',[0.5,0.5,0.5]);
set(gca, 'Box', 'Off')
ylim([0,1]);
xlabel('Trial');
ylabel('Ratio')
%%%%%%%%%%%%%%%%%%%%%%% 
%%%%%%%%%%%%%%%%%%%%%%% 
xstart = 0.2;
xend = 0.8;
YFRET=[xstart:0.01:xend];

Dd1 = PAD1*(1-YFRET) + (1-PAD1);
Dd = PAD0*(1-YFRET) + (1-PAD0);

%%%CFP/YFP
R1=(RBT*(1-PAD1*YFRET)+ alpha*YFRET*PAD1 + ADirect)./(1-YFRET*PAD1);
R=(RBT*(1-PAD0*YFRET)+ alpha*YFRET*PAD0 + ADirect)./(1-YFRET*PAD0);
%%%
RR1=(RBTGR*(1-PAD1*YFRET)+ alphaGR*YFRET*PAD1 + ADirect)./(1-YFRET*PAD1);
RR=(RBTGR*(1-PAD0*YFRET)+ alphaGR*YFRET*PAD0 + ADirect)./(1-YFRET*PAD0);
%%%
YFRET2 = 1-YFRET;
tau1 = ((1-PAD1)*2.5^2+PAD1*(2.5*YFRET2).^2)./((1-PAD1)*2.5+PAD1*(2.5*YFRET2));
tau = ((1-PAD0)*2.5^2+PAD0*(2.5*YFRET2).^2)./((1-PAD0)*2.5+PAD0*(2.5*YFRET2));

subplot(3,2,6);
plot(YFRET, ((1+R.^(-1/2)).*R./(R1-R)).^2, 'color', 'black', 'linewidth', 2); 
hold on; 
plot(YFRET, ((1+RR.^(-1/2)).*RR./(RR1-RR)).^2, 'color', [0.5,0.5,0.5], 'linewidth', 2);
plot(YFRET, (tau./(tau-tau1)).^2, 'color', 'blue', 'linewidth', 2); 
plot(YFRET, (Dd./(Dd-Dd1)).^2, 'color', 'green', 'linewidth', 2);
xlim([xstart, xend]);
%ylim([0,800]);
set(gca, 'Box', 'Off')
xlabel('FRET efficiency');
ylabel('Number of photons required')
