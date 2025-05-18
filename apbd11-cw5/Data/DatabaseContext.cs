using apbd11_cw5.Models;
using Microsoft.EntityFrameworkCore;

namespace apbd11_cw5.Data
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options)
            : base(options)
        { }

        public DbSet<Patient> Patients { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Medicament> Medicaments { get; set; }
        public DbSet<Prescription> Prescriptions { get; set; }
        public DbSet<PrescriptionMedicament> PrescriptionMedicaments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Patient>()
                .HasKey(p => p.IdPatient);

            modelBuilder.Entity<Doctor>()
                .HasKey(d => d.IdDoctor);

            modelBuilder.Entity<Medicament>()
                .HasKey(m => m.IdMedicament);

            modelBuilder.Entity<Prescription>()
                .HasKey(p => p.IdPrescription);

            modelBuilder.Entity<PrescriptionMedicament>()
                .HasKey(pm => new { pm.IdPrescription, pm.IdMedicament });

            modelBuilder.Entity<PrescriptionMedicament>()
                .HasOne(pm => pm.Prescription)
                .WithMany(p => p.PrescriptionMedications)
                .HasForeignKey(pm => pm.IdPrescription)
                .IsRequired();

            modelBuilder.Entity<PrescriptionMedicament>()
                .HasOne(pm => pm.Medicament)
                .WithMany(m => m.PrescriptionMedications)
                .HasForeignKey(pm => pm.IdMedicament)
                .IsRequired();

            modelBuilder.Entity<Prescription>()
                .HasOne(p => p.Patient)
                .WithMany(pt => pt.Prescriptions)
                .HasForeignKey(p => p.IdPatient)
                .IsRequired();

            modelBuilder.Entity<Prescription>()
                .HasOne(p => p.Doctor)
                .WithMany(d => d.Prescriptions)
                .HasForeignKey(p => p.IdDoctor)
                .IsRequired();

            modelBuilder.Entity<Doctor>().HasData(
                new Doctor { IdDoctor = 1, FirstName = "Jan", LastName = "Kowalski", Email = "jan.kowalski@przychodnia.pl" },
                new Doctor { IdDoctor = 2, FirstName = "Anna", LastName = "Nowak", Email = "anna.nowak@przychodnia.pl" }
            );

            modelBuilder.Entity<Patient>().HasData(
                new Patient { IdPatient = 1, FirstName = "Piotr", LastName = "Zieliński", Birthdate = new DateTime(1980, 5, 5) },
                new Patient { IdPatient = 2, FirstName = "Maria", LastName = "Wiśniewska", Birthdate = new DateTime(1990, 12, 12) }
            );

            modelBuilder.Entity<Medicament>().HasData(
                new Medicament { IdMedicament = 1, Name = "Ibuprofen", Description = "Lek przeciwbólowy", Type = "Analgesic" },
                new Medicament { IdMedicament = 2, Name = "Paracetamol", Description = "Lek przeciwgorączkowy", Type = "Antipyretic" }
            );
        }
    }
}
