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
        [SerializeReference]
        public string currentGraphGUID;
        private bool isWindowLoaded = false;
        private Action OnWindowLayoutFinished = null;
        
        [MenuItem("Graphs/Test Graph")]
        public static void OpenGraph()
        {
            var window = GetWindow<CoffeeGraphWindow>();
            window.titleContent = new GUIContent("C0ff33");
            
            window.Focus();
        }
        
        //TODO:: Debug features.
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
        
        #region Public API
        
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
            };
        }
        
        private void OnDisable()
        {
            rootVisualElement.Clear();
        }
        
        private void InitializeGraph()
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
        
        private void EnableGraphView()
        {
            graphView = new CoffeeGraphView
            {
                name = "Coffee Dialogue Graph"
            };
            InitializeGraph();
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

        protected internal virtual void LoadGraphExternal(GraphModel model)
        {
            //We have to wait until the window layout is created before we actually try
            //to load the graph, if the window isint loaded we use this delay call.
            OnWindowLayoutFinished = () =>
            {
                serializedGraphSelector.SetValueWithoutNotify(model);
                graphView.LoadGraph(model);
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
                graphView.UnloadGraph();
                return;
            }
            
            if (!(evt.newValue is GraphController gc)) return;
            
            var graphGuid = gc.AssetGuid;
            var editorGraph = AssetHelper.FindAssetWithGUID<GraphModel>(graphGuid);
            if (editorGraph == null)
            {
                editorGraph = GraphModel.BootstrapController(gc);
            }
            
            graphView.LoadGraph(editorGraph);
        }
        
        private ObjectField serializedGraphSelector = null;
        private void GenerateToolbar()
        {
            var toolbar = new Toolbar();
            
            serializedGraphSelector = new ObjectField {objectType = typeof(GraphController)};
            serializedGraphSelector.RegisterValueChangedCallback(OnObjectSelectorValueChanged);
            
            toolbar.Add(new Button( Debug__FoldoutAllItems ) 
                {text = "Foldout all Items"});
            toolbar.Add(new Button( Debug__ExpandAllItems ) 
                {text = "Expand all Items"});

            toolbar.Add(serializedGraphSelector);
            graphView.Add(toolbar);
        }
        
        #endregion
    }
    
}
