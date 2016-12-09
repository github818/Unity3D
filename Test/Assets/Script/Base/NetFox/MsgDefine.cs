using BYTE = System.Byte;
using WORD = System.UInt16;
//////////////////////////////////////////////////////////////////////////////////


//结构定义

// 网络内核
public struct TCP_Info
{
    public BYTE cbDataKind;                        // 数据类型
    public BYTE cbCheckCode;                       // 效验字段
    public WORD wPacketSize;					   // 数据大小
};

// 网络命令
public struct TCP_Command
{
    public WORD wMainCmdID;                        // 主命令码
    public WORD wSubCmdID;						   // 子命令码
};

//网络包头
public struct TCP_Head
{
    public TCP_Info TCPInfo;                       // 基础结构
    public TCP_Command CommandInfo;                // 命令信息
};

public struct Aa
{
    public int a;

    [Array(32,true)]
    public string b;
};

public struct Bb
{
    public int index;

    [Array(5)]
    public Aa[] aa;
};
