using UnityEngine;
using System;
using System.Runtime.InteropServices;
using BYTE = System.Byte;
using WORD = System.UInt16;
using DWORD = System.UInt32;


public class NetFoxClient : AsyncSocketClient
{
    //网络版本
    const byte SOCKET_VER = 0x05;								        

    // 加密密钥
    const DWORD g_dwPacketKey = 0xA55AA55A;

    //网络缓冲
    const int SOCKET_TCP_BUFFER = 16384;

    //发送映射
    readonly BYTE[] g_SendByteMap =
    {
        0x70,0x2F,0x40,0x5F,0x44,0x8E,0x6E,0x45,0x7E,0xAB,0x2C,0x1F,0xB4,0xAC,0x9D,0x91,
        0x0D,0x36,0x9B,0x0B,0xD4,0xC4,0x39,0x74,0xBF,0x23,0x16,0x14,0x06,0xEB,0x04,0x3E,
        0x12,0x5C,0x8B,0xBC,0x61,0x63,0xF6,0xA5,0xE1,0x65,0xD8,0xF5,0x5A,0x07,0xF0,0x13,
        0xF2,0x20,0x6B,0x4A,0x24,0x59,0x89,0x64,0xD7,0x42,0x6A,0x5E,0x3D,0x0A,0x77,0xE0,
        0x80,0x27,0xB8,0xC5,0x8C,0x0E,0xFA,0x8A,0xD5,0x29,0x56,0x57,0x6C,0x53,0x67,0x41,
        0xE8,0x00,0x1A,0xCE,0x86,0x83,0xB0,0x22,0x28,0x4D,0x3F,0x26,0x46,0x4F,0x6F,0x2B,
        0x72,0x3A,0xF1,0x8D,0x97,0x95,0x49,0x84,0xE5,0xE3,0x79,0x8F,0x51,0x10,0xA8,0x82,
        0xC6,0xDD,0xFF,0xFC,0xE4,0xCF,0xB3,0x09,0x5D,0xEA,0x9C,0x34,0xF9,0x17,0x9F,0xDA,
        0x87,0xF8,0x15,0x05,0x3C,0xD3,0xA4,0x85,0x2E,0xFB,0xEE,0x47,0x3B,0xEF,0x37,0x7F,
        0x93,0xAF,0x69,0x0C,0x71,0x31,0xDE,0x21,0x75,0xA0,0xAA,0xBA,0x7C,0x38,0x02,0xB7,
        0x81,0x01,0xFD,0xE7,0x1D,0xCC,0xCD,0xBD,0x1B,0x7A,0x2A,0xAD,0x66,0xBE,0x55,0x33,
        0x03,0xDB,0x88,0xB2,0x1E,0x4E,0xB9,0xE6,0xC2,0xF7,0xCB,0x7D,0xC9,0x62,0xC3,0xA6,
        0xDC,0xA7,0x50,0xB5,0x4B,0x94,0xC0,0x92,0x4C,0x11,0x5B,0x78,0xD9,0xB1,0xED,0x19,
        0xE9,0xA1,0x1C,0xB6,0x32,0x99,0xA3,0x76,0x9E,0x7B,0x6D,0x9A,0x30,0xD6,0xA9,0x25,
        0xC7,0xAE,0x96,0x35,0xD0,0xBB,0xD2,0xC8,0xA2,0x08,0xF3,0xD1,0x73,0xF4,0x48,0x2D,
        0x90,0xCA,0xE2,0x58,0xC1,0x18,0x52,0xFE,0xDF,0x68,0x98,0x54,0xEC,0x60,0x43,0x0F
    };

    //接收映射
    readonly BYTE[] g_RecvByteMap =
    {
        0x51,0xA1,0x9E,0xB0,0x1E,0x83,0x1C,0x2D,0xE9,0x77,0x3D,0x13,0x93,0x10,0x45,0xFF,
        0x6D,0xC9,0x20,0x2F,0x1B,0x82,0x1A,0x7D,0xF5,0xCF,0x52,0xA8,0xD2,0xA4,0xB4,0x0B,
        0x31,0x97,0x57,0x19,0x34,0xDF,0x5B,0x41,0x58,0x49,0xAA,0x5F,0x0A,0xEF,0x88,0x01,
        0xDC,0x95,0xD4,0xAF,0x7B,0xE3,0x11,0x8E,0x9D,0x16,0x61,0x8C,0x84,0x3C,0x1F,0x5A,
        0x02,0x4F,0x39,0xFE,0x04,0x07,0x5C,0x8B,0xEE,0x66,0x33,0xC4,0xC8,0x59,0xB5,0x5D,
        0xC2,0x6C,0xF6,0x4D,0xFB,0xAE,0x4A,0x4B,0xF3,0x35,0x2C,0xCA,0x21,0x78,0x3B,0x03,
        0xFD,0x24,0xBD,0x25,0x37,0x29,0xAC,0x4E,0xF9,0x92,0x3A,0x32,0x4C,0xDA,0x06,0x5E,
        0x00,0x94,0x60,0xEC,0x17,0x98,0xD7,0x3E,0xCB,0x6A,0xA9,0xD9,0x9C,0xBB,0x08,0x8F,
        0x40,0xA0,0x6F,0x55,0x67,0x87,0x54,0x80,0xB2,0x36,0x47,0x22,0x44,0x63,0x05,0x6B,
        0xF0,0x0F,0xC7,0x90,0xC5,0x65,0xE2,0x64,0xFA,0xD5,0xDB,0x12,0x7A,0x0E,0xD8,0x7E,
        0x99,0xD1,0xE8,0xD6,0x86,0x27,0xBF,0xC1,0x6E,0xDE,0x9A,0x09,0x0D,0xAB,0xE1,0x91,
        0x56,0xCD,0xB3,0x76,0x0C,0xC3,0xD3,0x9F,0x42,0xB6,0x9B,0xE5,0x23,0xA7,0xAD,0x18,
        0xC6,0xF4,0xB8,0xBE,0x15,0x43,0x70,0xE0,0xE7,0xBC,0xF1,0xBA,0xA5,0xA6,0x53,0x75,
        0xE4,0xEB,0xE6,0x85,0x14,0x48,0xDD,0x38,0x2A,0xCC,0x7F,0xB1,0xC0,0x71,0x96,0xF8,
        0x3F,0x28,0xF2,0x69,0x74,0x68,0xB7,0xA3,0x50,0xD0,0x79,0x1D,0xFC,0xCE,0x8A,0x8D,
        0x2E,0x62,0x30,0xEA,0xED,0x2B,0x26,0xB9,0x81,0x7C,0x46,0x89,0x73,0xA2,0xF7,0x72
    };

    //内核命令
    public const WORD MDM_KN_COMMAND = 0;                           //内核命令
    public const WORD SUB_KN_DETECT_SOCKET = 1;						//检测命令

    ///各种回调事件
    public delegate void NetFoxHandler(NetFoxClient client, ClientEventArgs arg);

    //当连接断开时
    public event NetFoxHandler onReceiveMsg;

    //构造函数
    public NetFoxClient(String address, int port) :
            base(address, port)
    {
        m_cbRecvBuf = new BYTE[SOCKET_TCP_BUFFER * 10];
        m_wRecvSize = 0;
        m_cbSendRound = 0;
        m_cbRecvRound = 0;
        m_dwSendXorKey = 0x12345678;
        m_dwRecvXorKey = 0x12345678;
        m_dwSendPacketCount = 0;

        TCP_Command cmd = new TCP_Command();
        cmdSize = (WORD)Marshal.SizeOf(cmd);
        TCP_Info info = new TCP_Info();
        infoSize = (WORD)Marshal.SizeOf(info);
        headSize = (WORD)(cmdSize + infoSize);

        msgConverter = new MsgConverter();
        msgConverter.registerMsg(1, 2, typeof(Bb));

        onReceived += onReceivedData;
        onConnected += onConnect;
    }

    public void onConnect(AsyncSocketClient client, ClientEventArgs arg)
    {
        sendMsg(MDM_KN_COMMAND, SUB_KN_DETECT_SOCKET);
    }

    WORD SeedRandMap(WORD wSeed)
    {
        DWORD dwHold = wSeed;
        return (WORD)((dwHold = dwHold * 241103 + 2533101) >> 16);
    }

    // 映射发送数据
    BYTE MapSendByte(BYTE cbData)
    {
        BYTE cbMap = g_SendByteMap[(BYTE)(cbData + m_cbSendRound)];
        m_cbSendRound += 11;
	    return cbMap;
    }

    // 映射接收数据
    BYTE MapRecvByte(BYTE cbData)
    {
        BYTE cbMap = (BYTE)(g_RecvByteMap[cbData] - m_cbRecvRound);
        m_cbRecvRound += 11;
        return cbMap;
    }

    void memset(BYTE[] dest, BYTE val, int size)
    {
        memset(dest, 0, val, size);
    }

    void memset(BYTE[] dest, int index, BYTE val, int size)
    {
        for (int i = index; i < dest.Length; ++i)
        {
            dest[i] = val;
        }
    } 

    // 加密数据
    BYTE[] EncryptBuffer(BYTE[] pcbDataBuffer)
    {
        // 调整长度
        WORD wDataSize = (WORD)pcbDataBuffer.Length;
        WORD wEncryptSize = (WORD)(wDataSize - cmdSize);
        WORD wSnapCount = 0;
        if ((wEncryptSize % sizeof(DWORD)) != 0)
        {
            wSnapCount = (WORD)(sizeof(DWORD) - wEncryptSize % sizeof(DWORD));            
            memset(pcbDataBuffer, infoSize + wEncryptSize, 0, wSnapCount);
        }
        
        // 效验码与字节映射
        BYTE cbCheckCode = 0;
        for (WORD i = infoSize; i < wDataSize; ++i)
        {
            cbCheckCode += pcbDataBuffer[i];
            pcbDataBuffer[i] = MapSendByte(pcbDataBuffer[i]);
        }

        // 重写信息头
        TCP_Info info = new TCP_Info();
        info.cbDataKind = SOCKET_VER;
        info.cbCheckCode = (BYTE)(~cbCheckCode + 1);
        info.wPacketSize = wDataSize;
        byte[] infoData = msgConverter.getBytes(info);
        infoData.CopyTo(pcbDataBuffer, 0);

        // 创建密钥
        DWORD dwXorKey = m_dwSendXorKey;
        if (m_dwSendPacketCount == 0)
        {
            // 生成第一次随机种子
            Guid guid = Guid.NewGuid();
            byte[] guidData = guid.ToByteArray();
            
            dwXorKey = (DWORD)(Environment.TickCount * Environment.TickCount);
            dwXorKey ^= BitConverter.ToUInt32(guidData, 0);
            dwXorKey ^= BitConverter.ToUInt16(guidData, 4);
            dwXorKey ^= BitConverter.ToUInt16(guidData, 6);
            dwXorKey ^= BitConverter.ToUInt32(guidData, 8);

            // 随机映射种子
            dwXorKey = SeedRandMap((WORD)dwXorKey);
            dwXorKey |= ((DWORD)SeedRandMap((WORD)(dwXorKey >> 16))) << 16;
            dwXorKey = 1;
            dwXorKey ^= g_dwPacketKey;
            m_dwSendXorKey = dwXorKey;
            m_dwRecvXorKey = dwXorKey;
        }

        // 加密数据
        int seedIndex = infoSize;        
        int xorIndex = infoSize;
        WORD wEncrypCount = (WORD)((wEncryptSize + wSnapCount) / sizeof(DWORD));
        for (WORD i = 0; i < wEncrypCount; ++i)
        {
            DWORD curr = BitConverter.ToUInt32(pcbDataBuffer, xorIndex);
            curr ^= dwXorKey;
            byte[] currByte = BitConverter.GetBytes(curr);
            currByte.CopyTo(pcbDataBuffer, xorIndex);
            xorIndex += 4;
            dwXorKey = SeedRandMap(BitConverter.ToUInt16(pcbDataBuffer,seedIndex));
            seedIndex += 2;
            dwXorKey |= ((DWORD)SeedRandMap(BitConverter.ToUInt16(pcbDataBuffer, seedIndex)) << 16);
            seedIndex += 2;
            dwXorKey ^= g_dwPacketKey;
        }

        // 插入密钥
        if (m_dwSendPacketCount == 0)
        {
            //修改大小
            info.wPacketSize += sizeof(DWORD);
            msgConverter.getBytes(info).CopyTo(pcbDataBuffer, 0);

            byte[] ret = new byte[info.wPacketSize];
            //消息头
            Buffer.BlockCopy(pcbDataBuffer, 0, ret, 0, headSize);
            // 密钥
            byte[] keyByte = BitConverter.GetBytes(m_dwSendXorKey);
            Buffer.BlockCopy(keyByte,0,ret,headSize, keyByte.Length);
            //写入后面消息部分
            Buffer.BlockCopy(pcbDataBuffer, headSize, ret, headSize+ sizeof(DWORD), pcbDataBuffer.Length - headSize);
            pcbDataBuffer = ret;
        }

        // 设置变量
        ++m_dwSendPacketCount;
        m_dwSendXorKey = dwXorKey;

        return pcbDataBuffer;
    }

    // 解密数据
    WORD CrevasseBuffer(ref BYTE[] pcbDataBuffer)
    {
        WORD wDataSize = (WORD)pcbDataBuffer.Length;
        // 调整长度
        WORD wSnapCount = 0;
        if ((wDataSize % sizeof(DWORD)) != 0)
        {
            wSnapCount = (WORD)(sizeof(DWORD) - wDataSize % sizeof(DWORD));
            memset(pcbDataBuffer, wDataSize, 0, wSnapCount);
        }

        // 解密数据
        DWORD dwXorKey = m_dwRecvXorKey;        
        int xorIndex = infoSize;        
        int seedIndex = infoSize;
        WORD wEncrypCount = (WORD)((wDataSize + wSnapCount - infoSize) / 4);
        for (WORD i = 0; i < wEncrypCount; i++)
        {
            if ((i == (wEncrypCount - 1)) && (wSnapCount > 0))
            {                
                byte[] pcbKey = BitConverter.GetBytes(m_dwSendXorKey);
                Buffer.BlockCopy(pcbKey, 0, pcbDataBuffer, wDataSize, wSnapCount);                
            }
            dwXorKey = SeedRandMap(BitConverter.ToUInt16(pcbDataBuffer, seedIndex));
            seedIndex += 2;
            dwXorKey |= ((DWORD)SeedRandMap(BitConverter.ToUInt16(pcbDataBuffer, seedIndex))) << 16;
            seedIndex += 2;
            dwXorKey ^= g_dwPacketKey;            
            DWORD curr = BitConverter.ToUInt32(pcbDataBuffer, xorIndex);
            curr ^= m_dwRecvXorKey;
            byte[] currByte = BitConverter.GetBytes(curr);
            currByte.CopyTo(pcbDataBuffer, xorIndex);
            xorIndex += 4;
            m_dwRecvXorKey = dwXorKey;
        }

        // 效验码与字节映射
        int index = 0;
        TCP_Head head = msgConverter.parseTCPHead(ref pcbDataBuffer,ref index);
        BYTE cbCheckCode = head.TCPInfo.cbCheckCode;
        for (int i = infoSize; i < wDataSize; ++i)
        {
            pcbDataBuffer[i] = MapRecvByte(pcbDataBuffer[i]);
            cbCheckCode += pcbDataBuffer[i];
        }

        if (cbCheckCode != 0)
        {
            Debug.LogFormat("数据包效验码错误");            
        }
        return wDataSize;
    }

    public void sendMsg(ushort mainCmdID, ushort subCmdID)
    {
        TCP_Head head = new TCP_Head();
        head.CommandInfo.wMainCmdID = mainCmdID;
        head.CommandInfo.wSubCmdID = subCmdID;        
        byte[] data = msgConverter.getBytes(head);
        //加密
        byte[] encrypt = EncryptBuffer(data);
        send(encrypt);
    }

    public void sendMsg(object msg)
    {
        byte[] data = msgConverter.convertTo(msg);
        //加密
        byte[] encrypt = EncryptBuffer(data);
        send(encrypt);
    }

    public void onReceivedData(AsyncSocketClient client, ClientEventArgs arg)
    {
        byte[] buff = (byte[])arg.atts["data"];
        buff.CopyTo(m_cbRecvBuf, m_wRecvSize);
        m_wRecvSize += buff.Length;

        if (m_wRecvSize >= headSize)
        {
            int index = 0;
            TCP_Head head = msgConverter.parseTCPHead(ref m_cbRecvBuf, ref index);
            int packetSize = head.TCPInfo.wPacketSize;
            //效验参数
            if (SOCKET_VER != head.TCPInfo.cbDataKind)
            {                
                Debug.LogFormat("数据包版本错误");
                m_wRecvSize = 0;
                return;
            }

            if (m_wRecvSize < packetSize)
            {
                Debug.LogFormat("数据不足，m_wRecvSize={0} wPacketSize={1} 继续读\n", m_wRecvSize, packetSize);
                return;
            }

            //拷贝数据
            byte[] dataBuffer = new Byte[packetSize];
            Buffer.BlockCopy(m_cbRecvBuf, 0, dataBuffer, 0, packetSize);
            m_wRecvSize -= packetSize;
            Buffer.BlockCopy(m_cbRecvBuf, 0, m_cbRecvBuf, packetSize, m_wRecvSize);

            //解密数据			
            WORD wRealySize = CrevasseBuffer(ref dataBuffer);
            index = 0;
            head = msgConverter.parseTCPHead(ref dataBuffer, ref index);
            //内核命令
            if (head.CommandInfo.wMainCmdID == MDM_KN_COMMAND)
            {
                switch (head.CommandInfo.wSubCmdID)
                {
                    case SUB_KN_DETECT_SOCKET:  //网络检测
                    {
                        //发送数据
                        sendMsg(MDM_KN_COMMAND, SUB_KN_DETECT_SOCKET);
                        return;
                    }
                }
            }

            ClientEventArgs msg = new ClientEventArgs();
            msg.atts.Add("msg", msgConverter.convertFrom(dataBuffer));
            raiseReceiveMsg(msg);            
        }
    }

    private void raiseReceiveMsg(ClientEventArgs arg)
    {
        if (null != onReceiveMsg)
        {
            onReceiveMsg(this, arg);
        }
    }

    private MsgConverter msgConverter;

    // 加密数据
    BYTE m_cbSendRound;                         // 字节映射
    BYTE m_cbRecvRound;                         // 字节映射
    DWORD m_dwSendXorKey;                       // 发送密钥
    DWORD m_dwRecvXorKey;                       // 接收密钥

    // 计数变量
	DWORD m_dwSendPacketCount;                  // 发送计数

    BYTE[] m_cbRecvBuf;			                // 接收缓冲

    int m_wRecvSize;

    WORD cmdSize;
    WORD infoSize;
    WORD headSize;

}
