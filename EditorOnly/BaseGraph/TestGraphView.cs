using UnityEditor.Experimental.GraphView;

namespace GraphFramework.Editor
{
    public class TestGraphView : CoffeeGraphView
    {
        private void CreateGrid()
        {
            var grid = new GridBackground();
            Insert(0, grid);
        }

        public override void OnCreateGraphGUI()
        {
            CreateGrid();
        }
    }
}