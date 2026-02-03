using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public interface IFirebaseStoreService
{
    bool IsInitialized { get; }
    UniTask Initialize();
    UniTask SetDocument(string collection, string documentId, Dictionary<string, object> data);
    UniTask<Dictionary<string, object>> GetDocument(string collection, string documentId);
    UniTask<List<Dictionary<string, object>>> GetCollection(string collection);
    void SetDocumentAsync(string collection, string documentId, Dictionary<string, object> data);
}
