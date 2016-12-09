using UnityEngine;


public class TestSocket : MonoBehaviour {


    //服务端有数据发送过来，就会执行这个方法。
    public void onReceived(NetFoxClient client, ClientEventArgs arg)
    {
        Bb msg = (Bb)arg.atts["msg"];
        
        //Bb bb = (Bb)instance.convertFrom(instance.convertTo(head));
        //client.send(instance.convertTo(head));
        //Debug.LogFormat("\n从{0}上来发来信息：head.wMainCmdID:{1};head.wSubCmdID:{2};\n", client.getRemoteEndPoint(), head.CommandInfo.wMainCmdID, head.CommandInfo.wSubCmdID);
        Debug.LogFormat("从{0}上来发来信息：;\n", client.getRemoteEndPoint());
        instance.sendMsg(msg);
        //String msg = Encoding.Default.GetString(buffer, 0, buffer.Length);
        //Debug.LogFormat("\n从{0}上来发来信息：{1}", client.getRemoteEndPoint(), msg);
    }

    public void onConnected(AsyncSocketClient client, ClientEventArgs arg)
    {
        //发送数据
        Bb bb = new Bb();
        bb.index = 77;
        Aa aa = new Aa();
        aa.a = 66;
        aa.b = "haha";
        bb.aa = new Aa[5];
        for (var i = 0; i < bb.aa.Length; ++i)
        {
            bb.aa[i] = aa;
        }
        instance.sendMsg(bb);
    }

    private NetFoxClient instance;

    // Use this for initialization
    void Start () {

        //instance = new MsgConverter();
        //instance.registerMsg(1, 2, typeof(Bb));
        //连接服务端
        //client = new AsyncSocketClient("127.0.0.1", 9372);
        //client.onConnected += onConnected;
        //client.onReceived += onReceived;
        //client.connect();

        instance = new NetFoxClient("127.0.0.1", 9372);
        instance.onReceiveMsg += onReceived;
        instance.onConnected += onConnected;
        instance.connect();
    }



    // Update is called once per frame
    void Update () {
	
	}
}
