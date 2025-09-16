using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ClinicaNoveldaSalud.Models
{
    public class PatientDepartment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PatientId { get; set; }
        public Patient Patient { get; set; } = null!;

        [Required, MaxLength(150)]
        [Display(Name = "Departamento")]
        public string DepartmentName { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Visit> Visits { get; set; } = new List<Visit>();
    }
}
