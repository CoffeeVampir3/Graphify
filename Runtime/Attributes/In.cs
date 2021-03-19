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
            this.showBackingValue = false;
        }
        
        public In(Capacity portCapacity)
        {
            capacity = portCapacity;
            direction = Direction.Input;
            this.showBackingValue = false;
        }
        
        public In(bool showBackingValue = false, Capacity portCapacity = Capacity.Single)
        {
            capacity = portCapacity;
            direction = Direction.Input;
            this.showBackingValue = showBackingValue;
        }
    }
}