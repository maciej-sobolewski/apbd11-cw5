using apbd11_cw5.DTOs;

namespace apbd11_cw5.Services
{
    public interface IDbService
    {
        Task<int> CreatePrescriptionAsync(PrescriptionRequestDto dto);
        Task<PatientDetailDto> GetPatientWithPrescriptionsAsync(int idPatient);
    }
}
