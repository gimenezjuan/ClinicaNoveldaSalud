using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicaNoveldaSalud.Models
{
    public class Patient
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(120)]
        [Display(Name = "Nombre")]
        public string FirstName { get; set; } = null!;

        [Required, MaxLength(120)]
        [Display(Name = "Apellidos")]
        public string LastName { get; set; } = null!;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de nacimiento")]
        public DateTime? BirthDate { get; set; }

        [MaxLength(50)]
        [Display(Name = "Teléfono")]
        public string? Phone { get; set; }

        [Required, MaxLength(200)]
        [Display(Name = "Aseguradora")]
        public string InsurerName { get; set; } = null!;

        public ICollection<PatientDepartment> PatientDepartments { get; set; }
            = new List<PatientDepartment>();

        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";

    }
}
