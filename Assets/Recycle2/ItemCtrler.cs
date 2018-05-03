using UnityEngine;
using System.Collections;
using System;

public class ItemCtrler : MonoBehaviour,IRecycle
{
    public GameObject GetGo()
    {
        return this.gameObject;
    }
    public UILabel mLbl
    {
        get
        {
            return transform.FindChild("Label").GetComponent<UILabel>();
        }
    }
    public void SetData(int i)
    {
        mLbl.text = i.ToString();
    }

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
