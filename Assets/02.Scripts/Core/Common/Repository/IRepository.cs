using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public interface IRepository<T>
{
    UniTask<List<T>> Initialize();
    void Save(T item);
}
