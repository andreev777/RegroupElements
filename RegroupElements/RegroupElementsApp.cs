using Autodesk.Revit.UI;
using RegroupElements.Commands;
using System;
using System.Linq;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace RegroupElements
{
    public class RegroupElementsApp : IExternalApplication
    {
        public static bool IsOpened = false;

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            string tabName = "Надстройки АСК";
            string panelName = "Группы";
            string assemblyName = Assembly.GetExecutingAssembly().Location;
            string commandName = typeof(StartCommand).FullName;

            string toolTip = "Объединяет элементы из выбранных групп в новую группу";

            try
            {
                application.CreateRibbonTab(tabName);
            }
            catch { }

            var ribbonPanels = application.GetRibbonPanels(tabName);
            var ribbonPanel = ribbonPanels.FirstOrDefault(panel => panel.Name == panelName) ?? application.CreateRibbonPanel(tabName, panelName);

            PushButtonData startCommandButtonData = new PushButtonData("StartCommand", "Объединение\nгрупп", assemblyName, commandName);

            PushButton startCommandButton = ribbonPanel.AddItem(startCommandButtonData) as PushButton;
            startCommandButton.ToolTip = toolTip;

            BitmapImage startCommandButtonLogo = new BitmapImage(new Uri("pack://application:,,,/RegroupElements;component/Images/regroupImage.png"));
            startCommandButton.LargeImage = startCommandButtonLogo;

            return Result.Succeeded;
        }
    }
}
