using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Diagnostics;
using System.ComponentModel;

namespace WindowsFormsApp80
{
  public  class Client:Component
    {
        public Client()
        {
            th = new Thread(new ThreadStart(_connect));
        }
        Socket sock=new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);
        string _ip = "";
        int _port = 2312;
        Thread th = null;
        public string IP { get { return _ip; } set { _ip = value; } }
        public int Port { get { return _port; } set { _port = value; } }
        public delegate void _Exception(Exception ex);
        public event _Exception OnException;
        public  enum State
        {
            None,Connecting,Connected
        }
        bool autoConnect = false;
      
        void exc(Exception ex)
        {
         
 Debug.WriteLine("|-------------------------------------------|");
            Debug.WriteLine("Hata : [" + ex.Message + "]");
            Debug.WriteLine("Kaynak : [" + ex.Source + "]" );
            Debug.WriteLine("Hedef : [" + ex.TargetSite.Name + "]");
                Debug.WriteLine("|-------------------------------------------|");
                Debug.WriteLine(Environment.NewLine); 

            
           

        }
     public void Connect(string ip,int port)
        {
            _ip = ip;
            _port = port; 

            th.Start();
        }
        bool c()
        {
            try
            {
                sock.Connect(new IPEndPoint(IPAddress.Parse(_ip),_port));
                return true;
            }
            catch(Exception ex)
            {
               
                return false;
            }
            }
        bool cn = false;
        void bRecevie(IAsyncResult result)
        {
            sock.EndReceive(result);
            using (PackFormatter p = new PackFormatter())
            {
                p.Deserialize((byte[])result.AsyncState);
                OnDataRecevied(p.flag, p.pack); 

            }
        }
       
        public delegate void DisConnected();
        public delegate void Connected();
        public event DisConnected OnDisconnected;
        public event Connected OnConnected;
        public delegate void Recevied(int flag, List<byte[]> pack);
        public event Recevied OnDataRecevied;
        bool asyncRead = false;
        public bool AsyncRead
        {
            get
            {
                return asyncRead;
            }
            set
            {
                asyncRead = value;
            }
        }
        void bConnect(IAsyncResult res)
        {
            try
            {
                sock.EndConnect(res);
              
            }
            catch
            {

            }
            }
        void send(IAsyncResult result)
        {
            sock.EndSend(result);
        }
        public  void Send(int flag, List<byte[]> pack)
        {
            int len = 0;
                using (PackFormatter f = new PackFormatter())
                {
                    f.Serialize(flag, pack);
              len=      sock.Send(f.ToArray, 0, f.Size, SocketFlags.None);
                Debug.WriteLine("Client : " + len.ToString() + " bytes ");
            }

           
        }
        public async void SendAsync(int flag,List<byte[]>pack)
        {
            await Task.Run(() => {
                using (PackFormatter f = new PackFormatter())
                {
                    f.Serialize(flag, pack);
                    sock.Send(f.ToArray, 0, f.Size, SocketFlags.None);
                }
                 
                });
        }
        bool ReverseBoolean(bool value)
        {
            if(value==true)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        void _connect()
        {
       while(ReverseBoolean(c()))
            {

            }
            while (true)
            {
                if(sock.Connected==false && autoConnect==true)
                {
                    while (ReverseBoolean(c()))
                    {

                    }
                }
                else
                {

                }
                if(cn!=sock.Connected)
                {
                    cn = sock.Connected; 
                    if(cn==true)
                    {
                        try
                        {
 OnConnected();
                        }
                        catch
                        {

                        }

                    }
                    else
                    {
                        try
                        {
 OnDisconnected();
                        }
                        catch
                        {

                        }
                    }
                }
                if (sock.Available>0)
                {

                    try
                    {
                        if(asyncRead==false)
                        {
                            using (PackFormatter formatter = new PackFormatter())
                            {
                                byte[] data;

                                data = new byte[sock.Available];
                                sock.Receive(data, 0, data.Length, SocketFlags.None);
                                formatter.Deserialize(data);
                                try
                                {
 OnDataRecevied(formatter.flag, formatter.pack);
                                }
                                catch
                                {

                                }

                                GC.SuppressFinalize(data);
                            }
                        }
                        else
                        {
                            Task.Run(() =>  
                             
                            {
                                using (PackFormatter formatter = new PackFormatter())
                                {
 byte[] data;

                                data = new byte[sock.Available];
                                sock.Receive(data, 0, data.Length, SocketFlags.None);
                                    formatter.Deserialize(data);
                                    try
                                    {
   OnDataRecevied(formatter.flag,formatter.pack);
                                    }
                                    catch
                                    {

                                    }

                                    GC.SuppressFinalize(data);
                                }

                                   
                            });
                        }
                    }
                   catch(Exception ex)
                    {

                    }
                }
                
            }
           
        }
    }
}
