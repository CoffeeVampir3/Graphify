using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GraphFramework.Attributes;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace GraphFramework.Editor
{
    public class GraphifyView : GraphView
    { 
        protected internal GraphfyWindow parentWindow;
        public GraphModel graphModel;
        protected readonly GraphSearchWindow searchWindow;
        protected readonly NavigationBlackboard navigationBlackboard;
        protected GraphSettings settings;
        
        //Edge -> EdgeModel
        protected readonly Dictionary<Edge, EdgeModel> edgeToModel =
            new Dictionary<Edge, EdgeModel>();
        //Provides a fast lookup path for the editor<->graph linker (GraphExecutor class)
        //Shared internals with GraphExecutor (readonly)
        protected internal readonly Dictionary<RuntimeNode, NodeView> runtimeNodeToView =
            new Dictionary<RuntimeNode, NodeView>();
        
        #region Initialization and Finalization
        
        protected internal GraphifyView()
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

            //navigationBlackboard = new NavigationBlackboard(this);
            //navigationBlackboard.SetPosition(new Rect(0, 150, 100, 300));
            //Add(navigationBlackboard);
        }
        
        //Thanks @Mert Kirimgeri for his lovely youtube series on GraphView API.
        private void InitializeSearchWindow()
        {
            searchWindow.Init(this);
            nodeCreationRequest = context =>
                SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), searchWindow);
        }

        /// <summary>
        /// Callback when the graph GUI is initialized.
        /// </summary>
        protected internal virtual void OnGraphGUIInitialized()
        {
        }

        private static void RecursiveChildDescent(GraphModel model, ref List<GraphModel> models)
        {
            foreach (var child in model.childGraphs)
            {
                models.Add(child);
                RecursiveChildDescent(child, ref models);
            }
        }

        /// <summary>
        /// Called when the graph window is about to close. No guarantee your data is stable by the time
        /// this is called, make sure to check null.
        /// </summary>
        protected internal virtual void OnGraphClosed(DetachFromPanelEvent panelEvent)
        {
            //Cleans up junk that will linger if we don't manually remove the assets.
            //This is a cleaner way to control the subnested assets because of how undo works.
            void CleanUndoRemnants()
            {
                //Go to the top most graph model.
                while (graphModel.parentGraph != null)
                {
                    graphModel = graphModel.parentGraph;
                }
                
                List<GraphModel> allGraphs = new List<GraphModel> {graphModel};
                RecursiveChildDescent(graphModel, ref allGraphs);

                var serializedAssets = AssetDatabase.LoadAllAssetRepresentationsAtPath(
                    AssetDatabase.GetAssetPath(graphModel));
                var runtimeNodes = new HashSet<RuntimeNode>();
                runtimeNodes.Add(graphModel.rootNodeModel.RuntimeData);

                foreach (var descendentModel in allGraphs)
                {
                    foreach (var model in descendentModel.nodeModels)
                    {
                        runtimeNodes.Add(model.RuntimeData);
                    }

                    //Don't forget about roots!
                    runtimeNodes.Add(descendentModel.rootNodeModel.RuntimeData);
                }
                
                foreach (var asset in serializedAssets)
                {
                    if (!(asset is RuntimeNode modelRuntimeNode)) continue;
                    if (runtimeNodes.Contains(modelRuntimeNode)) continue;
                    AssetDatabase.RemoveObjectFromAsset(modelRuntimeNode);
                }
            }
            if(graphModel != null)
            {
                graphModel.viewPosition = viewTransform.position;
                graphModel.viewZoom = viewTransform.scale;
                EditorUtility.SetDirty(graphModel);
                EditorUtility.SetDirty(graphModel.serializedGraphBlueprint);
                CleanUndoRemnants();
            }
            AssetDatabase.SaveAssets();
        }

        #endregion

        #region Internal API
        
        /// <summary>
        /// Loads the provided GraphModel.
        /// </summary>
        public virtual void LoadGraph(GraphModel modelToLoad)
        {
            UnloadGraph();
            graphModel = modelToLoad;

            if (this.Q<GridBackground>() == null)
            {
                var grid = new GridBackground();
                Insert(0, grid);
            }
            var oldSettings = settings;
            settings = GraphSettings.CreateOrGetSettings(graphModel);
            if (oldSettings != settings)
            {
                if(oldSettings != null)
                    styleSheets.Remove(oldSettings.graphViewStyle);
                styleSheets.Add(settings.graphViewStyle);
            }

            Undo.ClearAll();
            viewTransform.position = graphModel.viewPosition;
            viewTransform.scale = graphModel.viewZoom;
            graphModel.view = this;
            BuildGraph();

            if (graphModel.serializedGraphBlueprint.dataBlackboard != null)
            {
                Add(new DataBlackboardView(graphModel.serializedGraphBlueprint.dataBlackboard));
            }
        }

        /// <summary>
        /// Unloads any loaded graph.
        /// </summary>
        protected internal virtual void UnloadGraph()
        {
            ClearGraph();
            graphModel = null;
        }

        protected internal virtual void ClearAllNodesAfterPlaymodeEnds()
        {
            foreach (var node in nodes.ToList())
            {
                node.RemoveFromClassList("CurrentNode");
            }
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

        #endregion

        #region Public API

        public void ResetVirtualGraph(int graphId)
        {
            foreach (var elem in graphElements.ToList())
            {
                if (elem is NodeView nv)
                {
                    nv.RemoveFromClassList("CurrentNode");
                }
            }
            graphModel.rootNodeModel?.View?.AddToClassList("CurrentNode");
            graphModel.serializedGraphBlueprint.InitializeId(graphId);
        }
        
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
        /// Creates a new stack node on the graph.
        /// </summary>
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
                if (!(GetElementByGuid(viewGuid) is NodeView nv) || nv.nodeModel.RuntimeData is SubgraphNode) continue;
                var model = nv.nodeModel;
                var clone = model.Clone(graphModel);
                oldModelToCopiedModel.Add(model, clone);
                CreateNewNode(clone);
                
                //Dynamic port model copy, since the ICollectible thing is mystery.
                foreach (var modelPort in model.portModels)
                {
                    if (!(modelPort is DynamicPortModel dynPm)) continue;
                    var clonePort = clone.AllPortModels()
                        .FirstOrDefault(cp => cp.portName == dynPm.portName) as DynamicPortModel;
                    if (clonePort == null)
                        continue;
                    clonePort.Resize(dynPm.dynamicPorts.Count);
                    foreach (var port in dynPm.dynamicPorts)
                    {
                        foreach (var portCon in port.view.connections)
                        {
                            if (box.edgeGuids.Contains(portCon.viewDataKey))
                                continue;
                            box.edgeGuids.Add(portCon.viewDataKey);
                        }
                    }
                }
                
                AddToSelection(clone.View);
            }
            
            foreach (var edgeGuid in box.edgeGuids)
            {
                //Resolve the original edge into its model components
                Edge originalEdge = GetEdgeByGuid(edgeGuid);
                if (originalEdge == null)
                {
                    continue;
                }
                
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
                    var pIndex = inModel.AllPortModels().IndexOf(inPort);
                    if (targetInModel.AllPortModels().Count <= pIndex || pIndex < 0)
                        continue;
                    targetInPort = targetInModel.AllPortModels()[pIndex];
                }

                if (!oldModelToCopiedModel.TryGetValue(outModel, out var targetOutModel))
                {
                    targetOutModel = outModel;
                    targetOutPort = outPort;
                }
                else
                {
                    var pIndex = outModel.AllPortModels().IndexOf(outPort);
                    if (targetOutModel.AllPortModels().Count <= pIndex || pIndex < 0)
                        continue;
                    targetOutPort = targetOutModel.AllPortModels()[pIndex];
                }

                var realInPort = targetInPort.view;
                var realOutPort = targetOutPort.view;

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

        public void DeletePortEdges(PortModel portModel)
        {
            Port p = portModel.view;

            for (int i = p.connections.Count() - 1; i >= 0; i--)
            {
                var edge = p.connections.ElementAt(i);
                if (!edgeToModel.TryGetValue(edge, out var edgeModel)) continue;
                
                //Manually delete edges because otherwise bad things.
                DeleteEdge(edge, edgeModel);
                edge.input.Disconnect(edge);
                edge.output.Disconnect(edge);
                edge.parent.Remove(edge);
            }
        }
        
        #endregion

        #region Graph Building

        /// <summary>
        /// Creates a node from the given model, but does not add it to the undo record or
        /// editor graph model. This is intended for use only via BuildGraph()
        /// </summary>
        private void CreateNodeFromModelInternal(NodeModel model)
        {
            NodeView nv = model.CreateView();
            AddElement(nv);
            runtimeNodeToView.Add(model.RuntimeData, nv);

            //If our model was stacked... well, stack it again.
            model.stackedOn?.View?.StackOn(model, nv);
        }

        /// <summary>
        /// Creates a node that can't be copied or deleted.
        /// </summary>
        private void CreateNodeFromModelAsRoot(NodeModel model)
        {
            CreateNodeFromModelInternal(model);
            //Removes the capability of our root node to be deleted or copied.
            model.View.capabilities &= ~(Capabilities.Deletable | Capabilities.Copiable);
        }

        private void CreateStackFromModelInternal(StackModel model)
        {
            StackView sv = model.CreateView();
            AddElement(sv);
        }

        private void CreateNewStack(StackModel model)
        {
            CreateStackFromModelInternal(model);
            Undo.RecordObject(graphModel, "graphChanges");
            graphModel.stackModels.Add(model);
        }

        private void CreateNewNode(NodeModel model)
        {
            CreateNodeFromModelInternal(model);
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

        private void CreateEdgeFromModelInternal(EdgeModel model)
        {
            if (model.inputModel?.View == null || model.outputModel?.View == null 
                || model.inputPortModel.view == null || model.outputPortModel.view == null)
            {
                graphModel.edgeModels.Remove(model);
                return;
            }
            CreateConnectedEdge(model, model.inputPortModel.view, model.outputPortModel.view);
        }

        private void ClearGraph()
        {
            foreach (var elem in graphElements.ToList())
            {
                RemoveElement(elem);
            }

            runtimeNodeToView.Clear();
            edgeToModel.Clear();
        }

        private void SynchronizeNodeChange(NodeModel nm, ref HashSet<string> changedFieldNames)
        {
            foreach (var port in nm.portModels)
            {
                var field = port.serializedValueFieldInfo.FieldFromInfo;
                if (field == null)
                    continue;

                var data = field.GetValue(nm.RuntimeData);

                if (!(data is BasePort bp)) continue;
                for (var index = bp.Links.Count - 1; index >= 0; index--)
                {
                    var link = bp.Links[index];
                    if (!changedFieldNames.Contains(link.RemoteFieldName) &&
                        !changedFieldNames.Contains(link.LocalFieldName)) continue;
                    bp.Links.RemoveAt(index);
                }
            }
        }

        private void SynchronizeChanges(ref NodeModel[] models, ref HashSet<string> changedFieldNames)
        {
            SynchronizeNodeChange(graphModel.rootNodeModel, ref changedFieldNames);
            foreach (var model in models)
            {
                SynchronizeNodeChange(model, ref changedFieldNames);
            }

            for (var index = graphModel.links.Count - 1; index >= 0; index--)
            {
                var link = graphModel.links[index];
                if (!changedFieldNames.Contains(link.RemoteFieldName) &&
                    !changedFieldNames.Contains(link.LocalFieldName)) continue;
                graphModel.links.RemoveAt(index);
            }
        }
        
        private void BuildGraph()
        {
            if (graphModel.nodeModels == null)
                return;
            
            foreach (var model in graphModel.stackModels.ToArray())
            {
                CreateStackFromModelInternal(model);
            }
            
            NodeModel.ClearChangeTrackingCache();

            HashSet<string> changedFieldNames = new HashSet<string>();
            bool anyChanges = graphModel.rootNodeModel.TrackChanges(ref changedFieldNames);
            //Update root node model ports and build the root node.
            CreateNodeFromModelAsRoot(graphModel.rootNodeModel);

            var models = graphModel.nodeModels.ToArray();

            foreach (var model in models)
            {
                anyChanges |= model.TrackChanges(ref changedFieldNames);
                CreateNodeFromModelInternal(model);
            }

            if (anyChanges)
            {
                SynchronizeChanges(ref models, ref changedFieldNames);
            }

            foreach (var model in graphModel.edgeModels.ToArray())
            {
                CreateEdgeFromModelInternal(model);
            }
        }

        #endregion

        #region Undo-specific

        /// <summary>
        /// This is a hack to restore the state of ValuePort connections after an undo,
        /// long story short the undo system does not preserve their state, so we need
        /// to essentially rebuild them to match the current graph state which was undone.
        /// We do not know whether the undo created or removed items, so we have to account for
        /// both possibilities deductively, first we assume this was a delete operation. If
        /// nothing was deleted, we then check if any items were restored.
        /// </summary>
        private void PostUndoSync()
        {
            //Map guid->connection since we're going to be doing lots of lookups and
            //this is a more efficient data format.
            var graphKnownGuidToLink = new Dictionary<string, Link>();
            var untraversedLinks = new List<Link>(graphModel.links);
            var traversedLinks = new List<Link>(graphModel.links.Count);
            bool anyLinksRemoved = false;
            HashSet<RuntimeNode> bpRuntimeNodes = new HashSet<RuntimeNode>(graphModel.serializedGraphBlueprint.nodes);
            HashSet<GraphBlueprint> bpChildBlueprints = new HashSet<GraphBlueprint>(graphModel.serializedGraphBlueprint.childGraphs);
            BlueprintHelper bpHelper = new BlueprintHelper(graphModel.serializedGraphBlueprint);
            
            for (var i = 0; i < graphModel.links.Count; i++)
            {
                var link = graphModel.links[i];
                graphKnownGuidToLink.Add(link.GUID, link);
            }

            for (var modelIndex = 0; modelIndex < graphModel.nodeModels.Count; modelIndex++)
            {
                var node = graphModel.nodeModels[modelIndex];
                //Synchronize the blueprint if a node is deleted then undone.
                if (!bpRuntimeNodes.Contains(node.RuntimeData))
                {
                    bpHelper.AddNode(node.RuntimeData);
                }

                //Deletes links that the undo operation we just performed ideally would of deleted,
                //but it's not capable of doing that, so we do it manually.
                void DeleteUndoneLinks(PortModel port)
                {
                    var localPortInfo = port.serializedValueFieldInfo.FieldFromInfo;
                    if (!(localPortInfo.GetValue(node.RuntimeData) is BasePort basePort))
                        return;
                    for (int i = basePort.links.Count - 1; i >= 0; i--)
                    {
                        var link = basePort.links[i];
                        if (graphKnownGuidToLink.ContainsKey(link.GUID))
                        {
                            //We found a port containing this connection, so we mark it traversed.
                            traversedLinks.Add(link);
                            continue;
                        }

                        //The graph doesn't know about this connection, so it's undone. Remove it
                        //from the value port.
                        basePort.links.Remove(link);
                        anyLinksRemoved = true;
                    }
                }

                var allPortModels = node.AllPortModels();
                for (var index = 0; index < allPortModels.Count; index++)
                {
                    DeleteUndoneLinks(allPortModels[index]);
                }
            }

            //Synchronize deleted then undone graphs.
            foreach (var child in graphModel.childGraphs)
            {
                if (bpChildBlueprints.Contains(child.serializedGraphBlueprint)) continue;
                bpHelper.AddGraph(child.serializedGraphBlueprint);
            }
            
            bpHelper.Apply();
            
            if (anyLinksRemoved || untraversedLinks.Count <= 0)
                return;
            
            //If we didn't remove any connections, we're going to probe for restored connections
            //by checking to see if there's any connections we didn't traverse. If any exist,
            //those connections are the "redone" connections.
            var unaccountedForLinks = untraversedLinks.Except(traversedLinks);
            foreach (var conn in unaccountedForLinks)
            {
                BasePort localPort = conn.GetLocalPort();
                //Guards against the same connection being added twice.
                if (localPort.links.Any(localConn => localConn.GUID == conn.GUID)) continue;
                localPort.links.Add(conn);
            }
        }

        private void UndoPerformed()
        {
            ClearGraph();

            //There's some issue with the undo stack rewinding the object state and somehow
            //the editor graph can be null for a moment here. (It can get called more than once
            //for a single undo operation.) It do be like it is sometimes.
            if (graphModel == null) return;
            PostUndoSync();
            BuildGraph();
        }

        #endregion

        #region Graph Changes Processing
        
        private void DeleteNode(NodeModel model)
        {
            model.Delete(graphModel);
            
            Undo.DestroyObjectImmediate(model.RuntimeData);
            runtimeNodeToView.Remove(model.RuntimeData);
            graphModel.nodeModels.Remove(model);
        }

        private void DeleteStack(StackModel stack)
        {
            graphModel.stackModels.Remove(stack);
        }

        /// <summary>
        /// Deletes a connection by GUID, used because the undo system can spawn a different
        /// reference object so it's not safe to compare by-object.
        /// </summary>
        private void DeleteLinkByGuid(string guid)
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
            DeleteLinkByGuid(model.inputConnectionGuid);
            DeleteLinkByGuid(model.outputConnectionGuid);
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
            
            var localConnection = inModel.
                LinkPortTo(inputPort, outModel, outputPort);
            var remoteConnection = outModel.
                LinkPortTo(outputPort, inModel, inputPort);
            
            //If we're trying to reconnect a port that was already connected, discard the old connection first.
            if (edgeToModel.TryGetValue(edge, out var oldEdgeModel))
            {
                edgeToModel.Remove(edge);
                if (graphModel.edgeModels.Contains(oldEdgeModel))
                    DeleteEdge(edge, oldEdgeModel);
            }
            
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
        private static bool ResolveEdge(Edge edge,
            out NodeModel inModel, out NodeModel outModel,
            out PortModel inputPort, out PortModel outputPort)
        {
            if (edge.input.node is NodeView inView &&
                edge.output.node is NodeView outView)
            {
                inModel = inView.nodeModel;
                outModel = outView.nodeModel;
                if(inView.TryGetPortToModel(edge.input, out inputPort) &&
                   outView.TryGetPortToModel(edge.output, out outputPort))
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
                view.GetModel().UpdatePosition();
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
                        DeleteNode(view.nodeModel);
                        continue;
                    case Edge edge:
                        if (edgeToModel.TryGetValue(edge, out var edgeModel))
                            DeleteEdge(edge, edgeModel);
                        continue;
                    case StackView sView:
                        DeleteStack(sView.stackModel);
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
                if (ResolveEdge(edge, out var inModel, out var outModel,
                        out var inputPort, out var outputPort) &&
                    TryCreateConnection(edge, inModel, outModel, inputPort, outputPort))
                {
                    Undo.IncrementCurrentGroup();
                    AddEdgeClasses(edge);
                }
                else
                {
                    //We failed to create a connection so discard this edge, otherwise it's confusing
                    //to the user if an edge is created when a connection isint.
                    addedEdges.Remove(edge);
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

        private readonly Dictionary<string, ConnectionRules> ruleCache = 
            new Dictionary<string, ConnectionRules>();

        private ConnectionRules GetRules(Port p)
        {
            var parentView = p.GetFirstAncestorOfType<NodeView>();
            if (parentView == null || 
                !parentView.TryGetPortToModel(p, out var portModel))
                return ConnectionRules.Exact;

            if (ruleCache.TryGetValue(portModel.serializedValueFieldInfo.FieldName, out var rules))
            {
                return rules;
            }

            var field = portModel.serializedValueFieldInfo.FieldFromInfo;
            if(field == null)
                return ConnectionRules.Exact;
            
            var attr = field.GetCustomAttribute<DirectionalAttribute>();
            if(attr == null)
                return ConnectionRules.Exact;

            ruleCache.Add(portModel.serializedValueFieldInfo.FieldName, attr.rules);
            return attr.rules;
        }

        private static ConnectionRules GetStrictestRule(ConnectionRules first, ConnectionRules second)
        {
            int v1 = (int) first;
            int v2 = (int) second;
            return (ConnectionRules)Mathf.Min(v1, v2);
        }

        private static readonly Type anyType = typeof(Any);
        /// <summary>
        /// These default rules allow only exact types to connect to eachother.
        /// </summary>
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compPorts = new List<Port>();
            var startRules = GetRules(startPort);

            Type sType = startPort.portType;
            foreach (var port in ports.ToList())
            {
                var pType = port.portType;
                if (startPort == port || startPort.node == port.node) continue;
                if ((sType == anyType || pType == anyType))
                {
                    compPorts.Add(port);
                    continue;
                }
                var connectingRules = GetRules(port);
                var strictestRule = GetStrictestRule(startRules, connectingRules);
                switch (strictestRule)
                {
                    case ConnectionRules.Exact:
                        if (sType != pType) continue;
                        break;
                    case ConnectionRules.Inherited:
                        if (!pType.IsAssignableFrom(sType) && !sType.IsAssignableFrom(pType)) continue;
                        break;
                }
                compPorts.Add(port);
            }

            return compPorts;
        }

        #endregion
    }
}