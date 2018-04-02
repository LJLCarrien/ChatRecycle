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
    void AddPrefabs(GameObject parentGo);
}
public class MsgTypeOneCtrler : IMsgCtrler
{
    public UILabel lbl;
    public UISprite Icon;
    public void AddPrefabs(GameObject parentGo)
    {
        var goPrefab = Resources.Load("TypeOne", typeof(GameObject))as GameObject;
        //GameObject go = Instantiate(goPrefab) as GameObject;
        GameObject go = NGUITools.AddChild(parentGo, goPrefab);

        lbl = go.GetComponentInChildren<UILabel>();
        Icon = go.GetComponentInChildren<UISprite>();
        
    }
    public void InitPrefab(MsgTypeOne msg)
    {
        lbl.text = msg.content;
        Icon.color = msg.IconColor;
    }

}
public class MsgTypeTwoCtrler :  IMsgCtrler
{
    public UILabel lbl;
    public UISprite TypeSp;

    public void AddPrefabs(GameObject parentGo)
    {
        var goPrefab = Resources.Load("TypeTwo", typeof(GameObject)) as GameObject;
        //GameObject go = Instantiate(goPrefab) as GameObject;
        GameObject go = NGUITools.AddChild(parentGo, goPrefab);
        lbl = go.GetComponentInChildren<UILabel>();
        TypeSp = go.GetComponentInChildren<UISprite>();

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
    public UITable ParentGo;

    [ContextMenu("AddOne")]
    public void InitMsgTypeOne()
    {
        index++;
        MsgTypeOne msg = new MsgTypeOne();
        msg.content = index.ToString();
        msg.IconColor = Color.red;
        MsgList.Add(msg);
    }

    [ContextMenu("AddTwo")]
    public void InitMsgTypeTwo()
    {
        index++;
        MsgTypeTwo msg = new MsgTypeTwo();
        msg.content = index.ToString();
        msg.TypeColor = Color.blue;
        MsgList.Add(msg);

    }

    [ContextMenu("TestComeText")]
        public void TestComeText()
    {
        InitMsgTypeOne();
        InitMsgTypeTwo();
        InitMsgTypeTwo();
        InitMsgTypeTwo();
        InitMsgTypeOne();
        InitMsgTypeOne();
        CreatePrefabs();
        ShowMsg();
        ParentGo.Reposition();
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
            ctrler.AddPrefabs(ParentGo.gameObject);
            CtrlerList.Add(ctrler);
        }
        if (msg is MsgTypeTwo)
        {
            MsgTypeTwoCtrler ctrler = new MsgTypeTwoCtrler();
            ctrler.AddPrefabs(ParentGo.gameObject);
            CtrlerList.Add(ctrler);
        }
    }
}

