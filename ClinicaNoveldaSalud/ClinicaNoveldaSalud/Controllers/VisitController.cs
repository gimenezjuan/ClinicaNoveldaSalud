using ClinicaNoveldaSalud.Data;
using ClinicaNoveldaSalud.Models;
using ClinicaNoveldaSalud.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

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
                VisitDate = DateTime.UtcNow
            };
            return PartialView("_AddVisitPartial", vm);
        }

        // POST: /Visit/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(AddVisitViewModel vm)
        {
            if (!ModelState.IsValid)
                return PartialView("_AddVisitPartial", vm);

            // 1) Obtener la última ficha de departamento
            var pd = await _context.PatientDepartments
                        .Where(x => x.PatientId == vm.PatientId)
                        .OrderByDescending(x => x.CreatedAt)
                        .FirstOrDefaultAsync();
            if (pd == null)
                return BadRequest("El paciente no tiene departamento asignado.");

            // 2) Crear nueva visita
            var visit = new Visit
            {
                PatientDepartmentId = pd.Id,
                VisitDate = vm.VisitDate,
                Comments = vm.Comments
            };
            _context.Visits.Add(visit);
            await _context.SaveChangesAsync();

            // 3) Guardar archivos
            if (vm.Attachments != null && vm.Attachments.Any())
            {
                var uploadDir = Path.Combine("wwwroot", "uploads",
                    vm.PatientId.ToString(), visit.Id.ToString());
                Directory.CreateDirectory(uploadDir);

                foreach (var file in vm.Attachments)
                {
                    var fileName = Path.GetFileName(file.FileName);
                    var physicalPath = Path.Combine(uploadDir, fileName);
                    using var stream = System.IO.File.Create(physicalPath);
                    await file.CopyToAsync(stream);

                    _context.VisitAttachments.Add(new VisitAttachment
                    {
                        VisitId = visit.Id,
                        FileName = fileName,
                        FilePath = physicalPath.Replace("wwwroot", string.Empty),
                        UploadedAt = DateTime.UtcNow
                    });
                }
                await _context.SaveChangesAsync();
            }

            // 4) Volver a la ficha del paciente
            return Json(new { success = true });
        }
    }
}
