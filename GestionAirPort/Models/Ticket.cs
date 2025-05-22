using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionAirPort.Models
{
    public class Ticket
    {
        [Key]
        public int TicketId { get; set; }

        [Required(ErrorMessage = "Le prix est obligatoire")]
        [Range(0, 10000, ErrorMessage = "Le prix doit être entre 0 et 10000")]
        [Display(Name = "Prix")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Prix { get; set; }

        [Required(ErrorMessage = "Le numéro de siège est obligatoire")]
        [StringLength(10, ErrorMessage = "Le numéro de siège ne peut pas dépasser 10 caractères")]
        [Display(Name = "Numéro de Siège")]
        [RegularExpression(@"^[A-Z][0-9]+$", ErrorMessage = "Le format du siège doit être une lettre suivie de chiffres (ex: A12)")]
        public string Siege { get; set; }

        [Display(Name = "Statut VIP")]
        public bool VIP { get; set; }

        [Required(ErrorMessage = "Le passager est obligatoire")]
        [Display(Name = "Passager")]
        public virtual Passenger Passenger { get; set; }

        [Required(ErrorMessage = "Le numéro de passeport est obligatoire")]
        [Display(Name = "Numéro de Passeport")]
        [ForeignKey("Passenger")]
        public string PassengerFk { get; set; }

        [Required(ErrorMessage = "Le vol est obligatoire")]
        [Display(Name = "Vol")]
        public virtual Flight Flight { get; set; }

        [Required(ErrorMessage = "L'ID du vol est obligatoire")]
        [Display(Name = "Numéro de Vol")]
        [ForeignKey("Flight")]
        public int FlightFk { get; set; }

        [NotMapped]
        public string TicketInfo => $"Vol {FlightFk} - Siège {Siege} {(VIP ? "(VIP)" : "")}";
    }
}