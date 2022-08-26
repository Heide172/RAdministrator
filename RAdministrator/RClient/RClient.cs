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
        public TcpClient RTcpClient = new TcpClient();
        public IPAddress RClientIPAdress;


        public RClient() // Конструктор
        {

            String host = System.Net.Dns.GetHostName(); // Получение имени компьютера.
            RClientIPAdress = System.Net.Dns.GetHostEntry(host).AddressList[0]; // Получение ip-адреса.


        } 
        public bool Connect(IPAddress ServerAdress, int ServerPort) // TCP подключение клиента к серверу
        {
            try
            {
                while(!RTcpClient.Connected)
                {
                    RTcpClient.Connect(ServerAdress, ServerPort);
                }
                this.CheckIn()
                return RTcpClient.Connected;
            }

            catch (Exception ex)
            {
                return RTcpClient.Connected;
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