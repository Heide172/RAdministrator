using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using HeaderPack;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Text;
using NLog;

namespace RServer
{
    class RServer
    {
        private NLog.Logger logger;
        public TcpListener _listener;
        public TcpClient _client; 
        public List<ConnectedClient> connectedClients = new();
        private const int port = 3155;
        public RServer() // Конструктор
        {
            logger = NLog.LogManager.GetCurrentClassLogger(); // Получение логгера текущего класса
        }
        ~RServer() // Деструктор
        {
            NLog.LogManager.Shutdown(); // Закрытие логгера текущего класса
        }

        public bool Start() // Старт сервера
        {
            try
            {
                _listener = new TcpListener(IPAddress.Any, port);
                _listener.Start();
                logger.Info("TcpListner started");
                // запуск ассинхронной таски на принятие клиентов
                return true;
                
            }
            catch (Exception e)
            {
                logger.Error(e);
                _listener = null;
                throw;
            }
             
        }

        public void Stop() // Остановка сервера
        {
            if (this._listener != null)
            {
                this._listener.Stop();
                this._listener = null;
                logger.Info("Server app was stopped");
            }
        }

        private void AcceptClients() // Ожидание подключения клиентских приложений
        {

        }

    }

}