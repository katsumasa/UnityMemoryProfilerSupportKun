# UnityMemoryProfilerSupportKun

## 概要

[MemoryProfiler](https://docs.unity3d.com/Packages/com.unity.memoryprofiler@0.2/manual/index.html)はメモリーリーク等、メモリー関連の調査に必要不可欠なツールですが、GUIから手動で行う場合、完全に同じタイミングでCaptureを実行することは出来ません。
UnityMemoryProfilerSupportKunはスクリプトからのCaptureを容易に運用する為のRuntime APIとEditor拡張をセットにしたものです。

## 出来ること

- ScriptからMemoryProfilerのSnapshotを実行し、Application.temporaryCachePathへ保存します。
- 端末上に保存されたSnapshotをUnityEditorから取得出来ます。


## 動作環境

### 動作確認済みUnity

- Unity2019.4.19f1

### 動作確認済みプラットフォーム

- Android

## 使い方

- Prefabs/UnityMemoryProfilerをSceneに配置して下さい。このPrefabは常に存在している必要があることに注意して下さい。
- アプリケーションをビルドする際、`Development Build` 及び `AutoConnect Profiler` の両方にチェックを入れた状態でビルドを行って下さい。
- MemoryProfilerのCaptureを行う箇所で下記のAPIを実行して下さい。

```cs
UnityMemoryProfilerSupportKunClient.instance.TakeCapture("スナップショットのファイル名");
```

Capture処理が完了すると、下記の変数が`true`になります。

```cs
UnityMemoryProfilerSupportKunClient.instance.isDone
```

例えば、シーン切り替え直前・直後に上記メソッドを実行し差分を比較することでメモリーリークの解決の糸口を見つけることが出来る可能性があります。

### UnityMemoryProfilerSupportKunWindow

Window->UnityMemoryProfilerSupportKunWindowでWindowが開きます。

#### Connect to

接続先のPlayerを選択します。

#### Get Snap List

端末に保存されたMemoryProfilerのsnapの一覧を取得します。

#### DownLoad

指定されたsnapを任意のフォルダーにダウンロードします。

#### Delete

指定されたsnapを端末から削除します。

## サンプルプログラム

下記の２種類のサンプルを用意しています。

### Simple

実機側のボタンを押すことでSnapShotを取るシンプルなサンプル。シーン`simple.unty`のみビルドの対象として下さい。

### SceneMain

Scene切り替えのタイミングでSnapShotを取るサンプル。下記の３シーンをビルドに含めて下さい。

- `SceneMain.unity`
- `SceneSub0001.unity`
- `SceneSub0002.unity`
