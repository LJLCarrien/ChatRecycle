using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public interface IRecycle
{
    GameObject GetGo();
    Bounds bounds { get; set; }
    int dataIndex { get; set; }
    int itemType { get; set; }
}
public class Recycle
{
    public UIScrollView mScrollView { get; private set; }
    public UIPanel mPanel { get; private set; }
    public Bounds mPanelBounds { get; private set; }
    public UIScrollView.Movement mMovement { get { return mScrollView.movement; } }

    public Func<int, IRecycle> mAddItem;
    public Action<IRecycle> mUpdateItem;

    public readonly int Interval = 10;//间隔

    //展示中的
    public LinkedList<GameObject> showItemGoLinkList = new LinkedList<GameObject>();

    private GameObject mResPool;
    public Dictionary<GameObject, IRecycle> ItemGoDic = new Dictionary<GameObject, IRecycle>();


    public int mDataCount { get; private set; }

    public Func<int, int> GetDataType;

    public Recycle(UIScrollView sv, int itemInterval, Func<int, IRecycle> AddItem, Action<IRecycle> UpdateItem)
    {

        mScrollView = sv;
        mScrollView.restrictWithinPanel = false;
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
        IRecycle ctrler= default(IRecycle);
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
                ctrler = GetItem(++index);
                if (ctrler == null) return;
                go = ctrler.GetGo();
                if (mUpdateItem != null && ctrler != null) mUpdateItem(ctrler);
                itemBounds = NGUIMath.CalculateRelativeWidgetBounds(go.transform);//relactive/Abusoute?
                ctrler.bounds = itemBounds;
                go.transform.localPosition = new Vector3(0, tempBoundy, 0);
                //Debug.Log(tempBoundy);

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

    private IRecycle GetItem(int dindex)
    {
        int dataType = -1;
        IRecycle ctrler= default(IRecycle);

        if (GetDataType != null)
        {
            dataType = GetDataType(dindex);
            if (dataType == -1) return default(IRecycle);
            for (int i = 0; i < mResPool.transform.childCount; i++)
            {
                if (mResPool.transform.childCount <= 0) break;
                var t = mResPool.transform.GetChild(i).gameObject;
                ctrler = ItemGoDic[t];
                if (ctrler.itemType != dataType) continue;
                ctrler.dataIndex = dindex;
                RemoveFromResPool(t);
                return ctrler;
            }
        }
        else
        {
            if (mResPool.transform.childCount > 0)
            {
                var t = mResPool.transform.GetChild(0).gameObject;
                ctrler = ItemGoDic[t];
                ctrler.dataIndex = dindex;
                RemoveFromResPool(t);
                return ctrler;
            }
        }
        if (mAddItem != null)
        {
            ctrler = mAddItem(dindex);
            if (ctrler == null) return null;
            ctrler.dataIndex = dindex;
            var go = ctrler.GetGo();
            ItemGoDic.Add(go, ctrler);
        }

        return ctrler;
    }
    #endregion

    private void OnClipMove(UIPanel panel)
    {
        var scrollViewBounds = NGUIMath.CalculateRelativeWidgetBounds(mScrollView.transform);
        var tPanelOffset = panel.CalculateConstrainOffset(scrollViewBounds.min, scrollViewBounds.max);

        var moveTop = mMovement == UIScrollView.Movement.Vertical && tPanelOffset.y < -1;
        var moveDown = mMovement == UIScrollView.Movement.Vertical && tPanelOffset.y > 1;
        //Debug.LogError(tPanelOffset.x + "," + tPanelOffset.y);
        if (moveTop)
        {
            //往上拉
            //Debug.LogError("<-1");
            if (showItemGoLinkList.Count > 0)
            {
                var FirstGo = showItemGoLinkList.Last.Value;//最后一个
                IRecycle FirstCtrler = null;
                ItemGoDic.TryGetValue(FirstGo, out FirstCtrler);
                var FirstIndex = FirstCtrler.dataIndex;
                //Debug.Log(string.Format("{0},{1},{2},{3},{4}", FirstGo.transform.localPosition.y, FirstCtrler.mBounds.size.y, Interval, tempBoundy, FirstIndex));

                if (FirstIndex < mDataCount - 1)
                {
                    if (mMovement == UIScrollView.Movement.Vertical)
                    {
                        IRecycle ctrler = null;
                        var index = ++FirstIndex;
                        ctrler = GetItem(index);
                        //Debug.Log(index);

                        if (ctrler != null)
                        {
                            var go = ctrler.GetGo();
                            if (mUpdateItem != null) mUpdateItem(ctrler);
                            ctrler.bounds = NGUIMath.CalculateRelativeWidgetBounds(go.transform);
                            var tempBoundy = FirstGo.transform.localPosition.y - FirstCtrler.bounds.size.y - Interval;
                            go.transform.localPosition = new Vector3(0, tempBoundy, 0);
                            Add2ShowListFrom(ItemsState.Tail, go);
                            //Debug.Log(tempBoundy);
                        }
                    }
                }

                //删除
                bool isLastData = FirstCtrler.dataIndex == mDataCount - 1;
                if (mScrollView.isDragging && !isLastData)
                {
                    //Debug.Log("移删");
                    CheckBeyondRemoveToResPool();
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
                IRecycle FirstCtrler = null;
                ItemGoDic.TryGetValue(FirstGo, out FirstCtrler);
                var FirstIndex = FirstCtrler.dataIndex;

                if (FirstIndex > 0)
                {
                    if (mMovement == UIScrollView.Movement.Vertical)
                    {
                        IRecycle ctrler = null;
                        var index = --FirstIndex;
                        ctrler = GetItem(index);
                        if (ctrler != null)
                        {
                            var go = ctrler.GetGo();
                            if (mUpdateItem != null) mUpdateItem(ctrler);
                            ctrler.bounds = NGUIMath.CalculateRelativeWidgetBounds(go.transform);
                            var tempBoundy = FirstGo.transform.localPosition.y + Interval + ctrler.bounds.size.y;
                            go.transform.localPosition = new Vector3(0, tempBoundy, 0);
                            Add2ShowListFrom(ItemsState.Head, go);
                            //Debug.Log(ctrler.dataIndex);
                        }
                    }
                }
                //删除
                bool isFirstData = FirstCtrler.dataIndex == 0;
                if (mScrollView.isDragging && !isFirstData)
                {
                    //Debug.Log("移删");

                    CheckBeyondRemoveToResPool();
                }
            }
        }
    }

    private Bounds GetItemRealBounds(IRecycle ctrler)
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
    private void CheckBeyondRemoveToResPool(Action mFinishRemoveToResPool = null)
    {
        int showCount = showItemGoLinkList.Count;
        IRecycle ctrler = null;
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
            if (i == tmpList.Length - 1 && mFinishRemoveToResPool != null)
            {
                mFinishRemoveToResPool();
            }
        }
    }

    #region 事件
    private void RemoveEvent()
    {
        mPanel.onClipMove -= OnClipMove;
        mScrollView.onMomentumMove -= OnStoppedMoving;
        mScrollView.onDragStarted -= OnDragStarted;

    }

    private void RegisterEvent()
    {
        mPanel.onClipMove += OnClipMove;
        mScrollView.onStoppedMoving += OnStoppedMoving;
        mScrollView.onDragStarted += OnDragStarted;
        mScrollView.onDragFinished += OnDragFinished;

    }
    
    private void OnDragStarted()
    {
        //mScrollView.DisableSpring();
    }

    private void OnDragFinished()
    {

        if (showItemGoLinkList.Count <= 0) return;


        var firstGo = showItemGoLinkList.First.Value;
        var lastGo = showItemGoLinkList.Last.Value;

        var firstCtrler = ItemGoDic[firstGo];
        var lastCtrler = ItemGoDic[lastGo];

        var pos = Vector3.zero;
        Debug.LogError(string.Format("first:" + firstCtrler.dataIndex));
        Debug.LogError(string.Format("last:" + lastCtrler.dataIndex));

        //触顶
        if (firstCtrler.dataIndex == 0)
        {
            pos = Vector3.zero;
            SpringPanel.Begin(mPanel.gameObject, pos, 8f);

            Debug.LogError("顶部");
        }
        //触底
        else if (lastCtrler.dataIndex == mDataCount - 1)
        {
            var scrollViewBounds = NGUIMath.CalculateRelativeWidgetBounds(mScrollView.transform);
            var tPanelOffset = mPanel.CalculateConstrainOffset(scrollViewBounds.min, scrollViewBounds.max);

            var offsetMove = mPanel.transform.localPosition.y - Mathf.Abs(tPanelOffset.y);
            var vLast = new Vector3(mPanel.transform.localPosition.x, offsetMove, mPanel.transform.localPosition.z);
            SpringPanel.Begin(mPanel.gameObject, vLast, 8f);
            Debug.LogError("触底");

        }
    }
    private void OnStoppedMoving()
    {
        //CheckBeyondRemoveToResPool();

    }

    #endregion
}
