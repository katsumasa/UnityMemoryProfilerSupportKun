// (C) UTJ
using System;
using System.Text;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEditor.MemoryProfiler;
using UnityEditor.Networking.PlayerConnection;
using UnityEngine.Networking.PlayerConnection;

namespace Utj.UnityMemoryProfilerSupportKun
{
    //
    // UnityMemoryProfilerSupportKunのUnityEditor側の処理
    // MemoryProfilerのWindowが開いている状態で使用す必要があります。
    // ※MemoryProfilerWindowが継承して使用出来ればもう少しスマートになるのですが・・
    //
    public class UnityMemoryProfilerSupportKunEditor : EditorWindow
    {

        static readonly string TakeSnapShotText = "Take SnapShot";
        static string m_savePath;

        [NonSerialized]
        private bool m_registered = false;

        [NonSerialized]
        UnityEditor.MemoryProfiler.PackedMemorySnapshot m_snapshot;

        // AttachProfiler表示用
        Type AttachProfilerUI;
        MethodInfo m_attachProfilerUIOnGUILayOut;
        System.Object m_attachProfilerUI;


        [MenuItem("Window/UnityMemoryProfilerSupportKunEditor")]
        private static void Create()
        {
            m_savePath = System.IO.Directory.GetCurrentDirectory() + "/Temp";
            UnityMemoryProfilerSupportKunEditor window = (UnityMemoryProfilerSupportKunEditor)EditorWindow.GetWindow(typeof(UnityMemoryProfilerSupportKunEditor));
            window.Show();
            window.titleContent = new GUIContent("UnityMemoryProfilerSupportKunEditor");
        }


        void OnEnable()
        {
            EditorConnection.instance.Initialize();
            EditorConnection.instance.Register(UnityMemoryProfilerSupportKunClient.kMsgSendPlayerToEditor, OnMessageEvent);
        }


        void OnDisable()
        {
            EditorConnection.instance.Unregister(UnityMemoryProfilerSupportKunClient.kMsgSendPlayerToEditor, OnMessageEvent);
            EditorConnection.instance.DisconnectAll();
        }


        // Playerからのメッセージ処理用イベント関数
        private void OnMessageEvent(MessageEventArgs args)
        {
            var text = Encoding.ASCII.GetString(args.data);
            Debug.Log("Message from player: " + text);
            if (text.Contains(TakeSnapShotText))
            {
                UnityEditor.EditorUtility.DisplayProgressBar("Take Snapshot", "Downloading Snapshot...", 0.0f);
                try
                {
                    UnityEditor.MemoryProfiler.MemorySnapshot.RequestNewSnapshot();
                }
                finally
                {
                    var fname = text.Substring(TakeSnapShotText.Length + 1);
                    var fpath = string.Format("{0}/{1}{2}", m_savePath, fname, ".memsnap3");
                    
                    // 注意:この関数はprivateになっているのでpunlicに変更して下さい。
                    PackedMemorySnapshotUtility.SaveToFile(fpath, m_snapshot);


                    EditorConnection.instance.Send(UnityMemoryProfilerSupportKunClient.kMsgSendEditorToPlayer, Encoding.ASCII.GetBytes("Success"));
                    EditorUtility.ClearProgressBar();
                }
            }
        }


        public void Initialize()
        {
            if (!m_registered)
            {
                UnityEditor.MemoryProfiler.MemorySnapshot.OnSnapshotReceived += IncomingSnapshot;
                m_registered = true;
            }
            Reflection();
        }


        // この関数内の処理は全く、推奨出来ませんので参考にしないでください。
        void Reflection()
        {
#if UNITY_2017
            // AttachProfilerUIとは
            // ProfilerやConsole WindowにあるTargetの選択用Pulldown UI
            // internal classの為、Relectionで無理やり
            if (AttachProfilerUI == null)
            {
                Assembly assembly = Assembly.Load("UnityEditor");
                AttachProfilerUI = assembly.GetType("UnityEditor.AttachProfilerUI");
            }
            if ((m_attachProfilerUI == null) && (AttachProfilerUI != null))
            {
                m_attachProfilerUI = AttachProfilerUI.InvokeMember(
                    null
                , BindingFlags.CreateInstance
                , null
                , null
                , new object[] { }
                );
            }
            if (m_attachProfilerUIOnGUILayOut == null && AttachProfilerUI != null)
            {
                m_attachProfilerUIOnGUILayOut = AttachProfilerUI.GetMethod("OnGUILayout");
            }
#elif UNITY_2018_OR_NEWER
            // 2018ではAttachProilerUIの定義位置が変わったので一旦取りやめ
#endif
        }


        void OnGUI()
        {
            Initialize();
            
            // AttachProfiler
            if (m_attachProfilerUIOnGUILayOut != null && m_attachProfilerUI != null)
            {
                m_attachProfilerUIOnGUILayOut.Invoke(m_attachProfilerUI, new object[] { this });
            }

            // 接続済みPlayerのリスト表示部
            var playerCount = EditorConnection.instance.ConnectedPlayers.Count;
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(string.Format("{0} players connected.", playerCount));
            int i = 0;
            foreach (var p in EditorConnection.instance.ConnectedPlayers)
            {
                builder.AppendLine(string.Format("[{0}] - {1} {2}", i++, p.name, p.playerId));
            }
            EditorGUILayout.HelpBox(builder.ToString(), MessageType.Info);

            // Snapshot保存先
            EditorGUILayout.BeginHorizontal(GUI.skin.box);
            GUILayout.Label("SavePath");
            GUILayout.TextField(m_savePath, GUILayout.Width(300));
            if (GUILayout.Button("Browse"))
            {
                var path= EditorUtility.OpenFolderPanel("Save Directory Path", m_savePath, "");
                if(path != "")
                {
                    m_savePath = path;
                }
            }
            EditorGUILayout.EndHorizontal();
        }


        void IncomingSnapshot(PackedMemorySnapshot snapshot)
        {
            m_snapshot = snapshot;
        }
    }
}