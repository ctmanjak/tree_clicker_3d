public interface ICurrencyRepository
{
    void Initialize();
    void Save();
    Currency GetCurrency(CurrencyType type);
    void SaveCurrency(Currency currency);
}
