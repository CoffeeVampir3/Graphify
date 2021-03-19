using System;
using System.Collections.Generic;
using System.Linq;
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
        [SerializeField] 
        protected List<SerializableType> allowedTypes = null;

        [NonSerialized] 
        private StackView view;
        public StackView View => view;

        public static StackModel InstantiateModel(string nodeName, Type[] allowedTypes)
        {
            var model = new StackModel {nodeTitle = nodeName};
            if (allowedTypes != null && allowedTypes.Length > 0)
            {
                model.allowedTypes = new List<SerializableType>(allowedTypes.Length);
                foreach (var type in allowedTypes)
                {
                    model.allowedTypes.Add(new SerializableType(type));
                }
            }
            
            return model;
        }
        
        public StackView CreateView(GraphifyView graphView)
        {
            view = new StackView(this, graphView);
            view.Display();
            return view;
        }
        
        public void UpdatePosition()
        {
            position = view.GetPosition();
        }

        public bool IsTypeAllowed(Type t)
        {
            if (allowedTypes == null || allowedTypes.Count == 0)
                return true;
            return allowedTypes.Any(sType => t == sType.type);
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