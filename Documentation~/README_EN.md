# UnityMemoryProfilerSupportKun

![GitHub package.json version](https://img.shields.io/github/package-json/v/katsumasa/UnityMemoryProfilerSupportKun)

## Summary

[MemoryProfiler](https://docs.unity3d.com/Packages/com.unity.memoryprofiler@0.2/manual/index.html)is a powerful tool for investigating memory related matters such as memory leaks.
Though if you do it manually from the GUI, you won't be able to execute Capture at exactly same timing.
UnityMemoryProfilerSupportKun is a set of Runtime API and Editor extension to easily operates Capture from a script.

## What you can do

- Execute a MemoryProfiler's Snapeshot from a Script then saves it in Application.temporaryCachePath.
- You can get the Snapshot saved on the terminal from Unity Editor.

## Operating Environment

### Confirmed Unity version

- Unity2019.4.19f1

### Confirmed platform

- Android

## How to install

### Install using git

```
git clone https://github.com/katsumasa/UnityMemoryProfilerSupportKun.git
```

### Install using Unity Package Manager

1. Window-> PackageManager
2. Click Add![image](https://user-images.githubusercontent.com/29646672/137414393-25927fd4-a468-4269-9f59-451696793bc6.png)
3. The following dorpdown menu appears </br>
   ![image](https://user-images.githubusercontent.com/29646672/137414541-28598d85-5e02-4ad1-a3f4-fa66db9b5e23.png)
4. Select Add package from git URL from the dropdown. A text field and an Add button will appear.
5. Type in:　https://github.com/katsumasa/UnityMemoryProfilerSupportKun.git　in the text field.


## How to use

- Place UnityMemoryProfiler under Prefabs > Scene. Please note that this Prefab MUST always be there.
- When building the application, please build with both `Development Build` and `AutoConnect Profiler` checked.
- Execute the following API at the location where you want to capture the MemoryProfiler.

```cs
UnityMemoryProfilerSupportKunClient.instance.TakeCapture("File name of the Snapshot");
```

When the Capture is done, the following variables will be `true`

```cs
UnityMemoryProfilerSupportKunClient.instance.isDone
```

For example, you may be able to find a clue to solve a memory leak by executing the above method immediately before or after switching scenes and comparing the differences.

### UnityMemoryProfilerSupportKunWindow

Open the window by going: Window->UnityMemoryProfilerSupportKunWindow

![image](https://user-images.githubusercontent.com/29646672/112799481-60a17980-90a9-11eb-9e94-2a27f52c1457.png)

#### Connect to

Select the player you wish to connect.

#### Get Snap List

Get the list of Memory Profiler snap that are saved in the terminal.

#### DownLoad

Downloads the specified snap to any folder.

#### Delete

Deletes the specified snap from the terminal.

## Sample Program

Two samples (Simple and SceneMain) are available. 


### Simple

A simple sample that takes a SnapShot by pressing a button on the actual device. Only build the scene in `simple.unty`

### SceneMain

A sample that takes a SnapShot at the timing of switching Scene. Include the following 3 scenes in your build.

- `SceneMain.unity`
- `SceneSub0001.unity`
- `SceneSub0002.unity`
