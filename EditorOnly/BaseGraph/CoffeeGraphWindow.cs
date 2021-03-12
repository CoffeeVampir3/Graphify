﻿using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
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
            graphView.parentWindow = this;
            rootVisualElement.Add(graphView);
            graphView.RegisterCallback<GeometryChangedEvent>(OnGeometryChangedInitialization);
            graphView.RegisterCallback<DetachFromPanelEvent>(graphView.OnGraphClosed);
        }

        private void OnGeometryChangedInitialization(GeometryChangedEvent e)
        {
            GenerateToolbar();
            graphView.OnGraphLoaded();
            graphView.UnregisterCallback<GeometryChangedEvent>(OnGeometryChangedInitialization);
        }

        private void OnDisable()
        {
            rootVisualElement.Clear();
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

        public void VisitRuntimeNode(RuntimeNode node)
        {
            graphView?.RuntimeNodeVisited(node);
        }
        
        public void ExitRuntimeNode(RuntimeNode node)
        {
            graphView?.RuntimeNodeExited(node);
        }

        private const string debugSavePath = "Assets/!TestTrashbin/wombograph.asset";
        /// <summary>
        /// Creates a new EditorGraphModel and Graph Controller.
        /// </summary>
        public virtual void CreateNewGraph()
        {
            var registeredGraphControllerType = graphView.GetRegisteredGraphController();
            var model = EditorGraphModel.CreateNew(debugSavePath, GetType(),
                registeredGraphControllerType);

            if (model == null)
                return;
            
            serializedGraphSelector.SetValueWithoutNotify(model);
            graphView.LoadGraph(model);
        }

        private void OnObjectSelectorValueChanged(ChangeEvent<Object> evt)
        {
            if (evt.newValue == null)
            {
                graphView.UnloadGraph();
                return;
            }
            
            if (!(evt.newValue is GraphController gc)) return;
            
            var graphGuid = gc.AssetGuid;
            var editorGraph = AssetHelper.FindAssetWithGUID<EditorGraphModel>(graphGuid);
            if (editorGraph == null)
            {
                Debug.LogError("Was unable to find a matching editor graph for graph controller named: " + gc.name);
                return;
            }
            
            graphView.LoadGraph(editorGraph);
        }
        
        private ObjectField serializedGraphSelector = null;
        private void GenerateToolbar()
        {
            var toolbar = new Toolbar();
            
            serializedGraphSelector = new ObjectField {objectType = typeof(GraphController)};
            serializedGraphSelector.RegisterValueChangedCallback(OnObjectSelectorValueChanged);
            
            toolbar.Add(new Button( CreateNewGraph ) 
                {text = "Create New Graph"});
            toolbar.Add(new Button( Debug__FoldoutAllItems ) 
                {text = "Foldout all Items"});
            toolbar.Add(new Button( Debug__ExpandAllItems ) 
                {text = "Expand all Items"});

            toolbar.Add(serializedGraphSelector);
            graphView.Add(toolbar);
        }
    }
    
}
