using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace GraphFramework.Editor
{
    public class CoffeeGraphView : GraphView
    { 
        protected internal CoffeeGraphWindow parentWindow;
        protected internal GraphModel graphModel;
        protected readonly GraphSearchWindow searchWindow;
        protected GraphSettings settings;

        //Keeps track of all NodeView's and their relation to their model.
        //Shared internals with StackView (readonly)
        protected internal readonly Dictionary<MovableView, MovableModel> viewToModel =
            new Dictionary<MovableView, MovableModel>();
        //Edge -> EdgeModel
        protected readonly Dictionary<Edge, EdgeModel> edgeToModel =
            new Dictionary<Edge, EdgeModel>();
        //Provides a fast lookup path for the editor<->graph linker (GraphExecutor class)
        //Shared internals with GraphExecutor (readonly)
        protected internal readonly Dictionary<RuntimeNode, NodeView> runtimeNodeToView =
            new Dictionary<RuntimeNode, NodeView>();
        
        #region Initialization and Finalization
        
        protected internal CoffeeGraphView()
        {
            //Zoom, content click + drag, group select.
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            //These callbacks are derived from graphView.
            //Callback on cut/copy.
            serializeGraphElements = OnSerializeGraphElements;
            //Callback on paste.
            unserializeAndPaste = OnPaste;
            //Callback on "changes" particularly -- Edge creation, node movement, and deleting.
            graphViewChanged = OnGraphViewChanged;
            
            //This is called on anything being undone in the graph, including field changes.
            //This operation can get quite expensive for bigger graphs, but there's currently
            //no alternative to this due to the inflexibility of the undo system.
            Undo.undoRedoPerformed += UndoPerformed;

            searchWindow = ScriptableObject.CreateInstance<GraphSearchWindow>();
            InitializeSearchWindow();
        }
        
        //Thanks @Mert Kirimgeri for his lovely youtube series on GraphView API.
        private void InitializeSearchWindow()
        {
            searchWindow.Init(this);
            nodeCreationRequest = context =>
                SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), searchWindow);
        }

        /// <summary>
        /// Callback when the graph GUI had been created an
        /// </summary>
        protected internal virtual void OnGraphGUI()
        {
        }

        /// <summary>
        /// Called when the graph window is about to close. No guarantee your data is stable by the time
        /// this is called, make sure to check null.
        /// </summary>
        protected internal virtual void OnGraphClosed(DetachFromPanelEvent panelEvent)
        {
            //Cleans up junk that undo can leave behind in very specific edge cases.
            void CleanUndoRemnants()
            {
                var serializedAssets = AssetDatabase.LoadAllAssetRepresentationsAtPath(
                    AssetDatabase.GetAssetPath(graphModel));
                var runtimeNodes = new HashSet<RuntimeNode>();
                foreach (var model in graphModel.nodeModels)
                {
                    runtimeNodes.Add(model.RuntimeData);
                }
                foreach (var asset in serializedAssets)
                {
                    if (!(asset is RuntimeNode modelRuntimeNode)) continue;
                    if (runtimeNodes.Contains(modelRuntimeNode)) continue;
                    //The root node is not in graphModel.nodeModels so we need to account
                    //for it.
                    if (modelRuntimeNode is RootNode) continue;
                    AssetDatabase.RemoveObjectFromAsset(modelRuntimeNode);
                }
            }
            if(graphModel != null) 
                CleanUndoRemnants();
            AssetDatabase.SaveAssets();
        }

        #endregion

        /// <summary>
        /// Loads the provided GraphModel.
        /// </summary>
        protected internal virtual void LoadGraph(GraphModel modelToLoad)
        {
            UnloadGraph();
            graphModel = modelToLoad;

            var oldSettings = settings;
            settings = GraphSettings.CreateOrGetSettings(graphModel);

            if (oldSettings != settings)
            {
                styleSheets.Remove(settings.graphViewStyle);
                styleSheets.Add(settings.graphViewStyle);
            }
            Undo.ClearAll();
            BuildGraph();
        }

        /// <summary>
        /// Unloads any loaded graph.
        /// </summary>
        protected internal virtual void UnloadGraph()
        {
            ClearGraph();
            graphModel = null;
        }

        /// <summary>
        /// Attempts to add the "CurrentNode" css class the the provided runtime node.
        /// </summary>
        protected internal virtual void RuntimeNodeVisited(RuntimeNode node)
        {
            if (!runtimeNodeToView.TryGetValue(node, out var view))
                return;
            
            view.AddToClassList("CurrentNode");
        }
        
        /// <summary>
        /// Attempts to remove the "CurrentNode" css class the the provided runtime node.
        /// </summary>
        protected internal virtual void RuntimeNodeExited(RuntimeNode node)
        {
            if (!runtimeNodeToView.TryGetValue(node, out var view))
                return;
            
            view.RemoveFromClassList("CurrentNode");
        }

        #region Public API
        
        /// <summary>
        /// Creates a new node on the graph.
        /// </summary>
        /// <param name="runtimeDataType">The runtime data type.</param>
        /// <param name="atPosition">Editor screen space(?) position.</param>
        public void CreateNewNode(string nodeName, Type runtimeDataType, Vector2 atPosition)
        {
            var model = NodeModel.InstantiateModel(nodeName, graphModel, runtimeDataType);
            
            Vector2 spawnPosition = parentWindow.rootVisualElement.ChangeCoordinatesTo(
                parentWindow.rootVisualElement.parent,
                atPosition - parentWindow.position.position);

            spawnPosition = contentViewContainer.WorldToLocal(spawnPosition);
            Rect spawnRect = new Rect(spawnPosition.x - 75, spawnPosition.y - 75, 150, 150);
            model.Position = spawnRect;
            CreateNewNode(model);
        }
        
        /// <summary>
        /// Creates a new node on the graph.
        /// </summary>
        /// <param name="runtimeDataType">The runtime data type.</param>
        /// <param name="atPosition">Editor screen space(?) position.</param>
        public void CreateNewStack(string nodeName, Type[] allowedTypes, Vector2 atPosition)
        {
            var model = StackModel.InstantiateModel(nodeName, allowedTypes);
            
            Vector2 spawnPosition = parentWindow.rootVisualElement.ChangeCoordinatesTo(
                parentWindow.rootVisualElement.parent,
                atPosition - parentWindow.position.position);

            spawnPosition = contentViewContainer.WorldToLocal(spawnPosition);
            Rect spawnRect = new Rect(spawnPosition.x - 75, spawnPosition.y - 75, 150, 150);
            model.Position = spawnRect;
            CreateNewStack(model);
        }

        #endregion
        
        #region Copy and Paste

        //Helper class for us to drop some serialized data into.
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
                    //View data key contains the elements GUID.
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
                var clone = model.Clone(graphModel);
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
                    //TODO:: This may not be a viable solution once dynamic ports are added.
                    var pIndex = inModel.inputPorts.IndexOf(inPort);
                    if (targetInModel.inputPorts.Count <= pIndex)
                        continue;
                    targetInPort = targetInModel.inputPorts[pIndex];
                }
                
                if (!oldModelToCopiedModel.TryGetValue(outModel, out var targetOutModel))
                {
                    targetOutModel = outModel;
                    targetOutPort = outPort;
                }
                else
                {
                    //TODO:: This may not be a viable solution once dynamic ports are added.
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
                
                Edge newEdge = CreateNewEdge(realInPort, realOutPort);
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

        private void CreateNodeFromModelAsRoot(NodeModel model)
        {
            CreateNodeFromModel(model);
            //Removes the capability of our root node to be deleted or copied.
            model.View.capabilities &= ~(Capabilities.Deletable | Capabilities.Copiable);
        }

        private void CreateStackFromModel(StackModel model)
        {
            StackView sv = model.CreateView(this);
            AddElement(sv);
            viewToModel.Add(sv, model);
        }

        private void CreateNewStack(StackModel model)
        {
            CreateStackFromModel(model);
            Undo.RecordObject(graphModel, "graphChanges");
            graphModel.stackModels.Add(model);
        }

        private void CreateNewNode(NodeModel model)
        {
            CreateNodeFromModel(model);
            Undo.RegisterCreatedObjectUndo(model.RuntimeData, "graphChanges");
            Undo.RecordObject(graphModel, "graphChanges");
            graphModel.nodeModels.Add(model);
        }

        private static void AddEdgeClasses(Edge edge)
        {
            edge.AddToClassList("edgierEdge");
        }

        private static Edge CreateNewEdge(Port inputPort, Port outputPort)
        {
            Edge edge = new Edge {input = inputPort, output = outputPort};
            AddEdgeClasses(edge);
            return edge;
        }

        private void CreateConnectedEdge(EdgeModel model, Port inputPort, Port outputPort)
        {
            Edge edge = CreateNewEdge(inputPort, outputPort);
            edge.input.Connect(edge);
            edge.output.Connect(edge);
            AddElement(edge);
            edgeToModel.Add(edge, model);
        }

        private void CreateEdgeFromModel(EdgeModel model)
        {
            if (model.inputModel?.View == null || model.outputModel?.View == null ||
                !model.inputModel.View.TryGetModelToPort(model.inputPortModel.portGUID, out var inputPort) ||
                !model.outputModel.View.TryGetModelToPort(model.outputPortModel.portGUID, out var outputPort))
            {
                graphModel.edgeModels.Remove(model);
                return;
            }

            CreateConnectedEdge(model, inputPort, outputPort);
        }

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
        
        private void BuildGraph()
        {
            if (graphModel.nodeModels == null)
                return;

            foreach (var model in graphModel.stackModels.ToArray())
            {
                CreateStackFromModel(model);
            }
            
            CreateNodeFromModelAsRoot(graphModel.rootNodeModel);

            foreach (var model in graphModel.nodeModels.ToArray())
            {
                CreateNodeFromModel(model);
            }

            foreach (var model in graphModel.edgeModels.ToArray())
            {
                CreateEdgeFromModel(model);
            }
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
        private void PostUndoSyncNodePortConnections()
        {
            //Map guid->connection since we're going to be doing lots of lookups and
            //this is a more efficient data format.
            var graphKnownGuidToLink = new Dictionary<string, Link>();
            var untraversedLinks = new List<Link>(graphModel.links);
            var traversedLinks = new List<Link>(graphModel.links.Count);
            bool anyLinksRemoved = false;
            
            for (var i = 0; i < graphModel.links.Count; i++)
            {
                var link = graphModel.links[i];
                graphKnownGuidToLink.Add(link.GUID, link);
            }

            for (var modelIndex = 0; modelIndex < graphModel.nodeModels.Count; modelIndex++)
            {
                var node = graphModel.nodeModels[modelIndex];
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

            //If we didn't remove any connections, we're going to probe for restored connections
            //by checking to see if there's any connections we didn't traverse. If any exist,
            //those connections are the "redone" connections.
            var unaccountedForLinks = untraversedLinks.Except(traversedLinks);
            foreach (var conn in unaccountedForLinks)
            {
                ValuePort localPort = conn.GetLocalPort();
                //Guards against the same connection being added twice.
                if (localPort.links.Any(localConn => localConn.GUID == conn.GUID)) continue;
                localPort.links.Add(conn);
            }
        }

        private void UndoPerformed()
        {
            ClearGraph();

            //There's some issue with the undo stack rewinding the object state and somehow
            //the editor graph can be null for a moment here. It do be like it is sometimes.
            if (graphModel == null) return;
            PostUndoSyncNodePortConnections();
            BuildGraph();
        }

        #endregion

        #region Graph Changes Processing
        
        private void DeleteNode(NodeModel model)
        {
            Undo.DestroyObjectImmediate(model.RuntimeData);
            runtimeNodeToView.Remove(model.RuntimeData);
            graphModel.nodeModels.Remove(model);
            //Base graph view handles removal of the visual element itself.
        }

        private void DeleteConnectionByGuid(string guid)
        {
            for (int j = graphModel.links.Count - 1; j >= 0; j--)
            {
                Link currentLink = graphModel.links[j];
                if (currentLink.GUID != guid) continue;
                graphModel.links.Remove(currentLink);
                return;
            }
        }

        private void DeleteEdge(Edge edge, EdgeModel model)
        {
            graphModel.edgeModels.Remove(model);
            if (!ResolveEdge(edge, out var inModel, out var outModel,
                out var inputPort, out var outputPort))
                return;
            inModel.DeletePortLinkByGuid(inputPort, model.inputConnectionGuid);
            outModel.DeletePortLinkByGuid(outputPort, model.outputConnectionGuid);
            DeleteConnectionByGuid(model.inputConnectionGuid);
            DeleteConnectionByGuid(model.outputConnectionGuid);
        }

        private bool TryCreateConnection(Edge edge,
            NodeModel inModel, NodeModel outModel,
            PortModel inputPort, PortModel outputPort)
        {
            //Safeguard against over-connecting single capacity ports accidentally.
            if (edge.input.capacity == Port.Capacity.Single && edge.input.connections.Any() ||
                edge.output.capacity == Port.Capacity.Single && edge.output.connections.Any())
            {
                return false;
            }

            if (!inModel.CanPortLinkTo(inputPort, outModel, outputPort) ||
                !outModel.CanPortLinkTo(outputPort, inModel, inputPort))
            {
                return false;
            }
            var localConnection = inModel.LinkPortTo(inputPort, outModel, outputPort);
            var remoteConnection = outModel.LinkPortTo(outputPort, inModel, inputPort);

            var modelEdge = new EdgeModel(inModel, inputPort,
                outModel, outputPort,
                localConnection.GUID,
                remoteConnection.GUID);

            edgeToModel.Add(edge, modelEdge);
            graphModel.edgeModels.Add(modelEdge);
            graphModel.links.Add(localConnection);
            graphModel.links.Add(remoteConnection);
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
                
                //Dictionary lookup guaranteed to output NodeView->NodeModel.
                Debug.Assert(inModel != null, nameof(inModel) + " != null");
                Debug.Assert(outModel != null, nameof(outModel) + " != null");
                if(inModel.View.TryGetPortToModel(edge.input, out inputPort) &&
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
                    //to the user if an edge is created when a connection isint.
                    addedEdges.Remove(edge);
                }
                else
                {
                    AddEdgeClasses(edge);
                }
            }
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange changes)
        {
            //Save the state before any changes are made
            Undo.RegisterCompleteObjectUndo(graphModel, "graphChanges");
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
            
            Undo.IncrementCurrentGroup();
            return changes;
        }

        #endregion

        #region Default Connection Edge Rules

        /// <summary>
        /// These default rules allow only exact types to connect to eachother.
        /// </summary>
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