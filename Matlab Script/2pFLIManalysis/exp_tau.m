function y=exp_tau(beta0, x)
    thresh = 2/(x(2) - x(1));
    rate = beta0(2);
    if rate > thresh
        rate = thresh;
    elseif -rate > thresh
        rate = -thresh;
    end
    y=exp(-x*rate)*beta0(1)+beta0(3);
end