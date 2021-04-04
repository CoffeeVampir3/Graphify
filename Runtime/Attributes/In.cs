namespace GraphFramework.Attributes
{
    //Using preserve attribute so we don't get this stripped out in IL2CPP compilations
    /// <summary>
    /// Defines a ValuePort as an input with the given capacity.
    /// </summary>
    public class In : DirectionalAttribute
    {
        //Explicit constructors so they show up in autocomplete.
        public In()
        {
            capacity = Capacity.Single;
            direction = Direction.Input;
            rules = ConnectionRules.Exact;
            this.showBackingValue = false;
        }
        
        public In(bool showBackingValue)
        {
            capacity = Capacity.Single;
            direction = Direction.Input;
            rules = ConnectionRules.Exact;
            this.showBackingValue = showBackingValue;
        }
        
        public In(Capacity portCapacity)
        {
            capacity = portCapacity;
            direction = Direction.Input;
            rules = ConnectionRules.Exact;
            this.showBackingValue = false;
        }
        
        public In(ConnectionRules rules)
        {
            capacity = Capacity.Single;
            direction = Direction.Input;
            this.rules = rules;
            this.showBackingValue = false;
        }
        
        public In(bool showBackingValue, Capacity portCapacity)
        {
            capacity = portCapacity;
            direction = Direction.Input;
            rules = ConnectionRules.Exact;
            this.showBackingValue = showBackingValue;
        }
        
        public In(bool showBackingValue = false, Capacity portCapacity = Capacity.Single, ConnectionRules rules = ConnectionRules.Exact)
        {
            capacity = portCapacity;
            direction = Direction.Input;
            this.rules = rules;
            this.showBackingValue = showBackingValue;
        }
    }
}