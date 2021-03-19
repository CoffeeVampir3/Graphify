![Dialogue Graph Example Image](https://github.com/CoffeeVampir3/Graphify/blob/a1d336221eaf7f3d7c3e827a5b280b029a58e0fa/dialogueGraphExample.png?raw=true)

# Quickstart Guide:

Note: This is temporary documentation until the wiki gets set up.

Create a class that derives from GraphController:
```cs
    [CreateAssetMenu]
    public class TestGraphController : GraphController
    {
    }
```


Every graph needs a root node, to give our graph controller a root we must create and register our new root node:
```cs
    [RegisterTo(typeof(TestGraphController))]
    public class RootTester : RuntimeNode, RootNode
    {
    }
```

Now you can hop in and check out your first graph, simply create an instance of GraphController by taking advantage of the Create Asset Menu attribute you gave it. Then, double click on the graph controller and your graph will open. It's that simple to get an editable graph, but you probably want more than a root node and no content.
