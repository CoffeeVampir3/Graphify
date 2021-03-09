using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace GraphFramework.Editor
{
    public abstract class CoffeeGraphView : GraphView
    {
        protected internal CoffeeGraphWindow parentWindow;
        protected readonly GraphSettings settings;
        protected readonly CoffeeSearchWindow searchWindow;
        protected BetaEditorGraph editorGraph;

        //Keeps track of all NodeView's and their relation to their model.
        protected internal readonly Dictionary<MovableView, MovableModel> viewToModel =
            new Dictionary<MovableView, MovableModel>();
        //Keeps track of all edges and their relation to their model.
        protected readonly Dictionary<Edge, EdgeModel> edgeToModel =
            new Dictionary<Edge, EdgeModel>();

        /// <summary>
        /// Called once the graph view is resized to the editor window and all geometry has been
        /// calculated. (Internally, this is called after a GeometryChangedEvent)
        /// </summary>
        protected internal abstract void OnCreateGraphGUI();
        
        public void CreateNewNode(Type runtimeDataType, Vector2 atPosition)
        {
            var model = NodeModel.InstantiateModel(editorGraph, runtimeDataType);
            
            Vector2 spawnPosition = parentWindow.rootVisualElement.ChangeCoordinatesTo(
                parentWindow.rootVisualElement.parent,
                atPosition - parentWindow.position.position);

            spawnPosition = contentViewContainer.WorldToLocal(spawnPosition);
            Rect spawnRect = new Rect(spawnPosition.x - 75, spawnPosition.y - 75, 150, 150);
            model.Position = spawnRect;
            CreateNewNode(model);
        }

        //TODO::

        #region DeleteThis

        /// TODO::
        private void DEBUG__LOAD_GRAPH()
        {
            editorGraph = AssetExtensions.FindAssetsOfType<BetaEditorGraph>().FirstOrDefault();
            Undo.ClearAll();
            BuildGraph();
        }

        #endregion
        
        protected CoffeeGraphView()
        {
            settings = GraphSettings.CreateOrGetSettings(this);
            styleSheets.Add(settings.graphViewStyle);

            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            //These callbacks are derived from graphView.
            //Callback on cut/copy.
            serializeGraphElements = OnSerializeGraphElements;
            //Callback on paste.
            unserializeAndPaste = OnPaste;
            //Callback on "changes" particularly on element delete.

            Undo.undoRedoPerformed += UndoPerformed;

            searchWindow = ScriptableObject.CreateInstance<CoffeeSearchWindow>();
            InitializeSearchWindow();

            DEBUG__LOAD_GRAPH();

            graphViewChanged = OnGraphViewChanged;
        }
        
        #region Copy and Paste

        [Serializable]
        protected class CopyAndPasteBox
        {
            [SerializeReference]
            public List<string> viewGuids = new List<string>();
            [SerializeReference]
            public List<string> edgeGuids = new List<string>();
        }

        protected virtual string OnSerializeGraphElements(IEnumerable<GraphElement> selectedItemsToSerialize)
        {
            CopyAndPasteBox box = new CopyAndPasteBox();
            foreach (var elem in selectedItemsToSerialize)
            {
                switch (elem)
                {
                case NodeView view:
                    box.viewGuids.Add(view.viewDataKey);
                    break;
                case Edge edge:
                    box.edgeGuids.Add(edge.viewDataKey);
                    break;
                }
            }

            return JsonUtility.ToJson(box);
        }
        
        protected virtual void OnPaste(string op, string serializationData)
        {
            CopyAndPasteBox box = JsonUtility.FromJson<CopyAndPasteBox>(serializationData);
            if (box == null)
                return;

            ClearSelection();
            var oldModelToCopiedModel = new Dictionary<NodeModel, NodeModel>();
            foreach (var viewGuid in box.viewGuids)
            {
                //Create a new copy
                if (!(GetElementByGuid(viewGuid) is NodeView nv)) continue;
                var model = viewToModel[nv] as NodeModel;
                //It is not possible this is null as we looked up using a node view and will get a node model.
                // ReSharper disable once PossibleNullReferenceException
                var clone = model.Clone(editorGraph);
                oldModelToCopiedModel.Add(model, clone);
                CreateNewNode(clone);
                AddToSelection(clone.View);
            }
            
            foreach (var edgeGuid in box.edgeGuids)
            {
                Edge originalEdge = GetEdgeByGuid(edgeGuid);
                ResolveEdge(originalEdge,
                    out var inModel, out var outModel,
                    out var inPort, out var outPort);
                
                NodeModel targetInModel;
                NodeModel targetOutModel;
                PortModel targetInPort;
                PortModel targetOutPort;
                
                if (!oldModelToCopiedModel.TryGetValue(inModel, out targetInModel))
                {
                    targetInModel = inModel;
                    targetInPort = inPort;
                }
                else
                {
                    var pIndex = inModel.inputPorts.IndexOf(inPort);
                    if (targetInModel.inputPorts.Count <= pIndex)
                        continue;
                    targetInPort = targetInModel.inputPorts[pIndex];
                }
                
                if (!oldModelToCopiedModel.TryGetValue(outModel, out targetOutModel))
                {
                    targetOutModel = outModel;
                    targetOutPort = outPort;
                }
                else
                {
                    var pIndex = outModel.outputPorts.IndexOf(outPort);
                    if (targetOutModel.outputPorts.Count <= pIndex)
                        continue;
                    targetOutPort = targetOutModel.outputPorts[pIndex];
                }
                
                if (!targetInModel.View.TryGetModelToPort(targetInPort.portGUID, out var realInPort) ||
                    !targetOutModel.View.TryGetModelToPort(targetOutPort.portGUID, out var realOutPort))
                {
                    continue;
                }
                
                Edge actualEdge = new Edge {input = realInPort, output = realOutPort};
                if (TryCreateConnection(actualEdge, 
                    targetInModel, targetOutModel, 
                    targetInPort, targetOutPort))
                {
                    actualEdge.input.Connect(actualEdge);
                    actualEdge.output.Connect(actualEdge);
                    AddElement(actualEdge);
                    AddToSelection(actualEdge);
                }
            }
        }
        
        #endregion

        private void ClearGraph()
        {
            foreach (var elem in graphElements)
            {
                RemoveElement(elem);
            }

            viewToModel.Clear();
            edgeToModel.Clear();
        }

        #region Graph Building

        /// <summary>
        /// Creates a node from the given model, but does not add it to the undo record or
        /// editor graph. This is intended for use only via BuildGraph()
        /// </summary>
        private void CreateNodeFromModel(NodeModel model)
        {
            NodeView nv = model.CreateView();
            model.NodeTitle = "wow!";
            model.RuntimeData.name = "wow!";
            AddElement(nv);
            viewToModel.Add(nv, model);
        }

        private void CreateStackFromModel(StackModel model)
        {
            StackView sv = model.CreateView(this);
            model.NodeTitle = "stacky!";
            AddElement(sv);
            viewToModel.Add(sv, model);
        }

        private void CreateNewNode(NodeModel model)
        {
            CreateNodeFromModel(model);

            Undo.RegisterCreatedObjectUndo(model.RuntimeData, "graphChanges");
            Undo.RecordObject(editorGraph, "graphChanges");
            editorGraph.nodeModels.Add(model);
        }

        private void CreateEdgeFromModel(EdgeModel model)
        {
            if (model.inputModel?.View == null || model.outputModel?.View == null ||
                !model.inputModel.View.TryGetModelToPort(model.inputPortModel.portGUID, out var inputPort) ||
                !model.outputModel.View.TryGetModelToPort(model.outputPortModel.portGUID, out var outputPort))
            {
                editorGraph.edgeModels.Remove(model);
                return;
            }

            Edge edge = new Edge {input = inputPort, output = outputPort};
            edge.input.Connect(edge);
            edge.output.Connect(edge);
            AddElement(edge);
            edgeToModel.Add(edge, model);
        }

        private void BindConnections()
        {
            foreach (var conn in editorGraph.connections)
            {
                conn.BindRemote();
            }
        }

        private void BuildGraph()
        {
            if (editorGraph.nodeModels == null)
                return;

            foreach (var model in editorGraph.stackModels.ToArray())
            {
                CreateStackFromModel(model);
            }

            foreach (var model in editorGraph.nodeModels.ToArray())
            {
                CreateNodeFromModel(model);
            }

            foreach (var model in editorGraph.edgeModels.ToArray())
            {
                CreateEdgeFromModel(model);
            }

            BindConnections();
        }

        #endregion

        #region Undo-specific

        /// <summary>
        /// This is a hack to restore the state of ValuePort connections after an undo,
        /// long story short the undo system does not preserve their state, so we need
        /// to essentially rebuild them to match the current graph state which was undone.
        /// This operation is quite expensive, but this ensures their can be no synchronization
        /// loss between the graph and the ValuePorts.
        /// </summary>
        // NOTE:: It may be possible to cheapen this operation significantly by keeping track
        // of dirtied nodes, but the undo system makes this very difficult.
        private void PostUndoSyncNodePortConnections()
        {
            //Map guid->connection since we're going to be doing lots of lookups and
            //this is a more efficient data format.
            var graphKnownGuidToConnection = new Dictionary<string, Link>();
            List<Link> untraversedConnections = new List<Link>(editorGraph.connections);
            bool anyConnectionsRemoved = false;
            foreach (var conn in editorGraph.connections)
            {
                graphKnownGuidToConnection.Add(conn.GUID, conn);
            }

            foreach (var node in editorGraph.nodeModels)
            {
                void DeleteUndoneConnections(PortModel port)
                {
                    var localPortInfo = port.serializedValueFieldInfo.FieldFromInfo;
                    if (!(localPortInfo.GetValue(node.RuntimeData) is ValuePort valuePort))
                        return;
                    for (int i = valuePort.links.Count - 1; i >= 0; i--)
                    {
                        var conn = valuePort.links[i];
                        if (graphKnownGuidToConnection.ContainsKey(conn.GUID))
                        {
                            //We found a port containing this connection, so we mark it traversed.
                            untraversedConnections.Remove(conn);
                            continue;
                        }

                        //The graph doesn't know about this connection, so it's undone. Remove it
                        //from the value port.
                        valuePort.links.Remove(conn);
                        anyConnectionsRemoved = true;
                    }
                }

                foreach (var port in node.inputPorts)
                {
                    DeleteUndoneConnections(port);
                }

                foreach (var port in node.outputPorts)
                {
                    DeleteUndoneConnections(port);
                }
            }

            if (anyConnectionsRemoved || untraversedConnections.Count <= 0)
                return;

            //If we didin't remove any connections, we're going to probe for restored connections
            //by checking to see if there's any connections we didin't traverse. If any exist,
            //those connections are the "redone" connections.
            foreach (var conn in untraversedConnections)
            {
                //Their should be no side effects to binding more than once.
                conn.BindRemote();
                bool existsAlready = false;
                ValuePort localPort = conn.GetLocalPort();
                //We need to be careful not to add the same connection twice
                foreach (var localConn in localPort.links)
                {
                    if (localConn.GUID == conn.GUID)
                        existsAlready = true;
                }

                if (existsAlready) continue;
                localPort.links.Add(conn);
            }
        }

        private void UndoPerformed()
        {
            //The undo stack is VERY finnicky, so this order of operations is important.
            ClearGraph();

            //There's some issue with the undo stack rewinding the object state and somehow
            //the editor graph can be null for a moment here. It do be like it is sometimes.
            if (editorGraph == null) return;
            PostUndoSyncNodePortConnections();
            BuildGraph();
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(editorGraph));
        }

        #endregion

        #region Graph Changes Processing

        private void DeleteNode(NodeModel model)
        {
            //Register the state of the saved assets, because we're about to "implicitly" delete some.
            Undo.RegisterImporterUndo(AssetDatabase.GetAssetPath(model.RuntimeData), "graphChanges");
            Undo.DestroyObjectImmediate(model.RuntimeData);
            editorGraph.nodeModels.Remove(model);
        }

        private void DeleteConnectionByGuid(string guid)
        {
            for (int j = editorGraph.connections.Count - 1; j >= 0; j--)
            {
                Link currentLink = editorGraph.connections[j];
                if (currentLink.GUID != guid) continue;
                editorGraph.connections.Remove(currentLink);
                return;
            }
        }

        private void DeleteEdge(Edge edge, EdgeModel model)
        {
            editorGraph.edgeModels.Remove(model);
            if (!ResolveEdge(edge, out var inModel, out var outModel,
                out var inputPort, out var outputPort))
                return;
            inModel.DeletePortConnectionByGuid(inputPort, model.inputConnectionGuid);
            outModel.DeletePortConnectionByGuid(outputPort, model.outputConnectionGuid);
            DeleteConnectionByGuid(model.inputConnectionGuid);
            DeleteConnectionByGuid(model.outputConnectionGuid);
        }

        private bool TryCreateConnection(Edge edge,
            NodeModel inModel, NodeModel outModel,
            PortModel inputPort, PortModel outputPort)
        {
            //Safeguard against multi-connecting single capacity ports accidentally.
            if (edge.input.capacity == Port.Capacity.Single && edge.input.connections.Any() ||
                edge.output.capacity == Port.Capacity.Single && edge.output.connections.Any())
            {
                return false;
            }

            if (!inModel.CanPortConnectTo(inputPort, outModel, outputPort) ||
                !outModel.CanPortConnectTo(outputPort, inModel, inputPort))
            {
                return false;
            }
            var localConnection = inModel.ConnectPortTo(inputPort, outModel, outputPort);
            var remoteConnection = outModel.ConnectPortTo(outputPort, inModel, inputPort);

            var modelEdge = new EdgeModel(inModel, inputPort,
                outModel, outputPort,
                localConnection.GUID,
                remoteConnection.GUID);

            edgeToModel.Add(edge, modelEdge);
            editorGraph.edgeModels.Add(modelEdge);
            editorGraph.connections.Add(localConnection);
            editorGraph.connections.Add(remoteConnection);
            return true;
        }

        /// <summary>
        /// Resolves an edge connection to its related models
        /// </summary>
        private bool ResolveEdge(Edge edge,
            out NodeModel inModel, out NodeModel outModel,
            out PortModel inputPort, out PortModel outputPort)
        {
            if (edge.input.node is NodeView inView &&
                edge.output.node is NodeView outView &&
                viewToModel.TryGetValue(inView, out var movableIn) &&
                viewToModel.TryGetValue(outView, out var movableOut))
            {
                inModel = movableIn as NodeModel;
                outModel = movableOut as NodeModel;
                //Cannot be null, the view was a NodeView so the model will be a NodeModel
                // ReSharper disable once PossibleNullReferenceException
                if(inModel.View.TryGetPortToModel(edge.input, out inputPort) &&
                   // ReSharper disable once PossibleNullReferenceException
                   outModel.View.TryGetPortToModel(edge.output, out outputPort))
                        return true;
            }

            inModel = null;
            outModel = null;
            inputPort = null;
            outputPort = null;
            return false;
        }
        
        private void ProcessElementMoves(ref List<GraphElement> elements)
        {
            foreach (var elem in elements)
            {
                if (!(elem is MovableView view)) continue;
                if (!viewToModel.TryGetValue(view, out var model)) continue;
                model.UpdatePosition();
            }
        }

        private void ProcessElementRemovals(ref List<GraphElement> elements)
        {
            //Saves the current undo group.
            var index = Undo.GetCurrentGroup();
            foreach (var elem in elements)
            {
                switch (elem)
                {
                    case NodeView view:
                        if (viewToModel.TryGetValue(view, out var nodeModel))
                            DeleteNode(nodeModel as NodeModel);
                        break;
                    case Edge edge:
                        if (edgeToModel.TryGetValue(edge, out var edgeModel))
                            DeleteEdge(edge, edgeModel);
                        break;
                }
            }

            //Crushes all the delete operations into one undo operation.
            Undo.CollapseUndoOperations(index);
        }

        private void ProcessEdgesToCreate(ref List<Edge> addedEdges)
        {
            for (int i = addedEdges.Count - 1; i >= 0; i--)
            {
                Edge edge = addedEdges[i];

                //(NodeView) Input Node -> (NodeModel) In Model -> (PortModel) Input Port
                //(NodeView) Output Node -> (NodeModel) Out Model -> (PortModel) Output Port
                if (!ResolveEdge(edge, out var inModel, out var outModel,
                        out var inputPort, out var outputPort) ||
                    !TryCreateConnection(edge, inModel, outModel, inputPort, outputPort))
                {
                    //We failed to create a connection so discard this edge, otherwise it's confusing
                    //to the user if an edge is created when a connection isin't.
                    addedEdges.Remove(edge);
                }
            }
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange changes)
        {
            //Save the state before any changes are made
            Undo.RegisterCompleteObjectUndo(editorGraph, "graphChanges");
            if (changes.movedElements != null)
            {
                ProcessElementMoves(ref changes.movedElements);
            }

            //Checks for changes related to our nodes.
            if (changes.elementsToRemove != null)
            {
                ProcessElementRemovals(ref changes.elementsToRemove);
            }

            if (changes.edgesToCreate == null)
                return changes;

            ProcessEdgesToCreate(ref changes.edgesToCreate);
            //Bump up the undo increment so we're not undoing multiple change passes at once.
            Undo.IncrementCurrentGroup();

            return changes;
        }

        #endregion

        //Thanks @Mert Kirimgeri
        private void InitializeSearchWindow()
        {
            searchWindow.Init(this);
            nodeCreationRequest = context =>
                SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), searchWindow);
        }

        #region Helper Functions

        protected Vector2 GetViewRelativePosition(Vector2 pos, Vector2 offset = default)
        {
            //What the fuck unity. NEGATIVE POSITION???
            Vector2 relPos = new Vector2(
                -viewTransform.position.x + pos.x,
                -viewTransform.position.y + pos.y);

            //Hold the offset as a static value by scaling it in the reverse direction of our scale
            //This way we "undo" the division by scale for only the offset value, scaling everything else.
            relPos -= (offset * scale);
            return relPos / scale;
        }

        #endregion

        #region Default Connection Edge Rules

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compPorts = new List<Port>();

            foreach (var port in ports)
            {
                if (startPort == port || startPort.node == port.node) continue;
                if (startPort.portType != port.portType) continue;
                compPorts.Add(port);
            }

            return compPorts;
        }

        #endregion
    }
}