using R3;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    [ObservableTrigger(TriggerKinds.Update)]
    public partial class GeneratorSandbox
    {
    }
}

namespace R3
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class ObservableTriggerAttribute : Attribute
    {
        public TriggerKinds Kinds { get; private set; }

        public ObservableTriggerAttribute(TriggerKinds kinds, bool defineCore = false)
        {
            this.Kinds = kinds;
        }
    }

    public enum TriggerKinds
    {
        Start,
        StartAsTask,
        Awake,
        AakeAsTask,
        OnDestroy,
        OnDestroyAsTask,
        OnDestroyAsCancellationToken,
        OnMouseDown,
        Update,


        // IEventSystemHandler
    }
}
