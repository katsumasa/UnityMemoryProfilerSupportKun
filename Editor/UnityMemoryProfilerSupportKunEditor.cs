// (C) UTJ
using System;
using System.Collections;
using System.Collections.Generic;
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
    /// <summary>
    /// UnityMemoryProfilerSupportKunのUnityEditor側の処理    
    /// </summary>
    public class UnityMemoryProfilerSupportKunEditor : EditorWindow
    {            
        Vector2 scrollPos;
        List<GUIContent> m_snaps;
    
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
            EditorConnection.instance.Initialize();
            EditorConnection.instance.Register(UnityMemoryProfilerSupportKunClient.kMsgSendPlayerToEditor, OnMessageEvent);
        }


        void OnDisable()
        {
#if UNITY_2018_1_OR_NEWER
            attachProfilerState.Dispose();
            attachProfilerState = null;
#endif            
            EditorConnection.instance.Unregister(UnityMemoryProfilerSupportKunClient.kMsgSendPlayerToEditor, OnMessageEvent);
            EditorConnection.instance.DisconnectAll();
        }


        /// <summary>
        /// Playerからのメッセージを処理する
        /// </summary>
        /// <param name="args"></param>
        private void OnMessageEvent(MessageEventArgs args)
        {
            MessageDataBase messageDataBase;
            Converter.BytesToObject<MessageDataBase>(args.data, out messageDataBase);
            Debug.Log("UnityMemoryProfilerSupportKunClient.OnMessageEvent:" + messageDataBase.messageID);
            switch (messageDataBase.messageID)
            {
                case MessageID.Dir:
                    {
                        MessageDataDir messageDataDir;
                        Converter.BytesToObject<MessageDataDir>(args.data, out messageDataDir);
                        m_snaps = new List<GUIContent>();
                        for (var i = 0; i < messageDataDir.len; i++)
                        {
                            var fname = System.IO.Path.GetFileName(messageDataDir.snaps[i]);
                            var texture = new Texture2D(64, 64);
                            var content = new GUIContent(fname,texture);
                            m_snaps.Add(content);
                        }
                    }
                    break;

                case MessageID.DownLoad:
                    {
                        MessageDataDownload messageDataDownload;
                        Converter.BytesToObject<MessageDataDownload>(args.data, out messageDataDownload);
                        var path = EditorUtility.SaveFilePanel(
                            "Save snapshot file",
                            "",
                            messageDataDownload.fname,
                            "snap"
                            );
                        if (path.Length != 0)
                        {
                            System.IO.File.WriteAllBytes(path, messageDataDownload.snap);
                            if (messageDataDownload.image != null)
                            {
                                var imagePath = System.IO.Path.ChangeExtension(path, "png");
                                System.IO.File.WriteAllBytes(imagePath, messageDataDownload.image);
                                for (var i = 0; i < m_snaps.Count; i++)
                                {
                                    if (m_snaps[i].text == messageDataDownload.fname)
                                    {
                                        var src = new Texture2D(64, 64);
                                        src.LoadImage(messageDataDownload.image);
                                        var dst = new Texture2D(64, 64);
                                        Graphics.ConvertTexture(src, dst);
                                        m_snaps[i] = new GUIContent(messageDataDownload.fname, dst);                                        
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    break;

                case MessageID.Delete:
                    {
                        MessageDataDelete messageDataDelete;
                        Converter.BytesToObject<MessageDataDelete>(args.data, out messageDataDelete);
                        for(var i = 0; i < m_snaps.Count; i++)
                        {
                            if(m_snaps[i].text == messageDataDelete.fname)
                            {
                                m_snaps.Remove(m_snaps[i]);
                                break;
                            }
                        }
                    }
                    break;            
            }
        }
       

        public void Initialize()
        {
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

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Connect to ");
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

            if (GUILayout.Button("Get Snap List"))
            {
                MessageDataDir messageDataDir = new MessageDataDir();
                messageDataDir.messageID = MessageID.Dir;
                byte[] bytes;
                Converter.ObjectToBytes(messageDataDir, out bytes);
                EditorConnection.instance.Send(UnityMemoryProfilerSupportKunClient.kMsgSendEditorToPlayer, bytes);
            }
            EditorGUILayout.EndHorizontal();

            // 接続済みPlayerのリスト表示部
            {
                var playerCount = EditorConnection.instance.ConnectedPlayers.Count;
                StringBuilder builder = new StringBuilder();
                builder.AppendLine(string.Format("{0} players connected.", playerCount));
                int i = 0;
                foreach (var p in EditorConnection.instance.ConnectedPlayers)
                {
                    builder.AppendLine(string.Format("[{0}] - {1} {2}", i++, p.name, p.playerId));
                }
                EditorGUILayout.HelpBox(builder.ToString(), MessageType.Info);
            }
                     
            
            if(m_snaps != null)
            {
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
                for (var i = 0; i < m_snaps.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label(m_snaps[i]);
                    if (GUILayout.Button("DownLoad", GUILayout.Width(75)))
                    {
                        MessageDataDownload messageDataDownload = new MessageDataDownload();
                        messageDataDownload.messageID = MessageID.DownLoad;
                        messageDataDownload.fname = m_snaps[i].text;
                        byte[] bytes;
                        Converter.ObjectToBytes(messageDataDownload, out bytes);
                        EditorConnection.instance.Send(UnityMemoryProfilerSupportKunClient.kMsgSendEditorToPlayer, bytes);
                    }
                    if (GUILayout.Button("Delete" ,GUILayout.Width(75)))
                    {
                        MessageDataDelete messageDataDelete = new MessageDataDelete();
                        messageDataDelete.messageID = MessageID.Delete;
                        messageDataDelete.fname = m_snaps[i].text;
                        byte[] bytes;
                        Converter.ObjectToBytes(messageDataDelete, out bytes);
                        EditorConnection.instance.Send(UnityMemoryProfilerSupportKunClient.kMsgSendEditorToPlayer, bytes);
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndScrollView();
            }
        }
    }
}