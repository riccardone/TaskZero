namespace TaskZero.ReadModels.Elastic
{
    public interface IIndexer<T> where T : class
    {
        void Index(T doc);
        void Remove(string id);
    }
}
