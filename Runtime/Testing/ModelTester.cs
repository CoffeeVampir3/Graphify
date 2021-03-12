using UnityEngine;
using GraphFramework.Attributes;

namespace GraphFramework
{
    [RegisterToGraph(typeof(TestGraphController), "Wow/Node")]
    public class ModelTester : RuntimeNode
    {
        [In, SerializeField]
        public ValuePort<string> stringValue = new ValuePort<string>();
        [Out, SerializeField]
        public ValuePort<string> stringValue2 = new ValuePort<string>();
        
        public override RuntimeNode OnEvaluate()
        {
            if (!stringValue2.IsLinked()) return this;
            foreach (var link in stringValue2.Links)
            {
                Debug.Log("Value of link: " + link.GetValueAs<string>());
            }
            return stringValue2.FirstNode();
        }
    }
}