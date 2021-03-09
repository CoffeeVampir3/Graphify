using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

/*
namespace VisualNovelFramework.GraphFramework.Editor
{
    public class NavigationBlackboard : UnityEditor.Experimental.GraphView.Blackboard
    {
        private readonly StyleSheet categoryStyle;
        private readonly ListView listBase = new ListView();
        private readonly CoffeeGraphView coffeeGraph;
        public NavigationBlackboard(CoffeeGraphView gv) : base(gv)
        {
            styleSheets.Add(gv.settings.blackboardStyle);
            categoryStyle = gv.settings.blackboardCategoryStyle;
            coffeeGraph = gv;
            
            var blackboardNameLabel = this.Q<Label>("subTitleLabel");
            blackboardNameLabel.text = "Node Browser";
            
            listBase.AddToClassList("list-base");
            Add(listBase);
            SetupListView();
        }

        private void SetupListView()
        {
            listBase.reorderable = false;
            listBase.makeItem = () => new NavBlackboardNodeItem();
            listBase.bindItem = BindNodeToList;
            listBase.itemHeight = 22;
        }

        private void BindNodeToList(VisualElement ele, int index)
        {
            if (!(ele is NavBlackboardNodeItem bc))
                return;
            bc.styleSheets.Add(categoryStyle);
            if (!(listBase.itemsSource[index] is Node node))
                return;

            bc.foldout.text = node.name;
            bc.targetNode = node;

            if (node is BaseNode bn && coffeeGraph.IsNodeStacked(bn))
            {
                ele.AddToClassList("stacked");
            }
            else
            {
                ele.RemoveFromClassList("stacked");
            }

            bc.RegisterCallback<ClickEvent>(OnLabelClicked);
        }

        private void OnLabelClicked(ClickEvent evt)
        {
            if (!(evt.currentTarget is NavBlackboardNodeItem bbItem))
            {
                return;
            }
            
            Node targetNode = bbItem.targetNode;

            coffeeGraph.LookAtNode(targetNode);
            //Screaming internally
            listBase.SetSelection(listBase.itemsSource.IndexOf(targetNode));

            graphView.ClearSelection();
            graphView.AddToSelection(targetNode);
        }

        private List<Node> listItemNodes = new List<Node>();
        private void DelayedRefresh()
        { 
            //Warning! List view is highly unstable and alterations with the order of operations
            //here might cause unexpected errors.
            refreshingImminent = false;
            listBase.Clear();
            listItemNodes = coffeeGraph.GetNodesOrdered();
            listBase.itemsSource = listItemNodes;
            listBase.Refresh();
        }

        //This setup lets us refresh only once every 100MS at most, so we arent constantly
        //building and rebuilding the blackboard during say, a copy-paste operation
        //or during loading, but it's still quick enough to not be perceivable to the user.
        private bool refreshingImminent = false;
        public void RequestRefresh()
        {
            if (refreshingImminent) 
                return;
            
            schedule.Execute(DelayedRefresh).StartingIn(100);
            refreshingImminent = true;
        }
    }
}
*/