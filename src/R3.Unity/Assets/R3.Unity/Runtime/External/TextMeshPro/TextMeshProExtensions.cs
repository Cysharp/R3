#if R3_TEXTMESHPRO_SUPPORT

using System;
using TMPro;

namespace R3
{
    public static class TextMeshProExtensions
    {
        public static IDisposable SubscribeToText(this Observable<string> source, TMP_Text text)
        {
            return source.Subscribe(text, static (x, t) => t.text = x);
        }

        public static IDisposable SubscribeToText<T>(this Observable<T> source, TMP_Text text)
        {
            return source.Subscribe(text, static (x, t) => t.text = x.ToString());
        }

        public static IDisposable SubscribeToText<T>(this Observable<T> source, TMP_Text text, Func<T, string> selector)
        {
            return source.Subscribe((text, selector), static (x, state) => state.text.text = state.selector(x));
        }

        /// <summary>Observe onEndEdit(Submit) event.</summary>
        public static Observable<string> OnEndEditAsObservable(this TMP_InputField inputField)
        {
            return inputField.onEndEdit.AsObservable(inputField.GetDestroyCancellationToken());
        }

        /// <summary>Observe onValueChanged with current `text` value on subscribe.</summary>
        public static Observable<string> OnValueChangedAsObservable(this TMP_InputField inputField)
        {
            return Observable.Create<string, TMP_InputField>(inputField, static (observer, i) =>
            {
                observer.OnNext(i.text);
                return i.onValueChanged.AsObservable(i.GetDestroyCancellationToken()).Subscribe(observer);
            });
        }

        /// <summary>Observe onValueChanged with current `value` on subscribe.</summary>
        public static Observable<int> OnValueChangedAsObservable(this TMP_Dropdown dropdown)
        {
            return Observable.Create<int, TMP_Dropdown>(dropdown, static (observer, d) =>
            {
                observer.OnNext(d.value);
                return d.onValueChanged.AsObservable(d.GetDestroyCancellationToken()).Subscribe(observer);
            });
        }
    }
}
#endif
