using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utj.UnityMemoryProfilerSupportKun;

// ボタンを押したらMemoryProfilerのSnapShotを実行するサンプル
public class Simple : MonoBehaviour {


    int m_no = 0;

    public void TakeSnapShot()
    {
        UnityMemoryProfilerSupportKunClient.instance.Send(m_no.ToString());
        m_no++;
    }


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
