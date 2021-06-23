namespace ServiceLogonMultifactor.Enrichers
{
    public interface IEnricher<T>
    {
        T Enrich(T data);
    }
}