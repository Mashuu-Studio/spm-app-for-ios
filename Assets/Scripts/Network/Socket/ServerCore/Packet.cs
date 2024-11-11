using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace ServerCore
{
    public enum PacketType
    {
        CONNECTING = 0,
        EXERCISE_DATA = 10,
        EXERCISE_RESULT = 11,
    }

    public class ConnectingPacket : Packet
    {

    }

    public class ExerciseDataPacket : Packet
    {
        public int exerIndex;
        public string exercise;
        public List<Vector2[]> joints;
        public List<byte[]> images;

        public ExerciseDataPacket()
        {
        }

        public ExerciseDataPacket(int exerIndex, List<Vector2[]> joints, List<byte[]> images, int width, int height)
        {
            type = (ushort)PacketType.EXERCISE_DATA;

            this.exerIndex = exerIndex;
            exercise = ExerciseData.ExerciseNames[exerIndex];
            this.joints = new List<Vector2[]>();
            foreach (var arr in joints)
            {
                this.joints.Add(new Vector2[arr.Length]);
                for (int i = 0; i < arr.Length; i++)
                {
                    this.joints[this.joints.Count - 1][i] = arr[i];
                }
            }
            this.images = images;
        }

        public override void HandlePacket(byte[] data)
        {
            base.HandlePacket(data);
        }
    }

    public class ExerciseResultPacket : Packet
    {
        public int exerIndex;
        public string exercise;
        public List<List<float>> result;

        public ExerciseResultPacket()
        {

        }

        public ExerciseResultPacket(int exerIndex, List<List<float>> result)
        {
            type = (ushort)PacketType.EXERCISE_RESULT;

            this.exerIndex = exerIndex;
            exercise = ExerciseData.ExerciseNames[exerIndex];
            this.result = result;
        }

        public override void HandlePacket(byte[] data)
        {
            base.HandlePacket(data);
        }
    }

    public abstract class Packet
    {
        public ushort type;

        // 패킷을 전송받았을 때 역직렬화하고 작동시킬 메소드.
        public virtual void HandlePacket(byte[] data)
        {
            Deserialize(data);
        }

        // 패킷을 직렬화. 전송시 해당 과정을 거침.
        public byte[] Serialize()
        {
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                // 맨앞에 어떤 패킷인지 패킷의 종류가 나옴.
                writer.Write(type);
                // 패킷의 정보를 최대한 다 가져옴.
                // 필드, public, private, protected 등 일반적인 변수들을 담도록 함.
                foreach (var field in this.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    // type은 이미 읽었으므로 스킵.
                    if (field.Name == nameof(type)) continue;

                    object val = field.GetValue(this);
                    // 우선 몇몇 부분들에 있어서만 진행.
                    if (val is ushort u) writer.Write(u);
                    else if (val is int i) writer.Write(i);
                    else if (val is string s)
                    {
                        byte[] strData = Encoding.UTF8.GetBytes(s);
                        writer.Write(strData.Length);
                        writer.Write(strData);
                    }
                    else if (val is List<List<float>> fflist)
                    {
                        writer.Write((ushort)fflist.Count);
                        foreach (var list in fflist)
                        {
                            writer.Write((ushort)list.Count);
                            foreach (var f in list) writer.Write(f);
                        }
                    }
                    else if (val is List<float> flist)
                    {
                        writer.Write((ushort)flist.Count);
                        foreach (var f in flist) writer.Write(f);
                    }
                    else if (val is List<Vector2[]> varraylist)
                    {
                        writer.Write(varraylist.Count);
                        foreach (var array in varraylist)
                        {
                            writer.Write(array.Length);
                            foreach (var v in array)
                            {
                                writer.Write(v.x);
                                writer.Write(v.y);
                            }
                        }
                    }
                    else if (val is List<byte[]> bytearraylist)
                    {
                        writer.Write(bytearraylist.Count);
                        foreach (var array in bytearraylist)
                        {
                            writer.Write(array.Length);
                            writer.Write(array);
                        }
                    }
                }
                return stream.ToArray();
            }
        }

        // 패킷을 역직렬화. 수신시 해당 과정을 거쳐 패킷 클래스의 정보를 기입함.
        protected void Deserialize(byte[] data)
        {
            using (MemoryStream stream = new MemoryStream(data))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                // 패킷의 type은 가장 맨 앞에 있음.
                type = reader.ReadUInt16();
                foreach (var field in this.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    // type은 이미 읽었으므로 스킵.
                    if (field.Name == nameof(type)) continue;

                    if (field.FieldType == typeof(ushort)) field.SetValue(this, reader.ReadUInt16());
                    else if (field.FieldType == typeof(int)) field.SetValue(this, reader.ReadInt32());
                    else if (field.FieldType == typeof(string))
                    {
                        int strLen = reader.ReadInt32();
                        byte[] strData = reader.ReadBytes(strLen);
                        field.SetValue(this, Encoding.UTF8.GetString(strData));
                    }
                    else if (field.FieldType == typeof(List<float>))
                    {
                        int len = reader.ReadUInt16();
                        List<float> list = new List<float>();
                        for (int i = 0; i < len; i++)
                        {
                            list.Add(reader.ReadSingle());
                        }
                        field.SetValue(this, list);
                    }
                    else if (field.FieldType == typeof(List<List<float>>))
                    {
                        int llen = reader.ReadUInt16();
                        List<List<float>> fflist = new List<List<float>>();
                        for (int i = 0; i < llen; i++)
                        {
                            int len = reader.ReadUInt16();
                            List<float> flist = new List<float>();
                            for (int j = 0; j < len; j++)
                            {
                                flist.Add(reader.ReadSingle());
                            }
                            fflist.Add(flist);
                        }
                        field.SetValue(this, fflist);
                    }
                    else if (field.FieldType == typeof(List<Vector2[]>))
                    {
                        int listLen = reader.ReadInt32();
                        List<Vector2[]> list = new List<Vector2[]>();
                        for (int i = 0; i < listLen; i++)
                        {
                            int arrayLen = reader.ReadInt32();
                            list.Add(new Vector2[arrayLen]);
                            for (int j = 0; j < arrayLen; j++)
                            {
                                float x = reader.ReadSingle();
                                float y = reader.ReadSingle();
                                Vector2 vector = new Vector2(x, y);
                                list[i][j] = vector;
                            }
                        }
                        field.SetValue(this, list);
                    }
                    else if (field.FieldType == typeof(List<byte[]>))
                    {
                        int listLen = reader.ReadInt32();
                        List<byte[]> list = new List<byte[]>();
                        for (int i = 0; i < listLen; i++)
                        {
                            int arrayLen = reader.ReadInt32();
                            list.Add(reader.ReadBytes(arrayLen));
                        }
                        field.SetValue(this, list);
                    }
                }
            }
        }
    }
}
