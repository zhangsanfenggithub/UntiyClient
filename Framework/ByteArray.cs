using System;
using UnityEngine;

public class ByteArray
{
    //默认大小
    const int DEFAULT_SIZE = 1024;
    //初始大小
    int initSize = 0;
    //缓冲区
    public byte[] bytes;
    //读写位置
    public int readIndex = 0;
    public int writeIndex = 0;
    //容量
    private int capacity = 0;
    //剩余空间
    public int Remain
    {
        get { return capacity - writeIndex; }
    }
    //有效数据长度
    public int Length
    {
        get { return writeIndex - readIndex; }
    }

    public ByteArray(int size = DEFAULT_SIZE)
    {
        bytes = new byte[size];
        capacity = size;
        initSize = size;
        readIndex = 0;
        writeIndex = 0;
    }
    public ByteArray(byte[] defaultBytes)
    {
        bytes = defaultBytes;
        readIndex = 0;
        writeIndex = defaultBytes.Length;
    }
    /// <summary>
    /// 重设尺寸
    /// </summary>
    /// <param name="size"></param>
    public void Resize(int size)
    {
        if (size < Length) return;
        if (size < initSize) return;
        int n = 1;
        while (n < size) n *= 2;
        capacity = n;
        byte[] newBytes = new byte[capacity];
        Array.Copy(bytes, readIndex, newBytes, 0, writeIndex - readIndex);
        bytes = newBytes;
        writeIndex = Length;
        readIndex = 0;
    }

    /// <summary>
    /// 检查并移动数据
    /// </summary>
    public void CheckAndMoveBytes()
    {
        if (Length < 8)
            MoveBytes();
    }
    /// <summary>
    /// 移动数据
    /// </summary>
    /// <returns></returns>
    public void MoveBytes()
    {
        Array.Copy(bytes, readIndex, bytes, 0, writeIndex - readIndex);
        writeIndex = Length;
        readIndex = 0;
    }

    public int Write(byte[] bs, int offset, int count)
    {
        if (Remain < count)
            Resize(Length + count);
        Array.Copy(bs, offset, bytes, writeIndex, count);
        writeIndex += count;
        return count;
    }

    public int Read(byte[] bs, int offset, int count)
    {
        count = Math.Min(count, Length);
        Array.Copy(bytes, 0, bs, offset, count);
        readIndex += count;
        CheckAndMoveBytes();
        return count;
    }
    public override string ToString()
    {
        return BitConverter.ToString(bytes, readIndex, Length);
    }

    public string Debug()
    {
        return $"readIndex:{readIndex}, writeIndex:{writeIndex}, bytes:{BitConverter.ToString(bytes, 0, bytes.Length)}";
    }
}


