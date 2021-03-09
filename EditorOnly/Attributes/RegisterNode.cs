using System;
using UnityEngine;

namespace GraphFramework.Editor
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RegisterNodeToView : Attribute
    {
        public Type registeredGraphView = null;
        public string registeredPath = null;
        public RegisterNodeToView(System.Type registerToGraphView, string nodePath)
        {
            if (!typeof(CoffeeGraphView).IsAssignableFrom(registerToGraphView))
            {
                Debug.LogError("Must register node to a coffee graph type!");
                return;
            }
            registeredGraphView = registerToGraphView;
            registeredPath = nodePath;
        }
        public RegisterNodeToView(System.Type registerToGraphView)
        {
            if (!typeof(CoffeeGraphView).IsAssignableFrom(registerToGraphView))
            {
                Debug.LogError("Must register node to a coffee graph type!");
                return;
            }
            registeredGraphView = registerToGraphView;
            registeredPath = "";
        }
    }
}