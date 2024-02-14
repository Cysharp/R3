using R3;
using System;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.Threading;

public class NewBehaviourScript : MonoBehaviour
{
    public SerializableReactiveProperty<int> rpInt;
    public SerializableReactiveProperty<long> rpLong;
    public SerializableReactiveProperty<byte> rpByte;
    public SerializableReactiveProperty<float> rpFloat;
    public SerializableReactiveProperty<double> rpDouble;
    public SerializableReactiveProperty<string> rpString;
    public SerializableReactiveProperty<bool> rpBool;
    public SerializableReactiveProperty<Vector2> rpVector2;
    public SerializableReactiveProperty<Vector2Int> rpVector2Int;
    public SerializableReactiveProperty<Vector3> rpVector3;
    public SerializableReactiveProperty<Vector3Int> rpVector3Int;
    public SerializableReactiveProperty<Vector4> rpVector4;
    public SerializableReactiveProperty<Color> rpColor;
    public SerializableReactiveProperty<Rect> rpRect;
    public SerializableReactiveProperty<Bounds> rpBounds;
    public SerializableReactiveProperty<BoundsInt> rpBoundsInt;

    public SerializableReactiveProperty<Quaternion> rpQuaternion;
    public SerializableReactiveProperty<Matrix4x4> rpMatrix4x4;
    public SerializableReactiveProperty<FruitEnum> rpEnum;
    public SerializableReactiveProperty<FruitFlagsEnum> rpFlagsEnum;


    public SerializableReactiveProperty<int> NANTOKAMARU;
    //public NantonakuProperty NANTOKAMARU;


    public Button button;
    // Text text;

    void Start()
    {
        //button.OnClickAsObservable()
        //    .SelectAwait(async (_, ct) =>
        //    {
        //        var req = await UnityWebRequest.Get("https://google.com/").SendWebRequest().WithCancellation(ct);
        //        return req.downloadHandler.text;
        //    }, AwaitOperations.Drop)
        //    .SubscribeToText(text);


        button.OnClickAsObservable()
            .Do(x => Debug.Log($"Do:{Thread.CurrentThread.ManagedThreadId}"))
            .SubscribeAwait(async (_, ct) =>
            {
                Debug.Log($"Before Await:{Thread.CurrentThread.ManagedThreadId}");
                var time = Time.time;
                while (Time.time - time < 1f)
                {
                    transform.position += Vector3.forward * Time.deltaTime;
                    await UniTask.Yield(ct);
                    Debug.Log($"After Yield:{Thread.CurrentThread.ManagedThreadId}");
                }

            }, AwaitOperation.Sequential/*, configureAwait: false*/)
            .AddTo(this);


        //var subject = new Subject<int>();

        //subject
        //    .Do(x => Debug.Log($"Do:{Thread.CurrentThread.ManagedThreadId}"))
        //    .SubscribeAwait(async (_, ct) =>
        //    {
        //        Debug.Log($"Before Await:{Thread.CurrentThread.ManagedThreadId}");
        //        await UniTask.Yield(ct);
        //        Debug.Log($"After Yield:{Thread.CurrentThread.ManagedThreadId}");
        //    }, AwaitOperation.Sequential/*, configureAwait: false*/);


        //subject.OnNext(10);
        //subject.OnNext(20);
        //subject.OnNext(30);
    }



}



public enum FruitEnum
{
    Apple, Grape, Orange
}

[Flags]
public enum FruitFlagsEnum
{
    None = 0,
    Apple = 1,
    Grape = 2,
    Orange = 4
}

[Serializable]
public class NantonakuProperty
{
    public int value;
}
