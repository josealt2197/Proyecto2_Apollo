using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

//namespace Proyecto2_Apollo.Models.Extended
namespace Proyecto2_Apollo.Models
{
    [MetadataType(typeof(UserMetadata))]
    public partial class User
    {
        public string ConfirmPassword { get; set; }
    }

    public class UserMetadata
    {
        [Display(Name = "ID")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Se requiere su Cédula")]
        public string ID { get; set; }

        [Display(Name = "First Name")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Se requiere su Nombre")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Se requiere su Apellido")]
        public string LastName { get; set; }

        [Display(Name = "Email")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Se requiere su Correo Electronico")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Display(Name = "Phone")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Se requiere su Número de Teléfono")]
        [DataType(DataType.PhoneNumber)]
        public string Phone { get; set; }

        [Display(Name = "Address")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Se requiere su Dirección")]
        [DataType(DataType.Text)]
        public string Address { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Se requiere su contraseña")]
        [DataType(DataType.Password)]
        [MinLength(8, ErrorMessage = "Se requiere un mínimo de 8 caracteres")]
//        [MaxLength(11, ErrorMessage = "Se requiere un máximo de 11 caracteres")]
        public string Password { get; set; }

        [Display(Name = "Confirm Password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Ambas contraseñas deben coincidir")]
        public string ConfirmPassword { get; set; }
    }
}