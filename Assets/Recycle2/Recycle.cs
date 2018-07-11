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
/// <summary>
/// 预设：
///     当使用垂直拖动时，预设item中心，任一水平线居中 (预设中心距离左右距离) 
///     当使用水平拖动时，预设item中心，任一垂直线居中 (预设中心距离上下距离)
/// scrollView:
///     不要有子物体，子物体运行时会删掉。
///     Restrict Within Panel、Cancel Drag If Fits这两个勾选与否不产生影响。
///     如绑定ScrollView的onXX方法的回调,使用onRecycleXX代替，如没，则可以自行调用scrollView的。
/// 使用：
///     1、初始化：使用构造方法UIRecycleTableUp
///     2、更新数据：需要归位：ResetPostion(数量)，第一次使用一定要传入数量。不保持排版，刷新数据。
///                 不需要归为：UpdateData(数量)。保持当前排版，仅刷新数据。
///     3、跳转：MoveItemByIndex()
///     4、销毁：DestoryRecycle()
/// 回调绑定：
///     【必须】mAddItem    加载item,传入dataIndex
///     【必须】mUpdateItem 更新item,传回T和dataIndex
///     【可选】mGetDataType 多种样式的则【必须】绑定此方法，传入dataIndex，传出的类型用int类型区分。
///      
/// </summary>
public class Recycle<T> where T : class, IRecycle
{
    public UIScrollView mScrollView { get; private set; }
    public UIPanel mPanel { get; private set; }
    public Bounds mPanelBounds { get; private set; }
    public UIScrollView.Movement mMovement { get { return mScrollView.movement; } }

    #region 数据
    //<dataIndex,>
    private Func<int, T> mAddItem;      //添加
    //<,dataIndex>
    private Action<T, int> mUpdateItem; //更新
    private Action<T> mDeleteItem;      //删除
    //<dataIndex,type>
    private Func<int, int> mGetDataType; //获取类型

    //展示中的
    private LinkedList<GameObject> showItemGoLinkList = new LinkedList<GameObject>();
    public int mDataCount { get; private set; }
    //资源池里的
    private GameObject mResPool;
    //所有的
    private Dictionary<GameObject, T> ItemGoDic = new Dictionary<GameObject, T>();
    //间隔
    private readonly int Interval = 10;

    #endregion
    #region 入口
    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="sv"></param>
    /// <param name="itemInterval">物体item间隔</param> 
    /// <param name="addItem"> 添加回调</param>
    /// <param name="updateItem">更新回调</param>

    public Recycle(UIScrollView sv, int itemInterval, Func<int, T> addItem, Action<T, int> updateItem, Func<int, int> getDataType, Action<T> deleteItem)
    {
        if (sv == null) return;
        mScrollView = sv;
        mPanel = sv.panel;

        var mCenter = new Vector3(mPanel.finalClipRegion.x, mPanel.finalClipRegion.y, 0) + mScrollView.transform.localPosition;
        var mSize = new Vector3(mPanel.finalClipRegion.z, mPanel.finalClipRegion.w);
        mPanelBounds = new Bounds(mCenter, mSize);
        mPanel.clipOffset = Vector2.zero;
        mPanel.baseClipRegion = new Vector4(mCenter.x, mCenter.y, mSize.x, mSize.y);

        mScrollView.transform.localPosition = Vector3.zero;
        mScrollView.restrictWithinPanel = false;
        mScrollView.disableDragIfFits = false;
        mScrollView.transform.DestroyChildren();

        Interval = itemInterval;

        mAddItem = addItem;
        mUpdateItem = updateItem;
        mGetDataType = getDataType;
        mDeleteItem = deleteItem;

        mDataCount = 0;
        RegisterEvent();

        if (mResPool == null)
        {
            mResPool = NGUITools.AddChild(mScrollView.transform.parent.gameObject);
            mResPool.name = "ItemsResPool";
            mResPool.SetActive(false);
        }
        else
        {
            mResPool.transform.DestroyChildren();
        }



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

    #region 清理
    public void RemoveAllItems()
    {
        if (mDeleteItem == null || ItemGoDic == null) return;
        foreach (var tCtrler in ItemGoDic.Values)
        {
            mDeleteItem(tCtrler);
        }
    }
    /// <summary>
    /// 销毁清理
    /// </summary>
    public void DestoryRecycle()
    {
        RemoveEvent();

        OnRecycleStoppedMoving = null;
        OnRecycleDragStarted = null;
        OnRecycleDragFinished = null;
        OnRecycleScrollWheel = null;

        mAddItem = null;
        mUpdateItem = null;

        mDeleteItem = null;
        mGetDataType = null;

        mScrollView = null;
        mPanel = null;
        mDataCount = 0;

        ItemGoDic.Clear();
    }
    #endregion
    #region 预设管理

    private enum ItemsState
    {
        Head,
        Tail
    }
    //linkList
    private void Add2ShowListFrom(ItemsState state, GameObject go)
    {
        if (state == ItemsState.Tail)
            showItemGoLinkList.AddLast(go);
        else if (state == ItemsState.Head)
            showItemGoLinkList.AddFirst(go);

        //Debug.LogError(showItemGoLinkList.Count);
    }

    private void RemoveShowListFrom(ItemsState state)
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
    }


    /// <summary>
    /// 从缓存池获取预设，无则添加
    /// </summary>
    /// <param name="dindex"></param>
    /// <returns></returns>
    private T GetItemInResPoolOrAdd(int dindex)
    {
        int dataType = -1;
        if (mGetDataType != null)
        {
            dataType = mGetDataType(dindex);
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

    private void OnClipMove(UIPanel panel)
    {
        if (mScrollView == null || mDataCount == 0) return;
        var scrollViewBounds = NGUIMath.CalculateRelativeWidgetBounds(mScrollView.transform);
        var tPanelOffset = panel.CalculateConstrainOffset(scrollViewBounds.min, scrollViewBounds.max);

        var moveTop = mMovement == UIScrollView.Movement.Vertical && tPanelOffset.y < -1;
        var moveDown = mMovement == UIScrollView.Movement.Vertical && tPanelOffset.y > 1;

        var moveLeft = mMovement == UIScrollView.Movement.Horizontal && tPanelOffset.x > 1;
        var moveRight = mMovement == UIScrollView.Movement.Horizontal && tPanelOffset.x < -1;
        //var moveDir = moveTop ? -1 : moveDown ? 1 : 0;
        //Debug.LogError(tPanelOffset.x + "," + tPanelOffset.y);
        //Debug.LogError(tPanelOffset.x);
        //Debug.LogError(moveDir);
        if (showItemGoLinkList.Count == 0) return;

        var firstGo = showItemGoLinkList.First.Value;//第一个
        T firstCtrler = null;
        ItemGoDic.TryGetValue(firstGo, out firstCtrler);
        var firstCtrlRealBound = NGUIMath.CalculateRelativeWidgetBounds(firstGo.transform);
        firstCtrlRealBound.center += firstGo.transform.localPosition;

        var lastGo = showItemGoLinkList.Last.Value;//最后一个
        T lastCtrler = null;
        ItemGoDic.TryGetValue(lastGo, out lastCtrler);
        var lastCtrlRealBound = NGUIMath.CalculateRelativeWidgetBounds(lastGo.transform);
        lastCtrlRealBound.center += lastGo.transform.localPosition;

        if (moveTop || moveLeft)
        {
            //往上拉
            //Debug.LogError("往上拉<-1");
            if (showItemGoLinkList.Count > 0)
            {

                if (lastCtrler != null)
                {
                    var firstIndex = lastCtrler.dataIndex;
                    //Debug.Log(string.Format("{0},{1},{2},{3},{4}", FirstGo.transform.localPosition.y, FirstCtrler.mBounds.size.y, Interval, tempBoundy, FirstIndex));

                    if (firstIndex < mDataCount - 1)
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
                            if (mMovement == UIScrollView.Movement.Vertical)
                            {
                                var tempBoundy = lastCtrlRealBound.min.y - ctrler.bounds.max.y - Interval;//ctrler.bounds.max.y 这里相当于size/2
                                go.transform.localPosition = new Vector3(mPanelBounds.center.x, tempBoundy, 0);
                                Add2ShowListFrom(ItemsState.Tail, go);
                            }
                            else if (mMovement == UIScrollView.Movement.Horizontal)
                            {
                                var tempBoundx = lastCtrlRealBound.max.x + (-ctrler.bounds.min.x) + Interval;//ctrler.bounds.max.y 这里相当于size/2
                                go.transform.localPosition = new Vector3(tempBoundx, mPanelBounds.center.y, 0);
                                Add2ShowListFrom(ItemsState.Tail, go);
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
        else if (moveDown || moveRight)
        {
            //往下拉
            //Debug.LogError("往下拉>1");
            if (showItemGoLinkList.Count > 0)
            {

                if (firstCtrler != null)
                {
                    var firstIndex = firstCtrler.dataIndex;

                    if (firstIndex > 0)
                    {

                        T ctrler = null;
                        var index = --firstIndex;
                        ctrler = GetItemInResPoolOrAdd(index);
                        if (ctrler != null)
                        {
                            var go = ctrler.GetGo();
                            if (mUpdateItem != null) mUpdateItem(ctrler, index);
                            ctrler.bounds = NGUIMath.CalculateRelativeWidgetBounds(go.transform);
                            if (mMovement == UIScrollView.Movement.Vertical)
                            {
                                var tempBoundy = firstCtrlRealBound.max.y + (-ctrler.bounds.min.y) + Interval;//-ctrler.bounds.min.y 这里相当于size/2
                                go.transform.localPosition = new Vector3(mPanelBounds.center.x, tempBoundy, 0);
                                Add2ShowListFrom(ItemsState.Head, go);
                                //Debug.Log(ctrler.dataIndex);
                            }
                            else if (mMovement == UIScrollView.Movement.Horizontal)
                            {
                                var tempBoundx = firstCtrlRealBound.min.x - ctrler.bounds.max.x - Interval;//ctrler.bounds.max.x 这里相当于size/2
                                go.transform.localPosition = new Vector3(tempBoundx, mPanelBounds.center.y, 0);
                                Add2ShowListFrom(ItemsState.Head, go);
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
            bool isLastData = lastCtrler != null && lastCtrler.dataIndex == mDataCount - 1;
            bool isFirstData = firstCtrler != null && firstCtrler.dataIndex == 0;
            bRestrictWithinPanel = isFirstData || isLastData;

            if (!mScrollView.isDragging)
            {
                //Debug.LogError("234234");
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
        Bounds b;
        GameObject go;
        GameObject[] tmpList = new GameObject[showCount];
        showItemGoLinkList.CopyTo(tmpList, 0);
        //Debug.Log(tmpList.Length);
        for (int i = 0; i < tmpList.Length; i++)
        {
            go = tmpList[i];
            T ctrler = null;
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
            else if (mMovement == UIScrollView.Movement.Horizontal)
            {
                var scrollBounds = NGUIMath.CalculateRelativeWidgetBounds(mScrollView.transform);
                var center = new Vector3(scrollBounds.center.x - mScrollView.panel.clipOffset.x, scrollBounds.center.y);
                scrollBounds.center = center;
                var calOffsetX = mPanelBounds.min.x - scrollBounds.min.x - mPanel.clipSoftness.x;
                var pos = mScrollView.transform.localPosition + new Vector3(calOffsetX, 0, 0);
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
        mPanel.transform.localPosition = Vector3.zero;
        mPanel.clipOffset = Vector2.zero;

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
                if (mUpdateItem != null) mUpdateItem(ctrler, dataIndex);
                itemBounds = NGUIMath.CalculateRelativeWidgetBounds(go.transform);//relactive/Abusoute?
                ctrler.bounds = itemBounds;
                go.transform.localPosition = new Vector3(mPanelBounds.center.x, tempBoundy - itemBounds.max.y - mPanel.clipSoftness.y, 0);
                tempBoundy = tempBoundy - itemBounds.size.y - Interval;
                Add2ShowListFrom(ItemsState.Tail, go);
                dataIndex++;
            }
        }
        else if (mMovement == UIScrollView.Movement.Horizontal)
        {
            var tempBoundx = firstBounds.min.x;
            while (tempBoundx < mPanelBounds.max.x)
            {
                //if (dataIndex >= mDataCount) break;
                ctrler = GetItemInResPoolOrAdd(dataIndex);
                if (ctrler == null) return;
                go = ctrler.GetGo();
                if (mUpdateItem != null) mUpdateItem(ctrler, dataIndex);
                itemBounds = NGUIMath.CalculateRelativeWidgetBounds(go.transform);
                ctrler.bounds = itemBounds;
                go.transform.localPosition = new Vector3(tempBoundx + -itemBounds.min.x + mPanel.clipSoftness.x, mPanelBounds.center.y, 0);
                tempBoundx = tempBoundx + itemBounds.size.x + Interval;
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
        if (mPanel != null)
            mPanel.onClipMove -= OnClipMove;
        if (mScrollView != null)
        {
            mScrollView.onStoppedMoving -= OnStoppedMoving;
            mScrollView.onDragStarted -= OnDragStarted;
            mScrollView.onDragFinished -= OnDragFinished;
            mScrollView.onScrollWheel -= OnScrollWheel;
        }
    }

    private void RegisterEvent()
    {
        if (mPanel != null) mPanel.onClipMove += OnClipMove;
        if (mScrollView != null)
        {
            mScrollView.onStoppedMoving += OnStoppedMoving;
            mScrollView.onDragStarted += OnDragStarted;
            mScrollView.onDragFinished += OnDragFinished;
            mScrollView.onScrollWheel += OnScrollWheel;
        }
    }

    private void OnDragStarted()
    {
        if (mScrollView != null) mScrollView.DisableSpring();
        if (OnRecycleDragStarted != null) OnRecycleDragStarted();
    }

    private void OnDragFinished()
    {
        //处理moveDir == 0或者拖住却不移动 不回弹
        RestrictWithinBounds();
        if (OnRecycleDragFinished != null) OnRecycleDragFinished();
    }


    private void OnStoppedMoving()
    {
        //CheckBeyondRemoveToResPool();
        if (OnRecycleStoppedMoving != null) OnRecycleStoppedMoving();
    }
    private void OnScrollWheel()
    {
        if (ScrollViewHasSpace())
        {
            //Debug.LogError("asdsasd");
            mScrollView.DisableSpring();
            RestrictWithinBounds();
        }
        if (OnRecycleScrollWheel != null) OnRecycleScrollWheel();

    }
    #endregion

    #region 客户端所需scrollView相关回调

    public Action OnRecycleStoppedMoving;
    public Action OnRecycleDragStarted;
    public Action OnRecycleDragFinished;
    public Action OnRecycleScrollWheel;

    #endregion
}
