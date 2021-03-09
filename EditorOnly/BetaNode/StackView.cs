using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace GraphFramework.Editor
{
    public class StackView : StackNode, MovableView
    {
        private readonly StackModel stackModel;
        private readonly CoffeeGraphView parentGraphView;
        public StackView(StackModel model, CoffeeGraphView graphView)
        {
            stackModel = model;
            parentGraphView = graphView;
        }
        
        protected override bool AcceptsElement(GraphElement element, ref int proposedIndex, int maxIndex)
        {
            return true;
        }

        /// <summary>
        /// Keeps track of the first and last node in the stack whenever the
        /// stack order is changed.
        /// </summary>
        private void OnStackOrderChanged(GeometryChangedEvent geoChange)
        {
            if (!stackModel.stackedNodeModels.Any())
                return;

            for (var index = 0; index < stackModel.stackedNodeModels.Count; index++)
            {
                var nodeModel = stackModel.stackedNodeModels[index];
                var node = nodeModel.View;

                if (index == 0)
                {
                    //Node is first
                    node.RemoveFromClassList("lastInStack");
                    node.AddToClassList("firstInStack");
                } else if (index == stackModel.stackedNodeModels.Count - 1)
                {
                    //Node is last
                    node.RemoveFromClassList("firstInStack");
                    node.AddToClassList("lastInStack");
                }
                else
                {
                    //Node is not special
                    node.RemoveFromClassList("firstInStack");
                    node.RemoveFromClassList("lastInStack");
                }
            }
            UnregisterCallback<GeometryChangedEvent>(OnStackOrderChanged);
        }

        /// <summary>
        /// Called when an item is added to our stack node.
        /// </summary>
        public override bool DragPerform(DragPerformEvent evt, IEnumerable<ISelectable> selection, IDropTarget dropTarget, ISelection dragSource)
        {
            var selectables = selection as ISelectable[] ?? selection.ToArray();
            foreach (var s in selectables)
            {
                if (!(s is NodeView view)) continue;
                if (parentGraphView.viewToModel.TryGetValue(view, out var model))
                {
                    stackModel.stackedNodeModels.Add(model as NodeModel);
                }
            }

            Debug.Log("Added.");
            RegisterCallback<GeometryChangedEvent>(OnStackOrderChanged);
            return base.DragPerform(evt, selectables, dropTarget, dragSource);
        }

        /// <summary>
        /// Called when an item is removed from our stack node.
        /// </summary>
        public override void OnStartDragging(GraphElement ge)
        {
            RegisterCallback<GeometryChangedEvent>(OnStackOrderChanged);
            ge.RemoveFromClassList("firstInStack");
            ge.RemoveFromClassList("lastInStack");

            if (ge is NodeView view)
            {
                if (parentGraphView.viewToModel.TryGetValue(view, out var model))
                {
                    stackModel.stackedNodeModels.Remove(model as NodeModel);
                    Debug.Log("Removed.");
                }
            }
            base.OnStartDragging(ge);
        }

        public void OnDirty()
        {
            title = stackModel.NodeTitle;
            SetPosition(stackModel.Position);
            RefreshExpandedState();
            RefreshPorts();
        }
        
        public void Display()
        {
            OnDirty();
        }
    }
}