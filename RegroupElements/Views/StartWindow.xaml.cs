using RegroupElements.ViewModel;
using System;
using System.Windows;
using System.Windows.Input;

namespace RegroupElements.Views
{
    public partial class StartWindow : Window
    {
        public StartWindow(DataManageVM dataManageVM)
        {
            InitializeComponent();
            DataContext = dataManageVM;

            if (dataManageVM.HideAction == null)
            {
                dataManageVM.HideAction = new Action(Hide);
            }

            if (dataManageVM.CloseAction == null)
            {
                dataManageVM.CloseAction = new Action(Close);
            }
        }

        private void newGroupNameTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            defaultTextBlock.Visibility = System.Windows.Visibility.Hidden;
        }

        private void newGroupNameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (newGroupNameTextBox.Text == string.Empty)
            {
                defaultTextBlock.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void helpButton_Click(object sender, RoutedEventArgs e)
        {
            HelpWindow helpWindow = new HelpWindow();
            helpWindow.ShowDialog();
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
            RegroupElementsApp.IsOpened = false;
            SystemCommands.CloseWindow(this);
        }
        #endregion МЕТОДЫ ПЕРЕТАСКИВАНИЯ И ЗАКРЫТИЯ ОКНА
    }
}
