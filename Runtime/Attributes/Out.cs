namespace GraphFramework.Attributes
{
    //Using preserve attribute so we don't get this stripped out in IL2CPP compilations
    /// <summary>
    /// Defines a ValuePort as an output with the given capacity.
    /// </summary>
    public class Out : DirectionalAttribute
    {
        //Explicit constructors so they show up in autocomplete.
        public Out()
        {
            capacity = Capacity.Single;
            direction = Direction.Output;
            rules = ConnectionRules.Exact;
            this.showBackingValue = false;
        }
        
        public Out(bool showBackingValue)
        {
            capacity = Capacity.Single;
            direction = Direction.Output;
            rules = ConnectionRules.Exact;
            this.showBackingValue = showBackingValue;
        }
        
        public Out(Capacity portCapacity)
        {
            capacity = portCapacity;
            direction = Direction.Output;
            rules = ConnectionRules.Exact;
            this.showBackingValue = false;
        }
        
        public Out(ConnectionRules rules)
        {
            capacity = Capacity.Single;
            direction = Direction.Output;
            this.rules = rules;
            this.showBackingValue = false;
        }
        
        public Out(bool showBackingValue, Capacity portCapacity)
        {
            capacity = portCapacity;
            direction = Direction.Output;
            rules = ConnectionRules.Exact;
            this.showBackingValue = showBackingValue;
        }
        
        public Out(bool showBackingValue = false, Capacity portCapacity = Capacity.Single, ConnectionRules rules = ConnectionRules.Exact)
        {
            capacity = portCapacity;
            direction = Direction.Output;
            this.rules = rules;
            this.showBackingValue = showBackingValue;
        }
    }
}