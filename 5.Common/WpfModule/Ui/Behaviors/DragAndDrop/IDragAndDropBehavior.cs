namespace Common.WpfModule.Ui.Behaviors.DragAndDrop
{
    public interface IDragAndDropBehavior
    {
        /// <summary>
        /// Сигнализирует в возможности бросить элемент над текущим элементом.
        /// </summary>
        bool IsAllowDrop { get; set; }
    }
}
