using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.ComponentModel;
using System.Diagnostics;

namespace WindowsFormsApp80
{
    public class User
    {
        
        public User(Socket socket)
        {
           
            sock = socket;
            try
            {
  ip =((IPEndPoint)(sock.RemoteEndPoint)).Address.ToString();
                Debug.WriteLine(" Server : Connected" +ip );
            }
            catch
            {

            }
            
        }
        Socket sock;
        public delegate void Recevied(string ip,int flag,List<byte[]>pack);
        public event Recevied OnRecevied;
        Stopwatch watch = new Stopwatch();
        PackSerializer serializer = new PackSerializer();
        public string ip;
        void send(IAsyncResult result)
        {
            sock.EndSend(result);
        }
        public void Send(int flag, List<byte[]> pack)
        {

            using (PackFormatter f = new PackFormatter())
            {
                f.Serialize(flag, pack);
                sock.BeginSend(f.ToArray, 0, f.Size, SocketFlags.None, send, null);
            }


        }
        public async void SendAsync(int flag, List<byte[]> pack)
        {
            await Task.Run(() => {
                using (PackFormatter f = new PackFormatter())
                {
                    f.Serialize(flag, pack);
                    sock.BeginSend(f.ToArray, 0, f.Size, SocketFlags.None, send, null);
                }

            });
        }
        void read(IAsyncResult result)
            {
       //     serializer.Deserialize((byte[])result.AsyncState);
            sock.EndReceive(result);
            
            using (PackFormatter formatter = new PackFormatter())
            {
                formatter.Deserialize((byte[])result.AsyncState);
                if(formatter.flag<1000)
                {
 OnRecevied(ip,formatter.flag, formatter.pack); 
                }
               

            }


        }
        public int ping = 0;
        int oldping = 0;
        void r()
        {
            if(watch.IsRunning==false)
            {
watch.Start();
               
            }
            else
            {

                ping =Convert.ToInt32( watch.ElapsedMilliseconds);
                watch.Stop();
                watch.Reset();
            }
            if(oldping!=ping)
            {
                oldping = ping;
                Debug.WriteLine("Ping" + oldping.ToString());
            }
            byte[] data;
            data = new byte[sock.Available];
            //  sock.BeginReceive(data, 0, data.Length, SocketFlags.None, read, data);
            Debug.WriteLine("Reecvied by Server : " + data.Length.ToString() + " bytes");
            sock.Receive(data, data.Length, SocketFlags.None);
            using (PackFormatter f = new PackFormatter())
            {
                try
                {
  f.Deserialize(data);
                OnRecevied(ip, f.flag, f.pack);
                }
                catch
                {

                }
            }
                GC.SuppressFinalize(data);
        }

        public void Read(Server s)
        {
            if(sock.Available>0)
            {
                if(s.AsyncRead==false)
                {
                    r();
                }
                else
                {
                    Task.Run(() => {
                        r();

                    });
                }
            }
         
        }
    }
    public class Server : Component
    {
      //  public List<User> users = new List<User>();
        public Dictionary<string,User> users = new Dictionary<string, User>();
        public Server()
        {
            th = new Thread(new ThreadStart(listen));

        }
        public Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        Thread th;
        IPAddress add = IPAddress.Any;
        int _port = 2312;
        int Max_accept_user_count = 1;
        public delegate void NewClientConnected(string ip);
        public event NewClientConnected OnNewClientConnected;
        public delegate void Recevied(string ip, int flag, List<byte[]> pack);
        public event Recevied OnRecevied;
        bool asyncRead = false; 
        public bool AsyncRead { get
            {
                return asyncRead;
            }
            set
            {
                asyncRead = value;
            }
        }
        public int MaxAccept
        {
            get { return Max_accept_user_count; }
            set
            {
                Max_accept_user_count = value;
            }
        }
        public void Send(string ip,int flag,List<byte[]> pack)
        {
            users[ip].Send(flag, pack); 

            
        }
   
        public void Listen()
        {
            th.Start();
        }

        public void Listen(int port)
        {
            _port = port;
            th.Start();

        }
        public int Port { get { return _port; } set { _port = value; } }
        void recevied(string ip, int flag, List<byte[]> pack)
        {
            try
            {
 OnRecevied(ip, flag, pack);
            }
            catch(Exception ex)
            {

            }
}
        void baccept(IAsyncResult res)
        {
         Socket accpt=   sock.EndAccept(res);
            if (users.ContainsKey(((IPEndPoint)(accpt.RemoteEndPoint)).Address.ToString()) == false)
            {
                users.Add(((IPEndPoint)(accpt.RemoteEndPoint)).Address.ToString(), new User(accpt));
                try
                {
                    OnNewClientConnected(((IPEndPoint)(accpt.RemoteEndPoint)).Address.ToString());
                }
                catch
                {

                }
                users.ElementAt(users.Count - 1).Value.OnRecevied += new User.Recevied(recevied);

            }

        }
        void listen()
        {
            sock.Bind(new IPEndPoint(add, _port));
            sock.Listen(Max_accept_user_count);
            //   sock.BeginAccept(acceptcallback, sock);
         
            while (true)
            {

                sock.BeginAccept(new AsyncCallback(baccept), null);
                

               
          
                for (int i=0;i<users.Count;i++)
                {
                    users.ElementAt(i).Value.Read(this);
                }
                }
                    
            

        }
    }
}
