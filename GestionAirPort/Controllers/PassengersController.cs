using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionAirPort.Models;
using GestionAirPort.data;

namespace GestionAirPort.Controllers
{
    public class PassengersController : Controller
    {
        private readonly AirportContext _context;

        public PassengersController(AirportContext context)
        {
            _context = context;
        }

        // GET: Passengers
        public async Task<IActionResult> Index()
        {
            try
            {
                var passengers = await _context.Passengers
                    .Include(p => p.Tickets)
                    .OrderBy(p => p.FullName.LastName)
                    .ToListAsync();

                TempData["Success"] = "Liste des passagers chargée avec succès.";
                return View(passengers);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Une erreur s'est produite : {ex.Message}";
                return View(new List<Passenger>());
            }
        }

        // GET: Passengers/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "ID invalide";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var passenger = await _context.Passengers
                    .Include(p => p.Tickets)
                        .ThenInclude(t => t.Flight)
                    .FirstOrDefaultAsync(m => m.PassportNumber == id);

                if (passenger == null)
                {
                    TempData["Error"] = "Passager non trouvé";
                    return RedirectToAction(nameof(Index));
                }

                return View(passenger);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Une erreur s'est produite : {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Passengers/Create
        public IActionResult Create()
        {
            var passenger = new Passenger
            {
                BirthDate = DateTime.Today.AddYears(-18),
                FullName = new FullName()
            };
            return View(passenger);
        }

        // POST: Passengers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PassportNumber,FullName,BirthDate,TelNumber,EmailAddress")] Passenger passenger)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (await _context.Passengers.AnyAsync(p => p.PassportNumber == passenger.PassportNumber))
                    {
                        ModelState.AddModelError("PassportNumber", "Ce numéro de passeport existe déjà");
                        return View(passenger);
                    }


                    _context.Add(passenger);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Passager créé avec succès!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Une erreur s'est produite : {ex.Message}");
            }

            return View(passenger);
        }

        // GET: Passengers/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "ID invalide";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var passenger = await _context.Passengers
                    .Include(p => p.Tickets)
                    .FirstOrDefaultAsync(p => p.PassportNumber == id);

                if (passenger == null)
                {
                    TempData["Error"] = "Passager non trouvé";
                    return RedirectToAction(nameof(Index));
                }

                return View(passenger);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Une erreur s'est produite : {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Passengers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("PassportNumber,FullName,BirthDate,TelNumber,EmailAddress")] Passenger passenger)
        {
            if (id != passenger.PassportNumber)
            {
                TempData["Error"] = "ID invalide";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                if (ModelState.IsValid)
                {
                   
                    var existingPassenger = await _context.Passengers
                        .Include(p => p.Tickets)
                        .FirstOrDefaultAsync(p => p.PassportNumber == id);

                    if (existingPassenger == null)
                    {
                        TempData["Error"] = "Passager non trouvé";
                        return RedirectToAction(nameof(Index));
                    }

                    // Mise à jour des propriétés
                    existingPassenger.FullName = passenger.FullName;
                    existingPassenger.BirthDate = passenger.BirthDate;
                    existingPassenger.TelNumber = passenger.TelNumber;
                    existingPassenger.EmailAddress = passenger.EmailAddress;

                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Passager modifié avec succès!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PassengerExists(passenger.PassportNumber))
                {
                    TempData["Error"] = "Passager non trouvé";
                    return RedirectToAction(nameof(Index));
                }
                else throw;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Une erreur s'est produite : {ex.Message}");
            }

            return View(passenger);
        }

        // GET: Passengers/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "ID invalide";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var passenger = await _context.Passengers
                    .Include(p => p.Tickets)
                    .FirstOrDefaultAsync(m => m.PassportNumber == id);

                if (passenger == null)
                {
                    TempData["Error"] = "Passager non trouvé";
                    return RedirectToAction(nameof(Index));
                }

                return View(passenger);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Une erreur s'est produite : {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Passengers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            try
            {
                var passenger = await _context.Passengers
                    .Include(p => p.Tickets)
                    .FirstOrDefaultAsync(p => p.PassportNumber == id);

                if (passenger == null)
                {
                    TempData["Error"] = "Passager non trouvé";
                    return RedirectToAction(nameof(Index));
                }

                if (passenger.Tickets != null && passenger.Tickets.Any())
                {
                    TempData["Error"] = "Impossible de supprimer ce passager car il a des tickets associés";
                    return RedirectToAction(nameof(Index));
                }

                _context.Passengers.Remove(passenger);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Passager supprimé avec succès!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Une erreur s'est produite : {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool PassengerExists(string id)
        {
            return _context.Passengers.Any(e => e.PassportNumber == id);
        }
    }
}