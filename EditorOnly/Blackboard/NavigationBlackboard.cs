using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace GraphFramework.Editor
{
    public class NavigationBlackboard : UnityEditor.Experimental.GraphView.Blackboard
    {
        private readonly ListView listBase = new ListView();
        private readonly GraphifyView graphify;
        public NavigationBlackboard(GraphifyView gv) : base(gv)
        {
            graphify = gv;
            graphView = gv;
            
            this.AddToClassList("customBB");
            var blackboardSubNameLabel = this.Q<Label>("subTitleLabel");
            blackboardSubNameLabel.text = "";
            blackboardSubNameLabel.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            var blackboardNameLabel = this.Q<Label>("titleLabel");
            blackboardNameLabel.text = "Navigator";
            blackboardNameLabel.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);

            var btn = this.Q<Button>();
            btn.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);

            this.RegisterCallback<GeometryChangedEvent>(OnGeoInit);
            listBase.style.flexGrow = 1;
            listBase.style.flexShrink = 1;
            listBase.AddToClassList("list-base");
            Add(listBase);
            SetupListView();
            schedule.Execute(DelayedRefresh).Every(100);
        }

        private void OnGeoInit(GeometryChangedEvent geo)
        {
            listBase.style.minHeight = resolvedStyle.height;
        }

        private void SetupListView()
        {
            listBase.reorderable = false;
            listBase.makeItem = () => new NavBlackboardNodeItem();
            listBase.bindItem = BindNodeToList;
            listBase.itemHeight = 22;
        }

        private int currentStackCount = 0;
        private int previosuStackCount = 0;
        private void BindNodeToList(VisualElement ele, int index)
        {
            if (!(ele is NavBlackboardNodeItem bc))
                return;
            if (!(listBase.itemsSource[index] is Node node))
            {
                return;
            }
            
            bc.label.text = node.name;
            bc.targetNode = node;

            if (node is NodeView nv && graphify.viewToModel.TryGetValue(nv, out var model) 
                                    && model is NodeModel nm && nm.stackedOn != null)
            {
                ele.AddToClassList("stacked");
                //Stack count lets us avoid rearranging the node list if the only change
                //was a stack/unstack.
                currentStackCount++;
            }
            else
            {
                ele.RemoveFromClassList("stacked");
            }

            bc.RegisterCallback<PointerDownEvent>(OnLabelClicked);
        }

        private void UpdateSelection()
        {
            if (listBase.itemsSource == null)
                return;

            if (graphify.selection.Count == 0)
            {
                listBase.ClearSelection();
                return;
            }

            var newSelection = new List<int>();
            for (var index = 0; index < listBase.itemsSource.Count; index++)
            {
                var item = listBase.itemsSource[index];
                if (!(item is Node node))
                {
                    return;
                }
                if (graphify.selection.Contains(node))
                {
                    newSelection.Add(index);
                }
            }
            listBase.SetSelectionWithoutNotify(newSelection);
        }

        private ISelectable newSelectded;
        private void OnLabelClicked(PointerDownEvent evt)
        {
            if (!(evt.currentTarget is NavBlackboardNodeItem bbItem))
            {
                return;
            }
            Node targetNode = bbItem.targetNode;
            
            listBase.SetSelection(listBase.itemsSource.IndexOf(targetNode));
            
            graphify.ClearSelection();
            newSelectded = targetNode;
            graphify.AddToSelection(targetNode);
            //UI elements boys and girls.
            schedule.Execute(() =>
            {
                graphify.AddToSelection(newSelectded);
                graphify.FrameSelection();
            }).StartingIn(1);

            evt.StopImmediatePropagation();
        }

        private List<Node> listItemNodes = new List<Node>();
        private void DelayedRefresh()
        {
            listBase.itemsSource ??= graphify.nodes.ToList();
            if (listBase.itemsSource.Count != graphify.nodes.Count() || currentStackCount != previosuStackCount)
            {
                previosuStackCount = currentStackCount;
                listBase.Clear();
                listItemNodes = graphify.nodes.ToList();
                listBase.itemsSource = listItemNodes;
            }
            currentStackCount = 0;
            listBase.Refresh();
            UpdateSelection();
        }
    }
}