using System.ComponentModel.DataAnnotations;

namespace GestionAirPort.Models
{
    public class Staff : Passenger
    {
        [Required(ErrorMessage = "La fonction est obligatoire")]
        [Display(Name = "Fonction")]
        public string Function { get; set; }

        [Required(ErrorMessage = "La date d'emploi est obligatoire")]
        [Display(Name = "Date d'Embauche")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime EmployementDate { get; set; }

        [Required(ErrorMessage = "Le salaire est obligatoire")]
        [Range(0, double.MaxValue, ErrorMessage = "Le salaire doit être positif")]
        [Display(Name = "Salaire")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public float Salary { get; set; }

        [Display(Name = "Ancienneté")]
        public int YearsOfService => CalculateYearsOfService();

        private int CalculateYearsOfService()
        {
            var today = DateTime.Today;
            var years = today.Year - EmployementDate.Year;
            if (EmployementDate.Date > today.AddYears(-years)) years--;
            return years;
        }

        public override void PassengerType()
        {
            base.PassengerType();
            Console.WriteLine($"Je suis un membre du personnel, fonction: {Function}");
        }
    }
}