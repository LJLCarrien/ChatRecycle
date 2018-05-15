using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class ViewCtrlerK : MonoBehaviour
{

    public UIScrollView mScrollView;
    public RecycleK mRecycleK;
    List<Msg> dataList = new List<Msg>();

    public void InitData()
    {
        for (int i = 0; i < 13; i++)
        {
            MsgOne m = new MsgOne { fromWho = 1, contentOne = i.ToString() };
            dataList.Add(m);
        }
    }

    // Use this for initialization
    void Start()
    {
        InitData();
        mRecycleK = new RecycleK(mScrollView, 10, AddItem, UpdateItem);
        mRecycleK.onGetBounds = OnGetBounds;
        mRecycleK.onIsFirstOne = OnIsFirstOne;
        mRecycleK.onIsLastOne = OnIsLastOne;
        mRecycleK.onGetDataIndex = OnGetDataIndex;
        mRecycleK.ResetPostion(dataList.Count);

    }
    private Bounds OnGetBounds(GameObject go)
    {
        ItemCtrler ctrler;
        if (go2CtrlerDic.TryGetValue(go,out ctrler))
        {
            return ctrler.bounds;
        }
        return new Bounds(Vector3.zero,Vector3.zero);
    }
    private int OnGetDataIndex(GameObject go)
    {
        ItemCtrler ctrler;
        if (go2CtrlerDic.TryGetValue(go, out ctrler))
        {
            return ctrler.dataIndex;
        }
        return -1;
    }

    private bool OnIsLastOne(GameObject go)
    {
        ItemCtrler ctrler;
        if (go2CtrlerDic.TryGetValue(go, out ctrler))
        {
            return ctrler.dataIndex== dataList.Count-1;
        }
        return false;
    }

    private bool OnIsFirstOne(GameObject go)
    {
        ItemCtrler ctrler;
        if (go2CtrlerDic.TryGetValue(go, out ctrler))
        {
            return ctrler.dataIndex == 0;
        }
        return true;
    }

   


    // Update is called once per frame
    void Update()
    {

    }

    Dictionary<GameObject, ItemCtrler> go2CtrlerDic = new Dictionary<GameObject, ItemCtrler>();

    public GameObject AddItem(int dataIndex)
    {
        if (dataIndex >= dataList.Count) return null;
        if (dataList[dataIndex] is MsgOne)
        {
            var goPrefab = Resources.Load("TypeOne", typeof(GameObject)) as GameObject;
            GameObject go = NGUITools.AddChild(mScrollView.gameObject, goPrefab);
            var ctrler = go.AddComponent<ItemOneCtrler>();
            ctrler.itemType = (int)ItemCtrler.ItemTypes.itemOne;
            //Debug.Log("一类型");
            go2CtrlerDic.Add(go, ctrler);
            return go;
        }
        else if (dataList[dataIndex] is MsgTwo)
        {
            var goPrefab = Resources.Load("TypeTwo", typeof(GameObject)) as GameObject;
            GameObject go = NGUITools.AddChild(mScrollView.gameObject, goPrefab);
            var ctrler = go.AddComponent<ItemTwoCtrler>();
            ctrler.itemType = (int)ItemCtrler.ItemTypes.itemTwo;
            //Debug.Log("二类型");
            go2CtrlerDic.Add(go, ctrler);
            return go;
        }
        return null;
    }

    private void UpdateItem(int dataIndex, GameObject go)
    {
        if (dataIndex >= dataList.Count) return;
        ItemCtrler ctrler;
        go.name = dataIndex.ToString();
        if (go2CtrlerDic.TryGetValue(go, out ctrler))
        {
            Msg info = dataList[dataIndex];
            if (info is MsgOne)
            {
                MsgOne mo = info as MsgOne;
                ctrler.info = mo;
                ctrler.dataIndex = dataIndex;
                ctrler.bounds = NGUIMath.CalculateRelativeWidgetBounds(go.transform);
            }
            else if (info is MsgTwo)
            {
                MsgTwo mt = info as MsgTwo;
                ctrler.info = mt;
                ctrler.dataIndex = dataIndex;
                ctrler.bounds = NGUIMath.CalculateRelativeWidgetBounds(go.transform);
            }
        }
    }


}
