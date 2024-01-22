#if R3_UGUI_SUPPORT
using R3;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace R3
{
    public static partial class UnityUIComponentExtensions
    {
        public static IDisposable SubscribeToText(this Observable<string> source, Text text)
        {
            return source.Subscribe(text, static (x, t) => t.text = x);
        }

        public static IDisposable SubscribeToText<T>(this Observable<T> source, Text text)
        {
            return source.Subscribe(text, static (x, t) => t.text = x.ToString());
        }

        public static IDisposable SubscribeToText<T>(this Observable<T> source, Text text, Func<T, string> selector)
        {
            return source.Subscribe((text, selector), static (x, state) => state.text.text = state.selector(x));
        }

        public static IDisposable SubscribeToInteractable(this Observable<bool> source, Selectable selectable)
        {
            return source.Subscribe(selectable, static (x, s) => s.interactable = x);
        }

        /// <summary>Observe onClick event.</summary>
        public static Observable<Unit> OnClickAsObservable(this Button button)
        {
            return button.onClick.AsObservable(button.GetDestroyCancellationToken());
        }

        /// <summary>Observe onValueChanged with current `isOn` value on subscribe.</summary>
        public static Observable<bool> OnValueChangedAsObservable(this Toggle toggle)
        {
            // Optimized Defer + StartWith
            return Observable.Create<bool, Toggle>(toggle, static (observer, t) =>
            {
                observer.OnNext(t.isOn);
                return t.onValueChanged.AsObservable(t.GetDestroyCancellationToken()).Subscribe(observer);
            });
        }

        /// <summary>Observe onValueChanged with current `value` on subscribe.</summary>
        public static Observable<float> OnValueChangedAsObservable(this Scrollbar scrollbar)
        {
            return Observable.Create<float, Scrollbar>(scrollbar, static (observer, s) =>
            {
                observer.OnNext(s.value);
                return s.onValueChanged.AsObservable(s.GetDestroyCancellationToken()).Subscribe(observer);
            });
        }

        /// <summary>Observe onValueChanged with current `normalizedPosition` value on subscribe.</summary>
        public static Observable<Vector2> OnValueChangedAsObservable(this ScrollRect scrollRect)
        {
            return Observable.Create<Vector2, ScrollRect>(scrollRect, static (observer, s) =>
            {
                observer.OnNext(s.normalizedPosition);
                return s.onValueChanged.AsObservable(s.GetDestroyCancellationToken()).Subscribe(observer);
            });
        }

        /// <summary>Observe onValueChanged with current `value` on subscribe.</summary>
        public static Observable<float> OnValueChangedAsObservable(this Slider slider)
        {
            return Observable.Create<float, Slider>(slider, static (observer, s) =>
            {
                observer.OnNext(s.value);
                return s.onValueChanged.AsObservable(s.GetDestroyCancellationToken()).Subscribe(observer);
            });
        }

        /// <summary>Observe onEndEdit(Submit) event.</summary>
        public static Observable<string> OnEndEditAsObservable(this InputField inputField)
        {
            return inputField.onEndEdit.AsObservable(inputField.GetDestroyCancellationToken());
        }

        /// <summary>Observe onValueChanged with current `text` value on subscribe.</summary>
        public static Observable<string> OnValueChangedAsObservable(this InputField inputField)
        {
            return Observable.Create<string, InputField>(inputField, static (observer, i) =>
            {
                observer.OnNext(i.text);
                return i.onValueChanged.AsObservable(i.GetDestroyCancellationToken()).Subscribe(observer);
            });
        }

        /// <summary>Observe onValueChanged with current `value` on subscribe.</summary>
        public static Observable<int> OnValueChangedAsObservable(this Dropdown dropdown)
        {
            return Observable.Create<int, Dropdown>(dropdown, static (observer, d) =>
            {
                observer.OnNext(d.value);
                return d.onValueChanged.AsObservable(d.GetDestroyCancellationToken()).Subscribe(observer);
            });
        }
    }
}
#endif
