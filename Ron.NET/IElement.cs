namespace Ron.NET;

public interface IElement
{
    public IElement this[int index] { get; set; }
    
    public IElement this[string elementName] { get; set; }

    public T As<T>();

    public string Serialize(SerializeOptions options = SerializeOptions.None);
}