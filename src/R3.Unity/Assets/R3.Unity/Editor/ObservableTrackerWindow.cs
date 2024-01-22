#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System;
using UnityEditor.IMGUI.Controls;

namespace R3.Unity.Editor
{
    public class ObservableTrackerWindow : EditorWindow
    {
        static int interval;

        static ObservableTrackerWindow window;

        [MenuItem("Window/Observable Tracker")]
        public static void OpenWindow()
        {
            if (window != null)
            {
                window.Close();
            }

            // will called OnEnable(singleton instance will be set).
            GetWindow<ObservableTrackerWindow>("Observable Tracker").Show();
        }

        static readonly GUILayoutOption[] EmptyLayoutOption = new GUILayoutOption[0];

        ObservableTrackerTreeView treeView;
        object splitterState;

        // state on window
        static bool enableAutoReload;
        static bool enableTracking;
        static bool enableStackTrace;

        void OnEnable()
        {
            window = this; // set singleton.
            splitterState = SplitterGUILayout.CreateSplitterState(new float[] { 75f, 25f }, new int[] { 32, 32 }, null);
            treeView = new ObservableTrackerTreeView();

            // restore settings from EditorPrefs.
            enableAutoReload = EditorEnableState.EnableAutoReload;
            enableTracking = EditorEnableState.EnableTracking;
            enableStackTrace = EditorEnableState.EnableStackTrace;
        }

        void OnGUI()
        {
            // Head
            RenderHeadPanel();

            // Splittable
            SplitterGUILayout.BeginVerticalSplit(this.splitterState, EmptyLayoutOption);
            {
                // Column Tabble
                RenderTable();

                // StackTrace details
                RenderDetailsPanel();
            }
            SplitterGUILayout.EndVerticalSplit();
        }

        #region HeadPanel

        static readonly GUIContent EnableAutoReloadHeadContent = EditorGUIUtility.TrTextContent("Enable AutoReload", "Reload automatically.", (Texture)null);
        static readonly GUIContent ReloadHeadContent = EditorGUIUtility.TrTextContent("Reload", "Reload View.", (Texture)null);
        static readonly GUIContent GCHeadContent = EditorGUIUtility.TrTextContent("GC.Collect", "Invoke GC.Collect.", (Texture)null);
        static readonly GUIContent EnableTrackingHeadContent = EditorGUIUtility.TrTextContent("Enable Tracking", "Start to track Observable subscription. Performance impact: low", (Texture)null);
        static readonly GUIContent EnableStackTraceHeadContent = EditorGUIUtility.TrTextContent("Enable StackTrace", "Capture StackTrace when subscribed. Performance impact: high", (Texture)null);

        // [Enable Tracking] | [Enable StackTrace]
        void RenderHeadPanel()
        {
            EditorGUILayout.BeginVertical(EmptyLayoutOption);
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, EmptyLayoutOption);

            if (GUILayout.Toggle(enableAutoReload, EnableAutoReloadHeadContent, EditorStyles.toolbarButton, EmptyLayoutOption) != enableAutoReload)
            {
                EditorEnableState.EnableAutoReload = enableAutoReload = !enableAutoReload;
            }

            if (GUILayout.Toggle(enableTracking, EnableTrackingHeadContent, EditorStyles.toolbarButton, EmptyLayoutOption) != enableTracking)
            {
                EditorEnableState.EnableTracking = enableTracking = !enableTracking;
            }

            if (GUILayout.Toggle(enableStackTrace, EnableStackTraceHeadContent, EditorStyles.toolbarButton, EmptyLayoutOption) != enableStackTrace)
            {
                EditorEnableState.EnableStackTrace = enableStackTrace = !enableStackTrace;
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button(ReloadHeadContent, EditorStyles.toolbarButton, EmptyLayoutOption))
            {
                ObservableTracker.CheckAndResetDirty();
                treeView.ReloadAndSort();
                Repaint();
            }

            if (GUILayout.Button(GCHeadContent, EditorStyles.toolbarButton, EmptyLayoutOption))
            {
                GC.Collect(0);
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        #endregion

        #region TableColumn

        Vector2 tableScroll;
        GUIStyle tableListStyle;

        void RenderTable()
        {
            if (tableListStyle == null)
            {
                tableListStyle = new GUIStyle("CN Box");
                tableListStyle.margin.top = 0;
                tableListStyle.padding.left = 3;
            }

            EditorGUILayout.BeginVertical(tableListStyle, EmptyLayoutOption);

            this.tableScroll = EditorGUILayout.BeginScrollView(this.tableScroll, new GUILayoutOption[]
            {
                GUILayout.ExpandWidth(true),
                GUILayout.MaxWidth(2000f)
            });
            var controlRect = EditorGUILayout.GetControlRect(new GUILayoutOption[]
            {
                GUILayout.ExpandHeight(true),
                GUILayout.ExpandWidth(true)
            });


            treeView?.OnGUI(controlRect);

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void Update()
        {
            // reflect to ObservableTracker
            ObservableTracker.EnableTracking = enableTracking;
            ObservableTracker.EnableStackTrace = enableStackTrace;

            //var count = 0;
            //ObservableTracker.ForEachActiveTask(_ => count++);
            //Debug.Log(count);

            if (enableAutoReload)
            {
                if (interval++ % 120 == 0)
                {
                    if (ObservableTracker.CheckAndResetDirty())
                    {
                        treeView.ReloadAndSort();
                        Repaint();
                    }
                }
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        public static void SetObservableTrackerValues()
        {
            ObservableTracker.EnableTracking = EditorEnableState.EnableTracking;
            ObservableTracker.EnableStackTrace = EditorEnableState.EnableStackTrace;
        }

        #endregion

        #region Details

        static GUIStyle detailsStyle;
        Vector2 detailsScroll;

        void RenderDetailsPanel()
        {
            if (detailsStyle == null)
            {
                detailsStyle = new GUIStyle("CN Message");
                detailsStyle.wordWrap = false;
                detailsStyle.stretchHeight = true;
                detailsStyle.margin.right = 15;
            }

            string message = "";
            var selected = treeView.state.selectedIDs;
            if (selected.Count > 0)
            {
                var first = selected[0];
                var item = treeView.CurrentBindingItems.FirstOrDefault(x => x.id == first) as ObservableTrackerViewItem;
                if (item != null)
                {
                    message = item.Location;
                }
            }

            detailsScroll = EditorGUILayout.BeginScrollView(this.detailsScroll, EmptyLayoutOption);
            var vector = detailsStyle.CalcSize(new GUIContent(message));
            EditorGUILayout.SelectableLabel(message, detailsStyle, new GUILayoutOption[]
            {
                GUILayout.ExpandHeight(true),
                GUILayout.ExpandWidth(true),
                GUILayout.MinWidth(vector.x),
                GUILayout.MinHeight(vector.y)
            });
            EditorGUILayout.EndScrollView();
        }

        #endregion
    }
}

