using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace GraphFramework.Editor
{
    public class CoffeeGraphWindow : EditorWindow
    {
        [SerializeReference]
        protected CoffeeGraphView graphView;
        private bool isWindowLoaded = false;
        private Action OnWindowLayoutFinished = null;
        private string domainSafeWorkingAssetPath;

        [MenuItem("Graphs/Test Graph")]
        public static void OpenGraph()
        {
            var window = GetWindow<CoffeeGraphWindow>();
            window.titleContent = new GUIContent("C0ff33");
            
            window.Focus();
        }
        
        /// <summary>
        /// Loads the provided graph model to this window.
        /// </summary>
        protected internal virtual void LoadGraphExternal(GraphModel model)
        {
            //We have to wait until the window layout is created before we actually try
            //to load the graph, if the window isint loaded we use this delay call.
            OnWindowLayoutFinished = () =>
            {
                serializedGraphSelector.SetValueWithoutNotify(model.serializedGraphController);
                graphView.LoadGraph(model);
            };
            
            if (!isWindowLoaded) return;
            //If it turns out the window is already loaded, just load the graph.
            OnWindowLayoutFinished();
            OnWindowLayoutFinished = null;
        }
        
        private void FoldoutAllItems()
        {
            foreach (var node in graphView.nodes.ToList())
            {
                node.expanded = false;
                node.RefreshPorts();
                node.RefreshExpandedState();
            }
        }

        private void ExpandAllItems()
        {
            foreach (var node in graphView.nodes.ToList())
            {
                node.expanded = true;
                node.RefreshPorts();
                node.RefreshExpandedState();
            }
        }
        
        #region Linking API

        public void ResetGraph(int graphId)
        {
            graphView?.ResetVirtualGraph(graphId);
        }
        
        public void VisitRuntimeNode(RuntimeNode node)
        {
            graphView?.RuntimeNodeVisited(node);
        }
        
        public void ExitRuntimeNode(RuntimeNode node)
        {
            graphView?.RuntimeNodeExited(node);
        }
        
        #endregion
        
        #region Initialization and Finalization

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

                //Check if we were working on something before the domain reloaded.
                if (string.IsNullOrEmpty(domainSafeWorkingAssetPath))
                    return;
                
                LoadGraphControllerInternal(domainSafeWorkingAssetPath);
            };
        }

        private void InitializeView()
        {
            //Unloads the graph before the assembly reloads.
            //This is important, otherwise unity editor will soft-lock, presumably due to
            //graph view no longer existing but still being referenced.
            AssemblyReloadEvents.beforeAssemblyReload += () =>
            {
                isWindowLoaded = false;
                graphView = null;
            };
            
            graphView.StretchToParentSize();
            graphView.parentWindow = this;
            rootVisualElement.Add(graphView);
            graphView.RegisterCallback<GeometryChangedEvent>(OnGeometryChangedInitialization);
            graphView.RegisterCallback<DetachFromPanelEvent>(graphView.OnGraphClosed);
        }
        
        private void EnableGraphView()
        {
            graphView = new CoffeeGraphView
            {
                name = "Coffee Dialogue Graph"
            };
            InitializeView();
        }

        private void OnGeometryChangedInitialization(GeometryChangedEvent e)
        {
            GenerateToolbar();
            graphView.OnGraphGUIInitialized();
            if (OnWindowLayoutFinished != null)
            {
                OnWindowLayoutFinished();
                OnWindowLayoutFinished = null;
            }
            isWindowLoaded = true;
            graphView.UnregisterCallback<GeometryChangedEvent>(OnGeometryChangedInitialization);
        }
        
        #endregion

        /// <summary>
        /// Loads a graph controller via it's asset path, this gives us a persistent way to access
        /// it after a domain reload.
        /// </summary>
        protected internal virtual void LoadGraphControllerInternal(string gcPath)
        {
            OnWindowLayoutFinished = () =>
            {
                var gc = AssetDatabase.LoadAssetAtPath<GraphController>(gcPath);

                if (gc == null)
                    return;
                
                var editorGraph = AssetHelper.FindNestedAssetOfType<GraphModel>(gc);
                if (editorGraph == null)
                {
                    editorGraph = GraphModel.BootstrapController(gc);
                }

                serializedGraphSelector.SetValueWithoutNotify(gc);
                graphView.LoadGraph(editorGraph);
            };
            
            if (!isWindowLoaded) return;
            //If it turns out the window is already loaded, just load the graph.
            OnWindowLayoutFinished();
            OnWindowLayoutFinished = null;
        }

        #region Toolbar

        private void OnObjectSelectorValueChanged(ChangeEvent<Object> evt)
        {
            if (evt.newValue == null)
            {
                domainSafeWorkingAssetPath = null;
                graphView.UnloadGraph();
                return;
            }
            
            var gc = serializedGraphSelector.value as GraphController;
            var path = AssetDatabase.GetAssetPath(gc);
            domainSafeWorkingAssetPath = string.IsNullOrEmpty(path) ? null : path;

            LoadGraphControllerInternal(path);
        }
        
        private ObjectField serializedGraphSelector = null;
        private void GenerateToolbar()
        {
            var toolbar = new Toolbar();
            
            serializedGraphSelector = new ObjectField {objectType = typeof(GraphController)};
            serializedGraphSelector.RegisterValueChangedCallback(OnObjectSelectorValueChanged);
            
            toolbar.Add(new Button( FoldoutAllItems ) 
                {text = "Foldout all Items"});
            toolbar.Add(new Button( ExpandAllItems ) 
                {text = "Expand all Items"});

            toolbar.Add(serializedGraphSelector);
            graphView.Add(toolbar);
        }
        
        #endregion
    }
    
}
