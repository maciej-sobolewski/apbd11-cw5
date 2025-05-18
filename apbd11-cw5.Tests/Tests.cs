using apbd11_cw5.DTOs;
using apbd11_cw5.Data;
using apbd11_cw5.Models;
using apbd11_cw5.Services;
using Microsoft.EntityFrameworkCore;

namespace apbd11_cw5.Tests
{
    public class Tests
    {
        private DatabaseContext GetInMemoryContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<DatabaseContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            var context = new DatabaseContext(options);

            context.Doctors.AddRange(
                new Doctor { IdDoctor = 1, FirstName = "Jan", LastName = "Kowalski", Email = "jan@przychodnia.pl" },
                new Doctor { IdDoctor = 2, FirstName = "Anna", LastName = "Nowak", Email = "anna@przychodnia.pl" }
            );

            context.Medicaments.AddRange(
                new Medicament { IdMedicament = 1, Name = "LekA", Description = "OpisA", Type = "TypeA" },
                new Medicament { IdMedicament = 2, Name = "LekB", Description = "OpisB", Type = "TypeB" }
            );

            context.SaveChanges();
            return context;
        }

        [Fact]
        public async Task CreatePrescriptionAsync_TooManyMedicaments_ThrowsArgumentException()
        {
            // Arrange
            var ctx = GetInMemoryContext(Guid.NewGuid().ToString());
            var service = new DbService(ctx);
            var dto = new PrescriptionRequestDto
            {
                Date = DateTime.Today,
                DueDate = DateTime.Today.AddDays(1),
                IdDoctor = 1,
                IdPatient = 1,
                Medicaments = Enumerable.Range(1, 11)
                    .Select(i => new MedicamentEntryDto { IdMedicament = 1, Dose = 1, Details = "d" })
                    .ToList()
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => service.CreatePrescriptionAsync(dto));
        }

        [Fact]
        public async Task CreatePrescriptionAsync_InvalidDueDate_ThrowsArgumentException()
        {
            // Arrange
            var ctx = GetInMemoryContext(Guid.NewGuid().ToString());
            var service = new DbService(ctx);
            var dto = new PrescriptionRequestDto
            {
                Date = DateTime.Today,
                DueDate = DateTime.Today.AddDays(-1),
                IdDoctor = 1,
                IdPatient = 1,
                Medicaments = new List<MedicamentEntryDto> {
                    new MedicamentEntryDto { IdMedicament = 1, Dose = 1, Details = "d" }
                }
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => service.CreatePrescriptionAsync(dto));
        }

        [Fact]
        public async Task CreatePrescriptionAsync_NonExistingDoctor_ThrowsKeyNotFoundException()
        {
            // Arrange
            var ctx = GetInMemoryContext(Guid.NewGuid().ToString());
            var service = new DbService(ctx);
            var dto = new PrescriptionRequestDto
            {
                Date = DateTime.Today,
                DueDate = DateTime.Today.AddDays(1),
                IdDoctor = 99,
                IdPatient = 1,
                Medicaments = new List<MedicamentEntryDto> {
                    new MedicamentEntryDto { IdMedicament = 1, Dose = 1, Details = "d" }
                }
            };

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.CreatePrescriptionAsync(dto));
        }

        [Fact]
        public async Task CreatePrescriptionAsync_CreatesNewPatientAndPrescription()
        {
            // Arrange
            var ctx = GetInMemoryContext(Guid.NewGuid().ToString());
            var service = new DbService(ctx);
            var dto = new PrescriptionRequestDto
            {
                Date = new DateTime(2025, 1, 1),
                DueDate = new DateTime(2025, 1, 8),
                IdDoctor = 1,
                Patient = new PatientDto { FirstName = "Test", LastName = "User", Birthdate = new DateTime(1990, 1, 1) },
                Medicaments = new List<MedicamentEntryDto> {
                    new MedicamentEntryDto { IdMedicament = 2, Dose = 2, Details = "info" }
                }
            };

            // Act
            var prescId = await service.CreatePrescriptionAsync(dto);

            // Assert
            var presc = ctx.Prescriptions
                .Include(p => p.Doctor)
                .Include(p => p.Patient)
                .Include(p => p.PrescriptionMedications).ThenInclude(pm => pm.Medicament)
                .FirstOrDefault(p => p.IdPrescription == prescId);

            Assert.NotNull(presc);
            Assert.Equal(dto.Date, presc.Date);
            Assert.Equal(dto.DueDate, presc.DueDate);
            Assert.Equal(1, presc.Doctor.IdDoctor);
            Assert.Equal("Test", presc.Patient.FirstName);
            Assert.Single(presc.PrescriptionMedications);
            Assert.Equal(2, presc.PrescriptionMedications.First().Medicament.IdMedicament);
        }

        [Fact]
        public async Task GetPatientWithPrescriptionsAsync_ReturnsCorrectData()
        {
            // Arrange
            var ctx = GetInMemoryContext(Guid.NewGuid().ToString());

            var patient = new Patient { IdPatient = 1, FirstName = "P", LastName = "L", Birthdate = new DateTime(2000, 1, 1) };
            ctx.Patients.Add(patient);

            var doctor = ctx.Doctors.First();
            var presc = new Prescription
            {
                IdPrescription = 1,
                Date = new DateTime(2025, 2, 2),
                DueDate = new DateTime(2025, 2, 9),
                Patient = patient,
                Doctor = doctor,
                IdPatient = patient.IdPatient,
                IdDoctor = doctor.IdDoctor,
                PrescriptionMedications = new List<PrescriptionMedicament>()
            };
            ctx.Prescriptions.Add(presc);

            var med = ctx.Medicaments.First();
            var pm = new PrescriptionMedicament
            {
                IdPrescription = presc.IdPrescription,
                IdMedicament = med.IdMedicament,
                Prescription = presc,
                Medicament = med,
                Dose = 1,
                Details = "d"
            };
            ctx.PrescriptionMedicaments.Add(pm);

            ctx.SaveChanges();

            var service = new DbService(ctx);
            
            // Act
            var detail = await service.GetPatientWithPrescriptionsAsync(1);

            // Assert
            Assert.Equal(1, detail.IdPatient);
            Assert.Single(detail.Prescriptions);
            var dtoPresc = detail.Prescriptions.First();
            Assert.Equal(1, dtoPresc.IdPrescription);
            Assert.Equal(med.IdMedicament, dtoPresc.Medicaments.First().IdMedicament);
        }
    }
}