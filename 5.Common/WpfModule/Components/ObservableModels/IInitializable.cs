namespace Common.WpfModule.Components.ObservableModels;

/// <summary>
/// Интерфейс уведомления модели представления об инициализации с передачей параметра для инициализации.
/// </summary>
/// <typeparam name="T">Тип передаваемого параметра.</typeparam>
public interface IInitializable<in T>
{
    /// <summary>
    /// Инициализация модели представления.
    /// </summary>
    /// <param name="param">Аргумент.</param>
    void Initialize(T param);
}

public interface IInitializable
{
    /// <summary>
    /// Инициализация модели представления.
    /// </summary>
    void Initialize();
}