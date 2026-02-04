using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Outgame
{
    public interface IFirebaseStoreService
    {
        bool IsInitialized { get; }
        UniTask Initialize();
        UniTask SetDocument<T>(string collection, T data) where T : IIdentifiable;
        UniTask<List<T>> GetCollection<T>(string collection) where T : IIdentifiable;
        void SetDocumentAsync<T>(string collection, T data) where T : IIdentifiable;
    }
}
