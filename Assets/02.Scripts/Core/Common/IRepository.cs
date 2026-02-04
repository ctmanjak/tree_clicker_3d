using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Core
{
    public interface IRepository<T>
    {
        UniTask<List<T>> Initialize();
        void Save(T item);
    }
}
