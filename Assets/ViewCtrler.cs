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
    //dataIndex,Height
    Dictionary<int, int> SpecialHeightDic = new Dictionary<int, int>();
    private int count = 20;
    public void InitData()
    {
        SpecialHeightDic.Clear();
        for (int i = 0; i < count; i++)
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
    [ContextMenu("Add")]
    public void AddData()
    {
        for (int i = count; i < changIndex; i++)
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
        mRecycle.UpdateData(dataList.Count);
    }

    [ContextMenu("Init")]
    void Initit()
    {
        InitData();
        mRecycle.ResetPostion(dataList.Count);
    }
    void Start()
    {
        mRecycle = new Recycle<ItemCtrler>(mScrollView, 10, AddItem, UpdateItem, OnGetDataType,null);
        Initit();
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
    List<ItemCtrler> ctrlerList = new List<ItemCtrler>();

    public ItemCtrler AddItem(int dataIndex)
    {
        if (dataIndex >= dataList.Count) return null;

        if (dataList[dataIndex] is MsgOne)
        {
            var goPrefab = Resources.Load("TypeOne", typeof(GameObject)) as GameObject;
            GameObject go = NGUITools.AddChild(mScrollView.gameObject, goPrefab);
            var ctrler = go.AddComponent<ItemOneCtrler>();
            ctrler.itemType = (int)ItemCtrler.ItemTypes.itemOne;
            ctrlerList.Add(ctrler);
            //Debug.Log("一类型");
            return ctrler;
        }
        else if (dataList[dataIndex] is MsgTwo)
        {
            var goPrefab = Resources.Load("TypeTwo", typeof(GameObject)) as GameObject;
            GameObject go = NGUITools.AddChild(mScrollView.gameObject, goPrefab);
            var ctrler = go.AddComponent<ItemTwoCtrler>();
            ctrler.itemType = (int)ItemCtrler.ItemTypes.itemTwo;
            ctrlerList.Add(ctrler);
            //Debug.Log("二类型");
            return ctrler;
        }
        return null;
    }

    private void UpdateItem(ItemCtrler ctrler, int dataIndex)
    {
        var newHeight = 0;
        if (dataIndex >= dataList.Count) return;
        Msg info = dataList[dataIndex];

        if (info is MsgOne)
        {
            MsgOne mo = info as MsgOne;
            ctrler.info = mo;
            ctrler.UpdateItem();

            if (SpecialHeightDic.TryGetValue(dataIndex, out newHeight))
                ctrler.UpdateHeight(newHeight);
            else
                ctrler.UpdateHeight();

        }
        else if (info is MsgTwo)
        {
            MsgTwo mt = info as MsgTwo;
            ctrler.info = mt;
            ctrler.UpdateItem();
            if (SpecialHeightDic.TryGetValue(dataIndex, out newHeight))
                ctrler.UpdateHeight(newHeight);
            else
                ctrler.UpdateHeight();

        }
    }
    public int changIndex = 0;

    [ContextMenu("Move")]
    public void Move()
    {
        mRecycle.MoveItemByIndex(changIndex);
    }
    [ContextMenu("ForceRefresh")]
    public void ForceRefresh()
    {
        mRecycle.ForceReshItem();
    }
    [ContextMenu("SetHeight")]

    public void SetHeight()
    {
        var newHeight = 30;
        ctrlerList[changIndex].UpdateHeight(newHeight);
        if (SpecialHeightDic.ContainsKey(changIndex))
        {
            SpecialHeightDic[changIndex] = newHeight;
        }
        else
        {
            SpecialHeightDic.Add(changIndex, newHeight);
        }
    }
    [ContextMenu("resetPos")]

    public void resetPos()
    {
        mRecycle.ResetPostion();

    }
    [ContextMenu("DebugList")]
    public void DebugList()
    {
        ctrlerList.ForEach(item =>
        {
            Debug.LogError(string.Format("{0},{1}", item.dataIndex, item.height));
        });

    }
}
