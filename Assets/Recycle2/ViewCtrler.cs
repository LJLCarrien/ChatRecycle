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

        //mPanel = mScrollView.panel;
        //var mCenter = new Vector3(mPanel.finalClipRegion.x, mPanel.finalClipRegion.y, 0);
        //var mSize = new Vector3(mPanel.finalClipRegion.z, mPanel.finalClipRegion.w);
        //scrollb = new Bounds(mCenter, mSize);
    }
    //UIPanel mPanel;
    //public GameObject go;
    //Bounds b;
    //Bounds scrollb;
    //[ContextMenu("bounds")]
    //public void TestBounds()
    //{
    //    go = GameObject.Find("Sprite");

    //    Bounds b = NGUIMath.CalculateRelativeWidgetBounds(go.transform);
    //    Bounds tempBounds=new Bounds();
    //    tempBounds = b;
    //    //tempBounds.center = new Vector3(b.center.x, b.center.y + mScrollView.transform.localPosition.y + go.transform.localPosition.y, b.center.z);
    //    tempBounds.center +=  mScrollView.transform.localPosition+ go.transform.localPosition;

    //    b = tempBounds;

    //    Debug.LogError(string.Format("{0},{1},{2},{3}", b.center, b.min, b.max, b.size));
    //    Debug.LogError(string.Format("scrollb:{0},{1},{2},{3}", scrollb.center, scrollb.min, scrollb.max, scrollb.size));

    //    TestInter();
    //}

    //[ContextMenu("inter")]
    //public void TestInter()
    //{

    //    if (scrollb.Intersects(b))
    //    {
    //        Debug.LogError("相交");
    //    }
    //    else
    //    {
    //        Debug.LogError("不相交");

    //    }

    //}

    public ItemCtrler AddItem()
    {
        var goPrefab = Resources.Load("TypeOne", typeof(GameObject)) as GameObject;
        GameObject go = NGUITools.AddChild(mScrollView.gameObject, goPrefab);
        var ctrler = go.AddComponent<ItemCtrler>();
        return ctrler;
    }

    private void UpdateItem(ItemCtrler ctrler)
    {
        if (ctrler.dataIndex >= dataList.Count) return;
        ctrler.SetData(ctrler.dataIndex);
    }
}
