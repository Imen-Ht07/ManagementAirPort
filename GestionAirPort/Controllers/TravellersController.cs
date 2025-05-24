using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionAirPort.Models;
using GestionAirPort.data;

namespace GestionAirPort.Controllers
{
    public class TravellersController : Controller
    {
        private readonly AirportContext _context;

        public TravellersController(AirportContext context)
        {
            _context = context;
        }

        // GET: Travellers
        public async Task<IActionResult> Index()
        {
            try
            {
                var travellers = await _context.Travellers
                    .Include(t => t.Tickets)
                    .OrderBy(t => t.FullName.LastName)
                    .ToListAsync();

                TempData["Success"] = "Liste des voyageurs chargée avec succès.";
                return View(travellers);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Une erreur s'est produite : {ex.Message}";
                return View(new List<Traveller>());
            }
        }

        // GET: Travellers/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "ID invalide";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var traveller = await _context.Travellers
                    .Include(t => t.Tickets)
                        .ThenInclude(t => t.Flight)
                    .FirstOrDefaultAsync(m => m.PassportNumber == id);

                if (traveller == null)
                {
                    TempData["Error"] = "Voyageur non trouvé";
                    return RedirectToAction(nameof(Index));
                }

                return View(traveller);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Une erreur s'est produite : {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Travellers/Create
        public IActionResult Create()
        {
            var traveller = new Traveller
            {
                BirthDate = DateTime.Today.AddYears(-18),
                FullName = new FullName(),
                Nationality = "Tunisienne" // Valeur par défaut
            };
            return View(traveller);
        }

        // POST: Travellers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("HealthInformation,Nationality,PassportNumber,FullName,BirthDate,TelNumber,EmailAddress")] Traveller traveller)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (await _context.Passengers.AnyAsync(p => p.PassportNumber == traveller.PassportNumber))
                    {
                        ModelState.AddModelError("PassportNumber", "Ce numéro de passeport existe déjà");
                        return View(traveller);
                    }

                    _context.Add(traveller);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Voyageur créé avec succès!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Une erreur s'est produite : {ex.Message}");
            }

            return View(traveller);
        }

        // GET: Travellers/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "ID invalide";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var traveller = await _context.Travellers
                    .Include(t => t.Tickets)
                    .FirstOrDefaultAsync(t => t.PassportNumber == id);

                if (traveller == null)
                {
                    TempData["Error"] = "Voyageur non trouvé";
                    return RedirectToAction(nameof(Index));
                }

                return View(traveller);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Une erreur s'est produite : {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Travellers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("HealthInformation,Nationality,PassportNumber,FullName,BirthDate,TelNumber,EmailAddress")] Traveller traveller)
        {
            if (id != traveller.PassportNumber)
            {
                TempData["Error"] = "ID invalide";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                if (ModelState.IsValid)
                {
                    var existingTraveller = await _context.Travellers
                        .Include(t => t.Tickets)
                        .FirstOrDefaultAsync(t => t.PassportNumber == id);

                    if (existingTraveller == null)
                    {
                        TempData["Error"] = "Voyageur non trouvé";
                        return RedirectToAction(nameof(Index));
                    }

                   

                    // Mise à jour des propriétés
                    existingTraveller.FullName = traveller.FullName;
                    existingTraveller.BirthDate = traveller.BirthDate;
                    existingTraveller.TelNumber = traveller.TelNumber;
                    existingTraveller.EmailAddress = traveller.EmailAddress;
                    existingTraveller.HealthInformation = traveller.HealthInformation;
                    existingTraveller.Nationality = traveller.Nationality;

                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Voyageur modifié avec succès!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TravellerExists(traveller.PassportNumber))
                {
                    TempData["Error"] = "Voyageur non trouvé";
                    return RedirectToAction(nameof(Index));
                }
                else throw;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Une erreur s'est produite : {ex.Message}");
            }

            return View(traveller);
        }

        // GET: Travellers/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "ID invalide";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var traveller = await _context.Travellers
                    .Include(t => t.Tickets)
                    .FirstOrDefaultAsync(m => m.PassportNumber == id);

                if (traveller == null)
                {
                    TempData["Error"] = "Voyageur non trouvé";
                    return RedirectToAction(nameof(Index));
                }

                return View(traveller);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Une erreur s'est produite : {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Travellers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            try
            {
                var traveller = await _context.Travellers
                    .Include(t => t.Tickets)
                    .FirstOrDefaultAsync(t => t.PassportNumber == id);

                if (traveller == null)
                {
                    TempData["Error"] = "Voyageur non trouvé";
                    return RedirectToAction(nameof(Index));
                }

                if (traveller.Tickets != null && traveller.Tickets.Any())
                {
                    TempData["Error"] = "Impossible de supprimer ce voyageur car il a des tickets associés";
                    return RedirectToAction(nameof(Index));
                }

                _context.Travellers.Remove(traveller);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Voyageur supprimé avec succès!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Une erreur s'est produite : {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool TravellerExists(string id)
        {
            return _context.Travellers.Any(e => e.PassportNumber == id);
        }
    }
}