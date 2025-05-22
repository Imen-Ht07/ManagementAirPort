using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionAirPort.Models;
using GestionAirPort.data;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GestionAirPort.Controllers
{
    public class PlanesController : Controller
    {
        private readonly AirportContext _context;

        public PlanesController(AirportContext context)
        {
            _context = context;
        }

        // GET: Planes
        public async Task<IActionResult> Index()
        {
            try
            {
                var planes = await _context.Planes
                    .Include(p => p.Flights)
                    .OrderByDescending(p => p.ManufactureDate)
                    .ToListAsync();

                return View(planes);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Une erreur s'est produite : {ex.Message}";
                return View(new List<Plane>());
            }
        }

        // GET: Planes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var plane = await _context.Planes
                    .Include(p => p.Flights)
                        .ThenInclude(f => f.Tickets)
                    .FirstOrDefaultAsync(m => m.PlaneId == id);

                if (plane == null)
                {
                    return NotFound();
                }

                return View(plane);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Une erreur s'est produite : {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Planes/Create
        public IActionResult Create()
        {
            ViewData["PlaneType"] = new SelectList(Enum.GetValues(typeof(PlaneType))
                .Cast<PlaneType>()
                .Select(e => new
                {
                    Value = e,
                    Text = e.ToString()
                }), "Value", "Text");

            var plane = new Plane
            {
                ManufactureDate = DateTime.Today
            };

            return View(plane);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Capacity,ManufactureDate,PlaneType")] Plane plane)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (plane.ManufactureDate > DateTime.Today)
                    {
                        ModelState.AddModelError("ManufactureDate", "La date de fabrication ne peut pas être dans le futur");
                        PreparePlaneTypeList();
                        return View(plane);
                    }

                    plane.Flights = new List<Flight>();
                    _context.Add(plane);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Avion créé avec succès!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Une erreur s'est produite : {ex.Message}");
            }

            PreparePlaneTypeList();
            return View(plane);
        }

        // GET: Planes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var plane = await _context.Planes.FindAsync(id);
            if (plane == null)
            {
                return NotFound();
            }

            PreparePlaneTypeList(plane.PlaneType);
            return View(plane);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PlaneId,Capacity,ManufactureDate,PlaneType")] Plane plane)
        {
            if (id != plane.PlaneId)
            {
                return NotFound();
            }

            try
            {
                if (ModelState.IsValid)
                {
                    if (plane.ManufactureDate > DateTime.Today)
                    {
                        ModelState.AddModelError("ManufactureDate", "La date de fabrication ne peut pas être dans le futur");
                        PreparePlaneTypeList(plane.PlaneType);
                        return View(plane);
                    }

                    var existingPlane = await _context.Planes
                        .Include(p => p.Flights)
                        .FirstOrDefaultAsync(p => p.PlaneId == id);

                    if (existingPlane != null)
                    {
                        existingPlane.Capacity = plane.Capacity;
                        existingPlane.ManufactureDate = plane.ManufactureDate;
                        existingPlane.PlaneType = plane.PlaneType;

                        await _context.SaveChangesAsync();
                        TempData["Success"] = "Avion modifié avec succès!";
                        return RedirectToAction(nameof(Index));
                    }
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PlaneExists(plane.PlaneId))
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
                ModelState.AddModelError(string.Empty, $"Une erreur s'est produite : {ex.Message}");
            }

            PreparePlaneTypeList(plane.PlaneType);
            return View(plane);
        }

        // GET: Planes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var plane = await _context.Planes
                .Include(p => p.Flights)
                .FirstOrDefaultAsync(m => m.PlaneId == id);

            if (plane == null)
            {
                return NotFound();
            }

            return View(plane);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var plane = await _context.Planes
                    .Include(p => p.Flights)
                    .FirstOrDefaultAsync(p => p.PlaneId == id);

                if (plane != null)
                {
                    if (plane.Flights != null && plane.Flights.Any())
                    {
                        TempData["Error"] = "Impossible de supprimer cet avion car il a des vols associés.";
                        return RedirectToAction(nameof(Index));
                    }

                    _context.Planes.Remove(plane);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Avion supprimé avec succès!";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Une erreur s'est produite : {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        private bool PlaneExists(int id)
        {
            return _context.Planes.Any(e => e.PlaneId == id);
        }

        private void PreparePlaneTypeList(PlaneType? selectedType = null)
        {
            ViewData["PlaneType"] = new SelectList(
                Enum.GetValues(typeof(PlaneType))
                    .Cast<PlaneType>()
                    .Select(e => new
                    {
                        Value = e,
                        Text = e.ToString()
                    }), "Value", "Text", selectedType);
        }
    }
}