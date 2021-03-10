using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
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
        /// stack order is changed, updating the nodes CSS as appropriate.
        /// </summary>
        private void OnStackOrderChanged(GeometryChangedEvent geoChange)
        {
            var visualElements = Children().ToArray();
            for (var index = 0; index < visualElements.Length; index++)
            {
                var child = visualElements[index];
                child.RemoveFromClassList("firstInStack");
                child.RemoveFromClassList("lastInStack");
                if (index == 0)
                {
                    //Node is first
                    child.AddToClassList("firstInStack");
                } else if (index == visualElements.Length - 1)
                {
                    //Node is last
                    child.AddToClassList("lastInStack");
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
                if (!(s is NodeView view) ||
                    !parentGraphView.viewToModel.TryGetValue(view, out var mModel) ||
                    !(mModel is NodeModel model)) continue;
                model.stackedOn = stackModel;
            }
            
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

            if (ge is NodeView view &&
                parentGraphView.viewToModel.TryGetValue(view, out var mModel) &&
                mModel is NodeModel model)
            {
                model.stackedOn = null;
            }
            base.OnStartDragging(ge);
        }
        
        /// <summary>
        /// Stacks a node onto this node.
        /// </summary>
        public void StackOn(NodeModel model, NodeView view)
        {
            model.stackedOn = stackModel;
            AddElement(view);
            RegisterCallback<GeometryChangedEvent>(OnStackOrderChanged);
        }

        public void OnDirty()
        {
            var label = this.Q<Label>();
            title = stackModel.NodeTitle;
            label.text = stackModel.NodeTitle;
            SetPosition(stackModel.Position);
        }
        
        public void Display()
        {
            OnDirty();
        }
    }
}