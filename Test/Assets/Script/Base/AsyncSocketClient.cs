using System;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;

public class AsyncSocketClient
{

    //Tcp连接
    private TcpClient _tcpClient;

    //缓冲区
    private byte[] _buffer;

    //服务器地址
    public IPAddress _address { get; private set; }

    //端口
    public int _port { get; private set; }

    ///各种回调事件
    public delegate void ClientEventHandler(AsyncSocketClient client, ClientEventArgs arg);

    //当连接返回时
    public event ClientEventHandler onConnected;
    //当发送返回时
    public event ClientEventHandler onSend;
    //当收到数据时
    public event ClientEventHandler onReceived;    
    //当连接断开时
    public event ClientEventHandler onDisconnected;

    public bool isConnected() { return _tcpClient.Connected; }

    public EndPoint getRemoteEndPoint()
    {
        return _tcpClient.Client.RemoteEndPoint;
    }

    public AsyncSocketClient(String address, int port)
        :this(IPAddress.Parse(address),port)
    {
       
    }

    public AsyncSocketClient(IPAddress address, int port)
    {
        _address = address;
        _port = port;
        _tcpClient = new TcpClient();        
        this._buffer = new byte[_tcpClient.ReceiveBufferSize];
    }

    public void connect()
    {        
        _tcpClient.BeginConnect(_address, _port, connectCallBack, _tcpClient);
    }

    public void close()
    {
        _tcpClient.Close();
        ClientEventArgs arg = new ClientEventArgs();
        raiseEvent(onDisconnected, arg);
    }

    private void connectCallBack(IAsyncResult ar)
    {
        try
        {
            //还原原始的TcpClient对象
            TcpClient client = (TcpClient)ar.AsyncState;
            client.EndConnect(ar);
            ClientEventArgs arg = new ClientEventArgs();
            raiseEvent(onConnected, arg);
            if(client.Connected)
            {
                Debug.LogFormat("与服务器{0}连接成功", client.Client.RemoteEndPoint);
                //开始接受消息
                receive();
            }            
        }
        catch (Exception e)
        {
            Debug.LogFormat(e.ToString());
        }
    }

    private void raiseEvent(ClientEventHandler clientEvent, ClientEventArgs arg)
    {
        if (null != clientEvent)
        {
            clientEvent(this, arg);
        }
    }

    private void receive()
    {
        NetworkStream stream = _tcpClient.GetStream();        
        stream.BeginRead(_buffer, 0, _buffer.Length, receivedCallBack, stream);
    }

    private void receivedCallBack(IAsyncResult ar)
    {        
        try
        {
            NetworkStream stream = (NetworkStream)ar.AsyncState;
            int bytesRead = 0;
            bytesRead = stream.EndRead(ar);
            if (0 < bytesRead)
            {
                ClientEventArgs arg = new ClientEventArgs();
                MemoryStream mem = new MemoryStream();
                mem.Write(_buffer, 0, bytesRead);
                mem.Position = 0;
                arg.atts.Add("data", mem.ToArray());
                raiseEvent(onReceived, arg);
            }
            //继续读
            receive();
        }

        catch (Exception e)
        {
            Debug.LogFormat(e.ToString());
        }
    }

    public void send(byte[] data)
    {
        NetworkStream stream = _tcpClient.GetStream();
        stream.BeginWrite(data, 0, data.Length, sendCallBack, stream);
    }

    private void sendCallBack(IAsyncResult ar)
    {
        NetworkStream stream = (NetworkStream)ar.AsyncState;
        stream.EndWrite(ar);
        ClientEventArgs arg = new ClientEventArgs();
        raiseEvent(onSend, arg);
    }


    private void raiseDisconnected(ClientEventArgs arg)
    {

    }

}

public class ClientEventArgs
{
    /// 提示信息  
    public string msg;

    // 是否通过过了 
    public bool result;

    //附件
    public Dictionary<String, object> atts;

    public ClientEventArgs()
    {
        atts = new Dictionary<string, object>();
    }
}
