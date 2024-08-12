using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration; 
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;


namespace JavitoZHServerDMNA24
{
    class Program
    {
        static List<Thread> futoSzalak = new List<Thread>();
        static TcpListener figyelo = null;
        static Thread kapcsolatok = null;
        static List<User> userok = new List<User>();
        static List<Auto> autok = new List<Auto>();        

        class Auto
        {
            private string rendszam;

            public string Rendszam
            {
                get { return rendszam; }
                set { rendszam = value; }
            }

            private string tipus;

            public string Tipus
            {
                get { return tipus; }
                set { tipus = value; }
            }

            private string marka;

            public string Marka
            {
                get { return marka; }
                set { marka = value; }
            }

            private int km;

            public int KM
            {
                get { return km; }
                set { km = value; }
            }

            private bool kolcsonozveVan;

            public bool KolcsonozveVan
            {
                get { return kolcsonozveVan; }
                set { kolcsonozveVan = value; }
            }

            private User kivette;

            public User Kivette
            {
                get { return kivette; }
                set { kivette = value; }
            }

            public Auto(string rendszam, string tipus, string marka, int km)
            {
                this.Rendszam = rendszam;
                this.Tipus = tipus;
                this.Marka = marka;
                this.KM = km;
                this.KolcsonozveVan = false;
                this.Kivette = null;
            }

        }
        class User
        {            
            private string nev;

            public string Nev
            {
                get { return nev; }
                set { nev = value; }
            }

            private string jelszo;

            public string Jelszo
            {
                get { return jelszo; }
                set { jelszo = value; }
            }

            public User(string nev, string jelszo)
            {
                this.nev = nev;
                this.jelszo = jelszo;
            }
        }

        private static void beolvas()
        {

            StreamReader sr = new StreamReader("lista.txt");
            while (!sr.EndOfStream)
            {
                string sor = sr.ReadLine();
                string[] vag = sor.Split(';');
                autok.Add(new Auto(
                    rendszam : vag[0],
                    tipus : vag[1],
                    marka : vag[2],
                    km : int.Parse(vag[3])
                ));
            }
            sr.Close();
            StreamReader sr2 = new StreamReader("userok.txt");
            while (!sr2.EndOfStream)
            {
                string sor = sr2.ReadLine();
                string[] vag = sor.Split('|');
                userok.Add(new User(
                    nev: vag[0],
                    jelszo: vag[1]
                ));
            }
            sr2.Close();
        }
        static void Main(string[] args)
        {
            beolvas();
            //Console.WriteLine(userok.Count);
            //Console.WriteLine(autok.Count);
            string ipcim = ConfigurationManager.AppSettings["IP-cim"];
            string portszam = ConfigurationManager.AppSettings["portszam"];
            IPAddress ip = IPAddress.Parse(ipcim);
            int port = int.Parse(portszam);
            figyelo = new TcpListener(ip, port);
            figyelo.Start();
            kapcsolatok = new Thread(kapcsolatFogad);
            kapcsolatok.Start();
            Console.WriteLine(ipcim);
            Console.ReadLine();

            figyelo.Stop();
            kapcsolatok.Abort();
        }
        public static void kapcsolatFogad()
        {
            while (true)
            {
                TcpClient bejovo = figyelo.AcceptTcpClient();
                KliensKomm k = new KliensKomm(bejovo);
                Thread t = new Thread(k.kommIndit);
                lock (futoSzalak)
                {
                    futoSzalak.Add(t);
                }
                t.Start();
            }
        }
        class KliensKomm
        {
            protected StreamWriter iro;
            protected StreamReader olvaso;
            public KliensKomm(TcpClient bejovo)
            {
                iro = new StreamWriter(bejovo.GetStream(), Encoding.UTF8);
                olvaso = new StreamReader(bejovo.GetStream());
            }

            class ReaderFromStream
            {
                protected StreamReader r;
                protected string line = null;

                protected void DoRead()
                {
                    line = r.ReadLine();
                }

                public string ReadLine(StreamReader r, int timeOutMSec)
                {
                    this.r = r;
                    this.line = null;
                    Thread t = new Thread(DoRead);
                    t.Start();
                    if (t.Join(timeOutMSec) == false)
                    {
                        t.Abort();
                        return null;
                    }
                    return line;
                }
            }
            private User bejelentkezes(string nev, string kod)
            {
                bool talalat = false;
                foreach (var item in userok)
                {
                    if (item.Nev == nev && item.Jelszo == kod)
                    {                        
                        iro.WriteLine($"Sikeres bejelentkezes! Udv {item.Nev}!");
                        talalat = true;
                        iro.Flush();
                        return item;
                    }
                }
                if (!talalat)
                {
                    iro.WriteLine("Sikertelen bejelentkezes!");
                }
                iro.Flush();
                return null;
            }
            private void kijelentkezes(User user)
            {
                iro.WriteLine("Kijelentkezve!");
                iro.Flush();
            }

            private void elerhetoE(string rendszam, User user)
            {
                bool talalat = false;
                foreach (var item in autok)
                {
                    if (item.Rendszam == rendszam)
                    {
                        talalat = true;
                        if (item.KolcsonozveVan)
                        {
                            iro.WriteLine($"Jelenleg nem elérhető a kívánt autó ({item.Rendszam})!");
                        }
                        else
                        {
                            iro.WriteLine($"Sikeresen kikölcsönözte a kívánt autót ({item.Rendszam})!");
                            item.KolcsonozveVan = true;
                            item.Kivette = user;
                        }
                    }
                }
                if (talalat == false)
                {
                    iro.WriteLine($"Nem találhato autó ezzel a rendszámmal! ({rendszam})");
                }
                iro.Flush();
            }

            private void visszavisz(string rendszam,string km ,User user)
            {
                if (user == null)
                {
                    iro.WriteLine("Visszavitelhez be kell jelentkezned!");
                }
                else
                {
                    Auto alany = null;
                    foreach (var item in autok)
                    {
                        if (item.Rendszam == rendszam && item.Kivette == user)
                        {
                            alany = item;
                            if (alany.KM < int.Parse(km))
                            {
                                item.KolcsonozveVan = false;
                                item.Kivette = null;
                            }
                            else
                            {
                                alany = null;
                            }
                        }
                    }
                    if (alany != null)
                    {
                        iro.WriteLine($"Sikeresen visszavitted az autót ({alany.Rendszam})!");
                    }
                    else
                        iro.WriteLine($"Sikertelen visszavitel! Lehet hogy nem jól adtad meg a km-óra állását!");
                }
                iro.Flush();
            }
            //itt a program gerince
            public void kommIndit()
            {
                bool torle_kell = true;
                Console.WriteLine();
                iro.WriteLine("SZG SZERVER|1.0");
                iro.Flush();

                bool vege = false;
                try
                {
                    User user = null;
                    while (!vege)
                    {
                        ReaderFromStream rs = new ReaderFromStream();
                        string feladat = rs.ReadLine(olvaso, 900000000);
                        Console.WriteLine(feladat);
                        string[] ss = feladat.Split('|');
                        switch (ss[0])
                        {
                            case "LISTA":
                                iro.WriteLine("OK!*");
                                foreach (var item in autok)
                                {
                                    iro.WriteLine($"{item.Rendszam}, {item.Tipus}, {item.Marka}, {item.KM}");
                                }
                                iro.WriteLine("OK!");
                                iro.Flush();
                                break;
                            case "LOGIN":
                                user = bejelentkezes(ss[1], ss[2]);
                                break;
                            case "LOGOUT":
                                user = null;
                                kijelentkezes(user);
                                break;
                            case "KOLCSONZES":
                                if (user != null)
                                {
                                    elerhetoE(ss[1], user);
                                }
                                else
                                {
                                    iro.WriteLine("Kolcsonzeshez elobb jelentkezz be!");
                                    iro.Flush();
                                }
                                break;
                            case "VISSZAVISZ":
                                visszavisz(ss[1],ss[2], user);
                                break;
                            case "EXIT":
                                iro.WriteLine("BYE");
                                iro.Flush();
                                break;
                            default:
                                break;
                        }
                    }
                }
                catch (Exception e)
                {
                    if (e is ThreadAbortException)
                    {
                        torle_kell = false;
                    }
                }
                if (torle_kell)
                {
                    Console.WriteLine("Kapcsolat torolve");
                    lock (Program.futoSzalak)
                    {
                        Thread ez = Thread.CurrentThread;
                        int i = Program.futoSzalak.IndexOf(ez);
                        if (i != -1)
                        {
                            Program.futoSzalak.RemoveAt(i);
                        }
                    }
                }
            }
        }
    }
}
