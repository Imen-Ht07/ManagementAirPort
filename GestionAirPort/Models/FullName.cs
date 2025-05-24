using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestionAirPort.Models
{
    public class FullName
    {
        [Required(ErrorMessage = "Le prénom est obligatoire")]
        [StringLength(50, ErrorMessage = "Le prénom ne peut pas dépasser 50 caractères")]
        [Display(Name = "Prénom")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Le nom est obligatoire")]
        [StringLength(50, ErrorMessage = "Le nom ne peut pas dépasser 50 caractères")]
        [Display(Name = "Nom")]
        public string LastName { get; set; }

        // Propriété pour le nom complet
        public string FullNameValue => $"{LastName} {FirstName}";
    }
}
