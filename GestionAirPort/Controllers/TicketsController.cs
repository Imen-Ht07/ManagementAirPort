using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GestionAirPort.Models;
using GestionAirPort.data;
using System.Globalization;

namespace GestionAirPort.Controllers
{
    public class TicketsController : Controller
    {
        private readonly AirportContext _context;
        private const decimal VIP_SURCHARGE_PERCENTAGE = 0.5M;

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
                        .ThenInclude(f => f.Plane)
                    .Include(t => t.Passenger)
                        .ThenInclude(p => p.FullName)
                    .OrderByDescending(t => t.Flight.FlightDate)
                    .AsNoTracking() // Amélioration des performances pour la lecture seule
                    .ToListAsync();

                ViewBag.Statistics = new
                {
                    TotalTickets = tickets.Count,
                    VipTickets = tickets.Count(t => t.VIP),
                    TotalRevenue = tickets.Sum(t => t.Prix),
                    AveragePrice = tickets.Any() ? tickets.Average(t => t.Prix) : 0
                };

                return View(tickets);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erreur lors du chargement des tickets: {ex.Message}";
                return View(Enumerable.Empty<Ticket>());
            }
        }

        // GET: Tickets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound("ID du ticket non spécifié");
            }

            try
            {
                var ticket = await _context.Tickets
                    .Include(t => t.Flight)
                        .ThenInclude(f => f.Plane)
                    .Include(t => t.Passenger)
                        .ThenInclude(p => p.FullName)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(m => m.TicketId == id);

                if (ticket == null)
                {
                    return NotFound("Ticket non trouvé");
                }


                return View(ticket);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erreur lors du chargement du ticket: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Tickets/Create
        public IActionResult Create()
        {
            try
            {
                PrepareCreateViewData();
                return View(new Ticket
                {
                    VIP = false,
                    Prix = 0,
    
                });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erreur lors de la préparation du formulaire: {ex.Message}";
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
                    if (!await ValidateTicketCreation(ticket))
                    {
                        PrepareCreateViewData();
                        return View(ticket);
                    }

                    if (ticket.VIP)
                    {
                        ticket.Prix = CalculateVipPrice(ticket.Prix);
                    }

                    _context.Add(ticket);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Ticket créé avec succès!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Une erreur s'est produite: {ex.Message}");
            }

            PrepareCreateViewData();
            return View(ticket);
        }

        // GET: Tickets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound("ID du ticket non spécifié");
            }

            try
            {
                var ticket = await _context.Tickets
                    .Include(t => t.Flight)
                    .Include(t => t.Passenger)
                    .FirstOrDefaultAsync(t => t.TicketId == id);

                if (ticket == null)
                {
                    return NotFound("Ticket non trouvé");
                }

                if (!ticket.CanBeModified)
                {
                    TempData["Error"] = "Ce ticket ne peut plus être modifié.";
                    return RedirectToAction(nameof(Index));
                }

                PrepareEditViewData(ticket);
                return View(ticket);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erreur lors du chargement du ticket: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Tickets/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TicketId,Prix,Siege,VIP,PassengerFk,FlightFk,Status")] Ticket ticket)
        {
            if (id != ticket.TicketId)
            {
                return NotFound("ID du ticket invalide");
            }

            try
            {
                if (ModelState.IsValid)
                {
                    if (!await ValidateTicketEdit(ticket))
                    {
                        PrepareEditViewData(ticket);
                        return View(ticket);
                    }

                    var existingTicket = await _context.Tickets.FindAsync(id);
                    if (existingTicket == null)
                    {
                        return NotFound("Ticket non trouvé");
                    }

                    if (!existingTicket.CanBeModified)
                    {
                        TempData["Error"] = "Ce ticket ne peut plus être modifié.";
                        return RedirectToAction(nameof(Index));
                    }

                    // Mise à jour des propriétés modifiables
                    existingTicket.Prix = ticket.VIP ? CalculateVipPrice(ticket.Prix) : ticket.Prix;
                    existingTicket.Siege = ticket.Siege;
                    existingTicket.VIP = ticket.VIP;        
                    existingTicket.Status = ticket.Status;

                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Ticket modifié avec succès!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TicketExists(ticket.TicketId))
                {
                    return NotFound("Ticket non trouvé");
                }
                throw;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Une erreur s'est produite: {ex.Message}");
            }

 
            PrepareEditViewData(ticket);
            return View(ticket);
        }

        // GET: Tickets/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound("ID du ticket non spécifié");
            }

            try
            {
                var ticket = await _context.Tickets
                    .Include(t => t.Flight)
                        .ThenInclude(f => f.Plane)
                    .Include(t => t.Passenger)
                        .ThenInclude(p => p.FullName)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(m => m.TicketId == id);

                if (ticket == null)
                {
                    return NotFound("Ticket non trouvé");
                }

                return View(ticket);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erreur lors du chargement du ticket: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var ticket = await _context.Tickets.FindAsync(id);
                if (ticket == null)
                {
                    return NotFound("Ticket non trouvé");
                }

                _context.Tickets.Remove(ticket);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Ticket supprimé avec succès!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erreur lors de la suppression: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // API Endpoints avec amélioration de la validation et des réponses
        [HttpGet]
        public async Task<IActionResult> GetFlightDetails(int flightId)
        {
            if (flightId <= 0)
            {
                return BadRequest("ID de vol invalide");
            }

            try
            {
                var flight = await _context.Flights
                    .Include(f => f.Plane)
                    .Include(f => f.Tickets)
                    .Where(f => f.FlightId == flightId)
                    .Select(f => new
                    {
                        f.FlightId,
                        f.Departure,
                        f.Destination,
                        FlightDate = f.FlightDate.ToString("dd/MM/yyyy HH:mm"),
                        AvailableSeats = f.Plane.Capacity - f.Tickets.Count,
                        PlaneCapacity = f.Plane.Capacity,
                        IsFull = f.Tickets.Count >= f.Plane.Capacity
                    })
                    .AsNoTracking()
                    .FirstOrDefaultAsync();

                if (flight == null)
                {
                    return NotFound("Vol non trouvé");
                }

                return Json(flight);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur serveur: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> CheckSeatAvailability(int flightId, string seat)
        {
            if (flightId <= 0 || string.IsNullOrEmpty(seat))
            {
                return BadRequest("Paramètres invalides");
            }

            try
            {
                var isTaken = await IsSeatTaken(flightId, seat);
                return Json(new { isAvailable = !isTaken });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur serveur: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Search(string query)
        {
            try
            {
                var tickets = await _context.Tickets
                    .Include(t => t.Flight)
                        .ThenInclude(f => f.Plane)
                    .Include(t => t.Passenger)
                        .ThenInclude(p => p.FullName)
                    .Where(t =>
                        (t.Passenger.FullName.LastName.Contains(query) ||
                        t.Passenger.FullName.FirstName.Contains(query) ||
                        t.Flight.Departure.Contains(query) ||
                        t.Flight.Destination.Contains(query) ||
                        t.Siege.Contains(query)) &&
                        t.Status != TicketStatus.Cancelled)
                    .OrderByDescending(t => t.Flight.FlightDate)
                    .AsNoTracking()
                    .ToListAsync();

                ViewBag.SearchQuery = query;
                return View("Index", tickets);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erreur lors de la recherche: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // Méthodes privées
  
        private async Task<bool> ValidateTicketCreation(Ticket ticket)
        {
            var flight = await _context.Flights
                .Include(f => f.Plane)
                .Include(f => f.Tickets)
                .FirstOrDefaultAsync(f => f.FlightId == ticket.FlightFk);

            if (flight == null || flight.Tickets.Count >= flight.Plane.Capacity)
            {
                ModelState.AddModelError("FlightFk", "Ce vol n'est pas disponible ou est complet");
                return false;
            }

            if (await IsSeatTaken(ticket.FlightFk, ticket.Siege))
            {
                ModelState.AddModelError("Siege", "Ce siège est déjà occupé");
                return false;
            }

            return true;
        }

        private async Task<bool> ValidateTicketEdit(Ticket ticket)
        {
            var flight = await _context.Flights
                .Include(f => f.Plane)
                .Include(f => f.Tickets)
                .FirstOrDefaultAsync(f => f.FlightId == ticket.FlightFk);

            if (flight == null)
            {
                ModelState.AddModelError("FlightFk", "Vol non trouvé");
                return false;
            }

            if (await IsSeatTaken(ticket.FlightFk, ticket.Siege, ticket.TicketId))
            {
                ModelState.AddModelError("Siege", "Ce siège est déjà occupé");
                return false;
            }

            return true;
        }

        private async Task<bool> IsSeatTaken(int flightId, string seat, int? excludeTicketId = null)
        {
            var query = _context.Tickets
                .Where(t => t.FlightFk == flightId &&
                           t.Siege == seat &&
                           t.Status != TicketStatus.Cancelled);

            if (excludeTicketId.HasValue)
            {
                query = query.Where(t => t.TicketId != excludeTicketId.Value);
            }

            return await query.AnyAsync();
        }

        private void PrepareCreateViewData()
        {
            try
            {
                var flights = _context.Flights
                    .Include(f => f.Plane)
                    .Include(f => f.Tickets)
                    .Where(f =>
                        f.FlightDate > DateTime.Now &&
                        f.Tickets.Count(t => t.Status != TicketStatus.Cancelled) < f.Plane.Capacity)
                    .OrderBy(f => f.FlightDate)
                    .AsNoTracking()
                    .Select(f => new SelectListItem
                    {
                        Value = f.FlightId.ToString(),
                        Text = $"Vol {f.FlightId} - {f.Departure} → {f.Destination} - {f.FlightDate:dd/MM/yyyy HH:mm} ({f.Plane.Capacity - f.Tickets.Count(t => t.Status != TicketStatus.Cancelled)} places)"
                    })
                    .ToList();

                var passengers = _context.Passengers
                    .Include(p => p.FullName)
                    .OrderBy(p => p.FullName.LastName)
                    .AsNoTracking()
                    .Select(p => new SelectListItem
                    {
                        Value = p.PassportNumber,
                        Text = $"{p.FullName.LastName} {p.FullName.FirstName} ({p.PassportNumber})"
                    })
                    .ToList();

                ViewBag.FlightFk = flights;
                ViewBag.PassengerFk = passengers;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur lors de la préparation des données: {ex.Message}");
            }
        }

        private void PrepareEditViewData(Ticket ticket)
        {
            PrepareCreateViewData();
            if (ticket != null)
            {
                ViewBag.CurrentFlight = ticket.Flight;
                ViewBag.CurrentPassenger = ticket.Passenger;
                ViewBag.SelectedFlightId = ticket.FlightFk;
                ViewBag.SelectedPassengerId = ticket.PassengerFk;
            }
        }

        private decimal CalculateVipPrice(decimal basePrice) => basePrice * (1 + VIP_SURCHARGE_PERCENTAGE);

        private bool TicketExists(int id) => _context.Tickets.Any(e => e.TicketId == id);
    }
}