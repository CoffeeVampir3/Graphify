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
        private EditorGraphModel editorGraphModelData;
        
        //We need this reflection because unity does not have a typed version of
        //HasOpenInstances so we need make generic.
        private static readonly MethodInfo hasOpenInstancesMethod = 
            typeof(EditorWindow).GetMethod(nameof(EditorWindow.HasOpenInstances), 
                BindingFlags.Static | BindingFlags.Public);
        
        private MethodInfo genericHasInstancesOpen = null;
        //Unity didin't provide a function to do this, so we make a generic delegate for our
        //window type's HasOpenInstances -- then we cache that delegate.
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
        
        // This is run every call and not cached because linking can be volatile,
        // the window state can change at any moment and we cannot assume it's open.
       /// <summary>
       /// Checks to see if the editor window for out particular graph type is open.
       /// </summary>
       /// <returns>True if graph is open</returns>
        public bool IsEditorLinkedToGraphWindow()
        {
            //Short circuit if we're already linked.
            if (editorGraphModelData != null && linkedEditorWindow != null)
            {
                if (IsEditorWindowOpen(editorGraphModelData.graphWindowType.type)) 
                    return true;
                
                linkedEditorWindow = null;
                return false;
            }
            
            if (editorGraphModelData == null)
            {
                editorGraphModelData = AssetExtensions.FindAssetsOfType<EditorGraphModel>().FirstOrDefault();
            }

            if (editorGraphModelData == null || 
                !IsEditorWindowOpen(editorGraphModelData.graphWindowType.type))
            {
                return false;
            }

            linkedEditorWindow = 
                    EditorWindow.GetWindow(editorGraphModelData.graphWindowType.type);
            
            return editorGraphModelData != null && linkedEditorWindow != null;
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