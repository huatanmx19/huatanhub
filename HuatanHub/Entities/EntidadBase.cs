using System;

namespace HuatanHub.Entities
{
    public class EntidadBase
    {
        public EntidadBase()
        {
            Active = true;
            CreatedAt = DateTime.Now;
            Ejercicio = DateTime.Now.Year;
        }

        public int Id { get; set; }
        
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeleteAt { get; set; }

        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public string DeleteBy { get; set; }
        public int Ejercicio { get; set; }
        public bool Active { get; set; }
    }
}
