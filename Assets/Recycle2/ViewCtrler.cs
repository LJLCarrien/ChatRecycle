using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class ViewCtrler : MonoBehaviour
{
    public UIScrollView mScrollView;
    public Recycle<ItemCtrler> mRecycle;

    List<int> dataList = new List<int>();
    public void InitData()
    {
        for (int i = 0; i < 10; i++)
        {
            dataList.Add(i);
        }
    }

    void Start()
    {
        InitData();
        mRecycle = new Recycle<ItemCtrler>(mScrollView, AddItem, UpdateItem);
        mRecycle.ResetPostion(dataList.Count);
    }


    public ItemCtrler AddItem()
    {
        var goPrefab = Resources.Load("TypeOne", typeof(GameObject)) as GameObject;
        GameObject go = NGUITools.AddChild(mScrollView.gameObject, goPrefab);
        var ctrler = go.AddComponent<ItemCtrler>();
        return ctrler;
    }

    private void UpdateItem(ItemCtrler ctrler, int dataIndex)
    {
        if (dataIndex >= dataList.Count) return;
        ctrler.SetData(dataList[dataIndex]);
    }
}
