using System.Reflection;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

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
            Debug.Log(Assembly.GetCallingAssembly().GetName());
        }
    }
}