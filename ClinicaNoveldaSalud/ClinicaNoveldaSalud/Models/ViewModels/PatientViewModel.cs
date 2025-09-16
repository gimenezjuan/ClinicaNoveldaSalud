using System.ComponentModel.DataAnnotations;

namespace ClinicaNoveldaSalud.Models.ViewModels
{
    public class PatientViewModel
    {
        // Datos del paciente
        public int? Id { get; set; }

        [Required, MaxLength(120)]
        public string FirstName { get; set; } = null!;

        [Required, MaxLength(120)]
        public string LastName { get; set; } = null!;

        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }

        [MaxLength(50)]
        public string? Phone { get; set; }

        [Required, MaxLength(200)]
        public string InsurerName { get; set; } = null!;

        // Datos de departamento
        [Required, MaxLength(150)]
        public string DepartmentName { get; set; } = null!;

        // Primera visita (opcional)
        [DataType(DataType.Date)]
        public DateTime? VisitDate { get; set; }

        [MaxLength(2000)]
        public string? VisitComments { get; set; }

        // Archivos a subir
        public List<IFormFile>? Attachments { get; set; }
    }
}
