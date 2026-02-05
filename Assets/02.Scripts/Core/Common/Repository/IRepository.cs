using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Core
{
    public interface IRepository<T> where T : IIdentifiable
    {
        string CollectionName { get; }
        UniTask<List<T>> Initialize();
        void Save(T item);
    }
}
