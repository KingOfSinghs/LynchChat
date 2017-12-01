using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net.Sockets; //server sockets
using System.Threading; //thread
using System.Net; //ip address

namespace ChatServer
{
    class Server
    {
        static readonly object _lock = new object();
        
        static readonly Dictionary<int, TcpClient> list_clients = new Dictionary<int, TcpClient>(); //list of keys and values (integers to matching clients)
        static readonly Dictionary<int, string> list_usernames = new Dictionary<int, string>(); //for usernames

        static void Main(string[] args)
        {
            int count = 1;
            
            string hostName = Dns.GetHostName(); // Retrive the Name of HOST  
            Console.WriteLine("\t\tComputer host is: " + hostName);
            // Get the IP  
            string myIP = Dns.GetHostEntry(hostName).AddressList[0].ToString();
            Console.WriteLine("My IP Address is: " + myIP);
            IPAddress ip = IPAddress.Parse(myIP);

            TcpListener ServerSocket = new TcpListener(ip, 5000); //5000 is the port nunmber for the client to connect to
            ServerSocket.Start();
            Console.WriteLine(">> Server Started <<\n");

            while (true) //accepting multiple clients
            {
                TcpClient client = ServerSocket.AcceptTcpClient(); //accepts client into the server
                Console.WriteLine("Someone joined!!!");
                lock (_lock) list_clients.Add(count, client);  //put the client into the list with count being its ID
                Thread t = new Thread(handle_clients);
                t.Start(count);
                count++;
            }

        }
        public static void handle_clients(object o)
        {
            int id = (int)o;
            TcpClient client;
            string userName=""; //i had to add this feature using string manipulation


            lock (_lock) client = list_clients[id];

            while (true)
            {
                NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[1024]; //blank byte array
                int byte_count = stream.Read(buffer, 0, buffer.Length); //reads user input (from stream) and gets count of byte[] 

                if (byte_count == 0) //blank handling
                {
                    break;
                }

                string data = Encoding.ASCII.GetString(buffer, 0, byte_count); //converts byte[] userinput into data string userinput

                if (data[0] == '_') //check if the first char is _ (is it the username?)
                {
                    userName = data.Remove(0, 1); //works
                    list_usernames.Add(id, userName); //like a list but w pairs
                    Console.WriteLine(userName + " joined the server");
                }
                else
                {
                    userName = list_usernames[id];
                    broadcast(data, userName); //broadcast to all the TcpClients
                    Console.WriteLine(userName + " said " + data);
                }
           }

            lock (_lock) list_clients.Remove(id);
            client.Client.Shutdown(SocketShutdown.Both);
            client.Close();            
        }

        public static void broadcast(string data, string userName)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(userName + " said " + data + "\n");

            lock (_lock)
            {
                foreach (TcpClient c in list_clients.Values)
                {
                    NetworkStream stream = c.GetStream();

                    stream.Write(buffer, 0, buffer.Length); //send to every client in the list
                }
            }
        }
    }
}
