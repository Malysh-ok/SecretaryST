namespace ProblemDomain.Entities._Contracts;

/// <summary>
/// Интерфейс сущности, реализующий копирование.
/// </summary>
public interface ICopyEntity
{
    /// <summary>
    /// Копирование экземпляра класса в <paramref name="destination"/>.
    /// </summary>
    public void Copy(IAbstractEntity destination);
}