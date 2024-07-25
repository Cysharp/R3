using R3;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

public class SubjectBench : MonoBehaviour
{
    public Button button1;
    public Button button2;
    public Text text1;
    public Text text2;

    EventBusMsg<int> msg = new EventBusMsg<int>() { Value = 1 };

    public int iii;

    public class EventBusMsg<T>
    {
        public T Value { get; set; }
    }

    void Start()
    {
        SetupSubject();

        button1.onClick.AddListener(() =>
        {
            var sw = Stopwatch.StartNew();
            TestR3LegacySubject();
            sw.Stop();
            text1.text = "legacy subject:" + sw.ElapsedMilliseconds;
        });

        button2.onClick.AddListener(() =>
        {
            var sw = Stopwatch.StartNew();
            TestR3Subject();
            sw.Stop();
            text2.text = "new subject:" + sw.ElapsedMilliseconds;
        });
    }

    public int ccccc;
    Subject<EventBusMsg<int>> subject = new Subject<EventBusMsg<int>>();
    // LegacySubject<EventBusMsg<int>> legacysubject = new LegacySubject<EventBusMsg<int>>();


    void SetupSubject()
    {

        var msg = new EventBusMsg<int>() { Value = 1 };
        for (int i = 0; i < 1000; i++)
        {
            subject.Subscribe(OnReceiveMsg);
        }
        for (int i = 0; i < 1000; i++)
        {
            // legacysubject.Subscribe(OnReceiveMsg);
        }

    }
    public void TestR3Subject()
    {


        for (int i = 0; i < 100000; i++)
        {
            subject.OnNext(msg);
        }
    }


    public void TestR3LegacySubject()
    {
        for (int i = 0; i < 100000; i++)
        {
            // legacysubject.OnNext(msg);
        }
    }


    public void OnReceiveMsg(EventBusMsg<int> msg)
    {
        ccccc++;
    }



}
