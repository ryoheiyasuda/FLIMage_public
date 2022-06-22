function str_all = spc_num2str(nums)
nums = nums(:)';
str_all = [];
left_over = nums;

while length(left_over) > 2
    
    for i=length(left_over):-1:2
        num1 = left_over(1:i);
        dif = num1(2:end) - num1(1:end-1);
        if all (dif == dif(1))
            str1 = sprintf('%d:%d', num1(1), num1(end));
            break;
        end
    end
   % display(str1);

    left_over = left_over(i+1:end);
    if isempty(str_all)
        str_all = str1;
    else
        str_all = [str_all, ', ', str1];
    end
end

if length(left_over) == 1
    if isempty(str_all)
        str_all = num2str(left_over);
    else
        str_all = [str_all, ', ', num2str(left_over)];
    end
end