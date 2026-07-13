using System.Windows;
using Common.WpfModule.Ui.Views._Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Common.WpfModule.Ui.Services._Contracts;

/// <summary>
/// Сервис для управления представлениями (View) приложения.
/// </summary>
/// <remarks>
/// <para>
/// Предоставляет методы для открытия дочерних окон с автоматическим разрешением ViewModel
/// через DI-контейнер. Все дочерние представления создаются с использованием
/// <see cref="ActivatorUtilities"/>, что позволяет передавать зависимости через конструктор.
/// </para>
/// <para>
/// Сервис также управляет фокусом: при активации главного окна последнее активное дочернее
/// окно автоматически получает фокус (если оно открыто).
/// </para>
/// </remarks>
public interface IViewService
{
    /// <summary>
    /// Инициализирует сервис с главным представлением (окном).
    /// </summary>
    /// <remarks>
    /// Вызывается один раз при запуске приложения. Главное представление используется
    /// как владелец для всех дочерних представлений и передаётся в ViewModel
    /// как <see cref="IViewWithResources"/> при создании через DI.
    /// </remarks>
    /// <param name="mainView">Главное представление (владелец дочерних представлений).</param>
    void Initialize(Window mainView);

    /// <summary>
    /// Открывает дочернее представление, создавая ViewModel через DI-контейнер.
    /// </summary>
    /// <remarks>
    /// ViewModel создаётся через <see cref="ActivatorUtilities.CreateInstance{T}(IServiceProvider, object[])"/>,
    /// что позволяет автоматически разрешать все зависимости из DI-контейнера.
    /// Если конструктор ViewModel требует <see cref="IViewWithResources"/>,
    /// то последний будет передан из главного представления.
    /// </remarks>
    /// <typeparam name="TView">Тип представления (регистрируется в DI).</typeparam>
    /// <typeparam name="TViewModel">Тип ViewModel (регистрируется в DI).</typeparam>
    void ShowView<TView, TViewModel>() 
        where TView : Window, new() 
        where TViewModel : class;

    /// <summary>
    /// Открывает дочернее представление с переданной ViewModel.
    /// </summary>
    /// <remarks>
    /// Используется, когда ViewModel уже создана или требует дополнительной инициализации.
    /// </remarks>
    /// <typeparam name="TView">Тип представления (регистрируется в DI).</typeparam>
    /// <param name="viewModel">ViewModel, которая будет установлена как DataContext.</param>
    void ShowView<TView>(object viewModel) 
        where TView : Window, new();

    /// <summary>
    /// Открывает дочернее представление, создавая ViewModel через DI,
    /// и выполняет дополнительную инициализацию через делегат.
    /// </summary>
    /// <remarks>
    /// Полезно, если нужно передать параметры, которые не являются зависимостями (например, строки, числа).
    /// </remarks>
    /// <typeparam name="TView">Тип представления (регистрируется в DI).</typeparam>
    /// <typeparam name="TViewModel">Тип ViewModel (регистрируется в DI).</typeparam>
    /// <param name="initializer">Делегат для дополнительной настройки ViewModel (вызывается после создания последней).</param>
    void ShowView<TView, TViewModel>(Action<TViewModel> initializer) 
        where TView : Window, new() 
        where TViewModel : class;

    /// <summary>
    /// Проверяет, открыто ли представление указанного типа, и если да — активирует его.
    /// </summary>
    /// <remarks>
    /// Используется для предотвращения повторного открытия уже существующего окна.
    /// </remarks>
    /// <typeparam name="TView">Тип представления.</typeparam>
    /// <returns>true, если представление найдено и активировано; иначе false.</returns>
    bool TryActivateView<TView>() where TView : Window;
}