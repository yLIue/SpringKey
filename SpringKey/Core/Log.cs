using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Interop;

namespace SpringKey.Core
{
    internal class Log
    {
        public static string Demarcation = "--------------------";

        public static void warning(string _msg)
        {
            Trace.WriteLine($"\n!!!warning: {_msg}\n");
        }

        public static void print(string _msg)
        {
            Trace.WriteLine($"\n{Demarcation}");

            Trace.WriteLine("LogInfo\ntype: string\n");
            Trace.WriteLine(_msg);

            Trace.WriteLine($"{Demarcation}");
        }

        public static void print(List<string> _list)
        {
            Trace.WriteLine($"\n{Demarcation}");

            Trace.WriteLine("LogInfo\ntype: List<string>");
            Trace.WriteLine($"length: {_list.Count}\n");
            for(int i = 0; i < _list.Count; i++)
            {
                Trace.WriteLine($"{i}: <{_list[i]}>");
            }
            Trace.WriteLine("<listEnd>");

            Trace.WriteLine($"{Demarcation}");
        }

        public static void print(int _info)
        {
            Trace.Write($"\n{_info}\n");
        }
    }
}
