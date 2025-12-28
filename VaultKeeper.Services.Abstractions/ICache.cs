namespace VaultKeeper.Services.Abstractions;

public interface ICache<T>
{
    T? Get();

    void Set(T value);

    void Clear();
}
