using System;
using UnityEngine;

namespace GraphFramework.Editor
{
    [Serializable]
    public class StackModel : MovableModel
    {
        [SerializeField] 
        protected string nodeTitle = "Untitled.";
        [SerializeField] 
        protected Rect position = Rect.zero;

        [NonSerialized] 
        private StackView view;
        public StackView View => view;

        public static StackModel InstantiateModel()
        {
            var model = new StackModel();
            return model;
        }
        
        public StackView CreateView(CoffeeGraphView graphView)
        {
            view = new StackView(this, graphView);
            view.Display();
            return view;
        }
        
        public void UpdatePosition()
        {
            position = view.GetPosition();
        }
        
        public string NodeTitle
        {
            get => nodeTitle;

            set
            {
                nodeTitle = value;
                view?.OnDirty();
            }
        }

        public Rect Position
        {
            get => position;
            set
            {
                position = value;
                view?.OnDirty();
            }
        }
    }
}