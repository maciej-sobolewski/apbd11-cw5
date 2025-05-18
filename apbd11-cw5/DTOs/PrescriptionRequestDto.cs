namespace apbd11_cw5.DTOs
{
    public class PrescriptionRequestDto
    {
        public DateTime Date { get; set; }
        public DateTime DueDate { get; set; }
        public int? IdPatient { get; set; }
        public PatientDto? Patient { get; set; }
        public int IdDoctor { get; set; }
        public List<MedicamentEntryDto> Medicaments { get; set; }
    }
}
