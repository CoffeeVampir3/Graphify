namespace GraphFramework
{
    public interface IDataBlackboard
    {
        bool TryGetValue<T>(string lookupKey, out T val);
        object this[string key] { set; get; }
    }
}