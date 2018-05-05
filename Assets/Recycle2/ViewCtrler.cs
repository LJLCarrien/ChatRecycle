using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Msg
{
    public int fromWho;
}
public class MsgOne : Msg
{
    public string contentOne;
}
public class MsgTwo : Msg
{
    public string contentTwo;
}
public class ViewCtrler : MonoBehaviour
{
    public UIScrollView mScrollView;
    public Recycle<ItemCtrler> mRecycle;

    List<Msg> dataList = new List<Msg>();

    public void InitData()
    {
        for (int i = 0; i < 13; i++)
        {
            if (i % 2 == 0)
            {
                MsgOne m = new MsgOne { fromWho = 1, contentOne = i.ToString() };
                dataList.Add(m);
            }
            else
           {
                MsgTwo m = new MsgTwo { fromWho = 1, contentTwo = 0 + i.ToString() };
                dataList.Add(m);
            }
        }
    }

    void Start()
    {
        InitData();
        mRecycle = new Recycle<ItemCtrler>(mScrollView, 10, AddItem, UpdateItem);
        mRecycle.ResetPostion(dataList.Count);
        mRecycle.GetDataType = OnGetDataType;
    }
    private int OnGetDataType(int dIndex)
    {
        int type = -1;
        if (dataList[dIndex] is MsgOne)
        {
            type = (int)ItemCtrler.ItemTypes.itemOne;
            //Debug.Log("一类型");

        }
        else if (dataList[dIndex] is MsgTwo)
        {
            type = (int)ItemCtrler.ItemTypes.itemTwo;
            //Debug.Log("二类型");

        }
        return type;
    }

    public ItemCtrler AddItem(int dataIndex)
    {
        if (dataIndex >= dataList.Count) return null;

        if (dataList[dataIndex] is MsgOne)
        {
            var goPrefab = Resources.Load("TypeOne", typeof(GameObject)) as GameObject;
            GameObject go = NGUITools.AddChild(mScrollView.gameObject, goPrefab);
            var ctrler = go.AddComponent<ItemOneCtrler>();
            ctrler.itemType = (int)ItemCtrler.ItemTypes.itemOne;
            //Debug.Log("一类型");
            return ctrler;
        }
        else if (dataList[dataIndex] is MsgTwo)
        {
            var goPrefab = Resources.Load("TypeTwo", typeof(GameObject)) as GameObject;
            GameObject go = NGUITools.AddChild(mScrollView.gameObject, goPrefab);
            var ctrler = go.AddComponent<ItemTwoCtrler>();
            ctrler.itemType = (int)ItemCtrler.ItemTypes.itemTwo;
            //Debug.Log("二类型");
            return ctrler;
        }
        return null;
    }

    private void UpdateItem(ItemCtrler ctrler)
    {
        if (ctrler.dataIndex >= dataList.Count) return;
        Msg info = dataList[ctrler.dataIndex];
        if (info is MsgOne)
        {
            MsgOne mo = info as MsgOne;
            ctrler.info = mo;
        }
        else if (info is MsgTwo)
        {
            MsgTwo mt = info as MsgTwo;
            ctrler.info = mt;
        }
    }
}
