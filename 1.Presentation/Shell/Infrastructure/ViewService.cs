using System;
using System.Windows;
using Common.WpfModule.Ui.Services._Contracts;
using Common.WpfModule.Ui.Views._Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Presentation.Shell.Infrastructure
{
    /// <summary>
    /// Реализация сервиса управления представлениями с поддержкой фокуса и активации.
    /// </summary>
    public class ViewService : IViewService
    {
        private readonly IServiceProvider _serviceProvider;
        private Window _mainView = null!;
        private Window? _lastActiveChildView;
        private bool _shouldActivateChild;                  // флаг для возврата фокуса при активации главного представления

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="serviceProvider">DI-контейнер для разрешения зависимостей представлений (View) и ViewModel.</param>
        public ViewService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc />
        public void Initialize(Window mainView)
        {
            _mainView = mainView ?? throw new ArgumentNullException(nameof(mainView));
            _mainView.Activated += OnMainViewActivated;
        }

        /// <inheritdoc />
        public void ShowView<TView, TViewModel>()
            where TView : Window, new()
            where TViewModel : class
        {
            // Создаём ViewModel через DI, передавая главное представление как IViewWithResources
            var viewModel = ActivatorUtilities.CreateInstance<TViewModel>(_serviceProvider, _mainView);
            
            ShowView<TView>(viewModel);
        }

        /// <inheritdoc />
        public void ShowView<TView>(object viewModel)
            where TView : Window, new()
        {
            // Если представление этого типа уже открыто — активируем его и выходим
            if (TryActivateView<TView>())
                return;

            // Создаём дочернее представление через DI
            var childView = ActivatorUtilities.CreateInstance<TView>(_serviceProvider);
            childView.Owner = _mainView;
            childView.DataContext = viewModel;

            // Подписываемся на события только если окно НЕ реализует INoFocusTracking
            // ReSharper disable once SuspiciousTypeConversion.Global
            if (childView is not IViewWithoutFocusTracking)
            {
                SubscribeChildView(childView);
                _shouldActivateChild = true;    // Устанавливаем флаг для возврата фокуса при активации гл. представления
            }

            childView.Show();
        }

        /// <inheritdoc />
        public void ShowView<TView, TViewModel>(Action<TViewModel>? initializer)
            where TView : Window, new()
            where TViewModel : class
        {
            // Создаём ViewModel через DI, передавая главное представление как IViewWithResources
            var viewModel = ActivatorUtilities.CreateInstance<TViewModel>(_serviceProvider, _mainView);
            
            initializer?.Invoke(viewModel);
            ShowView<TView>(viewModel);
        }

        /// <inheritdoc />
        public bool TryActivateView<TView>() where TView : Window
        {
            foreach (Window view in _mainView.OwnedWindows)
            {
                if (view is TView)
                {
                    if (view.IsVisible)
                        view.Activate();
                    else
                        view.Show();
                    return true;
                }
            }

            return false;
        }

        #region [---------- Приватные методы ----------]

        /// <summary>
        /// Подписываемся на события активации и закрытия дочернего окна.
        /// </summary>
        private void SubscribeChildView(Window childView)
        {
            childView.Activated += OnChildViewActivated;
            childView.Closed += OnChildViewClosed;
        }

        /// <summary>
        /// Отписываемся от событий дочернего окна.
        /// </summary>
        private void UnsubscribeChildView(Window childView)
        {
            childView.Activated -= OnChildViewActivated;
            childView.Closed -= OnChildViewClosed;
        }

        /// <summary>
        /// Обработчик активации дочернего окна: запоминает его как последнее активное.
        /// </summary>
        private void OnChildViewActivated(object? sender, EventArgs e)
        {
            _lastActiveChildView = sender as Window;
        }

        /// <summary>
        /// Обработчик закрытия дочернего окна: очищает ссылку и отписывается от событий.
        /// </summary>
        private void OnChildViewClosed(object? sender, EventArgs e)
        {
            if (sender is Window closingView)
            {
                if (_lastActiveChildView == closingView)
                    _lastActiveChildView = null;

                UnsubscribeChildView(closingView);
            }
        }

        /// <summary>
        /// Обработчик активации главного окна: возвращает фокус на последнее дочернее окно (костыль).
        /// </summary>
        private void OnMainViewActivated(object? sender, EventArgs e)
        {
            // REMARK: Костыль - при закрытии Backstage фокус возвращается в главное окно,
            // и нам нужно перехватить это событие, чтобы активировать дочернее окно.

            // Если флаг установлен и есть последнее активное дочернее представление,
            // но оно не активно — активируем его и сбрасываем флаг.
            if (_shouldActivateChild && _lastActiveChildView != null && _lastActiveChildView.IsVisible && !_lastActiveChildView.IsActive)
            {
                _shouldActivateChild = false;
                _lastActiveChildView.Activate();
                _lastActiveChildView.Focus();
            }
        }

        #endregion
    }
}