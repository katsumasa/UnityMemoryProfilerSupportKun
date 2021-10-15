using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Profiling.Memory.Experimental;


public  class Test
{
    static Action<string, bool> action;

    [MenuItem("Window/UTJ/UnityMemoryProfilerSupportKun/TEST")]
    public static void Func()
    {

        //        System.Type t = Types.GetType("Unity.MemoryProfiler.Editor", "Unity.MemoryProfiler.Editor.dll");
        //var assembly =  Assembly.Load("Unity.MemoryProfiler.Editor");
        //var type = assembly.GetType("Unity.MemoryProfiler.Editor.MemoryProfilerWindow");
#if false
        var memoryProfilerWindow = type.InvokeMember(
                   null
               , BindingFlags.CreateInstance
               , null
               , null
               , new object[] { }
               );

        
        var methodInfo = type.GetMethod("TakeCapture", BindingFlags.NonPublic | BindingFlags.Instance);
        methodInfo.Invoke(memoryProfilerWindow, null);
#endif
        //var window = EditorWindow.GetWindow(type);
        //var methodInfo = type.GetMethod("TakeCapture", BindingFlags.NonPublic | BindingFlags.Instance);
        //var methodInfo = type.GetMethod("DelayedSnapshotRoutine", BindingFlags.NonPublic | BindingFlags.Instance);



        //        methodInfo.Invoke(window, null);
        //      Debug.Log(window);
        var now = System.DateTime.Now;        
        string fpath = System.IO.Directory.GetCurrentDirectory() + "/Temp/" + now.ToString("yyyyMMddHHmmss") + ".snap";        
        MemoryProfiler.TakeSnapshot(fpath, FinishCB, ScreenshotCallback);

    }


    static void FinishCB(string fpath,bool isSuccess)
    {
        Debug.Log(fpath);
        Debug.Log(isSuccess);
    }

    static void ScreenshotCallback(string fpath, bool isSuccess,UnityEngine.Profiling.Experimental.DebugScreenCapture debugScreenCapture)
    {
        Debug.Log(fpath);
        Debug.Log(isSuccess);
        Texture2D texture = new Texture2D(debugScreenCapture.width, debugScreenCapture.height, debugScreenCapture.imageFormat, false);
        try
        {
            var bytes = debugScreenCapture.rawImageDataReference.ToArray();
            texture.LoadRawTextureData(bytes);
            var pngData = texture.EncodeToPNG();
            var path = System.IO.Path.GetFileNameWithoutExtension(fpath) + ".png";
            System.IO.File.WriteAllBytes(path, pngData);
        }
        catch(Exception e)
        {
            Debug.Log(e);
        }
    }



    static void CallBack(string s,bool flag)
    {
        Debug.Log(s);
        Debug.Log(flag);
        
    }
}
