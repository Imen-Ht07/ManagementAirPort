using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GestionAirPort.Models;
using GestionAirPort.data;

namespace GestionAirPort.Controllers
{
    public class TicketsController : Controller
    {
        private readonly AirportContext _context;

        public TicketsController(AirportContext context)
        {
            _context = context;
        }

        // GET: Tickets
        public async Task<IActionResult> Index()
        {
            try
            {
                var tickets = await _context.Tickets
                    .Include(t => t.Flight)
                    .Include(t => t.Passenger)
                        .ThenInclude(p => p.FullName)
                    .OrderBy(t => t.Flight.FlightDate)
                    .ToListAsync();

                return View(tickets);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Erreur lors du chargement des tickets: " + ex.Message;
                return View(new List<Ticket>());
            }
        }

        // GET: Tickets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var ticket = await _context.Tickets
                    .Include(t => t.Flight)
                    .Include(t => t.Passenger)
                        .ThenInclude(p => p.FullName)
                    .FirstOrDefaultAsync(m => m.TicketId == id);

                if (ticket == null)
                {
                    return NotFound();
                }

                return View(ticket);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Erreur lors du chargement du ticket: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Tickets/Create
        public IActionResult Create()
        {
            try
            {
                PrepareSelectLists();
                return View(new Ticket { VIP = false }); // Valeur par défaut pour VIP
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Erreur lors de la préparation du formulaire: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Tickets/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Prix,Siege,VIP,PassengerFk,FlightFk")] Ticket ticket)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Vérifier si le siège est déjà pris pour ce vol
                    if (await IsSeatTaken(ticket.FlightFk, ticket.Siege))
                    {
                        ModelState.AddModelError("Siege", "Ce siège est déjà occupé pour ce vol");
                        PrepareSelectLists(ticket.FlightFk, ticket.PassengerFk);
                        return View(ticket);
                    }

                    // Vérifier la capacité de l'avion
                    var flight = await _context.Flights
                        .Include(f => f.Plane)
                        .Include(f => f.Tickets)
                        .FirstOrDefaultAsync(f => f.FlightId == ticket.FlightFk);

                    if (flight != null && flight.Tickets.Count >= flight.Plane.Capacity)
                    {
                        ModelState.AddModelError("FlightFk", "Ce vol est complet");
                        PrepareSelectLists(ticket.FlightFk, ticket.PassengerFk);
                        return View(ticket);
                    }

                    _context.Add(ticket);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Ticket créé avec succès!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Une erreur s'est produite: " + ex.Message);
            }

            PrepareSelectLists(ticket.FlightFk, ticket.PassengerFk);
            return View(ticket);
        }

        // GET: Tickets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var ticket = await _context.Tickets
                    .Include(t => t.Flight)
                    .Include(t => t.Passenger)
                    .FirstOrDefaultAsync(t => t.TicketId == id);

                if (ticket == null)
                {
                    return NotFound();
                }

                PrepareSelectLists(ticket.FlightFk, ticket.PassengerFk);
                return View(ticket);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Erreur lors du chargement du ticket: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Tickets/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TicketId,Prix,Siege,VIP,PassengerFk,FlightFk")] Ticket ticket)
        {
            if (id != ticket.TicketId)
            {
                return NotFound();
            }

            try
            {
                if (ModelState.IsValid)
                {
                    // Vérifier si le siège est déjà pris (sauf par le ticket actuel)
                    if (await IsSeatTaken(ticket.FlightFk, ticket.Siege, ticket.TicketId))
                    {
                        ModelState.AddModelError("Siege", "Ce siège est déjà occupé pour ce vol");
                        PrepareSelectLists(ticket.FlightFk, ticket.PassengerFk);
                        return View(ticket);
                    }

                    _context.Update(ticket);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Ticket modifié avec succès!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TicketExists(ticket.TicketId))
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
                ModelState.AddModelError(string.Empty, "Une erreur s'est produite: " + ex.Message);
            }

            PrepareSelectLists(ticket.FlightFk, ticket.PassengerFk);
            return View(ticket);
        }

        // GET: Tickets/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var ticket = await _context.Tickets
                    .Include(t => t.Flight)
                    .Include(t => t.Passenger)
                        .ThenInclude(p => p.FullName)
                    .FirstOrDefaultAsync(m => m.TicketId == id);

                if (ticket == null)
                {
                    return NotFound();
                }

                return View(ticket);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Erreur lors du chargement du ticket: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var ticket = await _context.Tickets.FindAsync(id);
                if (ticket != null)
                {
                    _context.Tickets.Remove(ticket);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Ticket supprimé avec succès!";
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Erreur lors de la suppression: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        private bool TicketExists(int id)
        {
            return _context.Tickets.Any(e => e.TicketId == id);
        }

        private async Task<bool> IsSeatTaken(int flightId, string seat, int? excludeTicketId = null)
        {
            return await _context.Tickets
                .AnyAsync(t => t.FlightFk == flightId
                    && t.Siege == seat
                    && (!excludeTicketId.HasValue || t.TicketId != excludeTicketId.Value));
        }

        private void PrepareSelectLists(int? selectedFlightId = null, string selectedPassengerId = null)
        {
            // Préparer la liste des vols avec plus d'informations
            ViewData["FlightFk"] = new SelectList(_context.Flights
                .Include(f => f.Plane)
                .OrderBy(f => f.FlightDate)
                .Select(f => new
                {
                    f.FlightId,
                    Description = $"Vol {f.FlightId} - {f.Departure} → {f.Destination} - {f.FlightDate:dd/MM/yyyy HH:mm}"
                }), "FlightId", "Description", selectedFlightId);

            // Préparer la liste des passagers avec plus d'informations
            ViewData["PassengerFk"] = new SelectList(_context.Passengers
                .Include(p => p.FullName)
                .OrderBy(p => p.FullName.FirstName)
                .Select(p => new
                {
                    p.PassportNumber,
                    Description = $"{p.FullName.FirstName} {p.FullName.LastName} ({p.PassportNumber})"
                }), "PassportNumber", "Description", selectedPassengerId);
        }
    }
}