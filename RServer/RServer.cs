using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using HeaderPack;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Text;

namespace RServer
{
    class RServer
    {
        public RServer() // Конструктор
        {

        }
        ~RServer() // Деструктор
        {

        }

        public bool Start() // Старт сервера
        {
            return true;
        }

        public void Stop() // Остановка сервера
        {

        }

        private void AcceptClients() // Ожидание подключения клиентских приложений
        {

        }

    }

}