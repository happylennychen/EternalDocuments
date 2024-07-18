using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CL3000Demo
{
    public class LoggingProperty
    {
        public string Name { get; private set; }
        public string Value { get; private set; }

        public LoggingProperty(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
}
