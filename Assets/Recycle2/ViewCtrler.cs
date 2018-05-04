using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class tMsg
{
   public int fromWho;
}
public class MsgOne: tMsg
{
    public int contentOne;
}
public class MsgTwo: tMsg
{
    public string contentTwo;
}
public class ViewCtrler : MonoBehaviour
{
    public UIScrollView mScrollView;
    public Recycle<ItemCtrler> mRecycle;

    List<tMsg> dataList = new List<tMsg>();

    public void InitData()
    {
        for (int i = 0; i < 10; i++)
        {
            MsgOne m = new MsgOne {fromWho =1,contentOne=i};            
            dataList.Add(m);
        }
        for (int i = 0; i < 10; i++)
        {
            MsgTwo m = new MsgTwo { fromWho = 1, contentTwo = 0+i.ToString() };
            dataList.Add(m);
        }
    }

    void Start()
    {
        InitData();
        mRecycle = new Recycle<ItemCtrler>(mScrollView,10, AddItem, UpdateItem);
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
        tMsg info = dataList[ctrler.dataIndex];
        if (info is MsgOne)
        {
            MsgOne mo = info as MsgOne;
            ctrler.SetData(mo.contentOne);

        }
        else if (info is MsgTwo)
        {
            MsgTwo mt = info as MsgTwo;
            ctrler.SetDataTwo(mt.contentTwo);

        }
    }
}
