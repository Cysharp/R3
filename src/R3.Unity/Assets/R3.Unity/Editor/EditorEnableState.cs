using UnityEditor;

namespace R3.Unity.Editor
{
    public static class EditorEnableState
    {
        const string EnableAutoReloadKey = "ObservableTrackerWindow_EnableAutoReloadKey";
        const string EnableTrackingKey = "ObservableTrackerWindow_EnableTrackingKey";
        const string EnableStackTraceKey = "ObservableTrackerWindow_EnableStackTraceKey";

        public static bool EnableAutoReload
        {
            get
            {
                return EditorPrefs.GetBool(EnableAutoReloadKey, false);
            }
            set
            {
                UnityEditor.EditorPrefs.SetBool(EnableAutoReloadKey, value);
            }
        }

        public static bool EnableTracking
        {
            get
            {
                return UnityEditor.EditorPrefs.GetBool(EnableTrackingKey, false);
            }
            set
            {
                UnityEditor.EditorPrefs.SetBool(EnableTrackingKey, value);
            }
        }

        public static bool EnableStackTrace
        {
            get
            {
                return UnityEditor.EditorPrefs.GetBool(EnableStackTraceKey, false);
            }
            set
            {
                UnityEditor.EditorPrefs.SetBool(EnableStackTraceKey, value);
            }
        }
    }
}
