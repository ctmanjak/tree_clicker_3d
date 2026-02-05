using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Core
{
    public interface IWriteBatchWrapper
    {
        void Set<T>(string collection, string documentId, T data);
        UniTask CommitAsync();
    }

    public interface IFirebaseStoreService
    {
        bool IsInitialized { get; }
        UniTask Initialize();
        UniTask SetDocument<T>(string collection, T data) where T : IIdentifiable;
        UniTask<List<T>> GetCollection<T>(string collection) where T : IIdentifiable;
        void SetDocumentAsync<T>(string collection, T data) where T : IIdentifiable;
        UniTask<long> GetServerTimeAsync();
        IWriteBatchWrapper CreateWriteBatch();
    }
}
