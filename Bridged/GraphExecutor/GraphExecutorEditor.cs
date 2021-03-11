#if UNITY_EDITOR
using System;
using System.Linq;
using System.Reflection;
using GraphFramework.Editor;
using UnityEditor;
using UnityEngine;

namespace GraphFramework.GraphExecutor
{
    public partial class GraphExecutor
    {
        private EditorWindow linkedEditorWindow;
        private BetaEditorGraph editorGraphData;
        
        //We need this reflection because unity does not have a typed version of
        //HasOpenInstances so we need make generic.
        private static readonly MethodInfo hasOpenInstancesMethod = 
            typeof(EditorWindow).GetMethod(nameof(EditorWindow.HasOpenInstances), 
                BindingFlags.Static | BindingFlags.Public);
        
        private MethodInfo genericHasInstancesOpen = null;
        private bool IsEditorWindowOpen(Type t)
        {
            if (genericHasInstancesOpen != null) 
                return (bool)genericHasInstancesOpen.Invoke(null, null);
            
            try
            {
                genericHasInstancesOpen = hasOpenInstancesMethod.MakeGenericMethod(t);
            }
            catch
            {
                Debug.Log("Failed to created generic method for EditorWindow.HasOpenInstances for type: " + t.Name);
                return false;
            }

            return (bool)genericHasInstancesOpen.Invoke(null, null);
        }
        /// <summary>
        /// This is run every call and not cached because linking can be volatile,
        /// the window state can change at any moment and we cannot assume it's open.
        /// </summary>
        public bool IsEditorLinkedToGraphWindow()
        {
            //Short circuit if we're already linked.
            if (editorGraphData != null && linkedEditorWindow != null)
            {
                if (IsEditorWindowOpen(editorGraphData.graphWindowType.type)) 
                    return true;
                
                linkedEditorWindow = null;
                return false;
            }
            
            if (editorGraphData == null)
            {
                editorGraphData = AssetExtensions.FindAssetsOfType<BetaEditorGraph>().FirstOrDefault();
            }

            if (editorGraphData == null || 
                !IsEditorWindowOpen(editorGraphData.graphWindowType.type))
            {
                return false;
            }

            linkedEditorWindow = 
                    EditorWindow.GetWindow(editorGraphData.graphWindowType.type);
            
            return editorGraphData != null && linkedEditorWindow != null;
        }
        
        public void EditorLinkedRuntimeNodeVisited(RuntimeNode node)
        {
            if (!(linkedEditorWindow is CoffeeGraphWindow cg)) return;
            cg.VisitRuntimeNode(node);
        }

        public void EditorLinkedRuntimeNodeExited(RuntimeNode node)
        {
            if (!(linkedEditorWindow is CoffeeGraphWindow cg)) return;
            cg.ExitRuntimeNode(node);
        }
    }
}

#endif