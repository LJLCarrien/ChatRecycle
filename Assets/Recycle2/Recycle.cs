using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public interface IRecycle
{
    GameObject GetGo();
    Bounds bounds { get; set; }
    int dataIndex { get; set; }
}
public class Recycle<T> where T : class, IRecycle
{
    public UIScrollView mScrollView { get; private set; }
    public UIPanel mPanel { get; private set; }
    public Bounds mPanelBounds { get; private set; }
    public UIScrollView.Movement mMovement { get { return mScrollView.movement; } }

    public Func<T> mAddItem;
    public Action<T> mUpdateItem;

    public readonly int Interval = 10;//间隔

    //展示中的
    public LinkedList<GameObject> showItemGoLinkList = new LinkedList<GameObject>();

    private GameObject mResPool;
    public Dictionary<GameObject, T> ItemGoDic = new Dictionary<GameObject, T>();


    public int mDataCount { get; private set; }

    public Recycle(UIScrollView sv, int itemInterval, Func<T> AddItem, Action<T> UpdateItem)
    {

        mScrollView = sv;
        mPanel = sv.panel;
        var mCenter = new Vector3(mPanel.finalClipRegion.x, mPanel.finalClipRegion.y, 0);
        var mSize = new Vector3(mPanel.finalClipRegion.z, mPanel.finalClipRegion.w);
        mPanelBounds = new Bounds(mCenter, mSize);

        Interval = itemInterval;
        mAddItem = AddItem;
        mUpdateItem = UpdateItem;

        mDataCount = 0;
        RegisterEvent();

        mResPool = NGUITools.AddChild(mScrollView.transform.parent.gameObject);
        mResPool.name = "ItemsResPool";
        mResPool.SetActive(false);

    }

    public void DestoryRecycle()
    {
        RemoveEvent();
        mScrollView = null;
        mPanel = null;
        
        
    }

    public void ResetPostion(int dataCount = 0)
    {
        T ctrler = null;
        GameObject go = null;

        Bounds itemBounds;
        float tempBoundy;

        mDataCount = dataCount;
        int index = -1;
        if (mMovement == UIScrollView.Movement.Vertical)
        {
            tempBoundy = mPanelBounds.max.y;
            while (tempBoundy > mPanelBounds.min.y)
            {
                go = GetItem(ref ctrler); ;
                ctrler.dataIndex = ++index;
                if (mUpdateItem != null && ctrler != null) mUpdateItem(ctrler);
                itemBounds = NGUIMath.CalculateRelativeWidgetBounds(go.transform);//relactive/Abusoute?
                ctrler.bounds = itemBounds;
                go.transform.localPosition = new Vector3(0, tempBoundy, 0);
                tempBoundy = tempBoundy - itemBounds.size.y - Interval;
                Add2ShowListFrom(ItemsState.Tail, go);
                //Debug.Log(string.Format("{0},{1},{2},{3},{4}", go.transform.localPosition, ctrler.mBounds.size.y, Interval, tempBoundy, ctrler.dataIndex));

            }
        }

    }
    #region 辅助

    public enum ItemsState
    {
        Head,
        Tail
    }
    //linkList
    public void Add2ShowListFrom(ItemsState state, GameObject go)
    {
        if (state == ItemsState.Tail)
            showItemGoLinkList.AddLast(go);
        else if (state == ItemsState.Head)
            showItemGoLinkList.AddFirst(go);

        //Debug.LogError(showItemGoLinkList.Count);
    }

    public void RemoveShowListFrom(ItemsState state)
    {
        if (showItemGoLinkList.Count <= 0) return;
        GameObject go = null;
        if (state == ItemsState.Tail)
        {
            go = showItemGoLinkList.Last.Value;
            showItemGoLinkList.RemoveLast();

        }
        else if (state == ItemsState.Head)
        {
            go = showItemGoLinkList.First.Value;
            showItemGoLinkList.RemoveFirst();
        }
        AddToResPool(go);
        //Debug.LogError(showItemGoLinkList.Count);
    }

    //resPool
    private void AddToResPool(GameObject go)
    {
        go.transform.SetParent(mResPool.transform);
        showItemGoLinkList.Remove(go);
    }
    private void RemoveFromResPool(GameObject go)
    {
        go.transform.SetParent(mScrollView.transform);
    }

    private GameObject GetItem(ref T ctrler)
    {
        GameObject go = null;
        if (mResPool.transform.childCount > 0)
        {
            go = mResPool.transform.GetChild(0).gameObject;
            RemoveFromResPool(go);
            if (ItemGoDic.TryGetValue(go, out ctrler))
            {
                Debug.LogError("池里找到了");
            }
        }
        else
        {
            if (mAddItem != null)
            {
                ctrler = mAddItem();
                go = ctrler.GetGo();
                ItemGoDic.Add(go, ctrler);
            }
        }
        return go;
    }
    #endregion

    private void OnClipMove(UIPanel panel)
    {
        var scrollViewBounds = NGUIMath.CalculateRelativeWidgetBounds(mScrollView.transform);
        var tPanelOffset = panel.CalculateConstrainOffset(scrollViewBounds.min, scrollViewBounds.max);

        var moveTop = mMovement == UIScrollView.Movement.Vertical && tPanelOffset.y < -1;
        var moveDown = mMovement == UIScrollView.Movement.Vertical && tPanelOffset.y > 1;
        //Debug.LogError(tPanelOffset.x+","+tPanelOffset.y);
        if (moveTop)
        {
            //往上拉
            //Debug.LogError("<-1");
            if (showItemGoLinkList.Count > 0)
            {
                var FirstGo = showItemGoLinkList.Last.Value;//最后一个
                T FirstCtrler = null;
                ItemGoDic.TryGetValue(FirstGo, out FirstCtrler);
                var FirstIndex = FirstCtrler.dataIndex;
                var tempBoundy = FirstGo.transform.localPosition.y - FirstCtrler.bounds.size.y - Interval;
                //Debug.Log(string.Format("{0},{1},{2},{3},{4}", FirstGo.transform.localPosition.y, FirstCtrler.mBounds.size.y, Interval, tempBoundy, FirstIndex));

                if (FirstIndex < mDataCount - 1)
                {
                    if (mMovement == UIScrollView.Movement.Vertical)
                    {
                        T ctrler = null;
                        var go = GetItem(ref ctrler);
                        if (ctrler != null)
                        {
                            ctrler.dataIndex = ++FirstIndex;
                            if (mUpdateItem != null) mUpdateItem(ctrler);
                            ctrler.bounds = NGUIMath.CalculateRelativeWidgetBounds(go.transform);
                            go.transform.localPosition = new Vector3(0, tempBoundy, 0);
                            Add2ShowListFrom(ItemsState.Tail, go);
                            Debug.Log(ctrler.dataIndex);
                        }
                    }
                }
            }

        }
        else if (moveDown)
        {
            //往下拉
            //Debug.LogError(">1");
            if (showItemGoLinkList.Count > 0)
            {
                var FirstGo = showItemGoLinkList.First.Value;//第一个
                T FirstCtrler = null;
                ItemGoDic.TryGetValue(FirstGo, out FirstCtrler);
                var FirstIndex = FirstCtrler.dataIndex;
                var tempBoundy = FirstGo.transform.localPosition.y + FirstCtrler.bounds.size.y + Interval;

                if (FirstIndex > 0)
                {
                    if (mMovement == UIScrollView.Movement.Vertical)
                    {
                        T ctrler = null;
                        var go = GetItem(ref ctrler);
                        if (ctrler != null)
                        {
                            ctrler.dataIndex = --FirstIndex;
                            if (mUpdateItem != null) mUpdateItem(ctrler);
                            ctrler.bounds = NGUIMath.CalculateRelativeWidgetBounds(go.transform);
                            go.transform.localPosition = new Vector3(0, tempBoundy, 0);
                            Add2ShowListFrom(ItemsState.Head, go);
                            Debug.Log(ctrler.dataIndex);
                        }
                    }
                }
            }
        }
    }

    private Bounds GetItemRealBounds(T ctrler)
    {
        Bounds itemBounds;
        GameObject go = ctrler.GetGo();
        itemBounds = NGUIMath.CalculateRelativeWidgetBounds(go.transform);
        //Debug.LogError(string.Format("前：{0},{1},{2},{3}", itemBounds.center, itemBounds.min, itemBounds.max, itemBounds.size));

        Bounds tempBounds = new Bounds();
        tempBounds = itemBounds;
        tempBounds.center += mScrollView.transform.localPosition + go.transform.localPosition;

        return tempBounds;
    }
    //检测超出移到资源池
    private void CheckBeyondRemoveToResPool()
    {
        int showCount = showItemGoLinkList.Count;
        T ctrler = null;
        Bounds b;
        GameObject go;
        GameObject[] tmpList = new GameObject[showCount];
        showItemGoLinkList.CopyTo(tmpList, 0);
        //Debug.Log(tmpList.Length);
        for (int i = 0; i < tmpList.Length; i++)
        {
            go = tmpList[i];
            if (ItemGoDic.TryGetValue(go, out ctrler))
            {
                b = GetItemRealBounds(ctrler);
                //Debug.LogError(string.Format("{0},{1},{2}", b.center, b.min, b.max));

                if (!mPanelBounds.Intersects(b))
                {
                    //Debug.Log("移除"+ b.center);
                    AddToResPool(go);
                }
            }
        }
    }

    #region 事件
    private void RegisterEvent()
    {
        mPanel.onClipMove += OnClipMove;
        mScrollView.onDragFinished += OnDragFinisehd;
    }
    private void RemoveEvent()
    {
        mPanel.onClipMove -= OnClipMove;
        mScrollView.onDragFinished -= OnDragFinisehd;
    }
    private void OnDragFinisehd()
    {
        CheckBeyondRemoveToResPool();
    }

    #endregion
}
