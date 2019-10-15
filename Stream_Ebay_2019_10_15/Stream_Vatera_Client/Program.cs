using System;
using System.Text;
using System.IO;
using System.Net.Sockets;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {

            string message, command, answer;

            TcpClient list = new TcpClient("127.0.0.1", 5000);
            StreamReader read = new StreamReader(list.GetStream(), Encoding.UTF8);
            StreamWriter write = new StreamWriter(list.GetStream(), Encoding.UTF8);

            Console.WriteLine("Succesfull connection");
            message = read.ReadLine();
            Console.WriteLine("Server message: {0}", message);

            while (true)
            {
                command = Console.ReadLine();
                write.WriteLine(command);
                write.Flush();
                answer = read.ReadLine();
                if (answer == "OK*")
                {
                    Console.WriteLine(answer);
                    while (answer != "OK!")
                    {
                        answer = read.ReadLine();
                        Console.WriteLine(answer);
                    }
                }
                else Console.WriteLine(answer);
                if (answer == "BYE") break;
            }

        }
    }
}
