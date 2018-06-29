using UnityEngine;
using System.Collections;

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
    public UISprite bg
    {
        get
        {
            return transform.FindChild("Sprite").GetComponent<UISprite>();
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

    public int height;

    public void UpdateHeight(int h=0)
    {
        if (h == 0)
        {
            if (info is MsgOne)
            {
                height = 80;
            }
            else if (info is MsgTwo)
            {
                height = 100;
            }
        }
       else
        {
            height = h;
        }

        bg.height = height;
    }
}
