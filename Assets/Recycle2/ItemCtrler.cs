using UnityEngine;
using System.Collections;
using System;

public class ItemCtrler : MonoBehaviour,IRecycle
{
    public GameObject GetGo()
    {
        return this.gameObject;
    }
    public UILabel mLbl
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

    public void SetData(int i)
    {
        mLbl.text = i.ToString();
    }
    public void SetDataTwo(string i)
    {
        mLbl.text = i;
    }
}
