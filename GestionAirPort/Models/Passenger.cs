using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestionAirPort.Models
{
    public class Passenger
    {
        [Key]
        [Required(ErrorMessage = "Le numéro de passeport est obligatoire")]
        [StringLength(7, MinimumLength = 7, ErrorMessage = "Le numéro de passeport doit contenir exactement 7 caractères")]
        [Display(Name = "Numéro de Passeport")]
        public string PassportNumber { get; set; }

        [Required(ErrorMessage = "Le nom complet est obligatoire")]
        [Display(Name = "Nom Complet")]
        public FullName FullName { get; set; }

        [Required(ErrorMessage = "La date de naissance est obligatoire")]
        [Display(Name = "Date de Naissance")]
        [DataType(DataType.Date)]
        public DateTime BirthDate { get; set; }

        [Required(ErrorMessage = "Le numéro de téléphone est obligatoire")]
        [RegularExpression(@"^\+?[0-9\s\-()]{7,20}$", ErrorMessage = "Numéro de téléphone invalide")]
        [Display(Name = "Téléphone")]
        public string TelNumber { get; set; }

        [Required(ErrorMessage = "L'adresse email est obligatoire")]
        [EmailAddress(ErrorMessage = "Format d'email invalide")]
        [Display(Name = "Email")]
        public string EmailAddress { get; set; }

        public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

        [Display(Name = "Âge")]
        public int Age => CalculateAge();

        private int CalculateAge()
        {
            var today = DateTime.Today;
            var age = today.Year - BirthDate.Year;
            if (BirthDate.Date > today.AddYears(-age)) age--;
            return age;
        }

        public override string ToString()
        {
            return $"{FullName.FirstName} {FullName.LastName} ({PassportNumber})";
        }

        public bool CheckProfile(string firstName, string lastName, string email = null)
        {
            var nameMatch = string.Equals(FullName.FirstName, firstName, StringComparison.OrdinalIgnoreCase) &&
                          string.Equals(FullName.LastName, lastName, StringComparison.OrdinalIgnoreCase);

            return email == null ? nameMatch : nameMatch && string.Equals(EmailAddress, email, StringComparison.OrdinalIgnoreCase);
        }

        public virtual void PassengerType()
        {
            Console.WriteLine($"Je suis le passager: {FullName.FirstName} {FullName.LastName}");
        }
    }
}

