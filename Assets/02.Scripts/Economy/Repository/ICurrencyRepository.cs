public interface ICurrencyRepository
{
    Currency GetCurrency(CurrencyType type);
    void SaveCurrency(Currency currency);
}
