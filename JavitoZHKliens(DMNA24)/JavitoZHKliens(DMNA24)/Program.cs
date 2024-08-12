using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace JavitoZHKliens_DMNA24_
{
    class Program
    {
        static void Main(string[] args)
        {
            TcpClient csatl = null;
            StreamReader r = null;
            StreamWriter w = null;
            try
            {
                string ipcim = "192.168.178.20";
                string portszam = "2456";
                IPAddress ip = IPAddress.Parse(ipcim);
                int port = int.Parse(portszam);
                csatl = new TcpClient(ipcim, port);
                r = new StreamReader(csatl.GetStream());
                w = new StreamWriter(csatl.GetStream(), Encoding.UTF8);
                Console.WriteLine("Sikerult");
            }
            catch
            {
                csatl = null;
            }
            string udvozles = r.ReadLine();
            Console.WriteLine(udvozles);
            //string lista = r.ReadLine();
            //Console.WriteLine(lista);


            bool end = false;
            while (!end)
            {
                string command = Console.ReadLine();
                w.WriteLine(command);
                w.Flush();
                string answer = r.ReadLine();
                //if (answer=="OK!*")
                //{
                //    while (answer!="OK!")
                //    {
                //        answer = r.ReadLine();
                //        Console.WriteLine(answer);
                //    }
                //}
                //else
                //    Console.WriteLine(answer);end = true;

                switch (answer)
                {
                    case "OK!*":
                        {
                            while (answer != "OK!")
                            {
                                answer = r.ReadLine();
                                Console.WriteLine(answer);
                            }
                        }
                        break;
                    case "BYE":
                        {
                            Console.WriteLine(answer);
                            end = true;
                        }
                        break;
                    default:
                        Console.WriteLine(answer);
                        break;
                }
            }
            Console.WriteLine("Kapcsolat torolve!");
            Console.ReadKey();
        }
    }
}
