using System;
using System.Net;

namespace RClient
{
    class Program
    {
        static IPAddress ServerAddress = IPAddress.Parse("127.0.0.1");
        static int ServerPort = 3155;
        static RClient Client = new RClient();
        static void Main(string[] args)
        {
            Client.Connect(ServerAddress, ServerPort);

        }

    }


}