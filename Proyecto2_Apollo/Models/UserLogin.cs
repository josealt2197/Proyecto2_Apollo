using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Proyecto2_Apollo.Models
{
    public class UserLogin
    {

        [Display(Name = "Correo Electrónico")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Se requiere su correo electrónico")]
        [DataType(DataType.Text)]
        public string Email { get; set; }

        [Display(Name = "Cédula")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Se requiere su cédula")]
        [DataType(DataType.Text)]
        public string ID { get; set; }

        [Display(Name = "Contraseña")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Se requiere su contraseña")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Recordarme")]
        public bool RememberMe { get; set; }

    }
}