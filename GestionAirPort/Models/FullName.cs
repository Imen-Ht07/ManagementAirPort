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
        [MinLength(3, ErrorMessage = "Min 3 caractères")]
        [MaxLength(10, ErrorMessage = "Max 10 caractères")]
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
