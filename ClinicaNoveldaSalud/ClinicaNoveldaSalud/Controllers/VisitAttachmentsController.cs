using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ClinicaNoveldaSalud.Data;
using ClinicaNoveldaSalud.Models;

namespace ClinicaNoveldaSalud.Controllers
{
    public class VisitAttachmentsController : Controller
    {
        private readonly MvcNoveldaSaludContexto _context;

        public VisitAttachmentsController(MvcNoveldaSaludContexto context)
        {
            _context = context;
        }

        // GET: VisitAttachments
        public async Task<IActionResult> Index()
        {
            var mvcNoveldaSaludContexto = _context.VisitAttachments.Include(v => v.Visit);
            return View(await mvcNoveldaSaludContexto.ToListAsync());
        }

        // GET: VisitAttachments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var visitAttachment = await _context.VisitAttachments
                .Include(v => v.Visit)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (visitAttachment == null)
            {
                return NotFound();
            }

            return View(visitAttachment);
        }

        // GET: VisitAttachments/Create
        public IActionResult Create()
        {
            ViewData["VisitId"] = new SelectList(_context.Visits, "Id", "Comments");
            return View();
        }

        // POST: VisitAttachments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,VisitId,FileName,FilePath,UploadedAt")] VisitAttachment visitAttachment)
        {
            if (ModelState.IsValid)
            {
                _context.Add(visitAttachment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["VisitId"] = new SelectList(_context.Visits, "Id", "Comments", visitAttachment.VisitId);
            return View(visitAttachment);
        }

        // GET: VisitAttachments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var visitAttachment = await _context.VisitAttachments.FindAsync(id);
            if (visitAttachment == null)
            {
                return NotFound();
            }
            ViewData["VisitId"] = new SelectList(_context.Visits, "Id", "Comments", visitAttachment.VisitId);
            return View(visitAttachment);
        }

        // POST: VisitAttachments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,VisitId,FileName,FilePath,UploadedAt")] VisitAttachment visitAttachment)
        {
            if (id != visitAttachment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(visitAttachment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VisitAttachmentExists(visitAttachment.Id))
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
            ViewData["VisitId"] = new SelectList(_context.Visits, "Id", "Comments", visitAttachment.VisitId);
            return View(visitAttachment);
        }

        // GET: VisitAttachments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var visitAttachment = await _context.VisitAttachments
                .Include(v => v.Visit)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (visitAttachment == null)
            {
                return NotFound();
            }

            return View(visitAttachment);
        }

        // POST: VisitAttachments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // 1) Recuperamos el adjunto
            var att = await _context.VisitAttachments.FindAsync(id);
            if (att == null)
                return NotFound();

            // 2) Borramos el fichero físico
            //    En FilePath tienes algo tipo "/uploads/{patientId}/{visitId}/{fileName}"
            var physPath = Path.Combine(
                 "wwwroot",
                 att.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar)
            );
            if (System.IO.File.Exists(physPath))
                System.IO.File.Delete(physPath);

            // 3) Guardamos el VisitId para la redirección posterior
            var visitId = att.VisitId;

            // 4) Eliminamos el registro de la BBDD
            _context.VisitAttachments.Remove(att);
            await _context.SaveChangesAsync();

            // 5) Sacamos el PatientId para llevar al usuario a la ficha correcta
            var visit = await _context.Visits
                .Include(v => v.PatientDepartment)
                .FirstOrDefaultAsync(v => v.Id == visitId);
            var patientId = visit?.PatientDepartment.PatientId;

            // 6) Redirigimos a Details de Patients
            return RedirectToAction("Details", "Patients", new { id = patientId });
        }

        private bool VisitAttachmentExists(int id)
        {
            return _context.VisitAttachments.Any(e => e.Id == id);
        }
    }
}
