using System;

public class ArrayAttribute : Attribute {

    //数组大小
    public int size;

    //是否为字符串
    public bool isString;

	public ArrayAttribute(int size, bool isString = false)
    {
        this.size = size;
        this.isString = isString;
    }
}
