using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace CL3000Demo
{
    internal class Program
    {
        static void Main(string[] args)
        {
            int CurrentDeviceId = 0;
            int returnCode = NativeMethods.CL3IF_OpenUsbCommunication(CurrentDeviceId, 5000);
            Console.WriteLine(returnCode);

            CL3IF_DISPLAYUNIT unit;
            NativeMethods.CL3IF_GetDisplayUnit(CurrentDeviceId, 0, 0, out unit);
            Console.WriteLine(unit.ToString());
            string cmd = string.Empty;
            while (cmd!="quit")
            {
                int value = Read();
                Console.WriteLine(value);
                cmd = Console.ReadLine();
            }

            Console.ReadKey();
            returnCode = NativeMethods.CL3IF_CloseCommunication(0);
        }

        private static int Read()
        {
            int CurrentDeviceId = 0;
            const int MaxRequestDataLength = 512000;
            byte[] buffer = new byte[MaxRequestDataLength];
            using (PinnedObject pin = new PinnedObject(buffer))
            {
                CL3IF_MEASUREMENT_DATA measurementData = new CL3IF_MEASUREMENT_DATA();
                measurementData.outMeasurementData = new CL3IF_OUTMEASUREMENT_DATA[NativeMethods.CL3IF_MAX_OUT_COUNT];

                int returnCode = NativeMethods.CL3IF_GetMeasurementData(CurrentDeviceId, pin.Pointer);
                if (returnCode != NativeMethods.CL3IF_RC_OK)
                {
                    OutputLogMessage("GetMeasurementData", returnCode);
                    return returnCode;
                }

                measurementData.addInfo = (CL3IF_ADD_INFO)Marshal.PtrToStructure(pin.Pointer, typeof(CL3IF_ADD_INFO));
                int readPosition = Marshal.SizeOf(typeof(CL3IF_ADD_INFO));
                for (int i = 0; i < NativeMethods.CL3IF_MAX_OUT_COUNT; i++)
                {
                    measurementData.outMeasurementData[i] = (CL3IF_OUTMEASUREMENT_DATA)Marshal.PtrToStructure(pin.Pointer + readPosition, typeof(CL3IF_OUTMEASUREMENT_DATA));
                    readPosition += Marshal.SizeOf(typeof(CL3IF_OUTMEASUREMENT_DATA));
                }

                //List<LoggingProperty> loggingProperties = new List<LoggingProperty>() { };
                //loggingProperties.Add(new LoggingProperty("triggerCount", measurementData.addInfo.triggerCount.ToString()));
                //loggingProperties.Add(new LoggingProperty("pulseCount", measurementData.addInfo.pulseCount.ToString()));
                //for (int i = 0; i < NativeMethods.CL3IF_MAX_OUT_COUNT; i++)
                //{
                //    string outNumber = "[OUT" + (i + 1) + "]";
                //    loggingProperties.Add(new LoggingProperty(outNumber + "measurementValue", measurementData.outMeasurementData[i].measurementValue.ToString()));
                //    loggingProperties.Add(new LoggingProperty(outNumber + "valueInfo", ((CL3IF_VALUE_INFO)measurementData.outMeasurementData[i].valueInfo).ToString()));
                //    loggingProperties.Add(new LoggingProperty(outNumber + "judgeResult", measurementData.outMeasurementData[i].judgeResult.ToString()));
                //}

                //OutputLogMessage("GetMeasurementData", returnCode, loggingProperties);
                return measurementData.outMeasurementData[0].measurementValue;

            }
        }

        private static void OutputLogMessage(string v, int returnCode)
        {
            Console.WriteLine(v + returnCode.ToString());
        }
        private static void OutputLogMessage(string methodName, int returnCode, IEnumerable<LoggingProperty> loggingProperties)
        {
            string result = returnCode == NativeMethods.CL3IF_RC_OK ? "OK" : "NG(" + returnCode + ")";
            Console.WriteLine(methodName + " " + result);
            if (returnCode == NativeMethods.CL3IF_RC_OK)
            {
                foreach (LoggingProperty property in loggingProperties)
                {
                    Console.WriteLine("    " + property.Name + ":" + property.Value);
                }
            }
        }
    }
}
