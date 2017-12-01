using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading;
using System.Net;
using System.Net.Sockets;


namespace ChatClient
{
    class ChatClient
    {
        static void Main(string[] args)
        {
            IPAddress ip = IPAddress.Parse("10.6.118.77"); //computer's IP address CHANGE IT 
            int port = 5000; //matching port

            TcpClient client = new TcpClient(); //client class
            client.Connect(ip, port);
            NetworkStream ns = client.GetStream();
            Console.WriteLine("You successfully joined the server\n");

            Console.Write("Enter Username: ");
            string userName = "_" + Console.ReadLine(); //_ to differentiate bw username and regular message
            //_  so i can differentiate when i send through the stream
            Console.Clear();

            Thread thread = new Thread(o => ReceiveData((TcpClient)o, ns)); //no idea what this does
            thread.Start(client);

            
            byte[] name = Encoding.ASCII.GetBytes(userName); //get name in byte[] form
            ns.Write(name, 0, name.Length); //send name
            ns.Flush(); //flush (empty) the stream for next input

            Console.WriteLine("Send a message...\nType EXIT to disconnect\n");

            string userInput;
            while (!string.IsNullOrEmpty((userInput = Console.ReadLine())) && userInput.ToLower() != "exit")
            {
                byte[] buffer = Encoding.ASCII.GetBytes(userInput); //turns UI into array of bytes, then sends through network stream
                ns.Write(buffer, 0, buffer.Length);
                Console.WriteLine("Send another message?");
            }

            client.Client.Shutdown(SocketShutdown.Send);
            thread.Join();
            ns.Close();
            client.Close();
            Console.WriteLine(userName.Remove(0,1) + " disconnected from server!!");
            Console.ReadKey();
        }

        static void ReceiveData(TcpClient client, NetworkStream ns)
        {
            //NetworkStream ns = client.GetStream();
            byte[] receivedBytes = new byte[1024];
            int byte_count;

            while ((byte_count = ns.Read(receivedBytes, 0, receivedBytes.Length)) > 0) //while what is being sent to client has actual stuff in it
            {
                Console.Write(">> " + Encoding.ASCII.GetString(receivedBytes, 0, byte_count)); //when server sends your message back
            }
        }
    }
}

