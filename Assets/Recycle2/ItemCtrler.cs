using UnityEngine;
using System.Collections;
using System;

public abstract class ItemCtrler : MonoBehaviour,IRecycle
{
    public GameObject GetGo()
    {
        return this.gameObject;
    }
    public UILabel lbl
    {
        get
        {
            return transform.FindChild("Label").GetComponent<UILabel>();
        }
    }

    public Bounds bounds
    {
        get;

        set;
    }

    public int dataIndex
    {
        get;
        set;
    }
    private Msg mInfo;
    public Msg info
    {
        get { return mInfo; }
        set
        {
            mInfo=value;
            UpdateItem();
        }
    }
    public enum ItemTypes
    {
        itemOne,
        itemTwo
    }
    public int itemType
    {
        get;
        set;
    }

    abstract public void UpdateItem();
    

}
