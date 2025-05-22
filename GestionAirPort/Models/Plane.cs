using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestionAirPort.Models
{
    public enum PlaneType 
            {
        Commercial,
        Cargo,
        Private,
        Military
    }
    public class Plane
    {
        public Plane()
        {
            // Initialiser la collection Flights dans le constructeur
            Flights = new HashSet<Flight>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PlaneId { get; set; }

        [Required]
        [Range(10, 1000, ErrorMessage = "La capacité doit être comprise entre 10 et 1000.")]
        public int Capacity { get; set; }

        [Required(ErrorMessage = "La date de fabrication est obligatoire")]
        public DateTime ManufactureDate { get; set; }

        public PlaneType PlaneType { get; set; }

        // Collection virtuelle de vols
        public virtual ICollection<Flight> Flights { get; set; }

        [NotMapped]
        public string Information { get { return PlaneId + " " + ManufactureDate + " " + Capacity; } }

        public override string ToString()
        {
            return "Capacity: " + Capacity + " PlaneType: " + PlaneType;
        }
    }
}
