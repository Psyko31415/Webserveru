using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace Webserveru
{
    class HTTPRequestHandler
    {
        public static string[] IMG_EXTENTIONS = { "png", "jpg" };
        public static string BASE_PATH = "../../www";

        public string Path { get; private set; }
        public string Ext { get; private set; }
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
                        Path = ValidatePath(pathGetSpit);
                        Ext = ValidateExt(Path);    
                        
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

        public void ErrorReply(string errorCode, string content)
        {
            string responseString = "HTTP/1.1 " + errorCode + "\r\n" +
                "Content-Type: text/html\r\n" +
                "Content-Length: " + Encoding.UTF8.GetByteCount(content) + "\r\n\r\n" +
                content;
            Send(Encoding.UTF8.GetBytes(responseString));
        }

        private void Send(Byte[] response)
        {
            Socket.Send(response, response.Length, 0);
        }

        private string ValidateExt(string path)
        {
            string[] pathSplit = path.Split('.');
            if (pathSplit.Length > 1)
            {
                return pathSplit[pathSplit.Length - 1];
            }
            else
            {
                return "html";
            }
        }

        private string ValidatePath(string[] pathGetSplit)
        {
            if (pathGetSplit.Length != 0)
            {
                string path = pathGetSplit[0];
                if (path.Split('.').Length - 1 == 1) // ONLY ONE dot is alowed
                {
                    if (File.Exists(BASE_PATH + path))
                    {
                        return path;
                    }
                }
            }
            return "";
        }

        public void Reply()
        {
            
            if (Path != "")
            {
                Byte[] responseBytes;
                string contentType;
                if (IMG_EXTENTIONS.Contains(Ext))
                {
                    responseBytes = File.ReadAllBytes(BASE_PATH + Path);
                    contentType = "image/" + Ext;
                }
                else
                {
                    responseBytes = Encoding.UTF8.GetBytes(File.ReadAllText(BASE_PATH + Path));
                    contentType = "text/html";
                }
                string responseString = "HTTP/1.1 200\r\n" +
                   "Content-Type: " + contentType + "\r\n" +
                   "Content-Length: " + responseBytes.Length + "\r\n\r\n";

                Send(Encoding.UTF8.GetBytes(responseString));
                Send(responseBytes);
            }
            else
            {
                ErrorReply("404", "<h1>404 Din mamma måste betala hyra</h1><img src=\"img.png\" />");
            }
           
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
