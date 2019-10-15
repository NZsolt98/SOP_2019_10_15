using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace EbayServer
{
    class Product
    {
        string name, code, user = null;
        int currentPrice;
        public string User
        {
            get { return user; }
            set { user = value; }
        }

        public string Code
        {
            get { return code; }
            set { code = value; }
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public int CurrentPrice
        {
            get { return currentPrice; }
            set { currentPrice = value; }
        }

        public Product(string name, string code, int currentPrice, string user)
        {
            this.Name = name;
            this.Code = code;
            this.CurrentPrice = currentPrice;
            this.User = user;
        }

    }

    class Protocol
    {
        static List<Product> Products = new List<Product>();
        public StreamReader r;
        public StreamWriter w;
        public string user = null;

        public Protocol(TcpClient c)
        {
            this.r = new StreamReader(c.GetStream(), Encoding.UTF8);
            this.w = new StreamWriter(c.GetStream(), Encoding.UTF8);
        }

        void Login(string name, string password)
        {
            // Validálás!
            user = name;
            w.WriteLine("OK");
        }

        void List()
        {
            w.WriteLine("OK*");
            lock (Products)
            {
                foreach (Product item in Products)
                    w.WriteLine("{0}, {1}, {2}, {3}",
                        item.Code,
                        item.Name,
                        item.CurrentPrice,
                        item.User);
            }
            w.WriteLine("OK!");
        }

        void Add(string code, string name, int price)
        {
            if (this.user == null) w.WriteLine("ERR|Log in, please");
            else
            {
                lock (Products)
                {
                    int i = 0;
                    for (i = 0; i < Products.Count && Products[i].Code != code; i++) ;
                    if (i < Products.Count)
                        w.WriteLine("ERR|Already exists");
                    else
                    {
                        Product temp = new Product(name, code, price, this.user);
                        Products.Add(temp);
                        w.WriteLine("OK");
                    }
                }
            }
        }

        void Bid(string code, int price)
        {
            if (user == null)
                w.WriteLine("ERR|Log in, please!");
            else
            {
                lock (Products)
                {
                    int i = 0;
                    for (i = 0; i < Products.Count && Products[i].Code != code; i++) ;
                    if (i >= Products.Count)
                        w.WriteLine("ERR|This code is not found");
                    else
                    {
                        if (Products[i].CurrentPrice < price)
                        {
                            Products[i].CurrentPrice = price;
                            Products[i].User = user;
                            w.WriteLine("OK");
                        }
                        else w.WriteLine("ERR|Low price");
                    }
                }
            }
        }

        public void Start()
        {
            w.WriteLine("Auction 1.0");
            w.Flush();
            bool ok = true;
            string command, message;
            string[] param;
            while (ok)
            {
                try
                {
                    message = r.ReadLine();
                    param = message.Split('|');
                    command = param[0].ToLower();
                    switch (command)
                    {
                        case "login":
                            if (param.Length > 2 && param[1] != null && param[2] != null)
                            {
                                Login(param[1], param[2]);
                            }
                            else w.WriteLine("ERR|Not enough params");
                            break;
                        case "add":
                            if (param.Length > 3)
                            {
                                int price = int.Parse(param[3]);
                                Add(param[1], param[2], price);
                            }
                            else w.WriteLine("ERR|Not enough params");
                            break;
                        case "list": List(); break;
                        case "bid":
                            if (param.Length > 2)
                            {
                                int price = int.Parse(param[2]);
                                Bid(param[1], price);
                            }
                            else w.WriteLine("ERR|Not enough params");
                            break;
                        case "bye":
                            w.WriteLine("BYE");
                            ok = false;
                            break;
                        default: w.WriteLine("ERR|Unknown command"); break;
                    }
                }
                catch (Exception e)
                {
                    w.WriteLine("ERR|{0}", e.Message);
                }
                w.Flush();
            }
            Console.WriteLine("The client disconnected");
        }

    }

    class Program
    {
        static void Main(string[] args)
        {
            IPAddress ip = IPAddress.Parse("127.0.0.1");
            TcpListener listener = new TcpListener(ip, 5000);
            listener.Start();
            bool end = false;
            while (!end)
            {
                Console.WriteLine("The server is waiting for an incoming connection...");
                TcpClient client = listener.AcceptTcpClient();
                Protocol pr = new Protocol(client);
                Thread th = new Thread(pr.Start);
                th.Start();
            }
        }
    }
}