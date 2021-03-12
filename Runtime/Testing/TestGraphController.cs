using GraphFramework.Attributes;

namespace GraphFramework
{
    [RegisterStack(typeof(TestGraphController), "sad")]
    public class TestGraphController : GraphController
    {
    }
}