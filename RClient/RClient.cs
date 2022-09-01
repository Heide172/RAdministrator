using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using HeaderPack;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Text;


namespace RClient
{
   

    class RClient
    {
        public TcpClient _client = new TcpClient();
        public IPAddress RClientIPAdress;
        private NLog.Logger logger;

        public RClient() // Конструктор
        {

            String host = System.Net.Dns.GetHostName(); // Получение имени компьютера.
            RClientIPAdress = System.Net.Dns.GetHostEntry(host).AddressList[0]; // Получение ip-адреса.
            logger = NLog.LogManager.GetCurrentClassLogger(); // Получение логгера текущего класса

        }
        ~RClient() // Деструктор
        {
            NLog.LogManager.Shutdown(); // Закрытие логгера текущего класса
        }
        public bool Connect(IPAddress ServerAddress, int ServerPort) // TCP подключение клиента к серверу
        {
            try
            {
                while(!_client.Connected)
                {
                    _client.Connect(ServerAddress, ServerPort);
                }
                logger.Info("RClient connected {ServAdr} {ServPrt}", ServerAddress, ServerPort);
                this.CheckIn();
                return _client.Connected;
            }

            catch (Exception e)
            {
                logger.Error(e);
                return _client.Connected;
            }


            
        }

        public void Disconnect() // Отключение клиента от сервера
        {


        }

        public void CheckIn() // Первоначальное отправление заголовка на сервер
        {


        }

        public void Send() // Отправка данных на сервер
        {


        }

        public async void Read() // Ассинхронное получение данных с сервера
        {


        }
    }
}