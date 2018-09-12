using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;

namespace Server
{
    class Server
    {
        Socket clientSocket;

        public Server() { }

        public Server(Socket clientSocket)
        {
            this.clientSocket = clientSocket;
        }

        private int receiveNumber()
        {
            byte[] bytes = new byte[1024];
            int bytesReceived = clientSocket.Receive(bytes);

            sendAnswer();

            return BitConverter.ToInt32(bytes, 0);
        }
        private void sendNumber(int number)
        {
            byte[] bytes = BitConverter.GetBytes(number);
            clientSocket.Send(bytes);

            receiveAnswer();
        }
        private string receiveString()
        {
            byte[] bytes = new byte[1024];
            int bytesReceived = clientSocket.Receive(bytes);

            sendAnswer();

            return System.Text.Encoding.Default.GetString(bytes).TrimEnd('\0');
        }
        private void sendString(string sendedString)
        {
            byte[] bytes = Encoding.Default.GetBytes(sendedString);
            clientSocket.Send(bytes);

            receiveAnswer();
        }
        private void createProcess(string fileName)
        {
            Process process = new Process();
            process.StartInfo.FileName = fileName;
            process.Start();

        }
        private void sendAllProcesses()
        {
            int filesCount = Process.GetProcesses().Count();
            sendNumber(filesCount);

            foreach (Process process in Process.GetProcesses())
            {
                sendString(process.ProcessName);

                receiveAnswer();
            }
        }
        private void removeProcess(string taskName)
        {
            Process process = Process.GetProcessesByName(taskName).First();
            process.Close();
        }
        private void sendAnswer()
        {
            byte[] bytes = BitConverter.GetBytes(1);
            clientSocket.Send(bytes);
        }
        private void receiveAnswer()
        {
            byte[] bytes = new byte[1024];
            clientSocket.Receive(bytes);
        }
        private async void ExecuteClientRequests(/*Socket clientSocket*/)
        {
            await Task.Run(() =>
            {
                try
                {
                    while (true)
                    {
                        MessageType messageType = (MessageType)receiveNumber();

                        switch(messageType)
                        {
                            case MessageType.CREATE_TASK:
                                string fileName = receiveString();
                                createProcess(fileName);
                                break;
                            case MessageType.GET_ALL_TASKS:
                                sendAllProcesses();
                                break;
                            case MessageType.REMOVE_TASK:
                                string processName = receiveString();
                                removeProcess(processName);
                                break;
                        }
                    }
                    //string client = null;
                    //string data = null;
                    //byte[] bytes = new byte[1024];
                    //// Получим от клиента DNS-имя хоста.
                    //// Метод Receive получает данные от сокета и заполняет массив байтов, переданный в качестве аргумента
                    //int bytesRec = clientSocket.Receive(bytes); // Возвращает фактически считанное число байтов
                    //client = Encoding.Default.GetString(bytes, 0, bytesRec); // конвертируем массив байтов в строку
                    //client += "(" + clientSocket.RemoteEndPoint.ToString() + ")";
                    //while (true)
                    //{
                    //    bytesRec = clientSocket.Receive(bytes); // принимаем данные, переданные клиентом. Если данных нет, поток блокируется
                    //    if (bytesRec == 0)
                    //    {
                    //        clientSocket.Shutdown(SocketShutdown.Both); // Блокируем передачу и получение данных для объекта Socket.
                    //        clientSocket.Close(); // закрываем сокет
                    //        return;
                    //    }
                    //    data = Encoding.Default.GetString(bytes, 0, bytesRec); // конвертируем массив байтов в строку

                    //    // uiContext.Send отправляет синхронное сообщение в контекст синхронизации
                    //    // SendOrPostCallback - делегат указывает метод, вызываемый при отправке сообщения в контекст синхронизации. 
                    //    uiContext.Send(d => listBox1.Items.Add(client) /* Вызываемый делегат SendOrPostCallback */,
                    //        null /* Объект, переданный делегату */); // добавляем в список имя клиента

                    //    uiContext.Send(d => listBox1.Items.Add(data), null); // добавляем в список сообщение от клиента
                    //    if (data.IndexOf("<end>") > -1) // если клиент отправил эту команду, то заканчиваем обработку сообщений
                    //        break;
                    //}
                    //string theReply = "Я завершаю обработку сообщений";
                    //byte[] msg = Encoding.Default.GetBytes(theReply); // конвертируем строку в массив байтов
                    //clientSocket.Send(msg); // отправляем клиенту сообщение
                    //clientSocket.Shutdown(SocketShutdown.Both); // Блокируем передачу и получение данных для объекта Socket.
                    //clientSocket.Close(); // закрываем сокет
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Сервер: " + ex.Message);
                    clientSocket.Shutdown(SocketShutdown.Both); // Блокируем передачу и получение данных для объекта Socket.
                    clientSocket.Close(); // закрываем сокет
                }
            });
        }

        public async void Accept()
        {
            await Task.Run(() =>
            {
                try
                {
                    // установим для сокета адрес локальной конечной точки
                    // уникальный адрес для обслуживания TCP/IP определяется комбинацией IP-адреса хоста с номером порта обслуживания
                   IPEndPoint ipEndPoint = new IPEndPoint(
                   IPAddress.Any /* Предоставляет IP-адрес, указывающий, что сервер должен контролировать действия клиентов на всех сетевых интерфейсах.*/,
                   25065 /* порт */);

                    // создаем потоковый сокет
                    Socket sListener = new Socket(AddressFamily.InterNetwork /*схема адресации*/, SocketType.Stream /*тип сокета*/, ProtocolType.Tcp /*протокол*/ );
                    /* Значение InterNetwork указывает на то, что при подключении объекта Socket к конечной точке предполагается использование IPv4-адреса.
                       SocketType.Stream поддерживает надежные двусторонние байтовые потоки в режиме с установлением подключения, без дублирования данных и 
                       без сохранения границ данных. Объект Socket этого типа взаимодействует с одним узлом и требует предварительного установления подключения 
                       к удаленному узлу перед началом обмена данными. Тип Stream использует протокол Tcp и схему адресации AddressFamily.
                     */

                    // Чтобы сокет клиента мог идентифицировать потоковый сокет TCP, сервер должен дать своему сокету имя
                    sListener.Bind(ipEndPoint); // Свяжем объект Socket с локальной конечной точкой.

                    // Установим объект Socket в состояние прослушивания.
                    sListener.Listen(10 /* Максимальная длина очереди ожидающих подключений.*/ );
                    Console.WriteLine("Server started");
                    while (true)
                    {
                        /* Блокируется поток до поступления от клиента запроса на соединение. При этом устанавливается связь имен клиента и сервера. 
                           Метод Accept извлекает из очереди ожидающих запросов первый запрос на соединение и создает для его обработки новый сокет.
                           Хотя новый сокет создан, первоначальный сокет продолжает слушать и может использоваться с многопоточной обработкой для 
                           приема нескольких запросов на соединение от клиентов. Сервер не должен закрывать слушающий сокет, который продолжает работать
                           наряду с сокетами, созданными методом Accept для обработки входящих запросов клиентов.
                         */
                        Socket handler = sListener.Accept();
                        Console.WriteLine("Client connected to server");
                        clientSocket = handler;
                        ExecuteClientRequests();
                        // обслуживание текущего запроса будем выполнять в отдельной асинхронной задаче
                        //ExecuteClientRequests(handler);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Сервер: " + ex.Message);
                }
            });
        }
    }
}
