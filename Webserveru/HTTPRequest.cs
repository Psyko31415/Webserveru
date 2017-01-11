using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Webserveru
{
    class HTTPRequestHandler
    {
        public string Path { get; private set; }
        public Dictionary<string, string> Get { get; private set; }
        public Dictionary<string, string> Post { get; private set; }
        public Socket Socket { get; private set; }

        public HTTPRequestHandler(string request, ref Socket s)
        {
            Socket = s;

            string[] headBodySplit = request.Split(new string[] { "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (headBodySplit.Length > 0)
            {
                string[] head = headBodySplit[0].Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                if (head.Length > 1)
                {

                    string[] head1 = head[0].Split(' ');
                    if (head1.Length > 2)
                    {
                        string[] pathGetSpit = head1[1].Split('?');
                        Path = pathGetSpit[0];
                        if (pathGetSpit.Length > 1)
                        {
                            Get = LoadData(pathGetSpit[1]);
                        }
                        else
                        {
                            Get = new Dictionary<string, string>();
                        }

                        if (head1[0] == "POST")
                        {
                            Post = LoadData(headBodySplit[1]);
                        }
                        else
                        {
                            Post = new Dictionary<string, string>();
                        }
                    }
                }
            }
        }

        public void Reply(string content)
        {
            string responseString = "HTTP/1.1 200\r\n" +
                "Content-Length: " + Encoding.UTF8.GetBytes(content).Length + "\r\n\r\n" +
                content;
            Byte[] responseBytes = Encoding.UTF8.GetBytes(responseString);

            Socket.Send(responseBytes, responseBytes.Length, 0);
        }

        private Dictionary<string, string> LoadData(string rawData)
        {
            string[] parameters = rawData.Split('&');
            Dictionary<string, string> ret = new Dictionary<string, string>();
            for (int i = 0; i < parameters.Length; i++)
            {
                string[] kvpair = parameters[i].Split('=');
                ret[kvpair[0]] = DecodeUrlString(kvpair[1]);
            }
            return ret;
        }

        private static string DecodeUrlString(string url)
        {
            string newUrl;
            while ((newUrl = Uri.UnescapeDataString(url)) != url)
            {
                url = newUrl;
            }
            return newUrl;
        }
    }
}
