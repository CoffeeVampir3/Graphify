using UnityEditor;
using UnityEngine;

namespace GraphFramework.Editor
{
    public class TestGraphWindow : CoffeeGraphWindow
    {
        [MenuItem("VNFramework/Test Graph")]
        public static void OpenGraph()
        {
            var window = GetWindow<TestGraphWindow>();
            window.titleContent = new GUIContent("C0ff33");
            
            window.Focus();
        }

        private void EnableGraphView()
        {
            graphView = new TestGraphView
            {
                name = "Coffee Dialogue Graph"
            };
            InitializeGraph();
        }
        
        private void OnEnable()
        {
            if (graphView != null)
            {
                return;
            }

            EnableGraphView();
            //Reloads the graph after the assembly reloads.
            AssemblyReloadEvents.afterAssemblyReload += () =>
            {
                rootVisualElement.Clear();
                EnableGraphView();
            };
        }
    }
}