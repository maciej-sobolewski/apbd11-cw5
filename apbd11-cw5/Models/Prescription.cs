using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace apbd11_cw5.Models
{
    public class Prescription
    {
        public int IdPrescription { get; set; }
        public DateTime Date { get; set; }
        public DateTime DueDate { get; set; }
        public int IdPatient { get; set; }
        public Patient Patient { get; set; }
        public int IdDoctor { get; set; }
        public Doctor Doctor { get; set; }
        public ICollection<PrescriptionMedicament> PrescriptionMedications { get; set; }
    }
}
