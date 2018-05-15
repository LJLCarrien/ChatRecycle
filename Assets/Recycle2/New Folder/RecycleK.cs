using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class RecycleK 
{

    public UIScrollView mScrollView { get; private set; }
    public UIPanel mPanel { get; private set; }
    public Bounds mPanelBounds { get; private set; }
    public UIScrollView.Movement mMovement { get { return mScrollView.movement; } }

    public Func<int, GameObject> mAddItem;
    public Action<int, GameObject> mUpdateItem;

    public readonly int Interval = 10;//间隔

    //展示在UI的
    public LinkedList<GameObject> showItemGoLinkList = new LinkedList<GameObject>();

    private GameObject mResPool;
    //所有
    public List<GameObject> ItemGoList = new List<GameObject>();

    public int mDataCount { get; private set; }

    public RecycleK(UIScrollView sv, int itemInterval, Func<int, GameObject> AddItem, Action<int, GameObject> UpdateItem)
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
    public void ResetPostion(int dataCount = 0)
    {

        Bounds itemBounds;
        float tempBoundy;
        mDataCount = dataCount;
        int index = -1;
        int newIndex = -1;
        GameObject newGo = null;
        if (mMovement == UIScrollView.Movement.Vertical)
        {
            tempBoundy = mPanelBounds.max.y;
            while (tempBoundy > mPanelBounds.min.y)
            {
                newIndex = ++index;
                newGo = GetItem(newIndex);
                if (mUpdateItem != null && newGo != null) mUpdateItem(newIndex, newGo);
                itemBounds = NGUIMath.CalculateRelativeWidgetBounds(newGo.transform);
                newGo.transform.localPosition = new Vector3(0, tempBoundy, 0);
                //Debug.Log(tempBoundy);

                tempBoundy = tempBoundy - itemBounds.size.y - Interval;
                Add2ShowListFrom(ItemsState.Tail, newGo);

                //Debug.Log(string.Format("{0},{1},{2},{3},{4}", go.transform.localPosition, ctrler.mBounds.size.y, Interval, tempBoundy, ctrler.dataIndex));

            }
        }

    }
    #region 辅助
    public Func<GameObject, int, bool> mIsSameGoType;

    public Func<GameObject, Bounds> onGetBounds;
    private Bounds GetBounds(GameObject go, out bool success)
    {
        if (onGetBounds != null)
        {
            success = true;
            return onGetBounds(go);
        }
        success = false;
        Debug.LogError("获取边界失败");
        return new Bounds(Vector3.zero, Vector3.zero);
    }

    public Func<GameObject, bool> onIsFirstOne;
    public Func<GameObject, bool> onIsLastOne;
    private bool IsFirstOne(GameObject go)
    {
        return onIsFirstOne != null && onIsFirstOne(go);
    }
    private bool IsLastOne(GameObject go)
    {
        return onIsLastOne != null && onIsLastOne(go);
    }

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

    public Func<GameObject, int> onGetDataIndex;
    private int GetDataIndex(GameObject go)
    {
        if (onGetDataIndex != null)
        {
            return onGetDataIndex(go);
        }
        return -1;
    }
    #endregion

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


        var pos = Vector3.zero;

        ////触顶
        //if (IsFirstOne(firstGo))
        //{
        //    pos = Vector3.zero;
        //    SpringPanel.Begin(mPanel.gameObject, pos, 8f);

        //    Debug.LogError("顶部");
        //}
        ////触底
        //else if (IsLastOne(lastGo))
        //{
        //    var scrollViewBounds = NGUIMath.CalculateRelativeWidgetBounds(mScrollView.transform);
        //    var tPanelOffset = mPanel.CalculateConstrainOffset(scrollViewBounds.min, scrollViewBounds.max);

        //    var offsetMove = mPanel.transform.localPosition.y - Mathf.Abs(tPanelOffset.y);
        //    var vLast = new Vector3(mPanel.transform.localPosition.x, offsetMove, mPanel.transform.localPosition.z);
        //    SpringPanel.Begin(mPanel.gameObject, vLast, 8f);
        //    Debug.LogError("触底");

        //}
    }
    private void OnStoppedMoving()
    {
        //CheckBeyondRemoveToResPool();

    }



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
                var go = showItemGoLinkList.Last.Value;
                if (!IsLastOne(go))
                {
                    if (mMovement == UIScrollView.Movement.Vertical)
                    {
                        var dataIndex = GetDataIndex(go);
                        bool bBoundsSuss = false;
                        var goBounds = GetBounds(go, out bBoundsSuss);

                        var newIndex = ++dataIndex;
                        var newGo = GetItem(newIndex);
                        Debug.Log(newIndex);

                        if (newGo != null && bBoundsSuss)
                        {
                            bool getBoundsSucc;
                            if (mUpdateItem != null) mUpdateItem(newIndex, newGo);
                            var newGoBounds = GetBounds(newGo, out getBoundsSucc);
                            if (getBoundsSucc)
                            {
                                var tempBoundy = go.transform.localPosition.y - goBounds.size.y - Interval;
                                newGo.transform.localPosition = new Vector3(0, tempBoundy, 0);
                                Add2ShowListFrom(ItemsState.Tail, newGo);
                                //Debug.Log(tempBoundy);
                            }

                        }
                    }
                }

                //删除
                if (mScrollView.isDragging && !IsLastOne(go))
                {
                    Debug.Log("移删");
                    CheckBeyondRemoveToResPool();
                }
            }

        }
        else if (moveDown)
        {
            ////往下拉
            ////Debug.LogError(">1");
            //if (showItemGoLinkList.Count > 0)
            //{
            //    var FirstGo = showItemGoLinkList.First.Value;//第一个
            //    T FirstCtrler = null;
            //    ItemGoDic.TryGetValue(FirstGo, out FirstCtrler);
            //    var FirstIndex = FirstCtrler.dataIndex;

            //    if (FirstIndex > 0)
            //    {
            //        if (mMovement == UIScrollView.Movement.Vertical)
            //        {
            //            T ctrler = null;
            //            var index = --FirstIndex;
            //            ctrler = GetItem(index);
            //            if (ctrler != null)
            //            {
            //                var go = ctrler.GetGo();
            //                if (mUpdateItem != null) mUpdateItem(ctrler);
            //                ctrler.bounds = NGUIMath.CalculateRelativeWidgetBounds(go.transform);
            //                var tempBoundy = FirstGo.transform.localPosition.y + Interval + ctrler.bounds.size.y;
            //                go.transform.localPosition = new Vector3(0, tempBoundy, 0);
            //                Add2ShowListFrom(ItemsState.Head, go);
            //                //Debug.Log(ctrler.dataIndex);
            //            }
            //        }
            //    }
            //    //删除
            //    bool isFirstData = FirstCtrler.dataIndex == 0;
            //    if (mScrollView.isDragging && !isFirstData)
            //    {
            //        //Debug.Log("移删");

            //        CheckBeyondRemoveToResPool();
            //    }
            //}
        }
    }
    #endregion

    #region resPool
  
    private void AddToResPool(GameObject go)
    {
        go.transform.SetParent(mResPool.transform);
        showItemGoLinkList.Remove(go);
    }
    private void RemoveFromResPool(GameObject go)
    {
        go.transform.SetParent(mScrollView.transform);
    }
    //从资源池或新增的方式获取go
    private GameObject GetItem(int dindex)
    {
        int dataType = -1;

        if (mIsSameGoType != null)
        {
            for (int i = 0; i < mResPool.transform.childCount; i++)
            {
                if (mResPool.transform.childCount <= 0) break;
                var go = mResPool.transform.GetChild(i).gameObject;
                if (mIsSameGoType(go, dindex))
                {
                    return go;
                }
            }
        }
        if (mAddItem != null)
        {
            return mAddItem(dindex);
        }
        return null;
    }
    //检测超出移到资源池
    private void CheckBeyondRemoveToResPool(Action mFinishRemoveToResPool = null)
    {
        int showCount = showItemGoLinkList.Count;
        Bounds bounds;
        bool bSucc;
        GameObject go;
        GameObject[] tmpList = new GameObject[showCount];
        showItemGoLinkList.CopyTo(tmpList, 0);
        //Debug.Log(tmpList.Length);
        for (int i = 0; i < tmpList.Length; i++)
        {
            go = tmpList[i];

            bounds = GetBounds(go, out bSucc);
            //Debug.LogError(string.Format("{0},{1},{2}", b.center, b.min, b.max));
            if (bSucc)
            {
                if (!mPanelBounds.Intersects(bounds))
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
    #endregion
}
