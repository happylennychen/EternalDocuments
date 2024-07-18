using System;
using System.Threading;
using Ivi.Visa;
using NationalInstruments.Visa;

class Program
{
    static private ResourceManager rm;
    static private IMessageBasedSession session;
    static void Main(string[] args)
    {
        // 设备的资源地址，根据实际连接方式修改
        string resourceAddress = "GPIB0::26::INSTR"; // 示例：GPIB地址为26

        // 创建一个资源管理器
        rm = new ResourceManager();

        // 打开设备会话
        session = rm.Open(resourceAddress) as IMessageBasedSession;

        if (session == null)
        {
            Console.WriteLine("无法打开设备会话。");
            return;
        }
        // 配置设备
        InitKeithley2602();
        string str = string.Empty;
        bool bQuit = false;
        while (!bQuit)
        {
            Console.WriteLine("选择功能: 1.V-I, 2.I-V, 3.GPIB Query 4. GPIB Write 9. Function q: 退出");
            str = Console.ReadLine();
            switch (str)
            {
                case "1":
                    VITest();
                    break;
                case "2":
                    IVTest();
                    break;
                case "3":
                    GPIBQuery();
                    break;
                case "4":
                    GPIBWrite();
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


        session.Dispose();
        rm.Dispose();
        //Console.ReadLine();

    }


    private static void GPIBWrite()
    {
        try
        {
            Console.WriteLine("输入SCPI Write指令:");
            string str = Console.ReadLine();
            session.FormattedIO.WriteLine(str);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            InitKeithley2602();
        }
    }

    private static void GPIBQuery()
    {
        try
        {
            Console.WriteLine("输入SCPI Query指令:");
            string str = Console.ReadLine();
            session.FormattedIO.WriteLine(str);
            str = session.FormattedIO.ReadLine();
            Console.WriteLine(str);
        }
        catch (Exception e)
        {
            session.Clear();
            session.FormattedIO.WriteLine("*RST"); // 复位设备
            Console.WriteLine(e.Message);
        }
    }

    private static void IVTestBadProcedure()    //此流程不行
    {
        session.FormattedIO.WriteLine("smua.reset()");
        session.FormattedIO.WriteLine("smua.source.func = smua.OUTPUT_DCAMPS"); // 设置为电流源模式
        //session.FormattedIO.WriteLine(":SOUR:CURR:MODE FIXED"); // 设置为固定电流模式
        //session.FormattedIO.WriteLine(":SENS:FUNC \"VOLT:DC\""); // 设置测量功能为电压

        string str = string.Empty;
        Console.WriteLine("设定输出电流范围(A):");
        str = Console.ReadLine();
        double sourceCurrentRange = 0;
        if (!double.TryParse(str, out sourceCurrentRange))
        {
            Console.WriteLine("无效值!");
            return;
        }
        session.FormattedIO.WriteLine($"smua.source.rangei = {sourceCurrentRange.ToString()}");
        str = ReadAttributeToString($"print(smua.source.rangei)");
        Console.WriteLine($"输出电流范围被设置为{str}A");

        Console.WriteLine("设定输出电流值(A):");
        str = Console.ReadLine();
        double current = 0;
        if (!double.TryParse(str, out current))
        {
            Console.WriteLine("无效值!");
            return;
        }
        // 设置输出电流
        session.FormattedIO.WriteLine($"smu.source.leveli = {current}");

        Console.WriteLine("设定输出电压保护(V):");
        str = Console.ReadLine();
        double sourceVoltageProtection = 0;
        if (!double.TryParse(str, out sourceVoltageProtection))
        {
            Console.WriteLine("无效值!");
            return;
        }
        session.FormattedIO.WriteLine($"smua.source.limitv = {sourceVoltageProtection.ToString()}");
        str = ReadAttributeToString("print(smua.source.limitv)");
        Console.WriteLine($"输出电压保护被设置为{str}V");

        Console.WriteLine("设定输入电压范围(V):");
        str = Console.ReadLine();
        double senseVoltageRange = 0;
        if (!double.TryParse(str, out senseVoltageRange))
        {
            Console.WriteLine("无效值!");
            return;
        }
        session.FormattedIO.WriteLine($"smua.measure.rangev {senseVoltageRange.ToString()}");
        str = ReadAttributeToString("print(smua.measure.rangev)");
        Console.WriteLine($"输入电压范围被设置为{str}V");

        session.FormattedIO.WriteLine("smua.source.output = smua.OUTPUT_ON"); // 打开输出

        Thread.Sleep(1);
        // 读取电压
        double voltage = MeasureVoltage();

        Console.WriteLine($"测量电压: {voltage} V");
        Console.WriteLine($"计算电流值：{voltage / 2000} A");
        Console.WriteLine($"计算电阻值：{voltage / current} Ohm");
        // 关闭输出
        session.FormattedIO.WriteLine("smua.source.output = smua.OUTPUT_OFF");
    }

    private static void IVTest2()
    {
        Console.WriteLine("设定电流值(V):");
        string str = Console.ReadLine();
        double curr = 0;
        if (!double.TryParse(str, out curr))
        {
            Console.WriteLine("无效值!");
            return;
        }

        session.FormattedIO.WriteLine("smua.source.func = smua.OUTPUT_DCAMPS"); // 设置为电压源模式
        session.FormattedIO.WriteLine($"smua.source.leveli = {curr}");// 设置输出电压
        session.FormattedIO.WriteLine("smua.source.output = smua.OUTPUT_ON"); // 打开输出

        Thread.Sleep(1);
        // 读取电压
        double volt = MeasureVoltage();

        Console.WriteLine($"测量电压: {volt} V");
        Console.WriteLine($"计算电阻值：{volt / curr} Ohm");
        // 关闭输出
        session.FormattedIO.WriteLine("smua.source.output = smua.OUTPUT_OFF");
    }
    private static void IVTest()
    {
        session.FormattedIO.WriteLine("smua.reset()");
        session.FormattedIO.WriteLine("smua.source.func = smua.OUTPUT_DCAMPS"); // 设置为电压源模式

        string str = string.Empty;
        Console.WriteLine("设定输出电流范围(A):");
        str = Console.ReadLine();
        double sourceCurrentRange = 0;
        if (!double.TryParse(str, out sourceCurrentRange))
        {
            Console.WriteLine("无效值!");
            return;
        }
        session.FormattedIO.WriteLine($"smua.source.rangei = {sourceCurrentRange.ToString()}");
        str = ReadAttributeToString($"print(smua.source.rangei)");
        Console.WriteLine($"输出电流范围被设置为{str}A");

        Console.WriteLine("设定电流值(A):");
        str = Console.ReadLine();
        double curr = 0;
        if (!double.TryParse(str, out curr))
        {
            Console.WriteLine("无效值!");
            return;
        }

        session.FormattedIO.WriteLine($"smua.source.leveli = {curr}");// 设置输出电流




        Console.WriteLine("设定输出电压保护(V):");
        str = Console.ReadLine();
        double sourceVoltageProtection = 0;
        if (!double.TryParse(str, out sourceVoltageProtection))
        {
            Console.WriteLine("无效值!");
            return;
        }
        session.FormattedIO.WriteLine($"smua.source.limitv = {sourceVoltageProtection.ToString()}");
        str = ReadAttributeToString("print(smua.source.limitv)");
        Console.WriteLine($"输出电压保护被设置为{str}V");



        Console.WriteLine("设定输入电压范围(V):");
        str = Console.ReadLine();
        double senseVoltageRange = 0;
        if (!double.TryParse(str, out senseVoltageRange))
        {
            Console.WriteLine("无效值!");
            return;
        }
        session.FormattedIO.WriteLine($"smua.measure.rangev {senseVoltageRange.ToString()}");
        str = ReadAttributeToString("print(smua.measure.rangev)");
        Console.WriteLine($"输入电压范围被设置为{str}V");

        session.FormattedIO.WriteLine("smua.source.output = smua.OUTPUT_ON"); // 打开输出

        Thread.Sleep(1);
        // 读取电压
        double volt = MeasureVoltage();

        Console.WriteLine($"测量电压: {volt} V");
        Console.WriteLine($"计算电阻值：{volt / curr} Ohm");
        // 关闭输出
        session.FormattedIO.WriteLine("smua.source.output = smua.OUTPUT_OFF");
    }

    private static void VITest1()
    {
        Console.WriteLine("设定电压值(V):");
        string str = Console.ReadLine();
        double voltage = 0;
        if (!double.TryParse(str, out voltage))
        {
            Console.WriteLine("无效值!");
            return;
        }

        session.FormattedIO.WriteLine("smua.source.func = smua.OUTPUT_DCVOLTS"); // 设置为电压源模式
        session.FormattedIO.WriteLine($"smua.source.levelv = {voltage}");// 设置输出电压
        session.FormattedIO.WriteLine("smua.source.output = smua.OUTPUT_ON"); // 打开输出

        Thread.Sleep(1);
        // 读取电压
        double current = MeasureCurrent();

        Console.WriteLine($"测量电流: {current} A");
        Console.WriteLine($"计算电阻值：{voltage / current} Ohm");
        // 关闭输出
        session.FormattedIO.WriteLine("smua.source.output = smua.OUTPUT_OFF");
    }

    private static void VITest()
    {
        session.FormattedIO.WriteLine("smua.reset()");
        session.FormattedIO.WriteLine("smua.source.func = smua.OUTPUT_DCVOLTS"); // 设置为电压源模式

        string str = string.Empty;
        Console.WriteLine("设定输出电压范围(A):");
        str = Console.ReadLine();
        double sourceVoltageRange = 0;
        if (!double.TryParse(str, out sourceVoltageRange))
        {
            Console.WriteLine("无效值!");
            return;
        }
        session.FormattedIO.WriteLine($"smua.source.rangev = {sourceVoltageRange.ToString()}");
        str = ReadAttributeToString($"print(smua.source.rangev)");
        Console.WriteLine($"输出电压范围被设置为{str}V");

        Console.WriteLine("设定电压值():");
        str = Console.ReadLine();
        double v = 0;
        if (!double.TryParse(str, out v))
        {
            Console.WriteLine("无效值!");
            return;
        }

        session.FormattedIO.WriteLine($"smua.source.levelv = {v}");// 设置输出电压




        Console.WriteLine("设定输出电流保护(V):");
        str = Console.ReadLine();
        double sourceCurrentProtection = 0;
        if (!double.TryParse(str, out sourceCurrentProtection))
        {
            Console.WriteLine("无效值!");
            return;
        }
        session.FormattedIO.WriteLine($"smua.source.limiti = {sourceCurrentProtection.ToString()}");
        str = ReadAttributeToString("print(smua.source.limiti)");
        Console.WriteLine($"输出电流保护被设置为{str}A");



        Console.WriteLine("设定输入电流范围(V):");
        str = Console.ReadLine();
        double senseCurrentRange = 0;
        if (!double.TryParse(str, out senseCurrentRange))
        {
            Console.WriteLine("无效值!");
            return;
        }
        session.FormattedIO.WriteLine($"smua.measure.rangei {senseCurrentRange.ToString()}");
        str = ReadAttributeToString("print(smua.measure.rangei)");
        Console.WriteLine($"输入电流范围被设置为{str}A");

        session.FormattedIO.WriteLine("smua.source.output = smua.OUTPUT_ON"); // 打开输出

        Thread.Sleep(1);
        // 读取电压
        double c = MeasureCurrent();

        Console.WriteLine($"测量电流: {c} A");
        Console.WriteLine($"计算电阻值：{v / c} Ohm");
        // 关闭输出
        session.FormattedIO.WriteLine("smua.source.output = smua.OUTPUT_OFF");
    }

    static void InitKeithley2602()
    {
        session.FormattedIO.WriteLine("*RST");
        session.FormattedIO.WriteLine("*CLS");
    }

    static double MeasureVoltage()
    {
        session.FormattedIO.WriteLine($"print(smua.measure.v())");
        string str = session.FormattedIO.ReadLine();
        double.TryParse(str, out double voltage);
        return voltage;
    }
    static double MeasureCurrent()
    {
        session.FormattedIO.WriteLine($"print(smua.measure.i())");
        string str = session.FormattedIO.ReadLine();
        double.TryParse(str, out double current);
        return current;
    }
    static double ReadAttributeToDouble(string cmd)
    {
        string str = ReadAttributeToString(cmd);
        double.TryParse(str, out double value);
        return value;
    }
    static string ReadAttributeToString(string cmd)
    {
        session.FormattedIO.WriteLine(cmd);
        string str = session.FormattedIO.ReadLine();
        return str;
    }

    private static double GetSourceVoltageRange()
    {
        session.FormattedIO.WriteLine($"print(smua.source.rangev)");
        string str = session.FormattedIO.ReadLine();
        str.TrimEnd('\n');
        double.TryParse(str, out var range);
        return range;
    }

    private static double GetSourceCurrentRange()
    {
        session.FormattedIO.WriteLine($"print(smua.source.rangei)");
        string str = session.FormattedIO.ReadLine();
        str.TrimEnd('\n');
        double.TryParse(str, out var range);
        return range;
    }

    private static void TestID1()
    {
        session.FormattedIO.WriteLine("smua.reset()");
        session.FormattedIO.WriteLine("smua.source.func = smua.OUTPUT_DCVOLTS"); // 设置为电压源模式
        for (double voltage = -41; voltage < 41; voltage += 0.1)
        {
            session.FormattedIO.WriteLine($"smua.source.rangev = {voltage.ToString()}");
            double sovr = GetSourceVoltageRange();
            Console.WriteLine($"Set {voltage.ToString()} Get {sovr.ToString()}");
        }
    }

    private static void TestID2()
    {
        session.FormattedIO.WriteLine("smua.reset()");
        session.FormattedIO.WriteLine("smua.source.func = smua.OUTPUT_DCAMPS");
        double[] list = new double[] { 3.1, 3, 3E-1, 3E-2, 3E-3, 3E-4, 3E-5, 3E-6, 3E-7, 3E-8, 3E-9 };
        foreach (var current in list)
        {
            session.FormattedIO.WriteLine($"smua.source.rangei = {current.ToString()}");
            double sovr = GetSourceCurrentRange();
            Console.WriteLine($"Set {current.ToString()} Get {sovr.ToString()}");
        }
    }

    private static void TestID3()
    {
        session.FormattedIO.WriteLine("smua.reset()");
        session.FormattedIO.WriteLine("smua.source.func = smua.OUTPUT_DCVOLTS"); // 设置为电压源模式
        string str = string.Empty;
        double range = 1;
        session.FormattedIO.WriteLine($"smua.source.rangev = {range.ToString()}");
        double voltage = 3;
        session.FormattedIO.WriteLine($"smua.source.levelv = {voltage.ToString()}");
        session.FormattedIO.WriteLine("print(smua.source.levelv)");
        str = session.FormattedIO.ReadLine();
        Console.WriteLine($"Set {voltage.ToString()} Get {str}");

        voltage = 0.5;
        session.FormattedIO.WriteLine($"smua.source.levelv = {voltage.ToString()}");
        session.FormattedIO.WriteLine("print(smua.source.levelv)");
        str = session.FormattedIO.ReadLine();
        Console.WriteLine($"Set {voltage.ToString()} Get {str}");

        voltage = 3;
        session.FormattedIO.WriteLine($"smua.source.levelv = {voltage.ToString()}");
        session.FormattedIO.WriteLine("print(smua.source.levelv)");
        str = session.FormattedIO.ReadLine();
        Console.WriteLine($"Set {voltage.ToString()} Get {str}");
    }
    private static void TestID4()
    {
        session.FormattedIO.WriteLine("smua.reset()");
        session.FormattedIO.WriteLine("smua.source.func = smua.OUTPUT_DCAMPS");
        string str = string.Empty;
        double range = 0.01;
        session.FormattedIO.WriteLine($"smua.source.rangei = {range.ToString()}");
        double current = 0.2;
        session.FormattedIO.WriteLine($"smua.source.leveli = {current.ToString()}");
        session.FormattedIO.WriteLine("print(smua.source.leveli)");
        str = session.FormattedIO.ReadLine();
        Console.WriteLine($"Set {current.ToString()} Get {str}");

        current = 0.005;
        session.FormattedIO.WriteLine($"smua.source.leveli = {current.ToString()}");
        session.FormattedIO.WriteLine("print(smua.source.leveli)");
        str = session.FormattedIO.ReadLine();
        Console.WriteLine($"Set {current.ToString()} Get {str}");

        current = 0.2;
        session.FormattedIO.WriteLine($"smua.source.leveli = {current.ToString()}");
        session.FormattedIO.WriteLine("print(smua.source.leveli)");
        str = session.FormattedIO.ReadLine();
        Console.WriteLine($"Set {current.ToString()} Get {str}");
    }
    private static void TestID5()
    {
        session.FormattedIO.WriteLine("smua.reset()");
        session.FormattedIO.WriteLine("smua.source.func = smua.OUTPUT_DCAMPS");
        for (double voltage = -40; voltage < 40; voltage += 0.1)
        {
            session.FormattedIO.WriteLine($"smua.source.limitv = {voltage.ToString()}");
            session.FormattedIO.WriteLine("print(smua.source.limitv)");
            string str = string.Empty;
            str = session.FormattedIO.ReadLine();
            str = str.TrimEnd('\n');
            Console.WriteLine($"Set {voltage.ToString()} Get {str}");
        }
    }
    private static void TestID6()
    {
        session.FormattedIO.WriteLine("smua.reset()");
        session.FormattedIO.WriteLine("smua.source.func = smua.OUTPUT_DCVOLTS");
        double[] list = new double[] { 3.1, 3, 3E-1, 3E-2, 3E-3, 3E-4, 3E-5, 3E-6, 3E-7, 3E-8, 3E-9 };
        foreach (var current in list)
        {
            session.FormattedIO.WriteLine($"smua.source.limiti = {current.ToString()}");
            session.FormattedIO.WriteLine("print(smua.source.limiti)");
            string str = string.Empty;
            str = session.FormattedIO.ReadLine();
            str = str.TrimEnd('\n');
            Console.WriteLine($"Set {current.ToString()} Get {str}");
        }
    }
    private static void TestID7()
    {
        session.FormattedIO.WriteLine("smua.reset()");
        session.FormattedIO.WriteLine("smua.source.func = smua.OUTPUT_DCAMPS");
        Console.WriteLine("Source Current");
        double limit = 2;
        session.FormattedIO.WriteLine($"smua.source.limitv = {limit.ToString()}");
        Console.WriteLine($"Set voltage limit to {limit}V");

        double current = 4.0 / 2000.0;
        session.FormattedIO.WriteLine($"smua.source.leveli = {current.ToString()}");
        session.FormattedIO.WriteLine("print(smua.source.leveli)");
        string str = string.Empty;
        str = session.FormattedIO.ReadLine();
        str = str.TrimEnd('\n');
        Console.WriteLine($"Set output current to {str}A");

        session.FormattedIO.WriteLine("smua.source.output = smua.OUTPUT_ON"); // 打开输出

        Thread.Sleep(1);
        // 读取电压
        double v = MeasureVoltage();
        double i = v / 2000;
        // 关闭输出
        session.FormattedIO.WriteLine("smua.source.output = smua.OUTPUT_OFF");
        Console.WriteLine($"Measured Voltage is {v}V, actual current is {i}A");
    }
    private static void TestID8()
    {
        session.FormattedIO.WriteLine("smua.reset()");
        session.FormattedIO.WriteLine("smua.source.func = smua.OUTPUT_DCVOLTS");
        Console.WriteLine("Source Voltage");
        double limit = 0.0001;
        session.FormattedIO.WriteLine($"smua.source.limiti = {limit.ToString()}");
        Console.WriteLine($"Set current limit to {limit}A");

        double voltage = 0.0005 * 2000.0;
        session.FormattedIO.WriteLine($"smua.source.levelv = {voltage.ToString()}");
        session.FormattedIO.WriteLine("print(smua.source.levelv)");
        string str = string.Empty;
        str = session.FormattedIO.ReadLine();
        str = str.TrimEnd('\n');
        Console.WriteLine($"Set output voltage to {str}V");

        session.FormattedIO.WriteLine("smua.source.output = smua.OUTPUT_ON"); // 打开输出

        Thread.Sleep(1);
        double i = MeasureCurrent();
        double v = i * 2000;
        // 关闭输出
        session.FormattedIO.WriteLine("smua.source.output = smua.OUTPUT_OFF");
        Console.WriteLine($"Measured current is {i}A, actual voltage is {v}V");
    }
    private static void TestID9()
    {
        session.FormattedIO.WriteLine("smua.reset()");
        session.FormattedIO.WriteLine("smua.source.func = smua.OUTPUT_DCAMPS");
        for (double voltage = -40; voltage < 40; voltage += 0.1)
        {
            session.FormattedIO.WriteLine($"smua.measure.rangev = {voltage.ToString()}");
            session.FormattedIO.WriteLine("print(smua.measure.rangev)");
            string str = string.Empty;
            str = session.FormattedIO.ReadLine();
            str = str.TrimEnd('\n');
            Console.WriteLine($"Set {voltage.ToString()} Get {str}");
        }
    }
    private static void TestID10()
    {
        session.FormattedIO.WriteLine("smua.reset()");
        session.FormattedIO.WriteLine("smua.source.func = smua.OUTPUT_DCVOLTS");
        double[] list = new double[] { 3.1, 3, 3E-1, 3E-2, 3E-3, 3E-4, 3E-5, 3E-6, 3E-7, 3E-8, 3E-9 };
        foreach (var current in list)
        {
            session.FormattedIO.WriteLine($"smua.measure.rangei = {current.ToString()}");
            session.FormattedIO.WriteLine("print(smua.measure.rangei)");
            string str = string.Empty;
            str = session.FormattedIO.ReadLine();
            str = str.TrimEnd('\n');
            Console.WriteLine($"Set {current.ToString()} Get {str}");
        }
    }

    private static void TestID11()
    {
        session.FormattedIO.WriteLine("smua.reset()");
        session.FormattedIO.WriteLine("smua.source.func = smua.OUTPUT_DCAMPS");
        Console.WriteLine("Source Current");
        double range = 1;
        session.FormattedIO.WriteLine($"smua.measure.rangev = {range.ToString()}");
        Console.WriteLine($"Set voltage range to {range}V");

        double current = 4.0 / 2000.0;
        session.FormattedIO.WriteLine($"smua.source.leveli = {current.ToString()}");
        session.FormattedIO.WriteLine("print(smua.source.leveli)");
        string str = string.Empty;
        str = session.FormattedIO.ReadLine();
        str = str.TrimEnd('\n');
        Console.WriteLine($"Set output current to {str}A");

        session.FormattedIO.WriteLine("smua.source.output = smua.OUTPUT_ON"); // 打开输出

        Thread.Sleep(1);
        // 读取电压
        double v = MeasureVoltage();
        double i = v / 2000;
        // 关闭输出
        session.FormattedIO.WriteLine("smua.source.output = smua.OUTPUT_OFF");
        Console.WriteLine($"Measured Voltage is {v}V, actual current is {i}A");
    }

    private static void TestID12()
    {
        session.FormattedIO.WriteLine("smua.reset()");
        session.FormattedIO.WriteLine("smua.source.func = smua.OUTPUT_DCVOLTS");
        Console.WriteLine("Source Voltage");
        double range = 0.0001;
        session.FormattedIO.WriteLine($"smua.measure.rangei = {range.ToString()}");
        Console.WriteLine($"Set current range to {range}A");

        double voltage = 0.0002 * 2000.0;
        session.FormattedIO.WriteLine($"smua.source.levelv = {voltage.ToString()}");
        session.FormattedIO.WriteLine("print(smua.source.levelv)");
        string str = string.Empty;
        str = session.FormattedIO.ReadLine();
        str = str.TrimEnd('\n');
        Console.WriteLine($"Set output voltage to {str}V");

        session.FormattedIO.WriteLine("smua.source.output = smua.OUTPUT_ON"); // 打开输出

        Thread.Sleep(1);
        // 读取电压
        double i = MeasureCurrent();
        double v = i * 2000;
        // 关闭输出
        session.FormattedIO.WriteLine("smua.source.output = smua.OUTPUT_OFF");
        Console.WriteLine($"Measured current is {i}A, actual voltage is {v}V");
    }

    private static void TestID13()
    {
        session.FormattedIO.WriteLine("smua.reset()");
        session.FormattedIO.WriteLine("smua.source.func = smua.OUTPUT_DCVOLTS");
        Console.WriteLine("Source Voltage");
        double range = 1;
        session.FormattedIO.WriteLine($"smua.source.rangev = {range.ToString()}");
        Console.WriteLine($"Set voltage range to {range}V");

        double voltage = 2;
        session.FormattedIO.WriteLine($"smua.source.levelv = {voltage.ToString()}");
        session.FormattedIO.WriteLine("print(smua.source.levelv)");
        string str = string.Empty;
        str = session.FormattedIO.ReadLine();
        str = str.TrimEnd('\n');
        Console.WriteLine($"Set output voltage to {str}V");

        session.FormattedIO.WriteLine("smua.source.output = smua.OUTPUT_ON"); // 打开输出

        Thread.Sleep(1);
        // 读取电压
        double i = MeasureCurrent();
        double v = i * 2000;
        // 关闭输出
        session.FormattedIO.WriteLine("smua.source.output = smua.OUTPUT_OFF");
        Console.WriteLine($"Measured current is {i}A, actual voltage is {v}V");



        voltage = 0.2;
        session.FormattedIO.WriteLine($"smua.source.levelv = {voltage.ToString()}");
        session.FormattedIO.WriteLine("print(smua.source.levelv)");
        str = session.FormattedIO.ReadLine();
        str = str.TrimEnd('\n');
        Console.WriteLine($"Set output voltage to {str}V");

        session.FormattedIO.WriteLine("smua.source.output = smua.OUTPUT_ON"); // 打开输出

        Thread.Sleep(1);
        // 读取电压
        i = MeasureCurrent();
        v = i * 2000;
        // 关闭输出
        session.FormattedIO.WriteLine("smua.source.output = smua.OUTPUT_OFF");
        Console.WriteLine($"Measured current is {i}A, actual voltage is {v}V");
    }
    /*
    private static void TestID6()
    {
        double range = 20;
        session.FormattedIO.WriteLine($"SOURce:VOLTage:PROT {range.ToString()}");
        double voltage = 40;
        session.FormattedIO.WriteLine($"SOURce:VOLTage:LEVel {voltage.ToString()}");
        session.FormattedIO.WriteLine(":SOUR:VOLT?");
        string str = string.Empty;
        str = session.FormattedIO.ReadLine();
        Console.WriteLine($"Set {voltage.ToString()} Get {str}");

        voltage = 2;
        session.FormattedIO.WriteLine($"SOURce:VOLTage:LEVel {voltage.ToString()}");
        session.FormattedIO.WriteLine(":SOUR:VOLT?");
        str = session.FormattedIO.ReadLine();
        Console.WriteLine($"Set {voltage.ToString()} Get {str}");

        voltage = 40;
        session.FormattedIO.WriteLine($"SOURce:VOLTage:LEVel {voltage.ToString()}");
        session.FormattedIO.WriteLine(":SOUR:VOLT?");
        str = session.FormattedIO.ReadLine();
        Console.WriteLine($"Set {voltage.ToString()} Get {str}");

        voltage = 2;
        session.FormattedIO.WriteLine($"SOURce:VOLTage:LEVel {voltage.ToString()}");
        session.FormattedIO.WriteLine(":SOUR:VOLT?");
        str = session.FormattedIO.ReadLine();
        Console.WriteLine($"Set {voltage.ToString()} Get {str}");
    }
    private static void TestID7()
    {
        for (double voltage = -220; voltage < 220; voltage += 0.1)
        {
            session.FormattedIO.WriteLine($"SOURce:VOLTage:RANGe {voltage.ToString()}");
            double sovr = GetSourceVoltageRange();
            Console.WriteLine($"Set {voltage.ToString()} Get {sovr.ToString()}");
        }
    }
    private static void TestID8()
    {
        double[] list = new double[] { -0.1, 0, 1E-8, 1E-7, 1E-6, 1E-5, 1E-4, 1E-3, 1E-2, 1E-1, 1, 2 };
        foreach (var current in list)
        {
            session.FormattedIO.WriteLine($"SENS:CURR:RANGe {current.ToString()}");
            session.FormattedIO.WriteLine(":SENS:CURR:RANG?");
            string str = string.Empty;
            str = session.FormattedIO.ReadLine();
            Console.WriteLine($"Set {current.ToString()} Get {str}");
        }
    }
    private static void TestID9()
    {
        session.FormattedIO.WriteLine("SOUR:FUNC CURR"); // 设置为电流源模式
        session.FormattedIO.WriteLine("SOUR:CURR:MODE FIXED"); // 设置为固定电流模式
        session.FormattedIO.WriteLine("SENS:FUNC \"VOLT:DC\""); // 设置测量功能为电压

        double current = 0;
        session.FormattedIO.WriteLine($":SOUR:CURR:LEV {current.ToString()}");
        session.FormattedIO.WriteLine("OUTP ON"); // 打开输出
        double range = 0.21;
        session.FormattedIO.WriteLine($":SENS:VOLTage:RANGe {range.ToString()}");
        current = 0.001;
        session.FormattedIO.WriteLine($":SOUR:CURR:LEV {current.ToString()}");
        double voltage = MeasureVoltage();
        Console.WriteLine($"Source {current.ToString()}A Get {voltage}V");

        current = 0.0001;
        session.FormattedIO.WriteLine($":SOUR:CURR:LEV {current.ToString()}");
        voltage = MeasureVoltage();
        Console.WriteLine($"Source {current.ToString()}A Get {voltage}V");

        current = 0.001;
        session.FormattedIO.WriteLine($":SOUR:CURR:LEV {current.ToString()}");
        voltage = MeasureVoltage();
        Console.WriteLine($"Source {current.ToString()}A Get {voltage}V");
        session.FormattedIO.WriteLine("OUTP OFF"); // 打开输出
    }
    private static void TestID10()
    {
        session.FormattedIO.WriteLine("SOUR:FUNC VOLT"); // 设置为电压源模式
        session.FormattedIO.WriteLine("SOUR:VOLT:MODE FIXED"); // 设置为固定电压模式
        session.FormattedIO.WriteLine("SENS:FUNC \"CURR:DC\""); // 设置测量功能为电流

        double V = 0;
        session.FormattedIO.WriteLine($":SOUR:VOLT:LEV {V.ToString()}");
        session.FormattedIO.WriteLine("OUTP ON");
        double range = 0.000105;
        session.FormattedIO.WriteLine($":SENS:CURR:RANGe {range.ToString()}");
        V = 2;
        session.FormattedIO.WriteLine($":SOUR:VOLT:LEV {V.ToString()}");
        double I = MeasureCurrent();
        Console.WriteLine($"Source {V.ToString()}V Get {I}A");

        V = 0.2;
        session.FormattedIO.WriteLine($":SOUR:VOLT:LEV {V.ToString()}");
        I = MeasureCurrent();
        Console.WriteLine($"Source {V.ToString()}V Get {I}A");

        V = 2;
        session.FormattedIO.WriteLine($":SOUR:VOLT:LEV {V.ToString()}");
        I = MeasureCurrent();
        Console.WriteLine($"Source {V.ToString()}V Get {I}A");
        session.FormattedIO.WriteLine("OUTP OFF");
    }
    private static void TestID11()
    {
        for (double voltage = -220; voltage < 220; voltage += 0.1)
        {
            session.FormattedIO.WriteLine($"SENS:VOLTage:PROT {voltage.ToString()}");
            session.FormattedIO.WriteLine(":SENS:VOLT:PROT:LEV?");
            string str = string.Empty;
            str = session.FormattedIO.ReadLine();
            Console.WriteLine($"Set {voltage.ToString()} Get {str}");
        }
    }
    private static void TestID12()
    {
        for (double current = -1; current < 1; current += 0.01)
        {
            session.FormattedIO.WriteLine($"SENS:CURR:PROT {current.ToString()}");
            session.FormattedIO.WriteLine(":SENS:CURR:PROT:LEV?");
            string str = string.Empty;
            str = session.FormattedIO.ReadLine();
            Console.WriteLine($"Set {current.ToString()} Get {str}");
        }
    }
    private static void TestID13()
    {
        session.FormattedIO.WriteLine("SOUR:FUNC CURR"); // 设置为电流源模式
        session.FormattedIO.WriteLine("SOUR:CURR:MODE FIXED"); // 设置为固定电流模式
        session.FormattedIO.WriteLine("SENS:FUNC \"VOLT:DC\""); // 设置测量功能为电压

        double current = 0;
        session.FormattedIO.WriteLine($":SOUR:CURR:LEV {current.ToString()}");
        session.FormattedIO.WriteLine("OUTP ON"); // 打开输出
        double prot = 0.15;
        session.FormattedIO.WriteLine($":SENS:VOLTage:PROT {prot.ToString()}");
        current = 0.0001;
        session.FormattedIO.WriteLine($":SOUR:CURR:LEV {current.ToString()}");
        double voltage = MeasureVoltage();
        Console.WriteLine($"Source {current.ToString()}A Get {voltage}V");

        current = 0.00005;
        session.FormattedIO.WriteLine($":SOUR:CURR:LEV {current.ToString()}");
        voltage = MeasureVoltage();
        Console.WriteLine($"Source {current.ToString()}A Get {voltage}V");

        current = 0.0001;
        session.FormattedIO.WriteLine($":SOUR:CURR:LEV {current.ToString()}");
        voltage = MeasureVoltage();
        Console.WriteLine($"Source {current.ToString()}A Get {voltage}V");
        session.FormattedIO.WriteLine("OUTP OFF"); // 打开输出
    }
    private static void TestID14()
    {
        session.FormattedIO.WriteLine("SOUR:FUNC VOLT"); // 设置为电压源模式
        session.FormattedIO.WriteLine("SOUR:VOLT:MODE FIXED"); // 设置为固定电压模式
        session.FormattedIO.WriteLine("SENS:FUNC \"CURR:DC\""); // 设置测量功能为电流

        double V = 0;
        session.FormattedIO.WriteLine($":SOUR:VOLT:LEV {V.ToString()}");
        session.FormattedIO.WriteLine("OUTP ON");
        double range = 0.00008;
        session.FormattedIO.WriteLine($":SENS:CURR:PROT {range.ToString()}");
        V = 2;
        session.FormattedIO.WriteLine($":SOUR:VOLT:LEV {V.ToString()}");
        double I = MeasureCurrent();
        Console.WriteLine($"Source {V.ToString()}V Get {I}A");

        V = 0.05;
        session.FormattedIO.WriteLine($":SOUR:VOLT:LEV {V.ToString()}");
        I = MeasureCurrent();
        Console.WriteLine($"Source {V.ToString()}V Get {I}A");

        V = 2;
        session.FormattedIO.WriteLine($":SOUR:VOLT:LEV {V.ToString()}");
        I = MeasureCurrent();
        Console.WriteLine($"Source {V.ToString()}V Get {I}A");
        session.FormattedIO.WriteLine("OUTP OFF");
    }
    */
    private static void Function()
    {
        Console.WriteLine("请输入测试ID号");
        int.TryParse(Console.ReadLine(), out int id);
        switch (id)
        {
            case 1: TestID1(); break;
            case 2: TestID2(); break;
            case 3: TestID3(); break;
            case 4: TestID4(); break;
            case 5: TestID5(); break;
            case 6: TestID6(); break;
            case 7: TestID7(); break;
            case 8: TestID8(); break;
            case 9: TestID9(); break;
            case 10: TestID10(); break;
            case 11: TestID11(); break;
            case 12: TestID12(); break;
            case 13: TestID13(); break;
            default: Console.WriteLine("Wrong ID!"); break;
        }
    }
}