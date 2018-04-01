using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region 数据类型
public class Msg
{
    public string content;
    public virtual void ShowMsg()
    {
        Debug.Log(content);
    }
}
public class MsgTypeOne : Msg
{
    public Color IconColor;
}
public class MsgTypeTwo : Msg
{
    public Color TypeColor;

}
#endregion

#region 预设contrler
public interface IMsgCtrler
{
    void AddPrefabs();
}
public class MsgTypeOneCtrler : MonoBehaviour, IMsgCtrler
{
    public UILabel lbl;
    public UISprite Icon;
    public void AddPrefabs()
    {
        var goPrefab = Resources.Load("TypeOne", typeof(GameObject));
        GameObject go = Instantiate(goPrefab) as GameObject;
        lbl = GetComponentInChildren<UILabel>();
        Icon = GetComponentInChildren<UISprite>();
    }
    public void InitPrefab(MsgTypeOne msg)
    {
        lbl.text = msg.content;
        Icon.color = msg.IconColor;
    }
    
}
public class MsgTypeTwoCtrler : MonoBehaviour, IMsgCtrler
{
    public UILabel lbl;
    public UISprite TypeSp;

    public void AddPrefabs()
    {
        var goPrefab = Resources.Load("TypeTwo", typeof(GameObject));
        GameObject go = Instantiate(goPrefab) as GameObject;
        lbl = GetComponentInChildren<UILabel>();
        TypeSp = GetComponentInChildren<UISprite>();
    }
    public void InitPrefab(MsgTypeTwo msg)
    {
        lbl.text = msg.content;
        TypeSp.color = msg.TypeColor;
    }
}
#endregion

public class test : MonoBehaviour
{
    List<Msg> MsgList = new List<Msg>();
    List<IMsgCtrler> CtrlerList = new List<IMsgCtrler>();
    public int index = 0;

    [ContextMenu("AddOne")]
    public void InitMsgTypeOne()
    {
        index++;
        MsgTypeOne msg = new MsgTypeOne();
        msg.content = index.ToString();

        MsgList.Add(msg);
    }

    [ContextMenu("AddTwo")]
    public void InitMsgTypeTwo()
    {
        index++;
        MsgTypeTwo msg = new MsgTypeTwo();
        msg.content = index.ToString();
        MsgList.Add(msg);

    }

    [ContextMenu("CreatePrefabs")]
    public void CreatePrefabs()
    {
        for (int i = 0; i < MsgList.Count; i++)
        {
            CreatePfb(MsgList[i]);
        }

    }
    [ContextMenu("ShowMsg")]
    public void ShowMsg()
    {
        Msg msg;
        MsgTypeOneCtrler oneCtrler;
        MsgTypeTwoCtrler twoCtrler;

        for (int i = 0; i < MsgList.Count; i++)
        {
            msg = MsgList[i];
            if (msg is MsgTypeOne)
            {
                oneCtrler = CtrlerList[i] as MsgTypeOneCtrler;
                oneCtrler.InitPrefab(msg as MsgTypeOne);
            }
            if (msg is MsgTypeTwo)
            {
                twoCtrler = CtrlerList[i] as MsgTypeTwoCtrler;
                twoCtrler.InitPrefab(msg as MsgTypeTwo);
            }
        }
       

    }

    public void CreatePfb(Msg msg)
    {
        if (msg is MsgTypeOne)
        {
            MsgTypeOneCtrler ctrler = new MsgTypeOneCtrler();
            ctrler.AddPrefabs();
            CtrlerList.Add(ctrler);
        }
        if (msg is MsgTypeTwo)
        {
            MsgTypeTwoCtrler ctrler = new MsgTypeTwoCtrler();
            ctrler.AddPrefabs();
            CtrlerList.Add(ctrler);
        }
    }
}

