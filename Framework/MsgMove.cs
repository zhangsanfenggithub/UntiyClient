using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MsgMove : MsgBase
{
    public MsgMove() { protoName = "MsgMove"; }
    public Dictionary<string, string> d = new Dictionary<string, string>();
    public int x = 0;
    public int y = 0;
    public int z = 0;
}
