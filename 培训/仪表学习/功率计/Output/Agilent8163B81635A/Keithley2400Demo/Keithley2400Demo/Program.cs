using System;
using System.Threading;
using Ivi.Visa;
using NationalInstruments.Visa;

class Program
{
    static private ResourceManager rmSession;
    static private IMessageBasedSession instrument;
    static void Main(string[] args)
    {
        // 设备的资源地址，根据实际连接方式修改
        string resourceAddress = "GPIB0::23::INSTR"; // 示例：GPIB地址为23

        // 创建一个资源管理器
        rmSession = new ResourceManager();

        // 打开设备会话
        instrument = rmSession.Open(resourceAddress) as IMessageBasedSession;

        if (instrument == null)
        {
            Console.WriteLine("无法打开设备会话。");
            return;
        }
        // 配置设备
        //InitKeithley2400();
        string str = string.Empty;
        bool bQuit = false;
        while (!bQuit)
        {
            Console.WriteLine("选择功能: 1.SCPI Query 2. SCPI Write 9. Function q: 退出");
            str = Console.ReadLine();
            switch (str)
            {
                case "1":
                    SCPIQuery();
                    break;
                case "2":
                    SCPIWrite();
                    break;
                case "9":
                    Function();
                    break;
                case "q":
                    bQuit = true;
                    break;
                default:
                    Console.WriteLine("无效命令");
                    break;
            }
        }


        instrument.Dispose();
        rmSession.Dispose();

    }
    private static void Function()
    {
        instrument.FormattedIO.WriteLine($"SENS1:CHAN1:POW:REF:STATE 0");
        instrument.FormattedIO.WriteLine($"*CLS");
        instrument.FormattedIO.WriteLine($"SENS1:CHAN1:POW:RANG:AUTO 1");
        instrument.FormattedIO.WriteLine($"SENS1:CHAN1:POW:UNIT DBM");
        instrument.FormattedIO.WriteLine($"SENS1:CHAN1:POW:ATIME 0.5S");
        instrument.FormattedIO.WriteLine($"INIT1:CHAN1:CONT 0");
        for (int i = 0; i < 10; i++)
        {
            Thread.Sleep(1000);
            instrument.FormattedIO.WriteLine($"READ1:CHAN1:POW?");
            string str = instrument.FormattedIO.ReadLine();
            str.TrimEnd('\n');
            double.TryParse(str, out double d);
            Console.WriteLine($"Read channel 1: {d.ToString()} at {DateTime.Now}");
        }
    }

    private static void SCPIWrite()
    {
        try
        {
            Console.WriteLine("输入SCPI Write指令:");
            string str = Console.ReadLine();
            instrument.FormattedIO.WriteLine(str);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

    private static void SCPIQuery()
    {
        try
        {
            Console.WriteLine("输入SCPI Query指令:");
            string str = Console.ReadLine();
            instrument.FormattedIO.WriteLine(str);
            str = instrument.FormattedIO.ReadLine();
            Console.WriteLine(str);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

}