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
public class Recycle<T> where T : class, IRecycle
{
    public UIScrollView mScrollView { get; private set; }
    public UIPanel mPanel { get; private set; }
    public Bounds mPanelBounds { get; private set; }
    public UIScrollView.Movement mMovement { get { return mScrollView.movement; } }

    public Func<int, T> mAddItem;
    public Action<T, int> mUpdateItem;

    public readonly int Interval = 10;//间隔

    //展示中的
    public LinkedList<GameObject> showItemGoLinkList = new LinkedList<GameObject>();

    private GameObject mResPool;
    public Dictionary<GameObject, T> ItemGoDic = new Dictionary<GameObject, T>();


    public int mDataCount { get; private set; }

    public Func<int, int> GetDataType;
    #region 入口
    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="sv"></param>
    /// <param name="itemInterval">物体item间隔</param> 
    /// <param name="addItem"> 添加回调</param>
    /// <param name="updateItem">更新回调</param>

    public Recycle(UIScrollView sv, int itemInterval, Func<int, T> addItem, Action<T, int> updateItem)
    {

        mScrollView = sv;
        mScrollView.restrictWithinPanel = false;
        mScrollView.transform.DestroyChildren();

        mPanel = sv.panel;
        var mCenter = new Vector3(mPanel.finalClipRegion.x, mPanel.finalClipRegion.y, 0);
        var mSize = new Vector3(mPanel.finalClipRegion.z, mPanel.finalClipRegion.w);
        mPanelBounds = new Bounds(mCenter, mSize);

        Interval = itemInterval;
        mAddItem = addItem;
        mUpdateItem = updateItem;

        mDataCount = 0;
        RegisterEvent();

        mResPool = NGUITools.AddChild(mScrollView.transform.parent.gameObject);
        mResPool.name = "ItemsResPool";
        mResPool.SetActive(false);

    }
    /// <summary>
    /// 销毁清理
    /// </summary>
    public void DestoryRecycle()
    {
        RemoveEvent();
        mScrollView = null;
        mPanel = null;
    }
    /// <summary>
    /// 重置位置
    /// </summary>
    /// <param name="dataCount"></param>
    public void ResetPostion(int dataCount = -1)
    {
        if (dataCount != -1)
            mDataCount = dataCount;

        MoveItemByIndex(0);
    }
    #endregion

    #region 预设管理

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
    private void MoveAllItemToResPool()
    {
        var count = showItemGoLinkList.Count;
        for (int i = 0; i < count; i++)
        {
            RemoveShowListFrom(ItemsState.Tail);
        }
        mPanel.clipOffset = Vector2.zero;
        mScrollView.transform.localPosition = Vector3.zero;
    }
    /// <summary>
    /// 从缓存池获取预设，无则添加
    /// </summary>
    /// <param name="dindex"></param>
    /// <returns></returns>
    private T GetItemInResPoolOrAdd(int dindex)
    {
        int dataType = -1;
        if (GetDataType != null)
        {
            dataType = GetDataType(dindex);
            if (dataType == -1) return null;
        }
        T ctrler = null;

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
    #region 辅助
    /// <summary>
    /// 获取物体的真正边界
    /// </summary>
    /// <param name="ctrler"></param>
    /// <returns></returns>
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

    #endregion
    #region 核心逻辑

    private bool bRestrictWithinPanel;
    private int moveDir;

    private void OnClipMove(UIPanel panel)
    {
        var scrollViewBounds = NGUIMath.CalculateRelativeWidgetBounds(mScrollView.transform);
        var tPanelOffset = panel.CalculateConstrainOffset(scrollViewBounds.min, scrollViewBounds.max);

        var moveTop = mMovement == UIScrollView.Movement.Vertical && tPanelOffset.y < -1;
        var moveDown = mMovement == UIScrollView.Movement.Vertical && tPanelOffset.y > 1;
        //Debug.LogError(tPanelOffset.x + "," + tPanelOffset.y);
        moveDir = moveTop ? -1 : moveDown ? 1 : 0;
        //Debug.LogError(tPanelOffset.y);
        //Debug.LogError(moveDir);
        if (moveTop)
        {
            //往上拉
            //Debug.LogError("往上拉<-1");
            if (showItemGoLinkList.Count > 0)
            {
                var lastGo = showItemGoLinkList.Last.Value;//最后一个
                T lastCtrler = null;
                ItemGoDic.TryGetValue(lastGo, out lastCtrler);
                if (lastCtrler != null)
                {
                    var firstIndex = lastCtrler.dataIndex;
                    //Debug.Log(string.Format("{0},{1},{2},{3},{4}", FirstGo.transform.localPosition.y, FirstCtrler.mBounds.size.y, Interval, tempBoundy, FirstIndex));

                    if (firstIndex < mDataCount - 1)
                    {
                        if (mMovement == UIScrollView.Movement.Vertical)
                        {
                            T ctrler = null;
                            var index = ++firstIndex;
                            ctrler = GetItemInResPoolOrAdd(index);
                            //Debug.Log(index);

                            if (ctrler != null)
                            {
                                var go = ctrler.GetGo();
                                if (mUpdateItem != null) mUpdateItem(ctrler, index);
                                ctrler.bounds = NGUIMath.CalculateRelativeWidgetBounds(go.transform);
                                var tempBoundy = lastGo.transform.localPosition.y - lastCtrler.bounds.size.y - Interval;
                                go.transform.localPosition = new Vector3(0, tempBoundy, 0);
                                Add2ShowListFrom(ItemsState.Tail, go);
                                //Debug.Log(tempBoundy);
                            }
                        }
                    }
                    //删除
                    bool isLastData = lastCtrler.dataIndex == mDataCount - 1;
                    bRestrictWithinPanel = isLastData;
                    //Debug.LogError(isLastData);
                    if (mScrollView.isDragging)
                    {
                        //Debug.Log("移删");
                        if (!isLastData)
                            CheckBeyondRemoveToResPool();
                    }
                    else if (isLastData)
                    {
                        RestrictWithinBounds();
                    }
                }


            }
        }
        else if (moveDown)
        {
            //往下拉
            //Debug.LogError("往下拉>1");
            if (showItemGoLinkList.Count > 0)
            {
                var firstGo = showItemGoLinkList.First.Value;//第一个
                T firstCtrler = null;
                ItemGoDic.TryGetValue(firstGo, out firstCtrler);
                if (firstCtrler != null)
                {
                    var firstIndex = firstCtrler.dataIndex;

                    if (firstIndex > 0)
                    {
                        if (mMovement == UIScrollView.Movement.Vertical)
                        {
                            T ctrler = null;
                            var index = --firstIndex;
                            ctrler = GetItemInResPoolOrAdd(index);
                            if (ctrler != null)
                            {
                                var go = ctrler.GetGo();
                                if (mUpdateItem != null) mUpdateItem(ctrler, index);
                                ctrler.bounds = NGUIMath.CalculateRelativeWidgetBounds(go.transform);
                                var tempBoundy = firstGo.transform.localPosition.y + Interval + ctrler.bounds.size.y;
                                go.transform.localPosition = new Vector3(0, tempBoundy, 0);
                                Add2ShowListFrom(ItemsState.Head, go);
                                //Debug.Log(ctrler.dataIndex);
                            }
                        }
                    }
                    //删除
                    bool isFirstData = firstCtrler.dataIndex == 0;
                    bRestrictWithinPanel = isFirstData;
                    if (mScrollView.isDragging)
                    {
                        //Debug.Log("移删");
                        if (!isFirstData)
                            CheckBeyondRemoveToResPool();
                    }
                    else if (isFirstData)
                    {
                        RestrictWithinBounds();
                    }
                }

            }

        }
        else
        {
            bRestrictWithinPanel = false;

            if (!mScrollView.isDragging)
            {
                CheckBeyondRemoveToResPool();
            }
        }

    }


    /// <summary>
    /// 超出移到资源池
    /// </summary>
    /// <param name="mFinishRemoveToResPool"></param>
    private void CheckBeyondRemoveToResPool(Action mFinishRemoveToResPool = null)
    {
        //Debug.LogError("移到资源池");

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
            if (i == tmpList.Length - 1 && mFinishRemoveToResPool != null)
            {
                mFinishRemoveToResPool();
            }
        }
    }
    /// <summary>
    /// 是否有空白
    /// </summary>
    /// <returns></returns>
    protected bool ScrollViewHasSpace()
    {
        var scrollBounds = NGUIMath.CalculateRelativeWidgetBounds(mScrollView.transform);

        var tHasSpace = mMovement == UIScrollView.Movement.Vertical ? scrollBounds.size.y < mPanelBounds.size.y :
               mMovement == UIScrollView.Movement.Horizontal ? scrollBounds.size.x < mPanelBounds.size.x : false;
        var isHasSpace = showItemGoLinkList.Count == mDataCount && tHasSpace;
        return isHasSpace;
    }

    //检测显示边界
    private void RestrictWithinBounds()
    {
        if (showItemGoLinkList.Count <= 0) return;
        if (mScrollView.isDragging) return;
        //处理在mdir=0时又不是首个/末个会回弹
        if (!bRestrictWithinPanel) return;
        if (ScrollViewHasSpace())
        {
            //-控制在头 左 / 上
            if (mMovement == UIScrollView.Movement.Vertical)
            {
                var scrollBounds = NGUIMath.CalculateRelativeWidgetBounds(mScrollView.transform);
                var center = new Vector3(scrollBounds.center.x, scrollBounds.center.y - mScrollView.panel.clipOffset.y);
                scrollBounds.center = center;
                var calOffsetY = mPanelBounds.max.y - scrollBounds.max.y - mPanel.clipSoftness.y;
                //Debug.LogError(calOffsetY);
                var pos = mScrollView.transform.localPosition + new Vector3(0, calOffsetY, 0);
                pos.x = Mathf.Round(pos.x);
                pos.y = Mathf.Round(pos.y);
                SpringPanel.Begin(mPanel.gameObject, pos, 8f);

            }
        }
        else
        {
            mScrollView.RestrictWithinBounds(false);
        }
    }
    #endregion

    #region 其他功能
    /// <summary>
    /// 跳转到指定下标
    /// </summary>
    /// <param name="dataIndex"></param>
    public void MoveItemByIndex(int dataIndex)
    {
        MoveAllItemToResPool();
        if (mDataCount == 0) return;
        MoveItemByIndex(dataIndex, mPanelBounds);
    }
    /// <summary>
    /// 更新数据，保持原排版
    /// </summary>
    /// <param name="dataCount"></param>
    public void UpdateData(int dataCount)
    {
        mDataCount = dataCount;
        ForceReshItem();
    }
    private void MoveItemByIndex(int dataIndex, Bounds firstBounds)
    {
        T ctrler;
        GameObject go;
        Bounds itemBounds;

        if (mMovement == UIScrollView.Movement.Vertical)
        {
            var tempBoundy = firstBounds.max.y;

            while (tempBoundy > mPanelBounds.min.y)
            {
                if (dataIndex >= mDataCount) break;
                ctrler = GetItemInResPoolOrAdd(dataIndex);
                if (ctrler == null) return;
                go = ctrler.GetGo();
                if (mUpdateItem != null && ctrler != null) mUpdateItem(ctrler, dataIndex);
                itemBounds = NGUIMath.CalculateRelativeWidgetBounds(go.transform);//relactive/Abusoute?
                ctrler.bounds = itemBounds;
                go.transform.localPosition = new Vector3(0, tempBoundy - itemBounds.max.y - mPanel.clipSoftness.y, 0);
                tempBoundy = tempBoundy - itemBounds.size.y - Interval;
                Add2ShowListFrom(ItemsState.Tail, go);
                dataIndex++;
            }
        }
    }
    /// <summary>
    /// 保持item排版,强制刷新
    /// </summary>
    public void ForceReshItem()
    {
        var firstGo = showItemGoLinkList.First.Value;
        var firstCtrler = ItemGoDic[firstGo];

        var scrollBounds = NGUIMath.CalculateRelativeWidgetBounds(mScrollView.transform);
        var center = new Vector3(scrollBounds.center.x, scrollBounds.center.y - mScrollView.panel.clipOffset.y + mPanel.clipSoftness.y);
        scrollBounds.center = center;

        MoveAllItemToResPool();
        MoveItemByIndex(firstCtrler.dataIndex, scrollBounds);
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
        mScrollView.DisableSpring();
    }

    private void OnDragFinished()
    {
        //处理moveDir == 0或者拖住却不移动 不回弹
        RestrictWithinBounds();
    }


    private void OnStoppedMoving()
    {
        //CheckBeyondRemoveToResPool();
    }

    #endregion
}
