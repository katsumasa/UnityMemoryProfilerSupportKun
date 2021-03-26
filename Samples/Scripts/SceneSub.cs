using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utj.UnityMemoryProfilerSupportKun;

//
//　シーン切り替え時にMemoryProfilerのSnapShotを実行するSampleのサブ
//
public class SceneSub : MonoBehaviour {

    delegate void Task();

    [SerializeField] GameObject m_cubePrefabs;
    [SerializeField] string m_nextSceneName;
    float m_time;
    Task m_task;
    GameObject[] m_cubeObjects;    
    float m_lastInterval;
    static int m_snapShotNo = 0;


    private void Awake()
    {
        m_lastInterval = 0;
    }


    // Use this for initialization
    void Start () {
        m_cubeObjects = new GameObject[10 * 10 * 10];
        var i = 0;
        for (var z = 0; z < 10; z++)
        {
            for (var y = 0; y < 10; y++)
            {
                for (var x = 0; x < 10; x++)
                {
                    var gameObject = Instantiate(m_cubePrefabs) as GameObject;
                    Vector3 v3 = new Vector3(x * 5.0f, y * 5.0f, z * 5.0f);
                    gameObject.transform.localPosition = v3;
                    m_cubeObjects[i++] = gameObject;
                }
            }
        }
        m_time = Time.realtimeSinceStartup;
        m_task = Task0001;
    }
	

	// Update is called once per frame
	void Update () {
		if(m_task != null)
        {
            m_task();
        }
	}


    void Task0001()
    {
        var currentTime = Time.realtimeSinceStartup;
        var delta = currentTime - m_time;
        if (delta > 3.0f)
        {
            UnityMemoryProfilerSupportKunClient.instance.Send(string.Format("{0:00000A}", m_snapShotNo++));
            m_task = Task0002;
        }
    }

    void Task0002 ()
    {
        var timeNow = Time.realtimeSinceStartup;
        var waitTime = timeNow - m_lastInterval;
        if ((UnityMemoryProfilerSupportKunClient.instance.isDone == true) || (waitTime > 10.0f))
        {
            for(var i = 0; i < m_cubeObjects.Length; i++)
            {
                Destroy(m_cubeObjects[i]);
                m_cubeObjects[i] = null;
            }
            m_cubeObjects = null;
            SceneMain.instance.ChangeScene(m_nextSceneName);
            m_task = null;
        }
    }
}
