// (C) UTJ
using System;
using System.Text;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEditor.MemoryProfiler;
using UnityEditor.Networking.PlayerConnection;
using UnityEngine.Networking.PlayerConnection;
using UnityEngine.Profiling.Memory.Experimental;
using UnityEditor.Profiling.Memory.Experimental;


#if UNITY_2018_1_OR_NEWER
using UnityEngine.Experimental.Networking.PlayerConnection;
using ConnectionUtility = UnityEditor.Experimental.Networking.PlayerConnection.EditorGUIUtility;
using ConnectionGUILayout = UnityEditor.Experimental.Networking.PlayerConnection.EditorGUILayout;
#endif



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
        static string m_fname;

        [NonSerialized]
        private bool m_registered = false;

        [NonSerialized]
        UnityEditor.MemoryProfiler.PackedMemorySnapshot m_snapshot;

        // AttachProfiler表示用
#if UNITY_2018_1_OR_NEWER
        IConnectionState attachProfilerState;
#else
        Type AttachProfilerUI;
        MethodInfo m_attachProfilerUIOnGUILayOut;
        System.Object m_attachProfilerUI;
#endif


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
#if UNITY_2018_1_OR_NEWER
            if (attachProfilerState == null)
            {
                attachProfilerState = ConnectionUtility.GetAttachToPlayerState(this);
            }
#endif
            UnityEditor.MemoryProfiler.MemorySnapshot.OnSnapshotReceived += IncomingSnapshot;
            EditorConnection.instance.Initialize();
            EditorConnection.instance.Register(UnityMemoryProfilerSupportKunClient.kMsgSendPlayerToEditor, OnMessageEvent);
        }


        void OnDisable()
        {
#if UNITY_2018_1_OR_NEWER
            attachProfilerState.Dispose();
            attachProfilerState = null;
#endif
            UnityEditor.MemoryProfiler.MemorySnapshot.OnSnapshotReceived -= IncomingSnapshot;
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
                var fname = text.Substring(TakeSnapShotText.Length + 1);
                m_fname = string.Format("{0}/{1}{2}", m_savePath, fname, ".memsnap3");
                UnityEditor.EditorUtility.DisplayProgressBar("Take Snapshot", "Downloading Snapshot...", 0.0f);
                try
                {
                    UnityEditor.MemoryProfiler.MemorySnapshot.RequestNewSnapshot();                                        
                }
                finally
                {
                    // var fname = text.Substring(TakeSnapShotText.Length + 1);
                    // var fpath = string.Format("{0}/{1}{2}", m_savePath, fname, ".memsnap3");
                    // 注意:この関数はprivateになっているのでpunlicに変更して下さい。
                    // PackedMemorySnapshotUtility.SaveToFile(fpath, m_snapshot);                                        
                }
            }
        }


        



        void FinishCB(string str,bool isSuccess)
        {
            EditorConnection.instance.Send(UnityMemoryProfilerSupportKunClient.kMsgSendEditorToPlayer, Encoding.ASCII.GetBytes("Success"));
            EditorUtility.ClearProgressBar();
        }


        public void Initialize()
        {
            if (!m_registered)
            {
                
                
                m_registered = true;
            }
#if! UNITY_2018_1_OR_NEWER
            Reflection();
#endif
        }


#if  !UNITY_2018_1_OR_NEWER
        // この関数内の処理は全く、推奨出来ませんので参考にしないでください。
        void Reflection()
        {
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

            // 2018ではAttachProilerUIの定義位置が変わったので一旦取りやめ

        }
#endif

        void OnGUI()
        {
            Initialize();
#if UNITY_2018_1_OR_NEWER
            if (attachProfilerState != null)
            {
                ConnectionGUILayout.AttachToPlayerDropdown(attachProfilerState, EditorStyles.toolbarDropDown);
            }
#else
            // AttachProfiler
            if (m_attachProfilerUIOnGUILayOut != null && m_attachProfilerUI != null)
            {
                m_attachProfilerUIOnGUILayOut.Invoke(m_attachProfilerUI, new object[] { this });
            }
#endif
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


        void IncomingSnapshot(UnityEditor.MemoryProfiler.PackedMemorySnapshot snapshot)
        {
            EditorUtility.ClearProgressBar();
            UnityEditor.Profiling.Memory.Experimental.PackedMemorySnapshot.Convert(snapshot, m_fname);

            var snap = UnityEditor.Profiling.Memory.Experimental.PackedMemorySnapshot.Load(m_fname);
            Debug.Log(snap);
        }
    }
}