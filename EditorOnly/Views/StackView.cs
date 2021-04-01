using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace GraphFramework.Editor
{
    public class StackView : StackNode, MovableView
    {
        public readonly StackModel stackModel;

        public StackView(StackModel model)
        {
            stackModel = model;
        }
        
        protected override bool AcceptsElement(GraphElement element, ref int proposedIndex, int maxIndex)
        {
            //Root node cannot be stacked because of a graph view bug allowing it to be deleted.
            if (element is NodeView view && !(view.nodeModel.RuntimeData is RootNode))
            {
                return stackModel.IsTypeAllowed(view.nodeModel.RuntimeData.GetType());
            }
            
            return false;
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
                //Can be both first and last.
                if (index == 0)
                {
                    //Node is first
                    child.AddToClassList("firstInStack");
                } 
                if (index == visualElements.Length - 1)
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
                if (!(s is NodeView view)) continue;
                view.nodeModel.stackedOn = stackModel;
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

            if (ge is NodeView view)
            {
                view.nodeModel.stackedOn = null;
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

        /// <summary>
        /// Updates this view to be consistent with it's data model.
        /// </summary>
        public void OnDirty()
        {
            var label = this.Q<Label>();
            name = stackModel.NodeTitle;
            title = stackModel.NodeTitle;
            label.text = stackModel.NodeTitle;
            SetPosition(stackModel.Position);
        }
        
        public void Display()
        {
            OnDirty();
        }
        
        public MovableModel GetModel()
        {
            return stackModel;
        }
    }
}