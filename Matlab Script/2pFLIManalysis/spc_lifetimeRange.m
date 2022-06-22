function spc_lifetimeRange(range);
%range = [lower, upper]
global spc;
spc.switches.lifetime_limit=range;
figure(spc.figure.lifetimeMap);
handle = gca;
set(handle, 'CLimMode', 'manual', 'CLim', spc.switches.lifetime_limit);
colorbar;