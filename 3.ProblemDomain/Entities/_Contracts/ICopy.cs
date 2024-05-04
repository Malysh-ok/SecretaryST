namespace ProblemDomain.Entities._Contracts;

/// <summary>
/// Интерфейс, реализующий копирование.
/// </summary>
public interface ICopy
{
    /// <summary>
    /// Копирование экземпляра класса в <paramref name="destination"/>.
    /// </summary>
    public void Copy(IAbstractEntity destination);
}