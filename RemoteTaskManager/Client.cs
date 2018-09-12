using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace RemoteTaskManager
{
    class Client
    {
        Socket socket
        {
            get;set;
        }
        bool isConnected
        {
            get;set;
        }
        

        private int receiveNumber()
        {
            byte[] bytes = new byte[1024];
            int bytesReceived = socket.Receive(bytes);

            sendAnswer();

            return BitConverter.ToInt32(bytes, 0);
        }
        private void sendNumber(int number)
        {
            byte[] bytes = BitConverter.GetBytes(number);
            socket.Send(bytes);

            receiveAnswer();
        }
        private string receiveString()
        {
            byte[] bytes = new byte[1024];
            int bytesReceived = socket.Receive(bytes);

            sendAnswer();

            return System.Text.Encoding.Default.GetString(bytes).TrimEnd('\0');
        }
        private void sendString(string sendedString)
        {
            byte[] bytes = Encoding.Default.GetBytes(sendedString);
            socket.Send(bytes);

            receiveAnswer();
        }
        private void sendAnswer()
        {
            byte[] bytes = BitConverter.GetBytes(1);
            socket.Send(bytes);
        }
        private void receiveAnswer()
        {
            byte[] bytes = new byte[1024];
            socket.Receive(bytes);
        }
        private void connect()
        {
            IPAddress ipAddr = IPAddress.Parse("192.168.0.1");

            // устанавливаем удаленную конечную точку для сокета
            // уникальный адрес для обслуживания TCP/IP определяется комбинацией IP-адреса хоста с номером порта обслуживания
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddr /* IP-адрес */, 49152 /* порт */);

            // создаем потоковый сокет
            socket = new Socket(AddressFamily.InterNetwork /*схема адресации*/, SocketType.Stream /*тип сокета*/, ProtocolType.Tcp /*протокол*/);
            /* Значение InterNetwork указывает на то, что при подключении объекта Socket к конечной точке предполагается использование IPv4-адреса.
              SocketType.Stream поддерживает надежные двусторонние байтовые потоки в режиме с установлением подключения, без дублирования данных и 
              без сохранения границ данных. Объект Socket этого типа взаимодействует с одним узлом и требует предварительного установления подключения 
              к удаленному узлу перед началом обмена данными. Тип Stream использует протокол Tcp и схему адресации AddressFamily.
            */

            // соединяем сокет с удаленной конечной точкой
            socket.Connect(ipEndPoint);
        }
    }
}
