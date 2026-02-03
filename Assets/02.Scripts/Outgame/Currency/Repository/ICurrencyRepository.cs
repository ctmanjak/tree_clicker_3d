using Cysharp.Threading.Tasks;

public interface ICurrencyRepository
{
    UniTask Initialize();
    void Save();
    Currency GetCurrency(CurrencyType type);
    void SaveCurrency(Currency currency);
}
