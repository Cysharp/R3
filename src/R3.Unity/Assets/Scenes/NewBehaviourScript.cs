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

    public Button button1;
    public Button button2;
    // Text text;

    public NoAwakeTest noAwake;

    async void Start()
    {
        Observer<Unit> dis = (Observer<Unit>)button1
               .OnClickAsObservable()
               .SubscribeAwait(async (_, ct) =>
               {
                   await UniTask.Delay(1000, cancellationToken: ct);
                   Debug.Log("Clicked!");
               }, AwaitOperation.Drop);

        Observer<Unit> dis2 = (Observer<Unit>)button2
            .OnClickAsObservable()
            .Subscribe(_ => Debug.Log("Clicked!"));

        await UniTask.Yield();

        Destroy(button1.gameObject);

        await UniTask.Yield();

        Debug.Log(dis.IsDisposed); // True

        await UniTask.Yield();

        Destroy(button2.gameObject);

        await UniTask.Yield();

        Debug.Log(dis2.IsDisposed); // True



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
