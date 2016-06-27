using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pseget
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                DisplayHelp();
                return;
            }
        }

        static void DisplayHelp()
        {
            Console.WriteLine("PSEGet Console. (c) 2016 Arnold Diaz.");
            Console.WriteLine("\tUsage: pseget -t [url | local path] -o [output path] -f [csv:<format> | ami] -d [date format]");
            Console.WriteLine("\t-t [url | local path]");
            Console.WriteLine("\t\tPSE Report location, e.g. \n");
            Console.WriteLine("\t-o [output path]");
            Console.WriteLine("\t\tOptional. Output Path. Defaults to executable path.\n");
            Console.WriteLine("\t-f [csv:<format> | ami:<database path>]");
            Console.WriteLine("\t\tOptional. Target Output. Defaults to CSV.");
            Console.WriteLine("\t\tCSV Optional <format> defaults to S;D;O;H;L;C;V;I\n");            
            Console.WriteLine("\t-d [date format]");
            Console.WriteLine("\t\tOptional Date Format. Defaults to MM/dd/yyyy.\n");
            Console.WriteLine("Example 1 (Defaults)      : pseget -t http:// ");
            Console.WriteLine("Example 2 (Typical)       : pseget -t http:// -o c:\\myfolder\\");
            Console.WriteLine("Example 3 (CSV)           : pseget -t http:// -o c:\\myfolder\\ -f csv:S;D;C");
            Console.WriteLine("Example 4 (Amibroker)     : pseget -t http:// -o c:\\myfolder\\ -f ami:\"c:\\program files\\\"");
        }
    }
}
