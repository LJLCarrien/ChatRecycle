using UnityEngine;
using System.Collections;

public class ItemOneCtrler : ItemCtrler
{
    public override void UpdateItem()
    {
        MsgOne infoO = info as MsgOne;
        lbl.text = infoO.contentOne;
    }
}
