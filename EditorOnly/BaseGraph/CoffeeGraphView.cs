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
        //Shared with StackView which only reads the collection.
        protected internal readonly Dictionary<MovableView, MovableModel> viewToModel =
            new Dictionary<MovableView, MovableModel>();
        //Keeps track of all edges and their relation to their model.
        protected readonly Dictionary<Edge, EdgeModel> edgeToModel =
            new Dictionary<Edge, EdgeModel>();
        //
        protected internal readonly Dictionary<RuntimeNode, NodeView> runtimeNodeToView =
            new Dictionary<RuntimeNode, NodeView>();

        /// <summary>
        /// A callback used once the graph loads, add your GUI widgets and stuff here.
        /// Called once the graph view is resized to the editor window and all geometry has been
        /// calculated. (Internally, this is called after a GeometryChangedEvent)
        /// </summary>
        protected internal abstract void OnCreateGraphGUI();

        internal void OnGraphLoaded()
        {
            DEBUG__LOAD_GRAPH();
            OnCreateGraphGUI();
        }

        /// <summary>
        /// A callback when the graph is closed.
        /// </summary>
        /// <param name="panelEvent"></param>
        protected internal virtual void OnGraphClosed(DetachFromPanelEvent panelEvent)
        {
            //Cleans up junk that undo can leave behind in very specific edge cases.
            void CleanUndoRemnants()
            {
                var serializedAssets = AssetDatabase.LoadAllAssetRepresentationsAtPath(
                    AssetDatabase.GetAssetPath(editorGraph));
                var runtimeNodes = new HashSet<RuntimeNode>();
                foreach (var model in editorGraph.nodeModels)
                {
                    runtimeNodes.Add(model.RuntimeData);
                }
                foreach (var asset in serializedAssets)
                {
                    if (!(asset is RuntimeNode modelRuntimeNode)) continue;
                    if (runtimeNodes.Contains(modelRuntimeNode)) continue;
                    AssetDatabase.RemoveObjectFromAsset(modelRuntimeNode);
                }
            }
            if(editorGraph != null) 
                CleanUndoRemnants();
            AssetDatabase.SaveAssets();
        }
        
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
            editorGraph.graphWindowType = new SerializableType(parentWindow.GetType());
            Undo.ClearAll();
            BuildGraph();
        }

        protected internal void RuntimeNodeVisited(RuntimeNode node)
        {
            if (!runtimeNodeToView.TryGetValue(node, out var view))
                return;
            
            view.AddToClassList("CurrentNode");
        }
        
        protected internal void RuntimeNodeExited(RuntimeNode node)
        {
            if (!runtimeNodeToView.TryGetValue(node, out var view))
                return;
            
            view.RemoveFromClassList("CurrentNode");
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
                if (!(GetElementByGuid(viewGuid) is NodeView nv)) continue;
                var model = viewToModel[nv] as NodeModel;
                //It is not possible this is null as we looked up using a node view and therefore the result
                //is guaranteed to be a node model.
                //ReSharper disable once PossibleNullReferenceException
                //Creates a copy of the model and adds it to the graph, simple.
                var clone = model.Clone(editorGraph);
                oldModelToCopiedModel.Add(model, clone);
                CreateNewNode(clone);
                AddToSelection(clone.View);
            }
            
            
            foreach (var edgeGuid in box.edgeGuids)
            {
                //Resolve the original edge into its model components
                Edge originalEdge = GetEdgeByGuid(edgeGuid);
                ResolveEdge(originalEdge,
                    out var inModel, out var outModel,
                    out var inPort, out var outPort);
                
                PortModel targetInPort;
                PortModel targetOutPort;

                //Resolve if the target of this edge's input is something we copied or
                //if it existed previously.
                if (!oldModelToCopiedModel.TryGetValue(inModel, out var targetInModel))
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
                
                //Resolve if the target of this edge's output is something we copied or
                //if it existed previously.
                if (!oldModelToCopiedModel.TryGetValue(outModel, out var targetOutModel))
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
                
                Edge newEdge = new Edge {input = realInPort, output = realOutPort};
                if (TryCreateConnection(newEdge, 
                    targetInModel, targetOutModel, 
                    targetInPort, targetOutPort))
                {
                    newEdge.input.Connect(newEdge);
                    newEdge.output.Connect(newEdge);
                    AddElement(newEdge);
                    AddToSelection(newEdge);
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

            runtimeNodeToView.Clear();
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
            AddElement(nv);
            viewToModel.Add(nv, model);
            runtimeNodeToView.Add(model.RuntimeData, nv);

            //If our model was stacked... well, stack it again.
            model.stackedOn?.View?.StackOn(model, nv);
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
            model.NodeTitle = model.RuntimeData.GetType().Name;
            model.RuntimeData.name = model.RuntimeData.GetType().Name;
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
            foreach (var conn in editorGraph.links)
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
        //NOTE:: Ugly because it's been slightly optimized.
        private void PostUndoSyncNodePortConnections()
        {
            //Map guid->connection since we're going to be doing lots of lookups and
            //this is a more efficient data format.
            var graphKnownGuidToLink = new Dictionary<string, Link>();
            var untraversedLinks = new List<Link>(editorGraph.links);
            var traversedLinks = new List<Link>(editorGraph.links.Count);
            bool anyLinksRemoved = false;
            for (var i = 0; i < editorGraph.links.Count; i++)
            {
                var link = editorGraph.links[i];
                graphKnownGuidToLink.Add(link.GUID, link);
            }

            for (var modelIndex = 0; modelIndex < editorGraph.nodeModels.Count; modelIndex++)
            {
                var node = editorGraph.nodeModels[modelIndex];
                //Deletes links that the undo operation we just performed ideally would of deleted,
                //but it's not capable of doing that, so we do it manually.
                void DeleteUndoneLinks(PortModel port)
                {
                    var localPortInfo = port.serializedValueFieldInfo.FieldFromInfo;
                    if (!(localPortInfo.GetValue(node.RuntimeData) is ValuePort valuePort))
                        return;
                    for (int i = valuePort.links.Count - 1; i >= 0; i--)
                    {
                        var link = valuePort.links[i];
                        if (graphKnownGuidToLink.ContainsKey(link.GUID))
                        {
                            //We found a port containing this connection, so we mark it traversed.
                            traversedLinks.Add(link);
                            continue;
                        }

                        //The graph doesn't know about this connection, so it's undone. Remove it
                        //from the value port.
                        valuePort.links.Remove(link);
                        anyLinksRemoved = true;
                    }
                }

                for (var index = 0; index < node.inputPorts.Count; index++)
                {
                    DeleteUndoneLinks(node.inputPorts[index]);
                }

                for (var index = 0; index < node.outputPorts.Count; index++)
                {
                    DeleteUndoneLinks(node.outputPorts[index]);
                }
            }

            if (anyLinksRemoved || untraversedLinks.Count <= 0)
                return;

            //If we didin't remove any connections, we're going to probe for restored connections
            //by checking to see if there's any connections we didin't traverse. If any exist,
            //those connections are the "redone" connections.
            var unaccountedForLinks = untraversedLinks.Except(traversedLinks);
            foreach (var conn in unaccountedForLinks)
            {
                //Their should be no side effects to binding more than once.
                conn.BindRemote();
                ValuePort localPort = conn.GetLocalPort();
                //We need to be careful not to add the same connection twice
                bool existsAlready = localPort.links.Any(
                    localConn => localConn.GUID == conn.GUID);

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
        }

        #endregion

        #region Graph Changes Processing

        //This is called by the change processor, so the graph actually handles the VE removal.
        private void DeleteNode(NodeModel model)
        {
            Undo.DestroyObjectImmediate(model.RuntimeData);
            runtimeNodeToView.Remove(model.RuntimeData);
            editorGraph.nodeModels.Remove(model);
        }

        private void DeleteConnectionByGuid(string guid)
        {
            for (int j = editorGraph.links.Count - 1; j >= 0; j--)
            {
                Link currentLink = editorGraph.links[j];
                if (currentLink.GUID != guid) continue;
                editorGraph.links.Remove(currentLink);
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
            editorGraph.links.Add(localConnection);
            editorGraph.links.Add(remoteConnection);
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
                        continue;
                    case Edge edge:
                        if (edgeToModel.TryGetValue(edge, out var edgeModel))
                            DeleteEdge(edge, edgeModel);
                        continue;
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