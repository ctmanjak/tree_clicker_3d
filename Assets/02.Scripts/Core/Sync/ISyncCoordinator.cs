namespace Core
{
    public interface ISyncCoordinator : IFlushable
    {
        void RegisterPending<T>(string collection, T item, ILocalRepository<T> localRepo)
            where T : IIdentifiable, ITimestamped;

        void MarkDirty<T>(string collection, string id, ILocalRepository<T> localRepo)
            where T : IIdentifiable;
    }
}
