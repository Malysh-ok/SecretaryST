namespace ProblemDomain.Entities._Contracts;

/// <summary>
/// Интерфейс сущности, реализующий копирование.
/// </summary>
public interface IEntityCopyable
{
    /// <summary>
    /// Копирование экземпляра класса в <paramref name="destination"/>.
    /// </summary>
    /// <remarks>
    /// Необходимо копировать только свойства-данные.<br/>
    /// Id и связи (внешние ключи, навигационные свойства) не копируем.
    /// </remarks>
    public void Copy(IAbstractEntity destination);
}