using HeaderPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RServer
{
    [Serializable]
    internal class ConnectedClient
    {
        internal event ClientConnectedEventHandler ConnectedNewClient;
        internal delegate void ClientConnectedEventHandler(ConnectedClient sender);
        internal event ClientDisconnectedEventHandler DisconnectedClient;
        internal delegate void ClientDisconnectedEventHandler(ConnectedClient sender, string Message);
        internal event ClientDataAvailableEventHandler DataAvailable;
        internal delegate void ClientDataAvailableEventHandler(ConnectedClient sender, HeaderDsc header, byte[] data);

        private NLog.Logger logger;
        public TcpClient NewTCPClient;
        public IPAddress ClientIpAdress;

        public ConnectedClient(TcpClient new_client)
        {
            logger = NLog.LogManager.GetCurrentClassLogger(); // Получение логгера текущего класса
            this.NewTCPClient = new_client;
            NewTCPClient.ReceiveBufferSize = TCPPack.BufferSize;
            NewTCPClient.SendBufferSize = TCPPack.BufferSize;
            ClientIpAdress = IPAddress.Parse(((IPEndPoint)NewTCPClient.Client.RemoteEndPoint).Address.ToString());
            logger.Info("New client connected {ClientIPAdress}", ClientIpAdress);
        }
        
        ~ConnectedClient()
        {
            NLog.LogManager.Shutdown(); // Закрытие логгера текущего класса
        }
    }
}
