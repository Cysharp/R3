using R3;
using System;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.Threading;
using System.Text;
using UnityEngine.LowLevel;
using System.Linq;

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

     void Start()
    {
        rpInt.Subscribe(x =>
        {
            Debug.Log(x);
        });



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


public class PlayerLoopInfo
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Init()
    {
        // var playerLoop = UnityEngine.LowLevel.PlayerLoop.GetDefaultPlayerLoop();
        var playerLoop = UnityEngine.LowLevel.PlayerLoop.GetCurrentPlayerLoop();
        DumpPlayerLoop("After SubsystemRegistration", playerLoop);
    }

    public static void DumpPlayerLoop(string which, UnityEngine.LowLevel.PlayerLoopSystem playerLoop)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"{which} PlayerLoop List");
        foreach (var header in playerLoop.subSystemList)
        {
            sb.AppendFormat("------{0}------", header.type.Name);
            sb.AppendLine();
            foreach (var subSystem in header.subSystemList)
            {
                sb.AppendFormat("{0}.{1}", header.type.Name, subSystem.type.Name);
                sb.AppendLine();

                if (subSystem.subSystemList != null)
                {
                    UnityEngine.Debug.LogWarning("More Subsystem:" + subSystem.subSystemList.Length);
                }
            }
        }

        UnityEngine.Debug.Log(sb.ToString());
    }

    public static Type CurrentLoopType { get; private set; }

    public static void Inject()
    {
        var system = PlayerLoop.GetCurrentPlayerLoop();

        for (int i = 0; i < system.subSystemList.Length; i++)
        {
            var loop = system.subSystemList[i].subSystemList.SelectMany(x =>
            {
                var t = typeof(WrapLoop<>).MakeGenericType(x.type);
                var instance = (ILoopRunner)Activator.CreateInstance(t, x.type);
                return new[] { new PlayerLoopSystem { type = t, updateDelegate = instance.Run }, x };
            }).ToArray();

            system.subSystemList[i].subSystemList = loop;
        }

        PlayerLoop.SetPlayerLoop(system);
    }

    interface ILoopRunner
    {
        void Run();
    }

    class WrapLoop<T> : ILoopRunner
    {
        readonly Type type;

        public WrapLoop(Type type)
        {
            this.type = type;
        }

        public void Run()
        {
            CurrentLoopType = type;
        }
    }
}
