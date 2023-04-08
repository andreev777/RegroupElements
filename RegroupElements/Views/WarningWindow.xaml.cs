using System.Windows;
using System.Windows.Input;

namespace RegroupElements.Views
{
    public partial class WarningWindow : Window
    {
        /// <summary>
        /// Создает окно с текстом предупреждения.
        /// </summary>
        public WarningWindow(string title, string warningMessage)
        {
            InitializeComponent();
            tableHeaderTextBlock.Text = title;
            warningMessageTextBlock.Text = warningMessage;
        }

        public WarningWindow(string title, string warningMessage, int height, int width)
        {
            InitializeComponent();
            Height = MinHeight = MaxHeight = height;
            Width = MinWidth = MaxWidth = width;

            tableHeaderTextBlock.Text = title;
            warningMessageTextBlock.Text = warningMessage;
        }

        #region МЕТОДЫ ПЕРЕТАСКИВАНИЯ И ЗАКРЫТИЯ ОКНА

        private void DragWithMouse(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (WindowState == WindowState.Maximized)
                {
                    Top = 0;
                    WindowState = WindowState.Normal;
                }

                DragMove();
            }
        }

        private void CommandBinding_CanExecute_1(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CommandBinding_Executed_1(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.CloseWindow(this);
        }

        #endregion МЕТОДЫ ПЕРЕТАСКИВАНИЯ И ЗАКРЫТИЯ ОКНА
    }
}
