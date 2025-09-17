using System.ComponentModel.DataAnnotations;

namespace ClinicaNoveldaSalud.Models.ViewModels
{
    public class AddVisitViewModel
    {
        public int PatientId { get; set; }

        [Required, DataType(DataType.Date)]
        [Display(Name = "Fecha de visita")]
        public DateTime VisitDate { get; set; }

        [Required]
        [MaxLength(2000)]
        [Display(Name = "Comentarios")]
        public string Comments { get; set; } = null!;

        [Display(Name = "Adjuntos")]
        public List<IFormFile>? Attachments { get; set; }
    }
}
