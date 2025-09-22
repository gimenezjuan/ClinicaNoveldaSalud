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
    public class VisitsController : Controller
    {
        private readonly MvcNoveldaSaludContexto _context;

        public VisitsController(MvcNoveldaSaludContexto context)
        {
            _context = context;
        }

        // GET: Visits
        public async Task<IActionResult> Index()
        {
            var mvcNoveldaSaludContexto = _context.Visits.Include(v => v.PatientDepartment);
            return View(await mvcNoveldaSaludContexto.ToListAsync());
        }

        // GET: Visits/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var visit = await _context.Visits
                .Include(v => v.PatientDepartment)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (visit == null)
            {
                return NotFound();
            }

            return View(visit);
        }

        // GET: Visits/Add?patientId=5
        [HttpGet]
        public IActionResult Add(int patientId)
        {
            var vm = new AddVisitViewModel
            {
                PatientId = patientId,
                VisitDate = DateTime.Today
            };
            return PartialView("_AddVisitPartial", vm);
        }

        // POST: Visits/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(AddVisitViewModel vm)
        {
            if (!ModelState.IsValid)
                return PartialView("_AddVisitPartial", vm);

            var pd = await _context.PatientDepartments
                .Where(x => x.PatientId == vm.PatientId)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            if (pd == null)
                return BadRequest("No existe ficha de departamento.");

            var visit = new Visit
            {
                PatientDepartmentId = pd.Id,
                VisitDate = vm.VisitDate,
                Comments = vm.Comments
            };
            _context.Visits.Add(visit);
            await _context.SaveChangesAsync();

            if (vm.Attachments != null && vm.Attachments.Any())
            {
                var uploadRoot = Path.Combine("wwwroot", "uploads",
                    vm.PatientId.ToString(), visit.Id.ToString());
                Directory.CreateDirectory(uploadRoot);
                foreach (var file in vm.Attachments)
                {
                    var fn = Path.GetFileName(file.FileName);
                    var phys = Path.Combine(uploadRoot, fn);
                    using var stream = System.IO.File.Create(phys);
                    await file.CopyToAsync(stream);
                    _context.VisitAttachments.Add(new VisitAttachment
                    {
                        VisitId = visit.Id,
                        FileName = fn,
                        FilePath = phys.Replace("wwwroot", ""),
                        UploadedAt = DateTime.UtcNow
                    });
                }
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Details", "Patients", new { id = vm.PatientId });
        }

        // GET: Visits/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var visit = await _context.Visits.FindAsync(id);
            if (visit == null)
            {
                return NotFound();
            }
            ViewData["PatientDepartmentId"] = new SelectList(_context.PatientDepartments, "Id", "DepartmentName", visit.PatientDepartmentId);
            return View(visit);
        }

        // POST: Visits/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,PatientDepartmentId,VisitDate,Comments")] Visit visit)
        {
            if (id != visit.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(visit);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VisitExists(visit.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["PatientDepartmentId"] = new SelectList(_context.PatientDepartments, "Id", "DepartmentName", visit.PatientDepartmentId);
            return View(visit);
        }

        // GET: Visits/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var visit = await _context.Visits
                .Include(v => v.PatientDepartment)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (visit == null)
            {
                return NotFound();
            }

            return View(visit);
        }

        // POST: Visits/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var visit = await _context.Visits.FindAsync(id);
            if (visit != null)
            {
                _context.Visits.Remove(visit);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VisitExists(int id)
        {
            return _context.Visits.Any(e => e.Id == id);
        }

        [HttpPost]
        public async Task<IActionResult> AddAjax(AddVisitViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errores = string.Join(" | ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                return BadRequest("Error de validación: " + errores);
            }

            if (model.PatientId == 0)
                return BadRequest("PatientId no recibido");

            var patientDepartmentId = await _context.PatientDepartments
                .Where(pd => pd.PatientId == model.PatientId)
                .OrderByDescending(pd => pd.CreatedAt)
                .Select(pd => pd.Id)
                .FirstOrDefaultAsync();

            if (patientDepartmentId == 0)
                return BadRequest("El paciente no tiene ningún departamento asignado.");

            var visit = new Visit
            {
                VisitDate = model.VisitDate,
                Comments = model.Comments,
                PatientDepartmentId = patientDepartmentId
            };

            _context.Visits.Add(visit);
            await _context.SaveChangesAsync();

            if (model.Attachments != null && model.Attachments.Any())
            {
                foreach (var file in model.Attachments)
                {
                    var fileName = Path.GetFileName(file.FileName);
                    var path = Path.Combine("wwwroot/uploads", $"{model.PatientId}", $"{visit.Id}", fileName);

                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                    using var stream = new FileStream(path, FileMode.Create);
                    await file.CopyToAsync(stream);

                    _context.VisitAttachments.Add(new VisitAttachment
                    {
                        FileName = fileName,
                        VisitId = visit.Id
                    });
                }

                await _context.SaveChangesAsync();
            }

            visit.Attachments = await _context.VisitAttachments
                .Where(a => a.VisitId == visit.Id)
                .ToListAsync();
            visit = await _context.Visits
                                    .Include(v => v.Attachments)
                                    .Include(v => v.PatientDepartment)
                                    .FirstOrDefaultAsync(v => v.Id == visit.Id);

            return PartialView("_VisitRowPartial", visit);
        }
    }
}
