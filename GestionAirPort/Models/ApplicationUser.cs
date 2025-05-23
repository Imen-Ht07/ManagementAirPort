// Models/ApplicationUser.cs
using Microsoft.AspNetCore.Identity;

namespace GestionAirPort.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLogin { get; set; }
        public bool IsActive { get; set; } = true;

        // Pour les passagers
        public string? PassportNumber { get; set; }
        public virtual Passenger? Passenger { get; set; }
    }
}