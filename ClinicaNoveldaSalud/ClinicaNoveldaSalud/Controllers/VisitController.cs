using ClinicaNoveldaSalud.Data;
using ClinicaNoveldaSalud.Models;
using ClinicaNoveldaSalud.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicaNoveldaSalud.Controllers
{
    public class VisitController : Controller
    {
        private readonly MvcNoveldaSaludContexto _context;

        public VisitController(MvcNoveldaSaludContexto context)
            => _context = context;

        // GET: /Visit/Add?patientId=5
        [HttpGet]
        public IActionResult Add(int patientId)
        {
            var vm = new AddVisitViewModel
            {
                PatientId = patientId,
                VisitDate = DateTime.Today
                //VisitDate = DateTime.UtcNow
            };
            return PartialView("_AddVisitPartial", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(AddVisitViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                // Si hay validación, volvemos al partial con errores
                return PartialView("_AddVisitPartial", vm);
            }

            // 1) Traer la última ficha de departamento de ese paciente
            var pd = await _context.PatientDepartments
                .Where(x => x.PatientId == vm.PatientId)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            if (pd == null)
                return BadRequest("No existe ficha de departamento para este paciente.");

            // 2) Crear la nueva visita
            var visit = new Visit
            {
                PatientDepartmentId = pd.Id,
                VisitDate = vm.VisitDate,
                Comments = vm.Comments
            };
            _context.Visits.Add(visit);
            await _context.SaveChangesAsync();

            // 3) Guardar archivos si existen
            if (vm.Attachments != null && vm.Attachments.Any())
            {
                var uploadRoot = Path.Combine("wwwroot", "uploads",
                    vm.PatientId.ToString(), visit.Id.ToString());
                Directory.CreateDirectory(uploadRoot);

                foreach (var file in vm.Attachments)
                {
                    var fn = Path.GetFileName(file.FileName);
                    var physicalPath = Path.Combine(uploadRoot, fn);
                    using var stream = System.IO.File.Create(physicalPath);
                    await file.CopyToAsync(stream);

                    _context.VisitAttachments.Add(new VisitAttachment
                    {
                        VisitId = visit.Id,
                        FileName = fn,
                        FilePath = physicalPath.Replace("wwwroot", string.Empty),
                        UploadedAt = DateTime.UtcNow
                    });
                }
                await _context.SaveChangesAsync();
            }

            // 4) Redirigir de nuevo a la ficha completa del paciente
            return RedirectToAction("Details", "Patients", new { id = vm.PatientId });
        }
    }
}
