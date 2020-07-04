using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using UnityEngine;

public class NetManager
{
    static Socket socket;
    static ByteArray readBuff;
    static Queue<ByteArray> writeQueue;
    public delegate void EventListener(string err);
    private static Dictionary<NetEvent, EventListener> eventListeners = new Dictionary<NetEvent, EventListener>();


    static bool isConnecting = false;
    static bool isClosing = false;

    public static void Close()
    {
        if (socket == null || !socket.Connected)
            return;
        if (isConnecting)
            return;
        //如果消息队列还有消息
        if (writeQueue.Count > 0)
        {
            isClosing = true;
        }
        else
        {
            socket.Close();
            FireEvent(NetEvent.Close, "");
        }
    }
    #region Connect
    public static void Connect(string ip, int port)
    {
        //状态判断
        if(socket != null && socket.Connected)
        {
            Debug.Log("Connect fail, already connected}");
            return;
        }
        if(isConnecting)
        {
            Debug.Log("Connecting fail, isConnecting");
            return;
        }

        //初始化成员
        InitState();
        socket.NoDelay = true;
        isConnecting = true;
        //开始发送
        socket.BeginConnect(ip, port, ConnectCallback, socket);
    }
    private static void ConnectCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket)ar.AsyncState;
            socket.EndConnect(ar);
            Debug.Log("Socket Connect Succ");
            FireEvent(NetEvent.ConnectSucc, "");
            isConnecting = false;
        }
        catch(SocketException ex)
        {
            Debug.Log($"Socket Connect fail {ex.ToString()}");
            FireEvent(NetEvent.ConnectFail, ex.ToString());
            isConnecting = false;
        }
    }
    #endregion 
    public static void Send(MsgBase msg)
    {
        if (socket == null || !socket.Connected)
            return;

        byte[] nameBytes = MsgBase.EcodeName(msg);
        byte[] bodyBytes = MsgBase.Encode(msg);
        Debug.Log($"[Send] {System.Text.Encoding.UTF8.GetString(nameBytes)}");
        Debug.Log($"[Send] {System.Text.Encoding.UTF8.GetString(bodyBytes)}");
        int len = nameBytes.Length + bodyBytes.Length;
        byte[] sendBytes = new byte[2 + len];//增加总消息长度
        //组装长度
        sendBytes[0] = (byte)(len % 256);
        sendBytes[1] = (byte)(len / 256);
        //组装名字
        Array.Copy(nameBytes, 0, sendBytes, 2, nameBytes.Length);
        //组装消息体
        Array.Copy(bodyBytes, 0, sendBytes, 2 + nameBytes.Length, bodyBytes.Length);
        Debug.Log($"[Send] {System.Text.Encoding.UTF8.GetString(sendBytes)}");
        //写入队列
        ByteArray ba = new ByteArray(sendBytes);
        int count = 0;
        lock (writeQueue)
        {
            writeQueue.Enqueue(ba);
            count = writeQueue.Count;
        }
        if (count == 1)
        {
            socket.BeginSend(sendBytes, 0, sendBytes.Length, 0, SendCallback, socket);
        }
    }

    private static void SendCallback(IAsyncResult ar)
    {
        Socket socket = (Socket)ar.AsyncState;
        if (socket == null || !socket.Connected)
            return;
        //EndSend结束时发送的长度
        int count = socket.EndSend(ar);
        ByteArray ba;
        //取队列的队首
        lock (writeQueue)
        {
            ba = writeQueue.First();
        }
        ba.readIndex += count;
        //当前的ba完全发送时取下一条数据
        if(ba.Length == 0)
        {
            lock(writeQueue)
            {
                writeQueue.Dequeue();
                ba = writeQueue.First();//ba指向队头
            }
        }
        //ba为空表明没有需要发送的消息
        if (ba != null)
        {
            socket.BeginSend(ba.bytes, ba.readIndex, ba.Length, 0, SendCallback, socket);
        }
        else if(isClosing)//如果正在关闭
        {
            socket.Close();
        }
    }
    private static void InitState()
    {
        //Socket
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //接收缓冲区
        readBuff = new ByteArray();
        //写入队列
        writeQueue = new Queue<ByteArray>();
        //是否正在连接
        isConnecting = false;
        //是否正在关闭
        isClosing = false;
    }
    
    //网络事件
    public static void AddEventListener(NetEvent netEvent, EventListener listener)
    {
        if(eventListeners.ContainsKey(netEvent))
        {
            eventListeners[netEvent] += listener;
        }
        else
        {
            eventListeners[netEvent] = listener;
        }
    }
    public static void RemoveEventListener(NetEvent netEvent, EventListener listener)
    {
        if(eventListeners.ContainsKey(netEvent))
        {
            eventListeners[netEvent] -= listener;
            if(eventListeners[netEvent] == null)
            {
                eventListeners.Remove(netEvent);
            }
        }
    }
    private static void FireEvent(NetEvent netEvent, string err)
    {
        if(eventListeners.ContainsKey(netEvent))
        {
            eventListeners[netEvent](err);
        }
    }
}


public enum NetEvent
{
    ConnectSucc = 1,
    ConnectFail = 2,
    Close = 3
}