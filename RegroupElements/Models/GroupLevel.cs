using Prism.Mvvm;

namespace RegroupElements.Models
{
    public class GroupLevel : BindableBase
    {
        public int Id { get; set; }
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

        public GroupLevel(int id, string name)
        {
            Id = id;
            Name = name;
            IsSelected = false;
        }
    }
}
