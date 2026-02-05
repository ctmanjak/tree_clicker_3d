namespace Core
{
    public interface ISyncCoordinator : IFlushable
    {
        void RegisterPending<T>(string collection, T item, IRepository<T> localRepo)
            where T : IIdentifiable, ITimestamped;
    }
}
