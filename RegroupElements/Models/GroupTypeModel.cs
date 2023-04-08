using Prism.Mvvm;
using System.Collections.Generic;

namespace RegroupElements.Models
{
    public class GroupTypeModel : BindableBase
    {
        public int Id { get; set; }
        public List<GroupModel> GroupModels { get; set; }
        public string Name { get; set; }

        private bool isSelected;
        public bool IsSelected
        {
            get => isSelected;
            set
            {
                isSelected = value;
                RaisePropertyChanged(nameof(IsSelected));
            }
        }

        public GroupTypeModel(int id, List<GroupModel> groupModels, string name)
        {
            Id = id;
            GroupModels = groupModels;
            Name = name;
            IsSelected = false;
        }
    }
}
