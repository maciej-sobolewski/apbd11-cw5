﻿using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace apbd11_cw5.Models
{
    public class PrescriptionMedicament
    {
        public int IdPrescription { get; set; }
        public Prescription Prescription { get; set; }
        public int IdMedicament { get; set; }
        public Medicament Medicament { get; set; }
        public int Dose { get; set; }
        public string Details { get; set; }
    }
}
