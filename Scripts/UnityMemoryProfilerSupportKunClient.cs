// (C) UTJ
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking.PlayerConnection;

namespace Utj.UnityMemoryProfilerSupportKun
{
    // UnityMemoryProfilerSupport君のClient側の処理
    // このスクリプトを適当なDestroyされないGameObjectに張り付けてお使い下さい
    //
    // ざっくりな使い方の例
    // 
    // UnityMemoryProfilerSupportKunClient.instance.Send("snapshot0001");
    //
    // while(1){    
    //   if(UnityMemoryProfilerSupportKunClient.instance.isDone){
    //          break;
    //   }
    // }
    //
    //
    //
    // 注意事項
    // isDoneがtrueになる迄時間が掛かる、trueにならないケース(UnityEditor側の準備が出来ていない等）があることに注意して下さい。
    //
    //
    public class UnityMemoryProfilerSupportKunClient : MonoBehaviour
    {        
        public static readonly Guid kMsgSendEditorToPlayer = new Guid("5735d5a131504489ac704b91707438a5");
        public static readonly Guid kMsgSendPlayerToEditor = new Guid("dfc22fbac4d242fe80e19b121510488b");


        static UnityMemoryProfilerSupportKunClient m_instance;
        public static UnityMemoryProfilerSupportKunClient instance
        {
            get { return m_instance; }
        }

        bool m_isDone;
        public bool isDone { get { return m_isDone; } }


        // 関数:EditorにSnapShotを実行するように命令を送る処理
        // 引数:fname:snapshotのファイル名
        public void Send(string fname)
        {
            m_isDone = false;
            var text = string.Format("{0}:{1}", "Take SnapShot", fname);
            var datas = System.Text.Encoding.ASCII.GetBytes(text);
            PlayerConnection.instance.Send(kMsgSendPlayerToEditor, datas);
            Debug.Log("Send:" + text);
        }


        void Awake()
        {
            if (m_instance != null)
            {
                Destroy(gameObject);
            }
            else
            {
                m_instance = this;
            }
        }


        // Use this for initialization
        void Start()
        {
            PlayerConnection.instance.RegisterConnection(ConnectionCB);
            PlayerConnection.instance.RegisterDisconnection(DisconnectionCB);
        }


        // Update is called once per frame
        void Update()
        {
        }


        private void OnDestroy()
        {
            m_instance = null;
        }


        private void OnEnable()
        {
            PlayerConnection.instance.Register(kMsgSendEditorToPlayer, OnMessageEvent);
        }


        private void OnDisable()
        {
            PlayerConnection.instance.Unregister(kMsgSendEditorToPlayer, OnMessageEvent);
        }


        private void OnMessageEvent(MessageEventArgs messageEventArgs)
        {
            var text = Encoding.ASCII.GetString(messageEventArgs.data);
            Debug.Log("UnityMemoryProfilerSupportKunClient.OnMessageEvent:" + text);
            if (text.Contains("Success") == true)
            {
                m_isDone = true;
            }
        }


        private void ConnectionCB(int playerId)
        {
            Debug.Log("UnityMemoryProfilerSupportKunClient:Connect");
        }


        private void DisconnectionCB(int playerId)
        {
            Debug.Log("UnityMemoryProfilerSupportKunClient:DisConnect");
        }
    }
}