using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;


public class MsgConverter  {

    public readonly static byte SOCKET_VER = 0x05;								        //网络版本

    public readonly static Encoding inCoding = Encoding.UTF8;                           //从网络传来转成编码
    public readonly static Encoding outCoding = Encoding.Default;                       //发送到网络转成编码

    // 从对象转成byte数组
    public byte[] convertTo(object msg)
    {
        byte[] msgData = getBytes(msg);
        KeyValuePair<ushort, ushort> cmdId;
        if (idMap.TryGetValue(msg.GetType(), out cmdId))
        {
            //消息头
            TCP_Head head = new TCP_Head();
            head.CommandInfo.wMainCmdID = cmdId.Key;
            head.CommandInfo.wSubCmdID = cmdId.Value;
            head.TCPInfo.cbDataKind = SOCKET_VER;
            head.TCPInfo.wPacketSize = (ushort)(msgData.Length + Marshal.SizeOf(head));
            byte[] headData = getBytes(head);
            MemoryStream mem = new MemoryStream();
            //先写消息头
            mem.Write(headData, 0, headData.Length);
            //然后是消息体
            mem.Write(msgData, 0, msgData.Length);
            byte[] ret = mem.ToArray();
            mem.Close();
            return ret;
        }
        else
        {
            Debug.LogFormat("未注册的消息：{0}", msg.GetType().Name);
            return null;
        }        
    }

    public byte[] getBytes(object msg)
    {
        MemoryStream mem = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(mem);
        getValue(msg, ref writer);
        writer.Flush();
        byte[] msgData = mem.ToArray();
        writer.Close();
        mem.Close();
        return msgData;
    }

    protected void getPrimitive(object obj, ref BinaryWriter writer)
    {
        TypeCode typeCode = Type.GetTypeCode(obj.GetType());
        switch (typeCode)
        {
            case TypeCode.Byte:
            case TypeCode.SByte:
                writer.Write((byte)obj);
                break;
            case TypeCode.Boolean:
                writer.Write((bool)obj);
                break;
            case TypeCode.Char:
                writer.Write((char)obj);
                break;
            case TypeCode.Single:
                writer.Write((float)obj);
                break;
            case TypeCode.Double:
                writer.Write((double)obj);
                break;
            case TypeCode.Int16:
                writer.Write((short)obj);
                break;
            case TypeCode.Int32:
                writer.Write((int)obj);
                break;
            case TypeCode.Int64:
                writer.Write((long)obj);
                break;
            case TypeCode.UInt16:
                writer.Write((ushort)obj);
                break;
            case TypeCode.UInt32:
                writer.Write((uint)obj);
                break;
            case TypeCode.UInt64:
                writer.Write((ulong)obj);
                break;
            default:
                break;
        }
    }

    protected void getArray(object obj, ref BinaryWriter writer)
    {
        Array arr = obj as Array;
        for (int i = 0; i < arr.Length; ++i)
        {
            getValue(arr.GetValue(i), ref writer);
        }
    }

    protected void getValue(object obj, ref BinaryWriter writer)
    {
        Type type = obj.GetType();
        FieldInfo[] fields = type.GetFields();
        foreach (FieldInfo field in fields)
        {            
            if (field.FieldType.IsPrimitive)
            {
                getPrimitive(field.GetValue(obj), ref writer);
            }
            else
            {
                //数组处理
                if (field.FieldType.IsArray)
                {
                    getArray(field.GetValue(obj), ref writer);
                }
                else
                {
                    if(typeof(string) == field.FieldType)
                    {
                        //尝试获取自定义特性
                        ArrayAttribute arrAtt = (ArrayAttribute)Attribute.GetCustomAttribute(field, typeof(ArrayAttribute));
                        if (null == arrAtt)
                        {
                            Debug.LogFormat("字符串未发现Array特性修饰！name:{0}\n", field.Name);
                            return;
                        }
                        byte[] buff = new byte[arrAtt.size];
                        string str = field.GetValue(obj) as string;
                        outCoding.GetBytes(str).CopyTo(buff,0);
                        writer.Write(buff);
                    }
                    else
                    {
                        //其余struct对象
                        getValue(field.GetValue(obj), ref writer);
                    }                    
                }
            }
        }
    }

    // 从byte数组转成对象
    public object convertFrom(byte[] data)
    {
        int index = 0;
        //读取消息头
        TCP_Head head = parseTCPHead(ref data, ref index);
        if (0 < head.TCPInfo.wPacketSize)
        {
            //长度预判
            if ( (data.Length) < head.TCPInfo.wPacketSize )
            {
                Debug.LogFormat("错误的消息长度: 需要:{0}; 实际:{1}\n", head.TCPInfo.wPacketSize, (data.Length - index));
                return null;
            }
            uint msgIndex = makeUint(head.CommandInfo.wMainCmdID, head.CommandInfo.wSubCmdID);
            Type msgType;
            if (msgMap.TryGetValue(msgIndex, out msgType))
            {                
                object obj = Activator.CreateInstance(msgType);
                setValue(ref obj, msgType, ref data, ref index);
                return obj;
            }
            else
            {
                Debug.LogFormat("未知的消息类型: wMainCmdID:{0}; wSubCmdID:{1}\n", head.CommandInfo.wMainCmdID, head.CommandInfo.wSubCmdID);
                return null;
            }
        }
        return head;
    }


    public TCP_Head parseTCPHead(ref byte[] data, ref int index)
    {
        
        object ret = new TCP_Head();
        //长度预判
        if (data.Length < Marshal.SizeOf(ret))
        {
            Debug.LogFormat("数据长度过小: {0}; wSubCmdID:{1}\n", data.Length);
        }
        setValue(ref ret, typeof(TCP_Head), ref data, ref index);
        return (TCP_Head)ret;
    }

    protected void setValue(ref object obj, Type type, ref byte[] data, ref int index)
    {
        FieldInfo[] fields = type.GetFields();
        foreach(FieldInfo field in fields)
        {
            setValue(ref obj, field, ref data, ref index);
        }
    }

    protected void setPrimitive(ref object obj, FieldInfo field, ref byte[] data, ref int index)
    {
        TypeCode typeCode = Type.GetTypeCode(field.FieldType);
        int size = Marshal.SizeOf(field.GetValue(obj));
        switch (typeCode)
        {
            case TypeCode.Byte:
            case TypeCode.SByte:
                field.SetValue(obj, data[index]);
                break;
            case TypeCode.Boolean:
                field.SetValue(obj, BitConverter.ToBoolean(data, index));
                break;
            case TypeCode.Char:
                field.SetValue(obj, BitConverter.ToChar(data, index));
                break;
            case TypeCode.Single:
                field.SetValue(obj, BitConverter.ToSingle(data, index));
                break;
            case TypeCode.Double:
                field.SetValue(obj, BitConverter.ToDouble(data, index));
                break;
            case TypeCode.Int16:
                field.SetValue(obj, BitConverter.ToInt16(data, index));
                break;
            case TypeCode.Int32:
                field.SetValue(obj, BitConverter.ToInt32(data, index));
                break;
            case TypeCode.Int64:
                field.SetValue(obj, BitConverter.ToInt64(data, index));
                break;
            case TypeCode.UInt16:
                field.SetValue(obj, BitConverter.ToUInt16(data, index));
                break;
            case TypeCode.UInt32:
                field.SetValue(obj, BitConverter.ToUInt32(data, index));
                break;
            case TypeCode.UInt64:
                field.SetValue(obj, BitConverter.ToUInt64(data, index));
                break;
            default:
                break;
        }
        index += size;
    }

    protected void setArray(ref object obj, FieldInfo field, ref byte[] data, ref int index)
    {
        //尝试获取自定义特性
        ArrayAttribute arrAtt = (ArrayAttribute)Attribute.GetCustomAttribute(field, typeof(ArrayAttribute));
        if (null == arrAtt)
        {
            Debug.LogFormat("数组未发现Array特性修饰！name:{0}\n", field.Name);
            return;
        }
        if (arrAtt.isString)
        {
            //按字符串处理                    
            field.SetValue(obj, inCoding.GetString(data, index, arrAtt.size));
            index += arrAtt.size;
        }
        else
        {
            //动态创建数组
            var elementType = field.FieldType.GetElementType();
            var listType = typeof(List<>).MakeGenericType(elementType);
            var list = Activator.CreateInstance(listType);
            var addMethod = listType.GetMethod("Add");
            var toArrayMethod = listType.GetMethod("ToArray");
            for (int i = 0; i < arrAtt.size; ++i)
            {
                //创建数组元素
                object subObj = Activator.CreateInstance(elementType);
                //赋值
                setValue(ref subObj, elementType, ref data, ref index);
                addMethod.Invoke(list, new object[] { subObj });
            }
            field.SetValue(obj, toArrayMethod.Invoke(list, new object[] { }));

        }
    }

    protected void setValue(ref object obj, FieldInfo field, ref byte[] data, ref int index)
    {
        if (field.FieldType.IsPrimitive)
        {
            setPrimitive(ref obj, field, ref data, ref index);
        }
        else
        {
            //数组处理
            if (field.FieldType.IsArray || typeof(string) == field.FieldType )
            {
                setArray(ref obj, field, ref data, ref index);
            }
            else
            {
                //其余struct对象
                object subObj = field.GetValue(obj);
                setValue(ref subObj, field.FieldType, ref data, ref index);
                field.SetValue(obj, subObj);
            }
        }
    }


    //注册消息（把消息id 与具体的消息类建立联系）
    public void registerMsg(ushort mainCmdID, ushort subCmdID, Type msgType)
    {
        uint index = makeUint(mainCmdID, subCmdID);
        msgMap.Add(index, msgType);
        idMap.Add(msgType, new KeyValuePair<ushort, ushort>(mainCmdID, subCmdID));
    }

    private Dictionary<uint, Type> msgMap;

    private Dictionary<Type, KeyValuePair<ushort,ushort>> idMap;

    public MsgConverter()
    {
        msgMap = new Dictionary<uint, Type>();
        idMap = new Dictionary<Type, KeyValuePair<ushort, ushort>>();
    }

    public static uint makeUint(ushort high, ushort low)
    {
        return (uint)((low & 0xffff) | ((high & 0xffff) << 16));
    }
}
