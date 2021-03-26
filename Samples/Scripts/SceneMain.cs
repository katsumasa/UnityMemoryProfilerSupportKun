using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utj.UnityMemoryProfilerSupportKun;



//
//　シーン切り替え時にMemoryProfilerのSnapShotを実行するSampleのメイン
//
public class SceneMain : MonoBehaviour {

    delegate void Task();


    static SceneMain _instance;
    string m_sceneName;
    AsyncOperation m_asyncOperation;
    Task m_task;
    int m_snapShotNo;
    float m_lastInterval;
    bool m_isDone;


    public bool isDone
    {
        get { return m_isDone; }
    }


    public static SceneMain instance
    {
        get { return _instance; }
    }


    public void ChangeScene(string sceneName)
    {
        m_isDone = false;
        if (m_sceneName != null) {
            m_asyncOperation = SceneManager.UnloadSceneAsync(m_sceneName);
            m_sceneName = sceneName;
            m_task = TaskUnLoadWait;
        }
        else
        {
            m_sceneName = sceneName;
            m_asyncOperation = SceneManager.LoadSceneAsync(m_sceneName, LoadSceneMode.Additive);
            m_task = TaskLoadWait;
        }               
    }


    private void Awake()
    {
        if(instance != null)
        {
            Destroy(gameObject);
        } else
        {
            _instance = this;
        }
    }


    // Use this for initialization
    void Start () {
        m_snapShotNo = 0;
        ChangeScene("SceneSub0001");
    }

	
	// Update is called once per frame
	void Update () {
		if(m_task != null)
        {
            m_task();
        }
	}


    void TaskUnLoadWait()
    {
        if(m_asyncOperation.isDone)
        {
            m_asyncOperation = Resources.UnloadUnusedAssets();
            m_task = TaskUnloadUnusedAssetsWait;
        }
        
    }


    void TaskUnloadUnusedAssetsWait()
    {
        if (m_asyncOperation.isDone == false)
        {
            return;
        }
        UnityMemoryProfilerSupportKunClient.instance.Send(string.Format("{0:00000B}", m_snapShotNo++));
        m_lastInterval = Time.realtimeSinceStartup;
        m_task = TaskMemoryProfilerWait;
    }


    void TaskMemoryProfilerWait()
    {
        var timeNow = Time.realtimeSinceStartup;
        var waitTime = timeNow - m_lastInterval;
        if ((UnityMemoryProfilerSupportKunClient.instance.isDone == true) || (waitTime > 10.0f))
        {
            // 10 sec 以上待たされた場合は諦める
            m_asyncOperation =  SceneManager.LoadSceneAsync(m_sceneName, LoadSceneMode.Additive);
            m_task = TaskLoadWait;
        }
    }

    void TaskLoadWait()
    {
        if (m_asyncOperation.isDone)
        {
            m_asyncOperation = null;
            m_task = null;
            m_isDone = true;
        }
    }
}
