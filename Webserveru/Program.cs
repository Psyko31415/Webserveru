using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Webserveru
{
    class Program
    {
        static void Main(string[] args)
        {
            TcpListener listener = new TcpListener(new IPAddress(new Byte[] { 127, 0, 0, 1 }), 5051);
            listener.Start();

            while (true)
            {
                Socket s = listener.AcceptSocket();
                if (s.Connected)
                {
                    Byte[] buffer = new Byte[1024];
                    s.Receive(buffer, buffer.Length, 0);

                    string request = Encoding.ASCII.GetString(buffer);
                    Console.WriteLine(request);

                    string content = "<!DOCTYPE html><form method=\"post\"><input name=\"trams\"/><input type=\"submit\" name=\"ost\" value=\"ost\" /></form>";
                    
                    HTTPRequestHandler requestHandler = new HTTPRequestHandler(request, ref s);
                    requestHandler.Reply();
                }
                s.Close();
            }
        }
    }
}
