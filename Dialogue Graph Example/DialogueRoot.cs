using GraphFramework;
using GraphFramework.Attributes;

namespace DefaultNamespace
{
    [RegisterTo(typeof(DialogueBP))]
    public class DialogueRoot : RuntimeNode, RootNode
    {
        [Out] 
        public ValuePort<Any> rootOut = new ValuePort<Any>();

        protected override RuntimeNode OnEvaluate(int graphId)
        {
            return rootOut.FirstNode();
        }
    }
}