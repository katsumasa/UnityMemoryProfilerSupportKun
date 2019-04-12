# UnityMemoryProfilerSupportKun
実機側から`UnityEditor`上の`MemoryProfiler`に対してSnapShotを実行タイミングを指定することが出来る便利クラスです。
シーン切り替え時など特定のタイミングで`MemoryProfiler`のSnapShotを取ることが出来るのでメモリーリークの調査に役立てることが出来ると思います。
## 動作環境
### 動作確認済みUnity
- Unity2017.4.24f1
- Unity2018.3.12f1
### 動作確認済みプラットフォーム
- iOS
- Android
- Windows 10
## 必要パッケージ
MemoryProfiler本体が別途必要ですので、下記のURLから取得して下さい。
https://bitbucket.org/Unity-Technologies/memoryprofiler
また`MemoryProfiler`に含まれる`PackedMemorySnapshotUtility.cs`
`static void SaveToFile(string filePath, PackedMemorySnapshot snapshot)`を`public static void SaveToFile(string filePath, PackedMemorySnapshot snapshot)` に変更して下さい。
## ファイル説明
- UnityMemoryProfilerSupportKunEditor.cs
UnityEditor側で使用するファイルです。`Editor`フォルダの下に置いて下さい。
- UnityMemoryProfilerSupportKunEditor.cs
UnityPlayer(アプリ）側で使用するファイルです。singletonのGameObjectとしてSceneに配置して使用して下さい。
## スナップショットの撮り方
スナップショットを撮りたいタイミングで下記のメソッドを実行して下さい。
```
UnityMemoryProfilerSupportKunClient.instance.Send("スナップショットのファイル名");
```
また、UnityEditor側でスナップショットの保存が終了した際に、`UnityMemoryProfilerSupportKunClient.instance.isDone` が`true`を返します。
## ビルド設定
`Development Build` 及び `AutoConnect Profiler` の両方にチェックを入れた状態でビルドを行って下さい。
## 使用方法
前提条件として`MemoryProfiler`が使用可能である必要があります。
`UnityMemorySProfilerSupportKun` Windowの他に`MemoryProfiler`及び`Profiler`Windowを開き、`Active Profiler`を計測対象のアプリケーションと接続して下さい。
`Active Profiler`との接続に関する詳細は下記のURLをご確認下さい。
https://docs.unity3d.com/ja/current/Manual/ProfilerWindow.html

## サンプルプログラム
下記の２種類のサンプルを用意しています。
### Simple
実機側のボタンを押すことでSnapShotを取るシンプルなサンプル。シーン`simple.unty`のみビルドの対象として下さい。
### SceneMain
Scene切り替えのタイミングでSnapShotを取るサンプル。下記の３シーンをビルドに含めて下さい。
- `SceneMain.unity`
- `SceneSub0001.unity`
- `SceneSub0002.unity`
