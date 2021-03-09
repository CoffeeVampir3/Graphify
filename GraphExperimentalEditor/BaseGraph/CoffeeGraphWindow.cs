using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using VisualNovelFramework.EditorExtensions;
using Object = UnityEngine.Object;

namespace GraphFramework.Editor
{
    public abstract class CoffeeGraphWindow : EditorWindow
    {
        [SerializeReference]
        protected CoffeeGraphView graphView;
        [SerializeReference]
        public string currentGraphGUID;
        
        protected void InitializeGraph()
        {
            if (string.IsNullOrWhiteSpace(currentGraphGUID))
            {
                currentGraphGUID = Guid.NewGuid().ToString();
            }

            //Unloads the graph before the assembly reloads.
            //This is important, otherwise unity editor will soft-lock, presumably due to
            //graph view no longer existing but still being referenced.
            AssemblyReloadEvents.beforeAssemblyReload += () =>
            {
                graphView = null;
            };
            
            graphView.StretchToParentSize();
            rootVisualElement.Add(graphView);
            graphView.RegisterCallback<GeometryChangedEvent>(OnGeometryChangedInitialization);
        }

        private void OnGeometryChangedInitialization(GeometryChangedEvent e)
        {
            GenerateToolbar();
            graphView.OnCreateGraphGUI();
            graphView.UnregisterCallback<GeometryChangedEvent>(OnGeometryChangedInitialization);
        }

        private void OnDisable()
        {
            rootVisualElement.Clear();
        }

        protected void SaveGraph()
        {
            //GraphSaver.SerializeGraph(graphView, currentGraphGUID, this.GetType());
        }

        //TODO:: oof. Maybe this is neccesary? The order of initialization
        //does not appear to be fixed, so we need to account for both cases?
        //Loading the toolbar from XML from file would be a solution here.
        private SerializedGraph delayedLoadedGraph = null;
        public void LoadGraph(SerializedGraph graph)
        {
            return;
            //var editorGraph = GraphLoader.LoadGraph(graphView, graph);
            //if (editorGraph == null) 
                //return;
            
            if (serializedGraphSelector != null)
            {
                serializedGraphSelector.SetValueWithoutNotify(graph);
                currentGraphGUID = graph.GetCoffeeGUID();
            }
            else
            {
                delayedLoadedGraph = graph;
            }
        }

        private void LoadGraphEvent(ChangeEvent<Object> evt)
        {
            var graph = evt.newValue as SerializedGraph;

            if (graph == null)
            {
                return;
            }
            LoadGraph(graph);
        }

        protected void RevertGraphToVersionOnDisk()
        {
            if (serializedGraphSelector == null)
                return;

            var currentGraph = serializedGraphSelector.value as SerializedGraph;
            if (currentGraph == null) 
                return;
        }

        /// <summary>
        /// TODO:: Test Code:
        /// </summary>
        protected void DuplicateGraph()
        {
            var currentGraph = serializedGraphSelector.value as SerializedGraph;
            if (currentGraph == null) 
                return;

            var p = AssetDatabase.GetAssetPath(currentGraph);
            var nP = p.Replace(".asset", "");
            nP += "2.asset";
            AssetDatabase.CopyAsset(p, nP);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(nP);

            var q= AssetDatabase.LoadAllAssetsAtPath(nP);

            foreach (var k in q)
            {
                if (k is HasCoffeeGUID cguid)
                {
                    cguid.SetCoffeeGUID(Guid.NewGuid().ToString());
                }
            }
        }

        private void Debug__FoldoutAllItems()
        {
            foreach (var node in graphView.nodes.ToList())
            {
                node.expanded = false;
                node.RefreshPorts();
                node.RefreshExpandedState();
            }
        }

        private void Debug__ExpandAllItems()
        {
            foreach (var node in graphView.nodes.ToList())
            {
                node.expanded = true;
                node.RefreshPorts();
                node.RefreshExpandedState();
            }
        }
        
        private ObjectField serializedGraphSelector = null;
        private void GenerateToolbar()
        {
            var toolbar = new Toolbar();
            
            serializedGraphSelector = new ObjectField {objectType = typeof(SerializedGraph)};
            serializedGraphSelector.RegisterValueChangedCallback(LoadGraphEvent);

            if (delayedLoadedGraph != null)
            {
                serializedGraphSelector.SetValueWithoutNotify(delayedLoadedGraph);
                currentGraphGUID = delayedLoadedGraph.GetCoffeeGUID();
                delayedLoadedGraph = null;
            }
            
            toolbar.Add(new Button( SaveGraph ) 
                {text = "Save"});
            toolbar.Add(new Button( DuplicateGraph ) 
                {text = "Duplicate Test"});
            toolbar.Add(new Button( RevertGraphToVersionOnDisk ) 
                {text = "Boop"});
            toolbar.Add(new Button( Debug__FoldoutAllItems ) 
                {text = "Foldout all Items"});
            toolbar.Add(new Button( Debug__ExpandAllItems ) 
                {text = "Expand all Items"});

            toolbar.Add(serializedGraphSelector);
            graphView.Add(toolbar);
        }
    }
    
}
