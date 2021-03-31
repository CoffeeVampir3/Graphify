using DefaultNamespace.Scenes;
using GraphFramework;
using GraphFramework.Attributes;
using UnityEngine;

[RegisterTo(typeof(DialogueBP), "Dialogue Node")]
public class DialogueNode : RuntimeNode
{
    [In(Capacity.Multi), SerializeField] 
    public ValuePort<string> stringIn = new ValuePort<string>();
    [Out(Capacity.Single), SerializeField] 
    public DynamicValuePort<string> stringOut = new DynamicValuePort<string>();
    [SerializeField]
    public string testDialogue = "";

    protected override RuntimeNode OnEvaluate(int graphId)
    {
        if (!stringOut.IsLinked())
        {
            Dialogue.SendText(testDialogue);
            return this;
        }

        if (!stringOut.HasMultipleLinks())
        {
            Dialogue.SendText(testDialogue);
            return stringOut.FirstNode();
        }

        int clickedIndex = Dialogue.ConsumeClickedIndex();
        if(clickedIndex >= 0)
        {
            Debug.Log(clickedIndex);
            foreach (var link in stringOut.Links)
            {
                if (link.PortIndex == clickedIndex)
                {
                    return link.Node;
                }
            }

            return this;
        }
        
        Dialogue.ClearChoices();
        Dialogue.SendText(testDialogue);
        foreach (var link in stringOut.Links)
        {
            if (link.Node is DialogueNode dn)
            {
                Dialogue.SendChoice(dn.testDialogue, link.PortIndex, Dialogue.OnChoiceClicked);
            }
        }
        Dialogue.StartChoices();
        return this;
    }
}
