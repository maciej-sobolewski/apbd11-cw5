using System.ComponentModel.DataAnnotations;

namespace apbd11_cw5.Models
{
    public class Medicament
    {
        public int IdMedicament { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public ICollection<PrescriptionMedicament> PrescriptionMedications { get; set; }
    }
}
