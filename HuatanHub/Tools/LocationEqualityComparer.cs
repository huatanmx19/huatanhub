using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HuatanHub.Entities;

namespace HuatanHub.Tools
{
    public class LocationEqualityComparer : IEqualityComparer<Location>
    {
        public bool Equals(Location? x, Location? y)
        {
            if (x == null || y == null)
                return false;

            return x.EmpleadoId.Equals(y.EmpleadoId);
        }

        public int GetHashCode(Location obj)
        {
            return obj.EmpleadoId.GetHashCode();
        }
    }
}
