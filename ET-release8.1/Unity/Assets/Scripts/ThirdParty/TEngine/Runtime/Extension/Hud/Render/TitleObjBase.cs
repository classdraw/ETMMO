using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleObjBase : MonoBehaviour
{
    private int m_iTitleId=0;//标头的id注册
    public bool m_bMain;//
    public string m_sName;//名称

    [ContextMenu("TestShowTitle")]
    void TestShowTitle(){
        // m_iTitleId
    }
}
