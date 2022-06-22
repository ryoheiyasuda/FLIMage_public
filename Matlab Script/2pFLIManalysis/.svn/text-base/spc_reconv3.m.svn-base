function y = spc_reconv3(beta0, x)
hw = 1.6; 
filter = exp(-(x-50).^2/2/hw^2);
sum = sum(filter);
%filter = exp(-(x-50).^2/2/hw^2)+exp(-(x-10).^2/2/hw^2);
filter = filter/sum;
pulse_int=80;

background = 50;

%Filter should be "actual" data.
%lifetime = beta0(1)*exp(-(x-beta0(5))/beta0(2)) +...
%           beta0(3)*exp(-(x-beta0(5))/beta0(4)) +...
%            beta0(6);
lifetime = beta0(1)*exp(-(x-beta0(5))/beta0(2))+...
           beta0(3)*exp(-(x-beta0(5))/beta0(4));
w=max(x);
square1 = rectpuls(x-beta0(5)-w/2, w);
lifetime = lifetime.*square1;

y = conv(lifetime, filter);
y = y(:);
y2 = y(pulse_int:1:length(y));
y2 = [y2; zeros(length(y)-length(y2),1)];
y = y + y2;
y = y + background;
%Trim product into X length.
offset = 50;
offset2 = offset+length(x)-1;
y = y(offset:1:offset2);