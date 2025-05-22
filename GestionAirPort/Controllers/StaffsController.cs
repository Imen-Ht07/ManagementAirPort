using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionAirPort.Models;
using GestionAirPort.data;

namespace GestionAirPort.Controllers
{
    public class StaffsController : Controller
    {
        private readonly AirportContext _context;

        public StaffsController(AirportContext context)
        {
            _context = context;
        }

        // GET: Staffs
        public async Task<IActionResult> Index()
        {
            try
            {
                var staffs = await _context.Staffs
                    .Include(s => s.Tickets)
                    .OrderBy(s => s.Function)
                    .ThenBy(s => s.FullName.LastName)
                    .ToListAsync();

                TempData["Success"] = "Liste du personnel chargée avec succès.";
                return View(staffs);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Une erreur s'est produite : {ex.Message}";
                return View(new List<Staff>());
            }
        }

        // GET: Staffs/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "ID invalide";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var staff = await _context.Staffs
                    .Include(s => s.Tickets)
                        .ThenInclude(t => t.Flight)
                    .FirstOrDefaultAsync(m => m.PassportNumber == id);

                if (staff == null)
                {
                    TempData["Error"] = "Membre du personnel non trouvé";
                    return RedirectToAction(nameof(Index));
                }

                return View(staff);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Une erreur s'est produite : {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Staffs/Create
        public IActionResult Create()
        {
            var staff = new Staff
            {
                BirthDate = DateTime.Today.AddYears(-20),
                EmployementDate = DateTime.Today,
                FullName = new FullName(),
                Function = "Agent" // Valeur par défaut
            };
            return View(staff);
        }

        // POST: Staffs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Function,EmployementDate,Salary,PassportNumber,FullName,BirthDate,TelNumber,EmailAddress")] Staff staff)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (await _context.Passengers.AnyAsync(p => p.PassportNumber == staff.PassportNumber))
                    {
                        ModelState.AddModelError("PassportNumber", "Ce numéro de passeport existe déjà");
                        return View(staff);
                    }

                    // Validations spécifiques au personnel
                    if (staff.Age < 18)
                    {
                        ModelState.AddModelError("BirthDate", "Le membre du personnel doit être majeur");
                        return View(staff);
                    }

                    if (staff.EmployementDate > DateTime.Today)
                    {
                        ModelState.AddModelError("EmployementDate", "La date d'emploi ne peut pas être dans le futur");
                        return View(staff);
                    }

                    if (staff.Salary <= 0)
                    {
                        ModelState.AddModelError("Salary", "Le salaire doit être positif");
                        return View(staff);
                    }

                    _context.Add(staff);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Membre du personnel créé avec succès!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Une erreur s'est produite : {ex.Message}");
            }

            return View(staff);
        }

        // GET: Staffs/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "ID invalide";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var staff = await _context.Staffs
                    .Include(s => s.Tickets)
                    .FirstOrDefaultAsync(s => s.PassportNumber == id);

                if (staff == null)
                {
                    TempData["Error"] = "Membre du personnel non trouvé";
                    return RedirectToAction(nameof(Index));
                }

                return View(staff);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Une erreur s'est produite : {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Staffs/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Function,EmployementDate,Salary,PassportNumber,FullName,BirthDate,TelNumber,EmailAddress")] Staff staff)
        {
            if (id != staff.PassportNumber)
            {
                TempData["Error"] = "ID invalide";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                if (ModelState.IsValid)
                {
                    var existingStaff = await _context.Staffs
                        .Include(s => s.Tickets)
                        .FirstOrDefaultAsync(s => s.PassportNumber == id);

                    if (existingStaff == null)
                    {
                        TempData["Error"] = "Membre du personnel non trouvé";
                        return RedirectToAction(nameof(Index));
                    }

                    // Validations
                    if (staff.Age < 18)
                    {
                        ModelState.AddModelError("BirthDate", "Le membre du personnel doit être majeur");
                        return View(staff);
                    }

                    if (staff.EmployementDate > DateTime.Today)
                    {
                        ModelState.AddModelError("EmployementDate", "La date d'emploi ne peut pas être dans le futur");
                        return View(staff);
                    }

                    if (staff.Salary <= 0)
                    {
                        ModelState.AddModelError("Salary", "Le salaire doit être positif");
                        return View(staff);
                    }

                    // Mise à jour des propriétés
                    existingStaff.FullName = staff.FullName;
                    existingStaff.BirthDate = staff.BirthDate;
                    existingStaff.TelNumber = staff.TelNumber;
                    existingStaff.EmailAddress = staff.EmailAddress;
                    existingStaff.Function = staff.Function;
                    existingStaff.EmployementDate = staff.EmployementDate;
                    existingStaff.Salary = staff.Salary;

                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Membre du personnel modifié avec succès!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StaffExists(staff.PassportNumber))
                {
                    TempData["Error"] = "Membre du personnel non trouvé";
                    return RedirectToAction(nameof(Index));
                }
                else throw;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Une erreur s'est produite : {ex.Message}");
            }

            return View(staff);
        }

        // GET: Staffs/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "ID invalide";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var staff = await _context.Staffs
                    .Include(s => s.Tickets)
                    .FirstOrDefaultAsync(m => m.PassportNumber == id);

                if (staff == null)
                {
                    TempData["Error"] = "Membre du personnel non trouvé";
                    return RedirectToAction(nameof(Index));
                }

                return View(staff);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Une erreur s'est produite : {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Staffs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            try
            {
                var staff = await _context.Staffs
                    .Include(s => s.Tickets)
                    .FirstOrDefaultAsync(s => s.PassportNumber == id);

                if (staff == null)
                {
                    TempData["Error"] = "Membre du personnel non trouvé";
                    return RedirectToAction(nameof(Index));
                }

                if (staff.Tickets != null && staff.Tickets.Any())
                {
                    TempData["Error"] = "Impossible de supprimer ce membre du personnel car il a des tickets associés";
                    return RedirectToAction(nameof(Index));
                }

                _context.Staffs.Remove(staff);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Membre du personnel supprimé avec succès!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Une erreur s'est produite : {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool StaffExists(string id)
        {
            return _context.Staffs.Any(e => e.PassportNumber == id);
        }
    }
}