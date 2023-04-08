using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace RegroupElements.Models
{
    public class ElementIdIEqualityComparer : IEqualityComparer<ElementId>
    {
        public bool Equals(ElementId x, ElementId y)
        {
            if (x == null && y == null)
                return true;
            else if (x == null || y == null)
                return false;
            else if (x.IntegerValue == y.IntegerValue)
                return true;
            else
                return false;
        }

        public int GetHashCode(ElementId obj)
        {
            return obj.GetHashCode();
        }
    }
}
