using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MsgBase
{
    //协议名称
    public string protoName = "";

    public static byte[] Encode(MsgBase msgBase)
    {
        string s = JsonMapper.ToJson(msgBase);
        Debug.Log(s);
        return System.Text.Encoding.UTF8.GetBytes(s);
    }

    public static object Decode(string protoName, byte[] bytes, int offset, int count)
    {
        string s = System.Text.Encoding.UTF8.GetString(bytes, offset, count);
        object msg = JsonMapper.ToObject(s, Type.GetType(protoName));
        return msg;
    }

    public static byte[] EcodeName(MsgBase msgBase)
    {
        byte[] nameBytes = System.Text.Encoding.UTF8.GetBytes(msgBase.protoName);
        Int16 len = (Int16)nameBytes.Length;
        byte[] bytes = new byte[2 + len];
        bytes[0] = (byte)(len % 256);
        bytes[1] = (byte)(len / 256);
        //组装名字bytes
        Array.Copy(nameBytes, 0, bytes, 2, len);
        return bytes;
    }

    public static string DecodeName(byte[] bytes, int offset, out int count)
    {
        count = 0;
        //名称长度必须>2
        if (offset + 2 > bytes.Length)
            return "";
        //解析长度
        Int16 nameLen = (Int16)((bytes[offset + 1] << 8) | bytes[offset]);
        //长度必须足够
        if (offset + 2 + nameLen > bytes.Length)
            return "";
        count = 2 + nameLen;
        string name = System.Text.Encoding.UTF8.GetString(bytes, offset + 2, nameLen);
        return name;
    }
}
