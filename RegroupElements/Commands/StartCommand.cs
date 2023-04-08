using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RegroupElements.ViewModel;
using RegroupElements.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RegroupElements.Commands
{
    [Regeneration(RegenerationOption.Manual)]
    [Transaction(TransactionMode.Manual)]
    public class StartCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                Assembly.LoadFrom(@"P:\03_БИБЛИОТЕКА\Revit_5_ПСМ\Скрипты C#\Библиотеки\AtomStyleLibrary\AtomStyleLibrary.dll");
            }
            catch
            {
                WarningWindow errorWindow = new WarningWindow("ОШИБКА", "Ошибка при загрузке библиотеки стилей");
                errorWindow.ShowDialog();

                return Result.Cancelled;
            }

            if (RegroupElementsApp.IsOpened)
            {
                WarningWindow errorWindow = new WarningWindow("ПРЕДУПРЕЖДЕНИЕ", "Программа уже запущена");
                errorWindow.ShowDialog();

                return Result.Cancelled;
            }

            Document doc = commandData.Application.ActiveUIDocument.Document;
            var allGroups = GetAllGroups(doc);

            try
            {
                if (allGroups == null || allGroups.Count() == 0)
                {
                    WarningWindow errorWindow = new WarningWindow("ПРЕДУПРЕЖДЕНИЕ", "Отсутствуют экземпляры групп");
                    errorWindow.ShowDialog();
                    return Result.Cancelled;
                }

                DataManageVM dataManageVM = new DataManageVM(doc);
                StartWindow startWindow = new StartWindow(dataManageVM);
                startWindow.ShowDialog();
            }
            catch (Exception e)
            {
                ExceptionWindow exceptionWindow = new ExceptionWindow(e.Message, e.StackTrace);
                exceptionWindow.ShowDialog();

                return Result.Cancelled;
            }

            return Result.Succeeded;
        }

        private IEnumerable<Element> GetAllGroups(Document doc)
        {
            return new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_IOSModelGroups)
                .WhereElementIsNotElementType()
                .ToElements();
        }
    }

}
