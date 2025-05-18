using apbd11_cw5.Data;
using apbd11_cw5.DTOs;
using apbd11_cw5.Models;
using Microsoft.EntityFrameworkCore;

namespace apbd11_cw5.Services
{
    public class DbService : IDbService
    {
        private readonly DatabaseContext _ctx;
        public DbService(DatabaseContext ctx) => _ctx = ctx;

        public async Task<int> CreatePrescriptionAsync(PrescriptionRequestDto dto)
        {
            if (dto.Medicaments.Count > 10)
                throw new ArgumentException("Prescription cannot have more than 10 medications.");
            if (dto.DueDate < dto.Date)
                throw new ArgumentException("DueDate should be >= Date");

            var doctor = await _ctx.Doctors.FindAsync(dto.IdDoctor)
                ?? throw new KeyNotFoundException("Doctor does not exist.");

            Patient patient;
            if (dto.IdPatient.HasValue)
            {
                patient = await _ctx.Patients.FindAsync(dto.IdPatient.Value)
                    ?? new Patient { FirstName = dto.Patient.FirstName, LastName = dto.Patient.LastName, Birthdate = dto.Patient.Birthdate };
                if (patient.IdPatient == 0) _ctx.Patients.Add(patient);
            }
            else
            {
                patient = new Patient { FirstName = dto.Patient.FirstName, LastName = dto.Patient.LastName, Birthdate = dto.Patient.Birthdate };
                _ctx.Patients.Add(patient);
            }

            var presc = new Prescription { Date = dto.Date, DueDate = dto.DueDate, Doctor = doctor, Patient = patient, PrescriptionMedications = new List<PrescriptionMedicament>() };
            _ctx.Prescriptions.Add(presc);

            foreach (var m in dto.Medicaments)
            {
                var med = await _ctx.Medicaments.FindAsync(m.IdMedicament)
                    ?? throw new KeyNotFoundException($"Medicament {m.IdMedicament} does not exist.");
                presc.PrescriptionMedications.Add(new PrescriptionMedicament
                {
                    Prescription = presc,
                    Medicament = med,
                    Dose = m.Dose,
                    Details = m.Details
                });
            }

            await _ctx.SaveChangesAsync();
            return presc.IdPrescription;
        }

        public async Task<PatientDetailDto> GetPatientWithPrescriptionsAsync(int idPatient)
        {
            var patient = await _ctx.Patients
                .Include(p => p.Prescriptions)
                    .ThenInclude(pr => pr.Doctor)
                .Include(p => p.Prescriptions)
                    .ThenInclude(pr => pr.PrescriptionMedications)
                        .ThenInclude(pm => pm.Medicament)
                .FirstOrDefaultAsync(p => p.IdPatient == idPatient);

            if (patient == null) throw new KeyNotFoundException("Patient does not exist");

            return new PatientDetailDto
            {
                IdPatient = patient.IdPatient,
                FirstName = patient.FirstName,
                LastName = patient.LastName,
                Birthdate = patient.Birthdate,
                Prescriptions = patient.Prescriptions
                    .OrderBy(pr => pr.DueDate)
                    .Select(pr => new PrescriptionDetailDto
                    {
                        IdPrescription = pr.IdPrescription,
                        Date = pr.Date,
                        DueDate = pr.DueDate,
                        Doctor = new DTOs.DoctorDto
                        {
                            IdDoctor = pr.Doctor.IdDoctor,
                            FirstName = pr.Doctor.FirstName,
                            LastName = pr.Doctor.LastName
                        },
                        Medicaments = pr.PrescriptionMedications
                            .Select(pm => new MedicamentDto
                            {
                                IdMedicament = pm.Medicament.IdMedicament,
                                Name = pm.Medicament.Name,
                                Dose = pm.Dose,
                                Description = pm.Medicament.Description
                            }).ToList()
                    }).ToList()
            };
        }
    }
}
