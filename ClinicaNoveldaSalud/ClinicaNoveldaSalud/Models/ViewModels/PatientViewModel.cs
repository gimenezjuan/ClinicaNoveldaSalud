using System.ComponentModel.DataAnnotations;

namespace ClinicaNoveldaSalud.Models.ViewModels
{
    public class PatientViewModel
    {
        // Datos del paciente
        public int? Id { get; set; }

        [Display(Name = "Nombre")]
        [Required, MaxLength(120)]
        public string FirstName { get; set; } = null!;

        [Display(Name = "Apellidos")]
        [Required, MaxLength(120)]
        public string LastName { get; set; } = null!;

        [Display(Name = "Fecha de nacimiento")]
        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }

        [Required(ErrorMessage = "El número de teléfono es obligatorio.")]
        [Phone]
        [Display(Name = "Teléfono")]
        public string Phone { get; set; } = null!;
        
        [Display(Name = "Aseguradora")]
        [Required(ErrorMessage = "El nombre de la aseguradora es un dato obligatorio, se puede poner la opción 'Ninguna'"), MaxLength(200)]
        public string InsurerName { get; set; } = null!;

        [Display(Name = "Departamento del paciente")]
        // Datos de departamento
        [Required, MaxLength(150)]
        public string DepartmentName { get; set; } = null!;


        [Display(Name = "Primera Visita")]
        // Primera visita (opcional)
        [DataType(DataType.Date)]
        public DateTime? VisitDate { get; set; }


        [Display(Name = "Comentarios")]
        [MaxLength(2000)]
        public string? VisitComments { get; set; }


        [Display(Name = "Subir archivos")]
        // Archivos a subir
        public List<IFormFile>? Attachments { get; set; }
    }
}
