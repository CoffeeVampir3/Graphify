#if UNITY_EDITOR
using System;
using System.Linq;
using System.Reflection;
using GraphFramework.Editor;
using UnityEditor;
using UnityEngine;

namespace GraphFramework
{
    public partial class GraphExecutor
    {
        private EditorWindow linkedEditorWindow;
        private GraphModel graphModelData;
        
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
            if (graphModelData != null && linkedEditorWindow != null)
            {
                if (IsEditorWindowOpen(graphModelData.graphWindowType.type)) 
                    return true;
                
                linkedEditorWindow = null;
                return false;
            }
            
            if (graphModelData == null)
            {
                graphModelData = AssetHelper.FindAssetsOf<GraphModel>().FirstOrDefault();
            }

            if (graphModelData == null || 
                !IsEditorWindowOpen(graphModelData.graphWindowType.type))
            {
                return false;
            }

            linkedEditorWindow = 
                    EditorWindow.GetWindow(graphModelData.graphWindowType.type);
            
            return graphModelData != null && linkedEditorWindow != null;
        }

       public void EditorLinkedResetGraph(int graphId)
       {
           if (!(linkedEditorWindow is GraphfyWindow cg)) return;
           cg.ResetGraph(graphId);
       }
        
        public void EditorLinkedRuntimeNodeVisited(RuntimeNode node)
        {
            if (!(linkedEditorWindow is GraphfyWindow cg)) return;
            cg.VisitRuntimeNode(node);
        }

        public void EditorLinkedRuntimeNodeExited(RuntimeNode node)
        {
            if (!(linkedEditorWindow is GraphfyWindow cg)) return;
            cg.ExitRuntimeNode(node);
        }
    }
}

#endif