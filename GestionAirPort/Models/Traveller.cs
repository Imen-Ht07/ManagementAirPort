using System.ComponentModel.DataAnnotations;

namespace GestionAirPort.Models
{
    public class Traveller : Passenger
    {
        [Required(ErrorMessage = "Les informations de santé sont obligatoires")]
        [Display(Name = "Informations de Santé")]
        public string HealthInformation { get; set; }

        [Required(ErrorMessage = "La nationalité est obligatoire")]
        [Display(Name = "Nationalité")]
        public string Nationality { get; set; }

        public override void PassengerType()
        {
            base.PassengerType();
            Console.WriteLine($"Je suis un voyageur de nationalité {Nationality}");
        }
    }
}