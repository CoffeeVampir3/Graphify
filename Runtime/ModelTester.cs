using UnityEngine;
using GraphFramework.Attributes;

namespace GraphFramework
{
    [RegisterNodeToView("TestGraphView", "Wow/Node")]
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
                //Debugging Only, graph will initialize this.
                link.BindRemote();
                Debug.Log("Value of link: " + link.GetValueAs<string>());
            }
            //Debugging Only, graph will initialize this.
            stringValue2.FirstLink().BindRemote();
            return stringValue2.FirstNode();
        }
    }
}