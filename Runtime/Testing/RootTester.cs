using GraphFramework.Attributes;
using UnityEngine;

namespace GraphFramework
{
    [RegisterNode(typeof(TestGraphController))]
    public class RootTester : RuntimeNode, RootNode
    {
        [Out, SerializeField]
        public ValuePort<string> rootPort = new ValuePort<string>();
        
        public override RuntimeNode OnEvaluate()
        {
            if (!rootPort.IsLinked()) return this;
            foreach (var link in rootPort.Links)
            {
                Debug.Log("Value of link: " + link.GetValueAs<string>());
            }
            return rootPort.FirstNode();
        }
    }
}