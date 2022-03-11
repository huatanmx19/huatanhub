using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NetTopologySuite.Geometries;

namespace HuatanHub.Entities
{
    public class Tramo : EntidadBase
    {
        public string Zona { get; set; }
        public string Nombre { get; set; }
        public string IniVialidad { get; set; }
        public string FinVialidad { get; set; }
        public Point Ini { get; set; }
        public Point Fin { get; set; }
        public Point Centroide { get; set; }
        public Point Bodega { get; set; }
        public string Color { get; set; }
        public string UrlKml { get; set; }
    }
}
