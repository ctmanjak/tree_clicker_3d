namespace Core
{
    public interface ILocalRepository<T> : IRepository<T> where T : IIdentifiable
    {
        T Get(string id);
    }
}