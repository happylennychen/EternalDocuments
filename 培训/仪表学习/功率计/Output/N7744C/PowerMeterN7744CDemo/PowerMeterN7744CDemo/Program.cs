using Ivi.Visa;
using NationalInstruments.Visa;

string resourceName = "TCPIP0::192.168.0.1::inst0::INSTR";  // 根据实际情况修改资源名称
string idnResponse;

try
{
    // 创建资源管理器
    var rm = new ResourceManager();

    // 打开会话
    using (var session = rm.Open(resourceName) as IMessageBasedSession)
    {
        if (session == null)
        {
            throw new InvalidOperationException("无法创建会话。");
        }

        // 发送*IDN?查询命令
        session.FormattedIO.WriteLine("*IDN?");
        idnResponse = session.FormattedIO.ReadLine();

        Console.WriteLine("设备标识: " + idnResponse);

        // 配置N7744C参数
        session.FormattedIO.WriteLine("SENS:POW:UNIT DBM");
        session.FormattedIO.WriteLine("INIT:CONT ON");

        // 进行测量
        session.FormattedIO.WriteLine("READ:POW?");
        string power = session.FormattedIO.ReadLine();

        Console.WriteLine("当前功率: " + power + " dBm");
    }
}
catch (Exception ex)
{
    Console.WriteLine("出现错误: " + ex.Message);
}
