using GraphFramework.EditorOnly.Attributes;
using UnityEditor.Experimental.GraphView;

namespace GraphFramework.Editor
{
    [RegisterGraphController(typeof(TestGraphController))]
    public class TestGraphView : CoffeeGraphView
    {
        private void CreateGrid()
        {
            var grid = new GridBackground();
            Insert(0, grid);
        }

        protected internal override void OnGUICreated()
        {
            CreateGrid();
        }
    }
}