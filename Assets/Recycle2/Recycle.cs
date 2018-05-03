using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public interface IRecycle
{
    GameObject GetGo();
}
public class Recycle<T> where T : class, IRecycle
{
    public UIScrollView mScrollView { get; private set; }
    public UIPanel mPanel { get; private set; }
    public Bounds mBounds { get; private set; }
    public UIScrollView.Movement mMovement { get { return mScrollView.movement; } }

    public Func<T> mAddItem;
    public Action<T, int> mUpdateItem;

    public int Interval = 80;//间隔

    //展示中的
    public LinkedList<GameObject> showItemGoLinkList = new LinkedList<GameObject>();

    private GameObject mResPool;
    public Dictionary<GameObject, T> ItemGoDic = new Dictionary<GameObject, T>();


    public int mDataCount { get; private set; }
    public int mShowDataIndex { get; private set; }

    public Recycle(UIScrollView sv, Func<T> AddItem, Action<T, int> UpdateItem)
    {
        mScrollView = sv;
        mPanel = sv.panel;
        var mCenter = new Vector3(mPanel.finalClipRegion.x, mPanel.finalClipRegion.y, 0);
        var mSize = new Vector3(mPanel.finalClipRegion.z, mPanel.finalClipRegion.w);
        mBounds = new Bounds(mCenter, mSize);

        mAddItem = AddItem;
        mUpdateItem = UpdateItem;

        mShowDataIndex = 0;
        mDataCount = 0;
        RegisterEvent();

        mResPool = NGUITools.AddChild(mScrollView.transform.parent.gameObject);
        mResPool.name = "ItemsResPool";
        mResPool.SetActive(false);

    }


    public void ResetPostion(int dataCount = 0)
    {
        T ctrler = null;
        GameObject go = null;

        Bounds itemBounds;
        float tempBoundy;

        mDataCount = dataCount;

        if (mMovement == UIScrollView.Movement.Vertical)
        {
            tempBoundy = mBounds.max.y;
            while (tempBoundy > mBounds.min.y)
            {
                go = GetItem(ref ctrler); ;
                if (mUpdateItem != null && ctrler != null) mUpdateItem(ctrler, mShowDataIndex++);
                itemBounds = NGUIMath.CalculateAbsoluteWidgetBounds(go.transform);//relactive/Abusoute?
                go.transform.localPosition = new Vector3(0, tempBoundy, 0);
                tempBoundy = tempBoundy - itemBounds.size.y - Interval;
                Add2ShowListFrom(ItemsState.Tail, go);
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

        Debug.LogError(showItemGoLinkList.Count);
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
        Debug.LogError(showItemGoLinkList.Count);
    }

    //resPool
    private void AddToResPool(GameObject go)
    {
        go.transform.SetParent(mResPool.transform);
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
        Bounds bound = NGUIMath.CalculateRelativeWidgetBounds(mScrollView.transform);
        var tPanelOffset = panel.CalculateConstrainOffset(bound.min, bound.max);

        var moveTop = mMovement == UIScrollView.Movement.Vertical && tPanelOffset.y < -1;
        var moveDown = mMovement == UIScrollView.Movement.Vertical && tPanelOffset.y > 1;

        Debug.LogError(tPanelOffset.x+","+tPanelOffset.y);
        if (moveTop)
        {
            //往上拉
            //Debug.LogError("<-1");
            if (showItemGoLinkList.Count > 0)
            {
                Bounds itemBounds;
                itemBounds = NGUIMath.CalculateRelativeWidgetBounds(showItemGoLinkList.First.Value.transform);
                T ctrler = null;
                GameObject go = null;

                float tempBoundy;

                if (mShowDataIndex < mDataCount)
                {
                    if (mMovement == UIScrollView.Movement.Vertical)
                    {
                        tempBoundy = mScrollView.bounds.min.y - Interval;

                        go = GetItem(ref ctrler);
                        if (mUpdateItem != null && ctrler != null) mUpdateItem(ctrler, mShowDataIndex++);
                        itemBounds = NGUIMath.CalculateAbsoluteWidgetBounds(go.transform);//relactive/Abusoute?
                        go.transform.localPosition = new Vector3(0, tempBoundy, 0);
                        tempBoundy = tempBoundy - itemBounds.size.y - Interval;
                        Add2ShowListFrom(ItemsState.Tail, go);
                        Debug.LogError("加一个");
                    }
                }


                //RemoveShowListFrom(ItemsState.Head);

            }
        }
        else if (moveDown)
        {
            //Debug.LogError(">1");
            //往下拉
            //RemoveShowListFrom(ItemsState.Tail);

        }
    }

    //检测超出移到资源池
    private void CheckBeyondRemoveToResPool()
    {

    }

    #region 事件
    private void RegisterEvent()
    {
        mPanel.onClipMove += OnClipMove;
    }
    private void RemoveEvent()
    {
        mPanel.onClipMove -= OnClipMove;

    }
    #endregion
}
