![Dialogue Graph Example Image](https://github.com/CoffeeVampir3/Graphify/blob/a1d336221eaf7f3d7c3e827a5b280b029a58e0fa/dialogueGraphExample.png?raw=true)

# Turn Anything Into a Graph!

Graphify is a Unity Api for quickly and easily making anything into a graph!

# Quickstart Guide:

Note: This is temporary documentation until the wiki gets set up. This API is in its alpha stage and has not been finalized, things might change!

### Installing:

Download and unzip in any unity directory. Package versions coming soon.

### Graph Controllers

Create a class that derives from GraphController:
```cs
    [CreateAssetMenu]
    public class TestGraphController : GraphController
    {
    }
```

### Root Nodes

Every graph needs a root node, to give our graph controller a root we must create and register our new root node:
```cs
    [RegisterTo(typeof(TestGraphController))]
    public class RootTester : RuntimeNode, RootNode
    {
    }
```

You'll also want the root node to have a port so it can connect to something.

Add a port to the root node:
```cs
    [RegisterTo(typeof(TestGraphController))]
    public class RootTester : RuntimeNode, RootNode
    {
        [Out, SerializeField]
        public ValuePort<Any> rootPort = new ValuePort<Any>();
    }
```

All ports are defined as a ValuePort<T>, in our case this is an Any which means it can connect to any type of port.

Now you can hop in and check out your first graph, simply create an instance of GraphController by taking advantage of the Create Asset Menu attribute you gave it. Then, double click on the graph controller and your graph will open with it's root node.. It's that simple to get an editable graph, but you probably want more than a root node and no content.

### More About Nodes

Now to add something we can connect to, we'll make another node, this time one we can create in the graph:
```cs
    [RegisterTo(typeof(TestGraphController), "My Second Node")]
    public class MySecondNode : RuntimeNode
    {
        //These port options are: "Show Backing Value?", "Port Capacity" and default to false/single if none are selected.
        [In(false, Port.Capacity.Multi), SerializeField]
        public ValuePort<string> stringValue = new ValuePort<string>();
        [Out(true, Port.Capacity.Multi), SerializeField]
        public ValuePort<string> stringValue2 = new ValuePort<string>();
    }
```

Viola! Now you can head into your graph and right-click anywhere in the world, you'll see in the dropdown menu options a "Create Node", click that and it'll pull up a search menu where you can find your node which you've named "My Second Node". You can give nodes a more detailed path, for example:

```cs
    [RegisterTo(typeof(TestGraphController), "Dialogue Nodes/My Second Node")]
```

Will create a new category called "Dialogue Nodes" which your "My Second Node" will now belong to.

Now you know how to make nodes and ports! Next, how do you make the graph do something? Lets extend your root node by overriding it's OnEvaluate function:
```cs
    [RegisterTo(typeof(TestGraphController))]
    public class RootTester : RuntimeNode, RootNode
    {
        [Out, SerializeField]
        public ValuePort<Any> rootPort = new ValuePort<Any>();

        protected override RuntimeNode OnEvaluate()
        {
            if (!rootPort.IsLinked()) return this;
            return rootPort.FirstNode();
        }
    }
```

Now when your root node gets evaluated, if it's linked to anything, it'll return the first node it's linked to. Similarly, we can extend our "My Second Node" with some functionality as well:
```cs
    [RegisterTo(typeof(TestGraphController), "Wow/Node")]
    public class MySecondNode : RuntimeNode
    {
        [In(false, Port.Capacity.Multi), SerializeField]
        public ValuePort<string> stringInput = new ValuePort<string>();
        [Out(true, Port.Capacity.Multi), SerializeField]
        public ValuePort<string> stringOutput = new ValuePort<string>();

        protected override RuntimeNode OnEvaluate()
        {
            foreach (var link in stringInput.Links)
            {
                if (link.TryGetValue<string>(out var value))
                {
                    Debug.Log("Value of link: " + value);
                }
            }

            if (!stringOutput.IsLinked()) return this;
            var index = (Random.Range(0, stringOutput.Links.Count));
            return stringOutput.Links[index].Node;
        }
    }
```

This will output the value of everything connected to the stringInput ports as long as it's a string. It will also randomly pick any connected port to traverse to next if there's any connections.

### Running a Graph:

To execute a graph, you use the GraphExecutor class:
```cs
    public class GraphTester : MonoBehaviour
    {
        public GraphExecutor executor;

        private void Start()
        {
            executor.Initialize();

            for (int i = 0; i < 4; i++)
            {
                executor.Step();
            }
        }
    }
```

Make sure to drag your GraphController into the executor, this will make the executor try to execute your graph up to four steps.

Wiki *coming soon* (TM)
