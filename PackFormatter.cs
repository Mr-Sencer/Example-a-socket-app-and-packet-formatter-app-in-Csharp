using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace SockLib
{
public    class PackFormatter :IDisposable
    {
        public PackFormatter(byte[] pack)
        {
            _in = pack;
            deserialize();
        }
        public PackFormatter()
        {

        }
        public PackFormatter(int flag,List<byte[]> list)
        {
            serialize(list,flag );
        }
        List<byte> array = new List<byte>(); //out
        int c = 0;
        // 1,2,3,4,5,6,7,8
        //poosition=4 

        public byte[] ReadBytes()
        {
            using (MemoryStream mem = new MemoryStream())
            {
                c = ReadInt32();
                for (int i=1;i<c;i++)
                {
                    try
                    {
 mem.WriteByte(_in[i+position]);
                    }
                    catch
                    {
                        if((i+position)==_in.Length || ((i + position)<_in.Length)==false)
                        {
                            break;
                        }
                    }
                   
                  
                }
                position +=Convert.ToInt32( mem.Length);
                return mem.ToArray();
            }
        }
        public void Serialize(int flag, List<byte[]> list)
        {
            serialize(list, flag);
        }
        public void Deserialize(byte[] pack)
        {
            _in = pack;
            deserialize();
        }
        void deserialize()
        {
      int flag =     ReadInt32();
            int count = ReadInt32(); 
            for(int i=1;i<count;i++)
            {
                pack.Add(ReadBytes());
            }
        }
        void addbyteArray(byte[] b)
        {
for(int i=0;i<b.Length;i++)
            {
                array.Add(b[i]);
            }
        }
       public byte[] _in;
        public byte[] ToArray { get { return array.ToArray(); } }
        public List<byte[]> pack = new List<byte[]>();
        public   void Add(int value)
        {
        addbyteArray(    BitConverter.GetBytes(value));
        }
        int position = 0;
        public int ReadInt32()
        {

            position += 4;
          return  BitConverter.ToInt32(_in, position-4);
        }
        public void Add(char value)
        {
            addbyteArray(BitConverter.GetBytes(value));
        }
        public void Add(float value)
        {
            addbyteArray(BitConverter.GetBytes(value));
        }
        void serialize(List<byte[]> bytes,int flag )
        {
            Add(flag);
            Add(bytes.Count); 
            for(int i=0;i<bytes.Count;i++)
            {
                Add(bytes[i].Length);
                addbyteArray(bytes[i]);
            }


        }

        public void Dispose()
        {
            GC.SuppressFinalize(_in);
            GC.SuppressFinalize(pack);
            GC.SuppressFinalize(c);
            GC.SuppressFinalize(array);
            GC.SuppressFinalize(this);
        }
    }
}
