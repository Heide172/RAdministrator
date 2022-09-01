using HeaderPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

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


        private bool IsRun; // Активно соединение с сервером
        private NLog.Logger logger;
        public bool ClientState = false;    // false - Client App, true - Server App
        public TcpClient _client;
        public IPAddress ClientIpAdress;

        public ConnectedClient(TcpClient client)
        {
            logger = NLog.LogManager.GetCurrentClassLogger(); // Получение логгера текущего класса
            this._client = client;
            _client.ReceiveBufferSize = TCPPack.BufferSize;
            _client.SendBufferSize = TCPPack.BufferSize;
            ClientIpAdress = IPAddress.Parse(((IPEndPoint)_client.Client.RemoteEndPoint).Address.ToString());
            IsRun = true;
            logger.Info("New client connected {ClientIPAdress}", ClientIpAdress);
        }
        public void Stop()
        {
            if (this._client != null)
            {
                IsRun = false;
                this._client.Client.Close();
                this._client.Close();
                DisconnectedClient(this, "stopped");
                logger.Info("Connection stopped");

            }

        }

        public async void Listen() // ассинхронное слушание порта
        {
            await Task.Run(() => Read());
        }
        public void Read() // принимает данные и вызывает событие DataAvailable
        {
            while (IsRun == true)
            {
                byte[]? data = default;
                byte[] buffer;
                byte[] header;

                NetworkStream Network;
                HeaderDsc headerDsc;


                try
                {
                    Network = _client.GetStream();

                    header = new byte[TCPPack.HeaderSize];

                    int readBytes = Network.Read(header, 0, header.Length); // считывание заголовка await async
                    if (readBytes == 0)
                    {
                        logger.Warn("Remote host dropped connection");
                        Stop();
                        break;

                    }
                    else
                    {
                        int lengthHeader = BitConverter.ToInt32(header, 0);

                        using (MemoryStream Memory = new MemoryStream(lengthHeader)) // десериализация заголовка
                        {
                            buffer = new byte[lengthHeader];
                            readBytes = Network.Read(buffer, 0, buffer.Length);
                            Memory.Write(buffer, 0, readBytes);
                            Memory.Position = 0;

                            BinaryFormatter bf = new BinaryFormatter();
                            headerDsc = (HeaderDsc)bf.Deserialize(Memory);
                            //Console.ReadKey();
                        }
                    }

                    if (headerDsc.DataSize > 0)
                    {
                        buffer = new byte[TCPPack.BufferSize];
                        data = new byte[headerDsc.DataSize];

                        int lengthPack = buffer.Length;
                        int receivedBytes = 0;

                        while (true)
                        {
                            var remBytes = headerDsc.DataSize - receivedBytes;
                            lengthPack = (remBytes < lengthPack) ? (int)remBytes : buffer.Length;// если осталось получить байтов меньше чем буффер ждем конкретно это кол-во байтов
                            readBytes = Network.Read(buffer, 0, lengthPack);

                            if (readBytes == 0)
                            {
                                Console.WriteLine("Remote host dropped connection");
                                break;
                            }

                            // Записываем строго столько байтов сколько прочтено методом Read()
                            Buffer.BlockCopy(buffer, 0, data, receivedBytes, readBytes);
                            receivedBytes += readBytes;

                            // Как только получены все байты файла, останавливаем цикл,
                            // иначе он заблокируется в ожидании новых сетевых данных
                            if (headerDsc.DataSize == receivedBytes)
                            {
                                // Все данные пришли. Выходим из цикла (readBytes всегда > 0)

                                break;
                            }

                        }

                    }
                    if (headerDsc.Message == ServiceMessage.Authorization)
                    {
                        using (MemoryStream Memory = new MemoryStream(headerDsc.DataSize)) // десериализация заголовка
                        {

                            Memory.Write(data, 0, data.Length);
                            Memory.Position = 0;

                            BinaryFormatter bf = new BinaryFormatter();
                        }
                    }
                    DataAvailable(this, headerDsc, data); // СОБЫТИЕ ДОСТУПНОСТИ ДАННЫХ



                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                    Stop();
                }
                finally
                {
                    // Обнулим все ссылки на многобайтные объекты
                    headerDsc = null;
                    // Network = null;
                    data = null;
                    header = null;
                    buffer = null;

                }

            }

        }
        public void Send(ServiceMessage message, Guid guid, byte[] data)
        {
            byte[] buffer;
            byte[] header;
            byte[] infobuffer; //???
            NetworkStream Network;
            HeaderDsc headerDsc;


            if (_client == null || !_client.Connected)
            {
                logger.Warn("Remote host dropped connection");
                return;
            }
            try
            {
                headerDsc = new HeaderDsc(); // формирование заголовка
                headerDsc.Message = message;
                headerDsc.guid = guid;
                headerDsc.DataSize = data.Length;

                using (MemoryStream Memory = new MemoryStream()) // сериализация заголовка
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(Memory, headerDsc);
                    Memory.Position = 0;
                    infobuffer = new byte[Memory.Length];
                    var r = Memory.Read(infobuffer, 0, infobuffer.Length);
                }

                buffer = new byte[TCPPack.BufferSize];
                header = BitConverter.GetBytes(infobuffer.Length);

                Buffer.BlockCopy(header, 0, buffer, 0, header.Length);
                Buffer.BlockCopy(infobuffer, 0, buffer, header.Length, infobuffer.Length);

                int bufferShift = header.Length + infobuffer.Length; // сдвиг на размер заголовка
                int rdShift = 0; // сдвиг на кол-во переданных байт
                int lengthPack = 0; // фактический размер буффера
                Network = _client.GetStream();

                while (rdShift < (headerDsc.DataSize + bufferShift)) // пока переданное кол-во байтов меньше ожидаемого 
                {
                    var remBytes = headerDsc.DataSize - rdShift;

                    if (remBytes < buffer.Length) lengthPack = remBytes;
                    else lengthPack = buffer.Length - bufferShift;

                    Buffer.BlockCopy(data, rdShift, buffer, bufferShift, lengthPack);
                    rdShift += lengthPack;
                    Network.Write(buffer, 0, lengthPack + bufferShift);
                    bufferShift = 0;




                }



            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
            finally
            {
                header = null;
                infobuffer = null;
                buffer = null;
                Network = null;
                headerDsc = null;
            }
        }
        public void SendCommand(ServiceMessage message, Guid guid, int comIndex)
        {
            byte[] buffer;
            byte[] header;
            byte[] infobuffer; //???
            byte[] data = new byte[0];
            NetworkStream Network;
            HeaderDsc headerDsc;


            if (_client == null || !_client.Connected)
            {
                logger.Warn("Remote host dropped connection");
                return;
            }
            try
            {
                headerDsc = new HeaderDsc(); // формирование заголовка
                headerDsc.Message = message;
                headerDsc.guid = guid;
                headerDsc.DataSize = data.Length;
                headerDsc.comIndex = comIndex;
                using (MemoryStream Memory = new MemoryStream()) // сериализация заголовка
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(Memory, headerDsc);
                    Memory.Position = 0;
                    infobuffer = new byte[Memory.Length];
                    var r = Memory.Read(infobuffer, 0, infobuffer.Length);
                }

                buffer = new byte[TCPPack.BufferSize];
                header = BitConverter.GetBytes(infobuffer.Length);

                Buffer.BlockCopy(header, 0, buffer, 0, header.Length);
                Buffer.BlockCopy(infobuffer, 0, buffer, header.Length, infobuffer.Length);

                int bufferShift = header.Length + infobuffer.Length; // сдвиг на размер заголовка
                int rdShift = 0; // сдвиг на кол-во переданных байт
                int lengthPack = 0; // фактический размер буффера
                Network = _client.GetStream();

                while (rdShift < (headerDsc.DataSize + bufferShift)) // пока переданное кол-во байтов меньше ожидаемого 
                {
                    var remBytes = headerDsc.DataSize - rdShift;

                    if (remBytes < buffer.Length) lengthPack = remBytes;
                    else lengthPack = buffer.Length - bufferShift;

                    Buffer.BlockCopy(data, rdShift, buffer, bufferShift, lengthPack);
                    rdShift += lengthPack;
                    Network.Write(buffer, 0, lengthPack + bufferShift);
                    bufferShift = 0;




                }



            }
            catch (Exception ex)
            {
                logger.Error(ex);

            }
            finally
            {
                header = null;
                infobuffer = null;
                buffer = null;
                Network = null;
                headerDsc = null;
            }
        }

        ~ConnectedClient()
        {
            NLog.LogManager.Shutdown(); // Закрытие логгера текущего класса
        }
    }
}
