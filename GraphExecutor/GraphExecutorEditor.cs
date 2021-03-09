#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

/*
namespace VisualNovelFramework.GraphFramework.GraphExecutor
{
    public partial class GraphExecutor
    {
        private EditorWindow linkedEditorWindow;
        
        //We need this reflection because unity does not have a typed version of
        //HasOpenInstances so we need make generic.
        static readonly MethodInfo hasOpenInstancesMethod = 
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
        /// the state can change at any moment.
        /// </summary>
        public bool IsEditorLinkedToGraphWindow()
        {
            //Short circuit if we're already linked.
            if (editorGraphData != null && linkedEditorWindow != null)
            {
                if (IsEditorWindowOpen(editorGraphData.editorWindowType.type)) 
                    return true;
                
                linkedEditorWindow = null;
                return false;
            }
            
            if (editorGraphData == null)
            {
                editorGraphData = CoffeeAssetDatabase.
                    FindAssetWithCoffeeGUID<EditorGraphData>(targetGraph.GetCoffeeGUID());
            }

            if (!IsEditorWindowOpen(editorGraphData.editorWindowType.type))
            {
                return false;
            }

            linkedEditorWindow = 
                    EditorWindow.GetWindow(editorGraphData.editorWindowType.type);
            
            return editorGraphData != null && linkedEditorWindow != null;
        }
        
        public void EditorLinkedRuntimeNodeVisited(RuntimeNode node)
        {
            if (linkedEditorWindow is CoffeeGraphWindow cg)
            {
                //cg.RuntimeNodeVisited(node);
                Debug.Log("Visited node: " + node.name);
            }
        }
    }
}
*/

#endif