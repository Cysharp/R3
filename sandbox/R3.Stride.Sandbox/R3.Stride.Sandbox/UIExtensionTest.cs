using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stride.Core.Mathematics;
using Stride.Input;
using Stride.Engine;
using Stride.UI;
using Stride.UI.Controls;

namespace R3.Stride.Sandbox
{
    public class UIExtensionTest : StartupScript
    {
        // Declared public member fields and properties will show in the game studio

        public override void Start()
        {
            var page = Entity.Get<UIComponent>().Page;
            var button1 = page.RootElement.FindVisualChildOfType<Button>();
            var text1 = page.RootElement.FindVisualChildOfType<EditText>();
            var slider1 = page.RootElement.FindVisualChildOfType<Slider>();
            var toggle1 = page.RootElement.FindVisualChildOfType<ToggleButton>();
            button1.MouseOverStateChangedAsObservable()
                .Subscribe(x =>
                {
                    Log.Info($"button1 mouseover: old = {x.arg.OldValue}, new = {x.arg.NewValue}");
                });
            button1.ClickAsObservable()
                .Subscribe(x =>
                {
                    Log.Info($"button1 clicked: {x.arg.RoutedEvent.Name}");
                });
            text1.TextChangedAsObservable()
                .Subscribe(x =>
                {
                    Log.Info($"text1 changed: {text1.Text}, {x.arg.RoutedEvent.Name}");
                });
            slider1.ValueChangedAsObservable()
                .Subscribe(x =>
                {
                    Log.Info($"slider1 changed: {slider1.Value}, {x.arg.RoutedEvent.Name}");
                });
            toggle1.CheckedAsObservable()
                .Subscribe(x =>
                {
                    Log.Info($"toggle1 checked");
                });
            toggle1.UncheckedAsObservable()
                .Subscribe(x =>
                {
                    Log.Info($"toggle1 unchecked");
                });
            toggle1.IndeterminateAsObservable()
                .Subscribe(x =>
                {
                    Log.Info($"toggle1 indeterminate");
                });
        }
    }
}
