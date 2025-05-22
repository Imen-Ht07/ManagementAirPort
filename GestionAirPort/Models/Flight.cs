using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;


namespace GestionAirPort.Models
{
    public class Flight
    {
        public int FlightId { get; set; }

        public string? AirlineLogo { get; set; }

        [NotMapped]
        [Display(Name = "Upload Logo")]
        public IFormFile? LogoFile { get; set; }

        [Required(ErrorMessage = "La date du vol est obligatoire")]
        [Display(Name = "Flight Date")]
        [DataType(DataType.DateTime)]
        public DateTime FlightDate { get; set; }

        [Required(ErrorMessage = "La durée estimée est obligatoire")]
        [Range(1, 1440, ErrorMessage = "La durée doit être entre 1 et 1440 minutes")]
        [Display(Name = "Duration (minutes)")]
        public int EstimatedDuration { get; set; }

        [Required(ErrorMessage = "L'heure d'arrivée effective est obligatoire")]
        [Display(Name = "Effective Arrival")]
        [DataType(DataType.DateTime)]
        public DateTime EffectiveArrival { get; set; }

        [Required(ErrorMessage = "Le lieu de départ est obligatoire")]
        [StringLength(100)]
        public string Departure { get; set; }

        [Required(ErrorMessage = "La destination est obligatoire")]
        [StringLength(100)]
        public string Destination { get; set; }

        [Required(ErrorMessage = "L'avion est obligatoire")]
        [Display(Name = "Plane")]
        public int PlaneFk { get; set; }

        [ForeignKey("PlaneFk")]
        public virtual Plane? Plane { get; set; }

        public virtual ICollection<Ticket>? Tickets { get; set; }
    }
}