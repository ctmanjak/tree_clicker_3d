public static class RepositoryFactory
{
    public static IAccountRepository CreateAccountRepository()
    {
        return new LocalAccountRepository();
    }

    public static ICurrencyRepository CreateCurrencyRepository()
    {
        return new LocalCurrencyRepository();
    }

    public static IUpgradeRepository CreateUpgradeRepository()
    {
        return new LocalUpgradeRepository();
    }
}
