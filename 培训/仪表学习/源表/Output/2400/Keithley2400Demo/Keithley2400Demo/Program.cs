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
        string resourceAddress = "GPIB0::21::INSTR"; // 示例：GPIB地址为21

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
        InitKeithley2400();
        string str = string.Empty;
        bool bQuit = false;
        while (!bQuit)
        {
            Console.WriteLine("选择功能: 1.V-I, 2.I-V, 3.:SOUR:VOLT:RANG, 4.:SOUR:VOLT:LEV, 5.:SENS:CURR:PROT, 6.:SENS:CURR:RANG, 7.SCPI Query 8. SCPI Write 9. Function q: 退出");
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
                    //SetSenseVoltagePROT();
                    SetSourceVoltageRange();
                    double sovr = GetSourceVoltageRange();
                    Console.WriteLine($"Source Voltage Range is {sovr.ToString()}V");
                    break;
                case "4":
                    SetSourceVoltageLimit();
                    double sovl = GetSourceVoltageLimit();
                    Console.WriteLine($"Source Voltage Limit is {sovl.ToString()}V");
                    break;
                case "5":
                    SetSenseCurrentLimit();
                    double secl = GetSenseCurrentLimit();
                    Console.WriteLine($"Sense Current Limit is {secl.ToString()}A");
                    break;
                case "6":
                    SetSenseCurrentRange();
                    double secr = GetSenseCurrentRange();
                    Console.WriteLine($"Sense Current Range is {secr.ToString()}A");
                    break;
                case "7":
                    SCPIQuery();
                    break;
                case "8":
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
        //Console.ReadLine();

    }
    private static void TestID1()
    {
        for (double voltage = -220; voltage < 220; voltage += 0.1)
        {
            instrument.FormattedIO.WriteLine($"SOURce:VOLTage:RANGe {voltage.ToString()}");
            double sovr = GetSourceVoltageRange();
            Console.WriteLine($"Set {voltage.ToString()} Get {sovr.ToString()}");
        }
    }
    private static void TestID2()
    {
        double[] list = new double[] { -0.1, 0, 1E-8, 1E-7, 1E-6, 1E-5, 1E-4, 1E-3, 1E-2, 1E-1, 1, 2 };
        foreach (var current in list)
        {
            instrument.FormattedIO.WriteLine($"SOURce:CURR:RANGe {current.ToString()}");
            double sovr = GetSourceCurrentRange();
            Console.WriteLine($"Set {current.ToString()} Get {sovr.ToString()}");
        }
    }
    private static void TestID3()
    {
        double range = 2.1;
        instrument.FormattedIO.WriteLine($"SOURce:VOLTage:RANGe {range.ToString()}");
        double voltage = 3;
        instrument.FormattedIO.WriteLine($"SOURce:VOLTage:LEVel {voltage.ToString()}");
        instrument.FormattedIO.WriteLine(":SOUR:VOLT?");
        string str = string.Empty;
        str = instrument.FormattedIO.ReadLine();
        Console.WriteLine($"Set {voltage.ToString()} Get {str}");

        voltage = 2;
        instrument.FormattedIO.WriteLine($"SOURce:VOLTage:LEVel {voltage.ToString()}");
        instrument.FormattedIO.WriteLine(":SOUR:VOLT?");
        str = instrument.FormattedIO.ReadLine();
        Console.WriteLine($"Set {voltage.ToString()} Get {str}");

        voltage = 3;
        instrument.FormattedIO.WriteLine($"SOURce:VOLTage:LEVel {voltage.ToString()}");
        instrument.FormattedIO.WriteLine(":SOUR:VOLT?");
        str = instrument.FormattedIO.ReadLine();
        Console.WriteLine($"Set {voltage.ToString()} Get {str}");
    }
    private static void TestID4()
    {
        double range = 0.00105;
        instrument.FormattedIO.WriteLine($"SOURce:CURR:RANGe {range.ToString()}");
        double current = 1;
        instrument.FormattedIO.WriteLine($"SOURce:CURR:LEVel {current.ToString()}");
        instrument.FormattedIO.WriteLine(":SOUR:CURR?");
        string str = string.Empty;
        str = instrument.FormattedIO.ReadLine();
        Console.WriteLine($"Set {current.ToString()} Get {str}");

        current = 0.001;
        instrument.FormattedIO.WriteLine($"SOURce:CURR:LEVel {current.ToString()}");
        instrument.FormattedIO.WriteLine(":SOUR:CURR?");
        str = instrument.FormattedIO.ReadLine();
        Console.WriteLine($"Set {current.ToString()} Get {str}");

        current = 1;
        instrument.FormattedIO.WriteLine($"SOURce:CURR:LEVel {current.ToString()}");
        instrument.FormattedIO.WriteLine(":SOUR:CURR?");
        str = instrument.FormattedIO.ReadLine();
        Console.WriteLine($"Set {current.ToString()} Get {str}");
    }
    private static void TestID5()
    {
        for (double voltage = -220; voltage < 220; voltage++)
        {
            instrument.FormattedIO.WriteLine($"SOURce:VOLTage:PROT {voltage.ToString()}");
            instrument.FormattedIO.WriteLine(":SOUR:VOLT:PROT?");
            string str = string.Empty;
            str = instrument.FormattedIO.ReadLine();
            Console.WriteLine($"Set {voltage.ToString()} Get {str}");
        }
    }
    private static void TestID6()
    {
        double range = 20;
        instrument.FormattedIO.WriteLine($"SOURce:VOLTage:PROT {range.ToString()}");
        double voltage = 40;
        instrument.FormattedIO.WriteLine($"SOURce:VOLTage:LEVel {voltage.ToString()}");
        instrument.FormattedIO.WriteLine(":SOUR:VOLT?");
        string str = string.Empty;
        str = instrument.FormattedIO.ReadLine();
        Console.WriteLine($"Set {voltage.ToString()} Get {str}");

        voltage = 2;
        instrument.FormattedIO.WriteLine($"SOURce:VOLTage:LEVel {voltage.ToString()}");
        instrument.FormattedIO.WriteLine(":SOUR:VOLT?");
        str = instrument.FormattedIO.ReadLine();
        Console.WriteLine($"Set {voltage.ToString()} Get {str}");

        voltage = 40;
        instrument.FormattedIO.WriteLine($"SOURce:VOLTage:LEVel {voltage.ToString()}");
        instrument.FormattedIO.WriteLine(":SOUR:VOLT?");
        str = instrument.FormattedIO.ReadLine();
        Console.WriteLine($"Set {voltage.ToString()} Get {str}");

        voltage = 2;
        instrument.FormattedIO.WriteLine($"SOURce:VOLTage:LEVel {voltage.ToString()}");
        instrument.FormattedIO.WriteLine(":SOUR:VOLT?");
        str = instrument.FormattedIO.ReadLine();
        Console.WriteLine($"Set {voltage.ToString()} Get {str}");
    }
    private static void TestID7()
    {
        for (double voltage = -220; voltage < 220; voltage += 0.1)
        {
            instrument.FormattedIO.WriteLine($"SOURce:VOLTage:RANGe {voltage.ToString()}");
            double sovr = GetSourceVoltageRange();
            Console.WriteLine($"Set {voltage.ToString()} Get {sovr.ToString()}");
        }
    }
    private static void TestID8()
    {
        double[] list = new double[] { -0.1, 0, 1E-8, 1E-7, 1E-6, 1E-5, 1E-4, 1E-3, 1E-2, 1E-1, 1, 2 };
        foreach (var current in list)
        {
            instrument.FormattedIO.WriteLine($"SENS:CURR:RANGe {current.ToString()}");
            instrument.FormattedIO.WriteLine(":SENS:CURR:RANG?");
            string str = string.Empty;
            str = instrument.FormattedIO.ReadLine();
            Console.WriteLine($"Set {current.ToString()} Get {str}");
        }
    }
    private static void TestID9()
    {
        instrument.FormattedIO.WriteLine("SOUR:FUNC CURR"); // 设置为电流源模式
        instrument.FormattedIO.WriteLine("SOUR:CURR:MODE FIXED"); // 设置为固定电流模式
        instrument.FormattedIO.WriteLine("SENS:FUNC \"VOLT:DC\""); // 设置测量功能为电压

        double current = 0;
        instrument.FormattedIO.WriteLine($":SOUR:CURR:LEV {current.ToString()}");
        instrument.FormattedIO.WriteLine("OUTP ON"); // 打开输出
        double range = 0.21;
        instrument.FormattedIO.WriteLine($":SENS:VOLTage:RANGe {range.ToString()}");
        current = 0.001;
        instrument.FormattedIO.WriteLine($":SOUR:CURR:LEV {current.ToString()}");
        double voltage = MeasureVoltage();
        Console.WriteLine($"Source {current.ToString()}A Get {voltage}V");

        current = 0.0001;
        instrument.FormattedIO.WriteLine($":SOUR:CURR:LEV {current.ToString()}");
        voltage = MeasureVoltage();
        Console.WriteLine($"Source {current.ToString()}A Get {voltage}V");

        current = 0.001;
        instrument.FormattedIO.WriteLine($":SOUR:CURR:LEV {current.ToString()}");
        voltage = MeasureVoltage();
        Console.WriteLine($"Source {current.ToString()}A Get {voltage}V");
        instrument.FormattedIO.WriteLine("OUTP OFF"); // 打开输出
    }
    private static void TestID10()
    {
        instrument.FormattedIO.WriteLine("SOUR:FUNC VOLT"); // 设置为电压源模式
        instrument.FormattedIO.WriteLine("SOUR:VOLT:MODE FIXED"); // 设置为固定电压模式
        instrument.FormattedIO.WriteLine("SENS:FUNC \"CURR:DC\""); // 设置测量功能为电流

        double V = 0;
        instrument.FormattedIO.WriteLine($":SOUR:VOLT:LEV {V.ToString()}");
        instrument.FormattedIO.WriteLine("OUTP ON");
        double range = 0.000105;
        instrument.FormattedIO.WriteLine($":SENS:CURR:RANGe {range.ToString()}");
        V = 2;
        instrument.FormattedIO.WriteLine($":SOUR:VOLT:LEV {V.ToString()}");
        double I = MeasureCurrent();
        Console.WriteLine($"Source {V.ToString()}V Get {I}A");

        V = 0.2;
        instrument.FormattedIO.WriteLine($":SOUR:VOLT:LEV {V.ToString()}");
        I = MeasureCurrent();
        Console.WriteLine($"Source {V.ToString()}V Get {I}A");

        V = 2;
        instrument.FormattedIO.WriteLine($":SOUR:VOLT:LEV {V.ToString()}");
        I = MeasureCurrent();
        Console.WriteLine($"Source {V.ToString()}V Get {I}A");
        instrument.FormattedIO.WriteLine("OUTP OFF");
    }
    private static void TestID11()
    {
        for (double voltage = -220; voltage < 220; voltage += 0.1)
        {
            instrument.FormattedIO.WriteLine($"SENS:VOLTage:PROT {voltage.ToString()}");
            instrument.FormattedIO.WriteLine(":SENS:VOLT:PROT:LEV?");
            string str = string.Empty;
            str = instrument.FormattedIO.ReadLine();
            Console.WriteLine($"Set {voltage.ToString()} Get {str}");
        }
    }
    private static void TestID12()
    {
        for (double current = -1; current < 1; current += 0.01)
        {
            instrument.FormattedIO.WriteLine($"SENS:CURR:PROT {current.ToString()}");
            instrument.FormattedIO.WriteLine(":SENS:CURR:PROT:LEV?");
            string str = string.Empty;
            str = instrument.FormattedIO.ReadLine();
            Console.WriteLine($"Set {current.ToString()} Get {str}");
        }
    }
    private static void TestID13()
    {
        instrument.FormattedIO.WriteLine("SOUR:FUNC CURR"); // 设置为电流源模式
        instrument.FormattedIO.WriteLine("SOUR:CURR:MODE FIXED"); // 设置为固定电流模式
        instrument.FormattedIO.WriteLine("SENS:FUNC \"VOLT:DC\""); // 设置测量功能为电压

        double current = 0;
        instrument.FormattedIO.WriteLine($":SOUR:CURR:LEV {current.ToString()}");
        instrument.FormattedIO.WriteLine("OUTP ON"); // 打开输出
        double prot = 0.15;
        instrument.FormattedIO.WriteLine($":SENS:VOLTage:PROT {prot.ToString()}");
        current = 0.0001;
        instrument.FormattedIO.WriteLine($":SOUR:CURR:LEV {current.ToString()}");
        double voltage = MeasureVoltage();
        Console.WriteLine($"Source {current.ToString()}A Get {voltage}V");

        current = 0.00005;
        instrument.FormattedIO.WriteLine($":SOUR:CURR:LEV {current.ToString()}");
        voltage = MeasureVoltage();
        Console.WriteLine($"Source {current.ToString()}A Get {voltage}V");

        current = 0.0001;
        instrument.FormattedIO.WriteLine($":SOUR:CURR:LEV {current.ToString()}");
        voltage = MeasureVoltage();
        Console.WriteLine($"Source {current.ToString()}A Get {voltage}V");
        instrument.FormattedIO.WriteLine("OUTP OFF"); // 打开输出
    }
    private static void TestID14()
    {
        instrument.FormattedIO.WriteLine("SOUR:FUNC VOLT"); // 设置为电压源模式
        instrument.FormattedIO.WriteLine("SOUR:VOLT:MODE FIXED"); // 设置为固定电压模式
        instrument.FormattedIO.WriteLine("SENS:FUNC \"CURR:DC\""); // 设置测量功能为电流

        double V = 0;
        instrument.FormattedIO.WriteLine($":SOUR:VOLT:LEV {V.ToString()}");
        instrument.FormattedIO.WriteLine("OUTP ON");
        double range = 0.00008;
        instrument.FormattedIO.WriteLine($":SENS:CURR:PROT {range.ToString()}");
        V = 2;
        instrument.FormattedIO.WriteLine($":SOUR:VOLT:LEV {V.ToString()}");
        double I = MeasureCurrent();
        Console.WriteLine($"Source {V.ToString()}V Get {I}A");

        V = 0.05;
        instrument.FormattedIO.WriteLine($":SOUR:VOLT:LEV {V.ToString()}");
        I = MeasureCurrent();
        Console.WriteLine($"Source {V.ToString()}V Get {I}A");

        V = 2;
        instrument.FormattedIO.WriteLine($":SOUR:VOLT:LEV {V.ToString()}");
        I = MeasureCurrent();
        Console.WriteLine($"Source {V.ToString()}V Get {I}A");
        instrument.FormattedIO.WriteLine("OUTP OFF");
    }
    private static void Function()
    {
        TestID14();
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
            InitKeithley2400();
        }
    }

    private static double GetSourceVoltageLimit()
    {
        instrument.FormattedIO.WriteLine(":SOUR:VOLT:PROT:LEV?");
        string str = string.Empty;
        str = instrument.FormattedIO.ReadLine();
        double.TryParse(str, out double svr);
        return svr;
    }
    private static double GetSenseCurrentLimit()
    {
        instrument.FormattedIO.WriteLine(":SENS:CURR:PROT:LEV?");
        string str = string.Empty;
        str = instrument.FormattedIO.ReadLine();
        double.TryParse(str, out double svr);
        return svr;
    }

    private static double GetSourceVoltageRange()
    {
        instrument.FormattedIO.WriteLine(":SOUR:VOLT:RANG?");
        string str = string.Empty;
        str = instrument.FormattedIO.ReadLine();
        double.TryParse(str, out double svr);
        return svr;
    }
    private static double GetSourceCurrentRange()
    {
        instrument.FormattedIO.WriteLine(":SOUR:CURR:RANG?");
        string str = string.Empty;
        str = instrument.FormattedIO.ReadLine();
        double.TryParse(str, out double scr);
        return scr;
    }
    private static double GetSenseCurrentRange()
    {
        instrument.FormattedIO.WriteLine(":SENS:CURR:RANG?");
        string str = string.Empty;
        str = instrument.FormattedIO.ReadLine();
        double.TryParse(str, out double svr);
        return svr;
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
            instrument.Clear();
            instrument.FormattedIO.WriteLine("*RST"); // 复位设备
            Console.WriteLine(e.Message);
        }
    }

    private static void SetSenseCurrentRange()
    {
        Console.WriteLine("SetSenseCurrentRange:");
        string str = Console.ReadLine();
        double current = 0;
        if (!double.TryParse(str, out current))
        {
            Console.WriteLine("无效值!");
            return;
        }
        instrument.FormattedIO.WriteLine($"SENSe:CURRent:RANGe {current.ToString()}");
    }

    private static void SetSourceVoltageLimit()
    {
        Console.WriteLine("SetSourceVoltageLimit:");
        string str = Console.ReadLine();
        double voltage = 0;
        if (!double.TryParse(str, out voltage))
        {
            Console.WriteLine("无效值!");
            return;
        }
        //instrument.FormattedIO.WriteLine($"SOURce:VOLTage:LEVEl {voltage.ToString()}"); //为什么是这样的命令格式？UG里面明明是SOUR:VOLT:PROT[:LEV]
        //instrument.FormattedIO.WriteLine($"SOURce:VOLTage:PROT:LEVEl {voltage.ToString()}");
        instrument.FormattedIO.WriteLine($"SOURce:VOLTage:PROT {voltage.ToString()}");
    }

    private static void SetSourceVoltageRange()
    {
        Console.WriteLine("SetSourceVoltageRange:");
        string str = Console.ReadLine();
        double voltage = 0;
        if (!double.TryParse(str, out voltage))
        {
            Console.WriteLine("无效值!");
            return;
        }
        instrument.FormattedIO.WriteLine($"SOURce:VOLTage:RANGe {voltage.ToString()}");
    }

    private static void SetSenseCurrentLimit()
    {
        Console.WriteLine("SetSenseCurrentProtection:");
        string str = Console.ReadLine();
        double current = 0;
        if (!double.TryParse(str, out current))
        {
            Console.WriteLine("无效值!");
            return;
        }
        instrument.FormattedIO.WriteLine($"SENSe:CURRent:PROTection {current.ToString()}");
    }

    private static void SetSenseVoltagePROT()
    {
        Console.WriteLine("设定电压保护门限值(V):");
        string str = Console.ReadLine();
        double voltage = 0;
        if (!double.TryParse(str, out voltage))
        {
            Console.WriteLine("无效值!");
            return;
        }
        instrument.FormattedIO.WriteLine($"SENSe:VOLTage:PROTection {voltage.ToString()}"); // 设置电压保护值，单位是V
    }

    private static void IVTest()
    {
        instrument.FormattedIO.WriteLine(":SOUR:FUNC CURR"); // 设置为电流源模式
        instrument.FormattedIO.WriteLine(":SOUR:CURR:MODE FIXED"); // 设置为固定电流模式
        instrument.FormattedIO.WriteLine(":SENS:FUNC \"VOLT:DC\""); // 设置测量功能为电压

        string str = string.Empty;
        Console.WriteLine("设定输出电流范围(A):");
        str = Console.ReadLine();
        double sourceCurrentRange = 0;
        if (!double.TryParse(str, out sourceCurrentRange))
        {
            Console.WriteLine("无效值!");
            return;
        }
        instrument.FormattedIO.WriteLine($":SOUR:CURR:RANG {sourceCurrentRange.ToString()}");
        instrument.FormattedIO.WriteLine($":SOUR:CURR:RANG?");
        str = instrument.FormattedIO.ReadLine();
        str = str.TrimEnd('\n');
        Console.WriteLine($"输出电流范围被设置为{str}A");

        Console.WriteLine("设定输出电压保护(V):");
        str = Console.ReadLine();
        double sourceVoltageProtection = 0;
        if (!double.TryParse(str, out sourceVoltageProtection))
        {
            Console.WriteLine("无效值!");
            return;
        }
        instrument.FormattedIO.WriteLine($":SOUR:VOLT:PROT {sourceVoltageProtection.ToString()}");
        instrument.FormattedIO.WriteLine($":SOUR:VOLT:PROT?");
        str = instrument.FormattedIO.ReadLine();
        str = str.TrimEnd('\n');
        Console.WriteLine($"输出电压保护被设置为{str}V");

        Console.WriteLine("设定输入电压保护(V):");
        str = Console.ReadLine();
        double senseVoltageProtection = 0;
        if (!double.TryParse(str, out senseVoltageProtection))
        {
            Console.WriteLine("无效值!");
            return;
        }
        instrument.FormattedIO.WriteLine($":SENS:VOLT:PROT {senseVoltageProtection.ToString()}");
        instrument.FormattedIO.WriteLine($":SOUR:VOLT:PROT?");
        str = instrument.FormattedIO.ReadLine();
        str = str.TrimEnd('\n');
        Console.WriteLine($"输入电压保护被设置为{str}V");

        Console.WriteLine("设定输入电压范围(V):");
        str = Console.ReadLine();
        double senseVoltageRange = 0;
        if (!double.TryParse(str, out senseVoltageRange))
        {
            Console.WriteLine("无效值!");
            return;
        }
        instrument.FormattedIO.WriteLine($":SENS:VOLT:RANG {senseVoltageRange.ToString()}");
        instrument.FormattedIO.WriteLine($":SOUR:VOLT:RANG?");
        str = instrument.FormattedIO.ReadLine();
        str = str.TrimEnd('\n');
        Console.WriteLine($"输入电压范围被设置为{str}V");

        Console.WriteLine("设定输出电流值(A):");
        str = Console.ReadLine();
        double current = 0;
        if (!double.TryParse(str, out current))
        {
            Console.WriteLine("无效值!");
            return;
        }

        // 设置输出电流
        instrument.FormattedIO.WriteLine($":SOUR:CURR {current}");

        instrument.FormattedIO.WriteLine(":OUTP ON"); // 打开输出

        Thread.Sleep(1);
        // 读取电压
        double voltage = MeasureVoltage();

        Console.WriteLine($"测量电压: {voltage} V");
        Console.WriteLine($"计算电流值：{voltage / 2000} A");
        // 关闭输出
        instrument.FormattedIO.WriteLine(":OUTP OFF");
    }

    private static void VITest()
    {
        Console.WriteLine("设定电压值(V):");
        string str = Console.ReadLine();
        double voltage = 0;
        if (!double.TryParse(str, out voltage))
        {
            Console.WriteLine("无效值!");
            return;
        }

        instrument.FormattedIO.WriteLine("SOUR:FUNC VOLT"); // 设置为电压源模式
        instrument.FormattedIO.WriteLine("SOUR:VOLT:MODE FIXED"); // 设置为固定电压模式
        instrument.FormattedIO.WriteLine("SENS:FUNC \"CURR:DC\""); // 设置测量功能为电流
        // 设置输出电流
        instrument.FormattedIO.WriteLine($"SOUR:VOLT {voltage}");

        instrument.FormattedIO.WriteLine("OUTP ON"); // 打开输出

        Thread.Sleep(1);
        // 读取电压
        double current = MeasureCurrent();

        Console.WriteLine($"测量电流: {current} A");
        Console.WriteLine($"计算电阻值：{voltage / current} Ohm");
        // 关闭输出
        instrument.FormattedIO.WriteLine("OUTP OFF");
    }

    static void InitKeithley2400()
    {
        // 清除设备状态
        instrument.Clear();

        // 设置为远程模式
        instrument.FormattedIO.WriteLine("*RST"); // 复位设备

        instrument.FormattedIO.WriteLine($":CURR:RANG:AUTO 1");//电流AutoRange
        instrument.FormattedIO.WriteLine($":VOLT:RANG:AUTO 1");//电压AutoRange

        instrument.FormattedIO.WriteLine(":SYST:RSEN OFF"); // 禁用远程测量
        instrument.FormattedIO.WriteLine(":SENSe:CURRent:PROTection 0.01"); // 设置电流保护值，单位是A 
        instrument.FormattedIO.WriteLine(":SENSe:VOLTage:PROTection 5"); // 设置电流保护值，单位是V
    }

    static double MeasureVoltage()
    {
        // 触发测量
        instrument.FormattedIO.WriteLine("INIT");

        // 读取测量结果
        instrument.FormattedIO.WriteLine("READ?");
        string result = instrument.FormattedIO.ReadLine();

        // 解析测量结果，获取电压值（假设电压值是返回的第一个值）
        string[] values = result.Split(',');
        //foreach (string value in values)
        //{
        //    Console.WriteLine(value);
        //}
        if (values.Length > 0 && double.TryParse(values[0], out double voltage))
        {
            return voltage;
        }
        else
        {
            throw new InvalidOperationException("无法解析测量结果。");
        }
    }
    static double MeasureCurrent()
    {
        // 触发测量
        instrument.FormattedIO.WriteLine("INIT");

        // 读取测量结果
        instrument.FormattedIO.WriteLine("READ?");
        string result = instrument.FormattedIO.ReadLine();

        // 解析测量结果，获取电压值（假设电压值是返回的第二个值）
        string[] values = result.Split(',');
        //foreach (string value in values)
        //{
        //    Console.WriteLine(value);
        //}
        if (values.Length > 0 && double.TryParse(values[1], out double current))
        {
            return current;
        }
        else
        {
            throw new InvalidOperationException("无法解析测量结果。");
        }
    }
}