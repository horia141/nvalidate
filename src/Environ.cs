namespace NValidate
{
    /// <summary>
    /// An example xml doc
    /// </summary>
    public interface Environ
    {
	Environ Add(object entity);
	T Get<T>();
    }
}
