namespace NValidate.Internal
{
    public interface Filter
    {
        // bool Filter(... object[] args);
    }

    public class DefaultFilter : Filter
    {
        public bool Filter() => true;
    }
}