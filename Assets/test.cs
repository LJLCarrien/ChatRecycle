using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region 数据类型
public class Msg1211111111111111111111111111
{
    public string content;
    public virtual void ShowMsg()
    {
        Debug.Log(content);
    }
}
public class MsgTypeOne54555555555 : Msg1211111111111111111111111111
{
    public Color IconColor;
}
public class MsgTypeTwo6666666666666666 : Msg1211111111111111111111111111
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
    public void InitPrefab(MsgTypeOne54555555555 msg)
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
    public void InitPrefab(MsgTypeTwo6666666666666666 msg)
    {
        lbl.text = msg.content;
        TypeSp.color = msg.TypeColor;
    }
}
#endregion

public class test : MonoBehaviour
{
    List<Msg1211111111111111111111111111> MsgList = new List<Msg1211111111111111111111111111>();
    List<IMsgCtrler> CtrlerList = new List<IMsgCtrler>();
    public int index = 0;
    public UITable ParentGo;

    [ContextMenu("AddOne")]
    public void InitMsgTypeOne()
    {
        index++;
        MsgTypeOne54555555555 msg = new MsgTypeOne54555555555();
        msg.content = index.ToString();
        msg.IconColor = Color.red;
        MsgList.Add(msg);
    }

    [ContextMenu("AddTwo")]
    public void InitMsgTypeTwo()
    {
        index++;
        MsgTypeTwo6666666666666666 msg = new MsgTypeTwo6666666666666666();
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
        Msg1211111111111111111111111111 msg;
        MsgTypeOneCtrler oneCtrler;
        MsgTypeTwoCtrler twoCtrler;

        for (int i = 0; i < MsgList.Count; i++)
        {
            msg = MsgList[i];
            if (msg is MsgTypeOne54555555555)
            {
                oneCtrler = CtrlerList[i] as MsgTypeOneCtrler;
                oneCtrler.InitPrefab(msg as MsgTypeOne54555555555);
            }
            if (msg is MsgTypeTwo6666666666666666)
            {
                twoCtrler = CtrlerList[i] as MsgTypeTwoCtrler;
                twoCtrler.InitPrefab(msg as MsgTypeTwo6666666666666666);
            }
        }


    }

    public void CreatePfb(Msg1211111111111111111111111111 msg)
    {
        if (msg is MsgTypeOne54555555555)
        {
            MsgTypeOneCtrler ctrler = new MsgTypeOneCtrler();
            ctrler.AddPrefabs(ParentGo.gameObject);
            CtrlerList.Add(ctrler);
        }
        if (msg is MsgTypeTwo6666666666666666)
        {
            MsgTypeTwoCtrler ctrler = new MsgTypeTwoCtrler();
            ctrler.AddPrefabs(ParentGo.gameObject);
            CtrlerList.Add(ctrler);
        }
    }
}

