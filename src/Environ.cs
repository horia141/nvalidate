namespace NValidate
{
    public interface Environ
    {
	Environ Add(object entity);
	T Get<T>();
    }
}
