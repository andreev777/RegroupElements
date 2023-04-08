using Prism.Mvvm;

namespace RegroupElements.Models
{
    public class GroupModel : BindableBase
    {
        public int Id { get; set; }
        public int GroupLevelId { get; set; }
        public string Name { get; set; }

        public GroupModel(int id, int groupLevelId, string name)
        {
            Id = id;
            GroupLevelId = groupLevelId;
            Name = name;
        }
    }
}
