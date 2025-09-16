using System;
using System.ComponentModel.DataAnnotations;

namespace ClinicaNoveldaSalud.Models
{
    public class VisitAttachment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int VisitId { get; set; }
        public Visit Visit { get; set; } = null!;

        [Required, MaxLength(255)]
        [Display(Name = "Nombre de archivo")]
        public string FileName { get; set; } = null!;

        [Required, MaxLength(255)]
        [Display(Name = "Ruta del archivo")]
        public string FilePath { get; set; } = null!;

        [Required]
        [Display(Name = "Fecha de subida")]
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}
