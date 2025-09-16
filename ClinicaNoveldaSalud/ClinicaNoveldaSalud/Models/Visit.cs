using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ClinicaNoveldaSalud.Models
{
    public class Visit
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PatientDepartmentId { get; set; }
        public PatientDepartment PatientDepartment { get; set; } = null!;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de visita")]
        public DateTime VisitDate { get; set; }

        [Required]
        [Display(Name = "Comentarios")]
        public string Comments { get; set; } = null!;

        public ICollection<VisitAttachment> Attachments { get; set; }
            = new List<VisitAttachment>();
    }
}
