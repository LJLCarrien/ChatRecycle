using UnityEngine;
using System.Collections;

public class ItemTwoCtrler : ItemCtrler {
    
    public override void UpdateItem()
    {
        MsgTwo infoT = info as MsgTwo;
        lbl.text = infoT.contentTwo;
    }
}
