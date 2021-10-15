//
// (C) Katsumasa.Kimura@UTJ
//
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Networking.PlayerConnection;
using UnityEngine.Profiling.Memory.Experimental;


namespace Utj.UnityMemoryProfilerSupportKun
{
    public static class Converter
    {
        /// <summary>
        /// byteデータの配列をオブジェクトに変換する
        /// </summary>
        /// <typeparam name="T">オブジェクトタイプ</typeparam>
        /// <param name="src">byte配列</param>
        /// <param name="dst">オブジェクト</param>
        public static void BytesToObject<T>(byte[] src, out T dst)
        {

            if (src != null)
            {
                var bf = new BinaryFormatter();
                var ms = new MemoryStream(src);
                try
                {
                    dst = (T)bf.Deserialize(ms);
                }
                finally
                {
                    ms.Close();
                }
            }
            else
            {
                dst = default(T);
            }
        }


        /// <summary>
        /// オブジェクトをbyte配列へ変換する
        /// </summary>
        /// <param name="src">オブジェクト</param>
        /// <param name="dst">byte配列</param>
        public static void ObjectToBytes(object src, out byte[] dst)
        {
            var bf = new BinaryFormatter();
            var ms = new MemoryStream();
            try
            {
                bf.Serialize(ms, src);
                dst = ms.ToArray();
            }
            finally
            {
                ms.Close();
            }
        }
    }


    /// <summary>
    /// Player<->Editor間のMessage識別子
    /// </summary>
    public enum MessageID
    {
        Dir,
        DownLoad,
        Delete,
    }
    
   
    /// <summary>
    /// Player<->Editor間のMessageDataの基底Class
    /// </summary>
    [System.Serializable]
    public class MessageDataBase
    {
        [SerializeField] public MessageID messageID;
    }


    /// <summary>
    /// Dirコマンド用のMessageData
    /// </summary>
    [System.Serializable]
    public class MessageDataDir : MessageDataBase
    {
        [SerializeField] public int len;
        [SerializeField] public string[] snaps;
        [SerializeField] public string[] imges;
    }


    /// <summary>
    /// Downloadコマンド用のMessageData
    /// </summary>
    [System.Serializable]
    public class MessageDataDownload : MessageDataBase
    {
        /// <summary>
        /// ファイル名
        /// </summary>
        [SerializeField] public string fname;

        /// <summary>
        /// snapデータのbyte配列
        /// </summary>
        [SerializeField] public byte[] snap;

        /// <summary>
        /// imageデータのbyte配列
        /// </summary>
        [SerializeField] public byte[] image;
    }


    /// <summary>
    /// Deleteコマンド実行時のMessageData
    /// </summary>
    [System.Serializable]
    public class MessageDataDelete : MessageDataBase
    {
        /// <summary>
        /// 削除したいsnapのファイル名
        /// </summary>
        [SerializeField] public string fname;
    }



    // UnityMemoryProfilerSupport君のClient側の処理
    // このスクリプトを適当なDestroyされないGameObjectに張り付けてお使い下さい
    //
    // ざっくりな使い方の例
    // 
    // UnityMemoryProfilerSupportKunClient.instance.TakeSnapshot("snapshot0001");
    

    
    public class UnityMemoryProfilerSupportKunClient : MonoBehaviour
    {        
        public static readonly Guid kMsgSendEditorToPlayer = new Guid("5735d5a131504489ac704b91707438a5");
        public static readonly Guid kMsgSendPlayerToEditor = new Guid("dfc22fbac4d242fe80e19b121510488b");
        static UnityMemoryProfilerSupportKunClient m_instance = null;


        public static UnityMemoryProfilerSupportKunClient instance
        {
            get { return m_instance; }
        }

        public bool isDone
        {
            get;
            private set;
        }

        /// <summary>
        /// MemoryProfiler.TakeSnapshotを実行する
        /// </summary>
        /// <param name="fname">snapshotのファイル名</param>
        public void TakeSnapshot(string fname)
        {
            isDone = false;
            var path = string.Format("{0}/{1}.snap", Application.temporaryCachePath, fname);            
            MemoryProfiler.TakeSnapshot(path, FinishCB, ScreenshotCallback);
        }


        void FinishCB(string fpath, bool isSuccess)
        {
            isDone = true;
            if (!isSuccess)
            {
                Debug.Log("Success " + isSuccess);
            }
        }


        void ScreenshotCallback(string fpath, bool isSuccess, UnityEngine.Profiling.Experimental.DebugScreenCapture debugScreenCapture)
        {
            if (isSuccess)
            {
                Texture2D texture = new Texture2D(debugScreenCapture.width, debugScreenCapture.height, debugScreenCapture.imageFormat, false);
                try
                {
                    var bytes = debugScreenCapture.rawImageDataReference.ToArray();
                    texture.LoadRawTextureData(bytes);
                    var pngData = texture.EncodeToPNG();
                    var path = System.IO.Path.ChangeExtension(fpath, "png");
                    System.IO.File.WriteAllBytes(path, pngData);
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            } else
            {
                Debug.Log(isSuccess);
            }
        }

        int GetSnapFiles(out string[] paths)
        {
            var fpath = Application.temporaryCachePath;
            paths = System.IO.Directory.GetFiles(fpath, "*.snap");
            
            if(paths != null)
            {
                return paths.Length;
            }
            return 0;
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
            //Debug.Log("UnityMemoryProfilerSupportKunClient.OnMessageEvent:");
            MessageDataBase messageDataBase;
            Converter.BytesToObject<MessageDataBase>(messageEventArgs.data, out messageDataBase);                      
            //Debug.Log("messageID:" + messageDataBase.messageID);
            switch(messageDataBase.messageID)
            {
                case MessageID.Dir:
                    {
                        MessageDataDir messageDataDir = new MessageDataDir();
                        messageDataDir.messageID = MessageID.Dir;
                        messageDataDir.len = GetSnapFiles(out messageDataDir.snaps);
                        messageDataDir.imges = new string[messageDataDir.snaps.Length];
                        for (var i = 0; i < messageDataDir.snaps.Length; i++)
                        {                           
                            string fpath = System.IO.Path.ChangeExtension(messageDataDir.snaps[i], "png");
                            if (System.IO.File.Exists(fpath))
                            {
                                messageDataDir.imges[i] = fpath;
                            } else
                            {
                                messageDataDir.imges[i] = "";
                            }
                            //Debug.Log(messageDataDir.snaps[i]);
                            //Debug.Log(messageDataDir.imges[i]);
                        }

                        byte[] bytes;
                        Converter.ObjectToBytes(messageDataDir, out bytes);
                        PlayerConnection.instance.Send(kMsgSendPlayerToEditor, bytes);

                    }
                    break;

                case MessageID.DownLoad:
                    {
                        MessageDataDownload messageDataDownload;
                        Converter.BytesToObject<MessageDataDownload>(messageEventArgs.data, out messageDataDownload);
                        string[] fpaths;
                        var len = GetSnapFiles(out fpaths);
                        for(var i = 0; i < len; i++)
                        {
                            var fpath = fpaths[i];
                            var fname = System.IO.Path.GetFileName(fpath);
                            if(messageDataDownload.fname == fname)
                            {
                                if (File.Exists(fpath))
                                {
                                    messageDataDownload.snap = File.ReadAllBytes(fpath);                                    
                                }
                                var imagePath = System.IO.Path.ChangeExtension(fpath, "png");
                                if (File.Exists(imagePath))
                                {
                                    messageDataDownload.image = File.ReadAllBytes(imagePath);
                                }
                                byte[] bytes;
                                Converter.ObjectToBytes(messageDataDownload, out bytes);
                                PlayerConnection.instance.Send(kMsgSendPlayerToEditor, bytes);
                                break;
                            }                            
                        }
                    }
                    break;

                case MessageID.Delete:
                    {
                        MessageDataDelete messageDataDelete;
                        Converter.BytesToObject<MessageDataDelete>(messageEventArgs.data, out messageDataDelete);
                        string[] fpaths;
                        var len = GetSnapFiles(out fpaths);
                        for (var i = 0; i < len; i++)
                        {
                            var fpath = fpaths[i];
                            var fname = System.IO.Path.GetFileName(fpath);
                            if (messageDataDelete.fname == fname)
                            {
                                if (File.Exists(fpath))
                                {
                                    File.Delete(fpath);
                                }
                                var imagePath = System.IO.Path.ChangeExtension(fpath, "png");
                                if (File.Exists(imagePath))
                                {
                                    File.Delete(imagePath);
                                }
                                byte[] bytes;
                                Converter.ObjectToBytes(messageDataDelete, out bytes);
                                PlayerConnection.instance.Send(kMsgSendPlayerToEditor, bytes);
                                break;
                            }
                        }
                    }
                    break;
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