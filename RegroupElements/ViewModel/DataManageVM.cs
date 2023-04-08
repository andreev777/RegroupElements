using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Prism.Mvvm;
using RegroupElements.Models;
using RegroupElements.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;

namespace RegroupElements.ViewModel
{
    [Regeneration(RegenerationOption.Manual)]
    [Transaction(TransactionMode.Manual)]
    public class DataManageVM : BindableBase
    {
        private Document _doc;
        private string _result = string.Empty;

        private string _newGroupName = string.Empty;
        public string NewGroupName 
        {
            get => _newGroupName;
            set
            {
                _newGroupName = value;
                RaisePropertyChanged(nameof(NewGroupName));
            }
        }

        private int _selectedGroupsCount;
        public int SelectedGroupsCount
        {
            get => _selectedGroupsCount;
            set 
            { 
                _selectedGroupsCount = value; 
                RaisePropertyChanged(nameof(SelectedGroupsCount));
            }
        }

        private int _groupsOnLevelTotalCount;
        public int GroupsOnLevelTotalCount
        {
            get => _groupsOnLevelTotalCount;
            set
            {
                _groupsOnLevelTotalCount = value;
                RaisePropertyChanged(nameof(GroupsOnLevelTotalCount));
            }
        }

        public ICollectionView GroupTypesCollection { get; set; }
        public List<GroupLevel> GroupLevelCollection { get; set; }
        public string DocumentTitle { get; private set; }
        public bool DeleteGroupsAfterJoin { get; set; }

        #region COMMANDS
        private RelayCommand _selectLevelCommand;
        public RelayCommand SelectLevelCommand
        {
            get
            {
                return _selectLevelCommand ?? (_selectLevelCommand = new RelayCommand(obj =>
                {
                    UnselectAllGroups();
                    FilterElements();
                    SetGroupsOnLevelTotalCount();
                }));
            }
        }

        private RelayCommand _selectGroupCommand;
        public RelayCommand SelectGroupCommand
        {
            get
            {
                return _selectGroupCommand ?? (_selectGroupCommand = new RelayCommand(obj => SetSelectedGroupsCount()));
            }
        }

        private RelayCommand _selectAllGroupsCommand;
        public RelayCommand SelectAllGroupsCommand
        {
            get
            {
                return _selectAllGroupsCommand ?? (_selectAllGroupsCommand = new RelayCommand(obj => SelectAllGroups()));
            }
        }

        private RelayCommand _unselectAllGroupsCommand;
        public RelayCommand UnselectAllGroupsCommand
        {
            get
            {
                return _unselectAllGroupsCommand ?? (_unselectAllGroupsCommand = new RelayCommand(obj => UnselectAllGroups()));
            }
        }

        private RelayCommand _regroupElementsCommand;
        public RelayCommand RegroupElementsCommand
        {
            get
            {
                return _regroupElementsCommand ?? (_regroupElementsCommand = new RelayCommand(obj => RegroupElements()));
            }
        }

        public Action HideAction { get; set; }
        public Action CloseAction { get; set; }
        #endregion

        public DataManageVM(Document doc)
        {
            _doc = doc;
            DocumentTitle = _doc.Title;

            var groups = GetAllGroups();
            var groupModels = GetAllGroupModels();
            GroupTypesCollection = CollectionViewSource.GetDefaultView(groupModels);
            GroupLevelCollection = GetAllGroupLevels(groups);

            FilterElements();
            SetGroupsOnLevelTotalCount();
        }

        #region METHODS
        private List<Element> GetAllGroups()
        {
            return new FilteredElementCollector(_doc)
                .OfCategory(BuiltInCategory.OST_IOSModelGroups)
                .WhereElementIsNotElementType()
                .ToElements()
                .ToList();
        }

        private List<Element> GetAllGroupTypes()
        {
            return new FilteredElementCollector(_doc)
                .OfCategory(BuiltInCategory.OST_IOSModelGroups)
                .WhereElementIsElementType()
                .ToElements()
                .OrderBy(element => element.Name)
                .ToList();
        }

        private List<GroupTypeModel> GetAllGroupModels()
        {
            var systemGroupTypes = GetAllGroupTypes();
            var groupTypes = new List<GroupTypeModel>();

            foreach (var systemGroupType in systemGroupTypes)
            {
                var allGroupsOfType = new FilteredElementCollector(_doc).OfCategory(BuiltInCategory.OST_IOSModelGroups)
                .WhereElementIsNotElementType()
                .ToElements()
                .Where(element => element.GetTypeId() == systemGroupType.Id)
                .ToList();

                var allGroupModelsOfType = new List<GroupModel>();

                foreach (var group in allGroupsOfType)
                {
                    var newGroupModel = new GroupModel(group.Id.IntegerValue, group.LevelId.IntegerValue, group.Name);
                    allGroupModelsOfType.Add(newGroupModel);
                }

                var newGroupType = new GroupTypeModel(systemGroupType.Id.IntegerValue, allGroupModelsOfType, systemGroupType.Name);
                groupTypes.Add(newGroupType);
            }

            return groupTypes;
        }

        private List<GroupLevel> GetAllGroupLevels(IList<Element> groups)
        {
            var groupSystemLevelIds = groups.Select(group => group.LevelId).Distinct().ToList();
            var groupSystemLevels = groupSystemLevelIds.Select(id => _doc.GetElement(id) as Level).ToList();
            var groupSystemLevelsOrdered = groupSystemLevels.OrderBy(level => level.Elevation).ToList();

            var groupLevels = new List<GroupLevel>();

            foreach (var systemLevel in groupSystemLevelsOrdered)
            {
                var level = new GroupLevel(systemLevel.Id.IntegerValue, systemLevel.Name);
                if (groupLevels.Count == 0)
                {
                    level.IsSelected = true;
                }

                groupLevels.Add(level);
            }

            return groupLevels;
        }

        private void SetSelectedGroupsCount()
        {
            var groupModels = GroupTypesCollection.Cast<GroupTypeModel>().ToList();
            SelectedGroupsCount = groupModels.Where(group => group.IsSelected).Count();
        }

        private void SetGroupsOnLevelTotalCount()
        {
            GroupsOnLevelTotalCount = GroupTypesCollection.Cast<GroupTypeModel>().ToList().Count();
        }

        private void FilterElements()
        {
            var selectedLevelId = GetSelectedLevelId();
            GroupTypesCollection.Filter = groupType => (groupType as GroupTypeModel).GroupModels.Any(groupModel => groupModel.GroupLevelId == selectedLevelId);
        }

        private int GetSelectedLevelId()
        {
            var selectedLevels = GroupLevelCollection.Where(level => level.IsSelected).Select(level => level.Id).ToList();
            return selectedLevels[0];
        }

        private void SelectAllGroups()
        {
            var groupModels = GroupTypesCollection.Cast<GroupTypeModel>().ToList();

            foreach (var groupModel in groupModels)
            {
                groupModel.IsSelected = true;
            }

            SetSelectedGroupsCount();
        }

        private void UnselectAllGroups()
        {
            var groupModels = GroupTypesCollection.Cast<GroupTypeModel>().ToList();

            foreach (var groupModel in groupModels)
            {
                groupModel.IsSelected = false;
            }

            SetSelectedGroupsCount();
        }

        private void RegroupElements()
        {
            var groupTypeModels = GroupTypesCollection.Cast<GroupTypeModel>().ToList();
            var selectedGroupTypeModels = groupTypeModels.Where(group => group.IsSelected).ToList();

            if (IsNewGroupNameEmpty())
                return;

            if (IsNewGroupNameExisted())
                return;

            if (!IsAnyGroupSelected(selectedGroupTypeModels))
                return;

            var selectedGroupModelIds = selectedGroupTypeModels.SelectMany(groupType => groupType.GroupModels.Select(group => group.Id)).ToList();
            var selectedGroups = selectedGroupModelIds.Select(id => _doc.GetElement(new ElementId(id)) as Group).ToList();

            var selectedLevelId = GetSelectedLevelId();
            var filteredGroupsByLevelId = GetFilteredGroupsByLevelId(selectedGroups, selectedLevelId);

            JoinGroups(filteredGroupsByLevelId, out bool newGroupCreatedSuccess);

            if (newGroupCreatedSuccess)
            {
                HideAction();

                ReportWindow resultnWindow = new ReportWindow(_result);
                resultnWindow.ShowDialog();

                CloseAction();
            }
            else
            {
                return;
            }
        }
        
        private bool IsNewGroupNameEmpty()
        {
            if (NewGroupName == string.Empty)
            {
                WarningWindow errorWindow = new WarningWindow("ПРЕДУПРЕЖДЕНИЕ", "Введите имя новой группы");
                errorWindow.ShowDialog();
                return true;
            }

            return false;
        }

        private bool IsNewGroupNameExisted()
        {
            var systemGroupTypes = GetAllGroupTypes();
            var systemGroupTypeNames = systemGroupTypes.Select(group => group.Name).ToList();

            if (systemGroupTypeNames.Any(systemGroupTypeName => systemGroupTypeName == NewGroupName))
            {
                WarningWindow errorWindow = new WarningWindow("ПРЕДУПРЕЖДЕНИЕ", "Группа с таким именем уже существует");
                errorWindow.ShowDialog();
                return true;
            }

            return false;
        }

        private bool IsAnyGroupSelected(List<GroupTypeModel> selectedGroupModels)
        {
            if (selectedGroupModels.Count() == 0)
            {
                WarningWindow errorWindow = new WarningWindow("ПРЕДУПРЕЖДЕНИЕ", "Выберите группы");
                errorWindow.ShowDialog();
                return false;
            }

            return true;
        }

        private List<Group> GetFilteredGroupsByLevelId(List<Group> selectedGroups, int selectedLevelId)
        {
            var filteredGroupsByLevelId = new List<Group>();

            if (selectedGroups != null)
            {
                foreach (var group in selectedGroups)
                {
                    if (group.LevelId.IntegerValue == selectedLevelId)
                    {
                        filteredGroupsByLevelId.Add(group);
                    }
                }
            }

            return filteredGroupsByLevelId;
        }

        private void JoinGroups(List<Group> selectedGroups, out bool newGroupCreatedSuccess)
        {
            var parentGroups = new List<Group>();
            var allEmbeddedElementIds = new List<ElementId>();
            var allGroupTypes = selectedGroups.Select(group => _doc.GetElement(group.GetTypeId())).ToList();

            using (Transaction transactionUngroup = new Transaction(_doc, "Объединение групп"))
            {
                transactionUngroup.Start();

                foreach (var group in selectedGroups)
                {
                    var embeddedElementIds = GetEmbeddedElementIds(group);

                    if (group.GroupId != null)
                    {
                        var parentElement = _doc.GetElement(group.GroupId);
                        if (parentElement != null)
                        {
                            var parentGroup = parentElement as Group;
                            parentGroups.Add(parentGroup);
                        }

                        _doc.Delete(group.Id);
                    }

                    if (embeddedElementIds != null)
                    {
                        var embeddedElementIdsTemp = new List<ElementId>();

                        foreach (var elementId in embeddedElementIds)
                        {
                            if (_doc.GetElement(elementId).Category?.Id.IntegerValue == -2000095)
                            {
                                var groupTemp = _doc.GetElement(elementId) as Group;
                                var elementIdsTemp = GetEmbeddedElementIds(groupTemp);

                                if (elementIdsTemp != null)
                                {
                                    embeddedElementIdsTemp.AddRange(elementIdsTemp);
                                }
                            }
                        }

                        embeddedElementIds.AddRange(embeddedElementIdsTemp);
                    }
                    
                    allEmbeddedElementIds.AddRange(embeddedElementIds);
                }

                var allEmbeddedElementIdsUniq = allEmbeddedElementIds.Distinct(new ElementIdIEqualityComparer()).ToList();

                try
                {
                    if (allEmbeddedElementIdsUniq.Count() > 0)
                    {
                        var group = _doc.Create.NewGroup(allEmbeddedElementIdsUniq);
                        group.GroupType.Name = NewGroupName;
                    }
                    else
                    {
                        WarningWindow errorWindow = new WarningWindow("ПРЕДУПРЕЖДЕНИЕ", "Выбранные группы не содержат элементов");
                        errorWindow.ShowDialog();
                        newGroupCreatedSuccess = true;
                        return;
                    }
                }
                catch (Exception e)
                {
                    ExceptionWindow exceptionWindow = new ExceptionWindow(e.Message, e.StackTrace);
                    exceptionWindow.ShowDialog();
                    CloseAction();
                    newGroupCreatedSuccess = false;
                    return;
                }

                // Разгруппирование всех родительских групп
                foreach (var parentGroup in parentGroups)
                {
                    try
                    {
                        parentGroup.UngroupMembers();
                    }
                    catch (Exception e)
                    {
                        ExceptionWindow exceptionWindow = new ExceptionWindow(e.Message, e.StackTrace);
                        exceptionWindow.ShowDialog();
                        CloseAction();
                        newGroupCreatedSuccess = false;
                        return;
                    }
                }

                var allEmptyGroupTypes = allGroupTypes.Where(groupType => IsElementTypeEmpty(groupType)).ToList();

                if (DeleteGroupsAfterJoin)
                {
                    DeleteEmptyJoinedGroups(allEmptyGroupTypes);
                }

                transactionUngroup.Commit();
            }

            _result += $"Создана группа: {NewGroupName}\n\n";
            _result += $"Объединено групп: {selectedGroups.Count}";
            newGroupCreatedSuccess = true;
        }

        private List<ElementId> GetEmbeddedElementIds(Group group)
        {
            try
            {
                var embeddedElementIds = group.UngroupMembers().ToList();
                return embeddedElementIds;
            }
            catch
            {
                return null;
            }
        }

        private bool IsElementTypeEmpty(Element element)
        {
            var elementId = element.Id.IntegerValue;
            return new FilteredElementCollector(_doc)
                .OfClass(typeof(Group))
                .Where(el => el.GetTypeId().IntegerValue.Equals(elementId)).Any() == true? false : true;
        }

        private void DeleteEmptyJoinedGroups(List<Element> allEmptyGroupTypes)
        {
            var allEmptyGroupTypeIds = allEmptyGroupTypes.Select(group => group.Id).ToList();
            _doc.Delete(allEmptyGroupTypeIds);
        }
        #endregion
    }
}
