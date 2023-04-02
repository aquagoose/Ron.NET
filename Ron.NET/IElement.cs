namespace Ron.NET;

public interface IElement
{
    public IElement this[int index] { get; }
    
    public IElement this[string elementName] { get; }

    public string Serialize(SerializeOptions options = SerializeOptions.None);
}