using ClinicaNoveldaSalud.Data;
using ClinicaNoveldaSalud.Models;
using ClinicaNoveldaSalud.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClinicaNoveldaSalud.Controllers
{
    public class PatientsController : Controller
    {
        private readonly MvcNoveldaSaludContexto _context;

        public PatientsController(MvcNoveldaSaludContexto context)
        {
            _context = context;
        }

        // GET: Patients
        public async Task<IActionResult> Index()
        {
            return View(await _context.Patients.ToListAsync());
        }

        // GET: Patients/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var patient = await _context.Patients
                .Include(p => p.PatientDepartments)
                    .ThenInclude(pd => pd.Visits)
                        .ThenInclude(v => v.Attachments)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (patient == null) return NotFound();
            return View(patient);
        }
        // GET: Patients/Create
        public IActionResult Create()
        {
            // Devolvemos un VM vacío al formulario
            return View(new PatientViewModel());
        }

        // POST: Patients/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PatientViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            // 1) Crear paciente
            var patient = new Patient
            {
                FirstName = vm.FirstName,
                LastName = vm.LastName,
                BirthDate = vm.BirthDate,
                Phone = vm.Phone,
                InsurerName = vm.InsurerName
            };
            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();

            // 2) Crear ficha de departamento
            var pd = new PatientDepartment
            {
                PatientId = patient.Id,
                DepartmentName = vm.DepartmentName
            };
            _context.PatientDepartments.Add(pd);
            await _context.SaveChangesAsync();

            // 3) Crear visita inicial si se proporcionó
            if (vm.VisitDate.HasValue || !string.IsNullOrWhiteSpace(vm.VisitComments))
            {
                var visit = new Visit
                {
                    PatientDepartmentId = pd.Id,
                    VisitDate = vm.VisitDate ?? DateTime.UtcNow,
                    Comments = vm.VisitComments ?? string.Empty
                };
                _context.Visits.Add(visit);
                await _context.SaveChangesAsync();

                // 4) Guardar adjuntos físicos y en BD
                if (vm.Attachments != null && vm.Attachments.Any())
                {
                    var baseUploads = Path.Combine("wwwroot", "uploads", patient.Id.ToString(), visit.Id.ToString());
                    Directory.CreateDirectory(baseUploads);

                    foreach (var file in vm.Attachments)
                    {
                        var fileName = Path.GetFileName(file.FileName);
                        var physicalPath = Path.Combine(baseUploads, fileName);

                        // Guardar en disco
                        using (var stream = System.IO.File.Create(physicalPath))
                            await file.CopyToAsync(stream);

                        // Registrar en BD
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
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Patients/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            // Cargamos paciente y su última ficha de departamento
            var patient = await _context.Patients
                               .Include(p => p.PatientDepartments)
                               .FirstOrDefaultAsync(p => p.Id == id.Value);

            if (patient == null)
                return NotFound();

            var lastPd = patient.PatientDepartments
                                .OrderByDescending(pd => pd.CreatedAt)
                                .FirstOrDefault();

            // Preparamos el VM con datos existentes
            var vm = new PatientViewModel
            {
                Id = patient.Id,
                FirstName = patient.FirstName,
                LastName = patient.LastName,
                BirthDate = patient.BirthDate,
                Phone = patient.Phone,
                InsurerName = patient.InsurerName,
                DepartmentName = lastPd?.DepartmentName ?? string.Empty
                // No cargamos aquí visitas ni adjuntos; se gestionan aparte
            };

            return View(vm);
        }

        // POST: Patients/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PatientViewModel vm)
        {
            if (id != vm.Id)
                return NotFound();

            if (!ModelState.IsValid)
                return View(vm);

            // 1) Actualizar datos de paciente
            var patient = await _context.Patients.FindAsync(id);
            if (patient == null)
                return NotFound();

            patient.FirstName = vm.FirstName;
            patient.LastName = vm.LastName;
            patient.BirthDate = vm.BirthDate;
            patient.Phone = vm.Phone;
            patient.InsurerName = vm.InsurerName;

            _context.Patients.Update(patient);
            await _context.SaveChangesAsync();

            // 2) Actualizar la ficha de departamento (última creada)
            var pd = await _context.PatientDepartments
                         .Where(x => x.PatientId == patient.Id)
                         .OrderByDescending(x => x.CreatedAt)
                         .FirstOrDefaultAsync();
            if (pd != null && pd.DepartmentName != vm.DepartmentName)
            {
                pd.DepartmentName = vm.DepartmentName;
                _context.PatientDepartments.Update(pd);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Patients/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var patient = await _context.Patients
                .FirstOrDefaultAsync(m => m.Id == id);
            if (patient == null)
            {
                return NotFound();
            }

            return View(patient);
        }

        // POST: Patients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient != null)
            {
                _context.Patients.Remove(patient);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PatientExists(int id)
        {
            return _context.Patients.Any(e => e.Id == id);
        }
    }
}
