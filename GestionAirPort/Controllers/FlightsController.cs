using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GestionAirPort.Models;
using GestionAirPort.data;

namespace GestionAirPort.Controllers
{
    public class FlightsController : Controller
    {
        private readonly AirportContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public FlightsController(AirportContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        // GET: Flights
        public async Task<IActionResult> Index()
        {
            try
            {
                var flights = await _context.Flights
                    .Include(f => f.Plane)
                    .Include(f => f.Tickets)
                    .OrderByDescending(f => f.FlightDate)
                    .ToListAsync();

                return View(flights);
            }
            catch (Exception ex)
            {
                // Log l'erreur et retourner une vue d'erreur
                return View("Error", new ErrorViewModel { RequestId = ex.Message });
            }
        }

        // GET: Flights/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var flight = await _context.Flights
                    .Include(f => f.Plane)
                    .Include(f => f.Tickets)
                        .ThenInclude(t => t.Passenger)
                    .FirstOrDefaultAsync(m => m.FlightId == id);

                if (flight == null)
                {
                    return NotFound();
                }

                return View(flight);
            }
            catch (Exception ex)
            {
                return View("Error", new ErrorViewModel { RequestId = ex.Message });
            }
        }

        // GET: Flights/Create
        public IActionResult Create()
        {
            try
            {
                // Préparation de la liste des avions avec informations détaillées
                ViewData["PlaneFk"] = new SelectList(_context.Planes
                    .Select(p => new
                    {
                        p.PlaneId,
                        Description = $"Plane {p.PlaneId} - {p.PlaneType} - Capacity: {p.Capacity}"
                    }), "PlaneId", "Description");

                // Initialisation d'un nouveau vol avec des valeurs par défaut
                var flight = new Flight
                {
                    FlightDate = DateTime.UtcNow,
                    EffectiveArrival = DateTime.UtcNow.AddHours(1)
                };

                return View(flight);
            }
            catch (Exception ex)
            {
                return View("Error", new ErrorViewModel { RequestId = ex.Message });
            }
        }

        // POST: Flights/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Flight flight)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Gestion du logo si présent
                    if (flight.LogoFile != null)
                    {
                        string wwwRootPath = _hostEnvironment.WebRootPath;
                        string fileName = Path.GetFileNameWithoutExtension(flight.LogoFile.FileName);
                        string extension = Path.GetExtension(flight.LogoFile.FileName);
                        flight.AirlineLogo = fileName + DateTime.Now.ToString("yymmssfff") + extension;
                        string path = Path.Combine(wwwRootPath, "uploads", flight.AirlineLogo);

                        using (var fileStream = new FileStream(path, FileMode.Create))
                        {
                            await flight.LogoFile.CopyToAsync(fileStream);
                        }
                    }

                    // Validation supplémentaire
                    if (flight.FlightDate >= flight.EffectiveArrival)
                    {
                        ModelState.AddModelError("EffectiveArrival", "L'heure d'arrivée doit être postérieure à l'heure de départ");
                        PreparePlaneSelectList(flight.PlaneFk);
                        return View(flight);
                    }

                    _context.Add(flight);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Vol créé avec succès!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Une erreur s'est produite lors de la création du vol: " + ex.Message);
            }

            PreparePlaneSelectList(flight.PlaneFk);
            return View(flight);
        }

        // GET: Flights/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var flight = await _context.Flights
                    .Include(f => f.Plane)
                    .FirstOrDefaultAsync(f => f.FlightId == id);

                if (flight == null)
                {
                    return NotFound();
                }

                PreparePlaneSelectList(flight.PlaneFk);
                return View(flight);
            }
            catch (Exception ex)
            {
                return View("Error", new ErrorViewModel { RequestId = ex.Message });
            }
        }

        // POST: Flights/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Flight flight)
        {
            if (id != flight.FlightId)
            {
                return NotFound();
            }

            try
            {
                if (ModelState.IsValid)
                {
                    // Gestion du logo
                    if (flight.LogoFile != null)
                    {
                        // Supprimer l'ancien logo s'il existe
                        if (!string.IsNullOrEmpty(flight.AirlineLogo))
                        {
                            var oldImagePath = Path.Combine(_hostEnvironment.WebRootPath, "uploads", flight.AirlineLogo);
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        // Sauvegarder le nouveau logo
                        string fileName = Path.GetFileNameWithoutExtension(flight.LogoFile.FileName);
                        string extension = Path.GetExtension(flight.LogoFile.FileName);
                        flight.AirlineLogo = fileName + DateTime.Now.ToString("yymmssfff") + extension;
                        string path = Path.Combine(_hostEnvironment.WebRootPath, "uploads", flight.AirlineLogo);

                        using (var fileStream = new FileStream(path, FileMode.Create))
                        {
                            await flight.LogoFile.CopyToAsync(fileStream);
                        }
                    }

                    _context.Update(flight);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Vol modifié avec succès!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FlightExists(flight.FlightId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Une erreur s'est produite lors de la modification: " + ex.Message);
            }

            PreparePlaneSelectList(flight.PlaneFk);
            return View(flight);
        }

        // GET: Flights/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var flight = await _context.Flights
                    .Include(f => f.Plane)
                    .Include(f => f.Tickets)
                    .FirstOrDefaultAsync(m => m.FlightId == id);

                if (flight == null)
                {
                    return NotFound();
                }

                return View(flight);
            }
            catch (Exception ex)
            {
                return View("Error", new ErrorViewModel { RequestId = ex.Message });
            }
        }

        // POST: Flights/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var flight = await _context.Flights
                    .Include(f => f.Tickets)
                    .FirstOrDefaultAsync(f => f.FlightId == id);

                if (flight != null)
                {
                    // Supprimer le logo si existe
                    if (!string.IsNullOrEmpty(flight.AirlineLogo))
                    {
                        var imagePath = Path.Combine(_hostEnvironment.WebRootPath, "uploads", flight.AirlineLogo);
                        if (System.IO.File.Exists(imagePath))
                        {
                            System.IO.File.Delete(imagePath);
                        }
                    }

                    _context.Flights.Remove(flight);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Vol supprimé avec succès!";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Erreur lors de la suppression: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        private bool FlightExists(int id)
        {
            return _context.Flights.Any(e => e.FlightId == id);
        }

        private void PreparePlaneSelectList(int? selectedPlaneId = null)
        {
            ViewData["PlaneFk"] = new SelectList(_context.Planes
                .Select(p => new
                {
                    p.PlaneId,
                    Description = $"Plane {p.PlaneId} - {p.PlaneType} - Capacity: {p.Capacity}"
                }), "PlaneId", "Description", selectedPlaneId);
        }

        // Méthode utilitaire pour valider le format de l'image
        private bool IsValidImage(IFormFile file)
        {
            if (file == null)
                return false;

            // Liste des types MIME autorisés
            var allowedTypes = new[] {
                "image/jpeg",
                "image/jpg",
                "image/png",
                "image/gif"
            };

            return allowedTypes.Contains(file.ContentType.ToLower());
        }
    }
}