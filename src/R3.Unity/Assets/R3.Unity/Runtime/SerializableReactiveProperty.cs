using System;
using UnityEngine;
using System.Reflection;
using System.Text.RegularExpressions;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace R3
{
    [Serializable]
    public class SerializableReactiveProperty<T> : ReactiveProperty<T>, ISerializationCallbackReceiver
    {
        [SerializeField]
        T value;

        public SerializableReactiveProperty()
            : base(default!)
        {
        }

        public SerializableReactiveProperty(T value)
            : base(value)
        {
        }

        protected override void OnValueChanged(T value)
        {
            this.value = value;
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            GetValueRef() = this.value; // force set
        }
    }

#if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(SerializableReactiveProperty<>))]
    internal class SerializableReactivePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var p = property.FindPropertyRelative("value");

            EditorGUI.BeginChangeCheck();

            if (p.propertyType == SerializedPropertyType.Quaternion)
            {
                label.text += "(EulerAngles)";
                EditorGUI.PropertyField(position, p, label, true);
            }
            else
            {
                EditorGUI.PropertyField(position, p, label, true);
            }

            if (EditorGUI.EndChangeCheck())
            {
                var paths = property.propertyPath.Split('.'); // X.Y.Z...
                var attachedComponent = property.serializedObject.targetObject;

                var targetProp = GetValueRecursive(attachedComponent, 0, paths);
                if (targetProp == null) return;

                property.serializedObject.ApplyModifiedProperties(); // deserialize to field
                var methodInfo = targetProp.GetType().GetMethod("ForceNotify", BindingFlags.IgnoreCase | BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (methodInfo != null)
                {
                    methodInfo.Invoke(targetProp, Array.Empty<object>());
                }
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var p = property.FindPropertyRelative("value");
            if (p.propertyType == SerializedPropertyType.Quaternion)
            {
                // Quaternion is Vector3(EulerAngles)
                return EditorGUI.GetPropertyHeight(SerializedPropertyType.Vector3, label);
            }
            else
            {
                return EditorGUI.GetPropertyHeight(p);
            }
        }

        object GetValueRecursive(object obj, int index, string[] paths)
        {
            var path = paths[index];

            FieldInfo fldInfo = null;
            var type = obj.GetType();
            while (fldInfo == null)
            {
                // attempt to get information about the field
                fldInfo = type.GetField(path, BindingFlags.IgnoreCase | BindingFlags.GetField | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                if (fldInfo != null ||
                    type.BaseType == null ||
                    type.BaseType.IsSubclassOf(typeof(ReactiveProperty<>))) break;

                // if the field information is missing, it may be in the base class
                type = type.BaseType;
            }

            // If array, path = Array.data[index]
            if (fldInfo == null && path == "Array")
            {
                try
                {
                    path = paths[++index];
                    var m = Regex.Match(path, @"(.+)\[([0-9]+)*\]");
                    var arrayIndex = int.Parse(m.Groups[2].Value);
                    var arrayValue = (obj as System.Collections.IList)[arrayIndex];
                    if (index < paths.Length - 1)
                    {
                        return GetValueRecursive(arrayValue, ++index, paths);
                    }
                    else
                    {
                        return arrayValue;
                    }
                }
                catch
                {
                    Debug.Log("SerializableReactivePropertyDrawer Exception, objType:" + obj.GetType().Name + " path:" + string.Join(", ", paths));
                    throw;
                }
            }
            else if (fldInfo == null)
            {
                throw new Exception("Can't decode path:" + string.Join(", ", paths));
            }

            var v = fldInfo.GetValue(obj);
            if (index < paths.Length - 1)
            {
                return GetValueRecursive(v, ++index, paths);
            }

            return v;
        }
    }

#endif
}
