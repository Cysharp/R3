using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Stride.UI;
using Stride.UI.Controls;
using Stride.UI.Events;
using Vortice.Vulkan;

namespace R3;
public static class R3StrideUIExtension
{
    // UIElement
    public static Observable<(object? sender, PropertyChangedArgs<MouseOverState> arg)> MouseOverStateChangedAsObservable(this UIElement element, CancellationToken token = default)
    {
        return Observable.FromEvent<PropertyChangedHandler<MouseOverState>, (object?, PropertyChangedArgs<MouseOverState>)>(h => new PropertyChangedHandler<MouseOverState>((sender, arg) => h((sender, arg))),
            h => element.MouseOverStateChanged += h,
            h => element.MouseOverStateChanged -= h,
            token);
    }
    public static Observable<(object? sender, TouchEventArgs)> PreviewTouchDownAsObservable(this UIElement element, CancellationToken token = default)
    {
        return Observable.FromEventHandler<TouchEventArgs>(h => element.PreviewTouchDown += h, h => element.PreviewTouchDown -= h, token);
    }
    public static Observable<(object? sender, TouchEventArgs)> PreviewTouchMoveAsObservable(this UIElement element, CancellationToken token = default)
    {
        return Observable.FromEventHandler<TouchEventArgs>(h => element.PreviewTouchMove += h, h => element.PreviewTouchMove -= h, token);
    }
    public static Observable<(object? sender, TouchEventArgs)> PreviewTouchUpAsObservable(this UIElement element, CancellationToken token = default)
    {
        return Observable.FromEventHandler<TouchEventArgs>(h => element.PreviewTouchUp += h, h => element.PreviewTouchUp -= h, token);
    }
    public static Observable<(object? sender, TouchEventArgs)> TouchDownAsObservable(this UIElement element, CancellationToken token = default)
    {
        return Observable.FromEventHandler<TouchEventArgs>(h => element.TouchDown += h, h => element.TouchDown -= h, token);
    }
    public static Observable<(object? sender, TouchEventArgs)> TouchMoveAsObservable(this UIElement element, CancellationToken token = default)
    {
        return Observable.FromEventHandler<TouchEventArgs>(h => element.TouchMove += h, h => element.TouchMove -= h, token);
    }
    public static Observable<(object? sender, TouchEventArgs)> TouchUpAsObservable(this UIElement element, CancellationToken token = default)
    {
        return Observable.FromEventHandler<TouchEventArgs>(h => element.TouchUp += h, h => element.TouchUp -= h, token);
    }
    public static Observable<(object? sender, TouchEventArgs)> TouchEnterAsObservable(this UIElement element, CancellationToken token = default)
    {
        return Observable.FromEventHandler<TouchEventArgs>(h => element.TouchEnter += h, h => element.TouchEnter -= h, token);
    }
    public static Observable<(object? sender, TouchEventArgs)> TouchLeaveAsObservable(this UIElement element, CancellationToken token = default)
    {
        return Observable.FromEventHandler<TouchEventArgs>(h => element.TouchLeave += h, h => element.TouchLeave -= h, token);
    }
    // Button
    public static Observable<(object? sender, RoutedEventArgs arg)> ClickAsObservable(this ButtonBase btn, CancellationToken token = default)
    {
        return Observable.FromEventHandler<RoutedEventArgs>(h => btn.Click += h, h => btn.Click -= h, token);
    }
    // Slider
    public static Observable<(object? sender, RoutedEventArgs arg)> ValueChangedAsObservable(this Slider slider, CancellationToken token = default)
    {
        return Observable.FromEventHandler<RoutedEventArgs>(h => slider.ValueChanged += h, h => slider.ValueChanged -= h, token);
    }
    // EditText
    public static Observable<(object? sender, RoutedEventArgs arg)> TextChangedAsObservable(this EditText editText, CancellationToken token = default)
    {
        return Observable.FromEventHandler<RoutedEventArgs>(h => editText.TextChanged += h, h => editText.TextChanged -= h, token);
    }
    // ToggleButton
    public static Observable<(object? sender, RoutedEventArgs arg)> CheckedAsObservable(this ToggleButton toggleButton, CancellationToken token = default)
    {
        return Observable.FromEventHandler<RoutedEventArgs>(h => toggleButton.Checked += h, h => toggleButton.Checked -= h, token);
    }
    public static Observable<(object? sender, RoutedEventArgs arg)> IndeterminateAsObservable(this ToggleButton button, CancellationToken token = default)
    {
        return Observable.FromEventHandler<RoutedEventArgs>(h => button.Indeterminate += h, h => button.Indeterminate -= h, token);
    }
    public static Observable<(object? sender, RoutedEventArgs arg)> UncheckedAsObservable(this ToggleButton toggleButton, CancellationToken token = default)
    {
        return Observable.FromEventHandler<RoutedEventArgs>(h => toggleButton.Unchecked += h, h => toggleButton.Unchecked -= h, token);
    }
    // ModalElement
    public static Observable<(object? sender, RoutedEventArgs arg)> OutsideClickAsObservable(this ModalElement modalElement, CancellationToken token = default)
    {
        return Observable.FromEventHandler<RoutedEventArgs>(h => modalElement.OutsideClick += h, h => modalElement.OutsideClick -= h, token);
    }
}
