classdef FLIM_Com
    
    properties
        flimCom;
    end
        
    methods        
        function obj = FLIM_Com()
            try
                obj = start(obj);
            catch
                disp('Server is not ready yet');
            end
        end
        
        function obj = start(obj)
            obj.flimCom = pipe_Client.pipeClient;
            setListener(obj);
            obj.flimCom.startReceiving();
       end
        
        function r=setListener(obj)
            r=addlistener(obj.flimCom,'r_tick', @obj.receivedMessageEvent);
        end
        
        function strR = sendMessage(obj, str)
            strR = obj.flimCom.sendCommand(str);
            disp(strR);
        end
        
        function receivedMessageEvent(obj, src, event)
            disp(obj.flimCom.Received);
        end
        
        function active = checkActiveRestart(obj)
            active = obj.flimCom.Client_On;
            if (~active)
                restart(obj);
            end
        end
        
        function restart(obj)
            close(obj);
            start(obj);
        end
        
        function close(obj)
            obj.flimCom.Close();
        end
    end
end