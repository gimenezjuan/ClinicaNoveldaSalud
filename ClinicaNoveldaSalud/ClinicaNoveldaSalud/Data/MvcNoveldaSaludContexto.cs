using ClinicaNoveldaSalud.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicaNoveldaSalud.Data
{
    public class MvcNoveldaSaludContexto : DbContext
    {
        public MvcNoveldaSaludContexto(DbContextOptions<MvcNoveldaSaludContexto> options)
            : base(options)
        {
        }

        public DbSet<Patient> Patients { get; set; } = null!;
        public DbSet<PatientDepartment> PatientDepartments { get; set; } = null!;
        public DbSet<Visit> Visits { get; set; } = null!;
        public DbSet<VisitAttachment> VisitAttachments { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder model)
        {
            base.OnModelCreating(model);

            // Paciente
            model.Entity<Patient>(b =>
            {
                b.HasKey(p => p.Id);
                b.Property(p => p.FirstName)
                 .IsRequired()
                 .HasMaxLength(120);
                b.Property(p => p.LastName)
                 .IsRequired()
                 .HasMaxLength(120);
                b.Property(p => p.InsurerName)
                 .IsRequired()
                 .HasMaxLength(200);
                b.HasMany(p => p.PatientDepartments)
                 .WithOne(pd => pd.Patient)
                 .HasForeignKey(pd => pd.PatientId);
            });

            // Ficha por departamento
            model.Entity<PatientDepartment>(b =>
            {
                b.HasKey(pd => pd.Id);
                b.Property(pd => pd.DepartmentName)
                 .IsRequired()
                 .HasMaxLength(150);
                b.Property(pd => pd.CreatedAt)
                 .IsRequired();
                b.HasMany(pd => pd.Visits)
                 .WithOne(v => v.PatientDepartment)
                 .HasForeignKey(v => v.PatientDepartmentId);
            });

            // Visita
            model.Entity<Visit>(b =>
            {
                b.HasKey(v => v.Id);
                b.Property(v => v.VisitDate)
                 .IsRequired();
                b.Property(v => v.Comments)
                 .IsRequired();
                b.HasMany(v => v.Attachments)
                 .WithOne(a => a.Visit)
                 .HasForeignKey(a => a.VisitId);
            });

            // Adjuntos de visita
            model.Entity<VisitAttachment>(b =>
            {
                b.HasKey(a => a.Id);
                b.Property(a => a.FileName)
                 .IsRequired()
                 .HasMaxLength(255);
                b.Property(a => a.FilePath)
                 .IsRequired()
                 .HasMaxLength(255);
                b.Property(a => a.UploadedAt)
                 .IsRequired();
            });
        }
    }
}
