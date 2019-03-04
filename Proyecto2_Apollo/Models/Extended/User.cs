using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Security;

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
        [Display(Name = "Cédula")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Se requiere su Cédula")]
        public string ID { get; set; }

        [Display(Name = "Nombre")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Se requiere su Nombre")]
        public string FirstName { get; set; }

        [Display(Name = "Apellido")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Se requiere su Apellido")]
        public string LastName { get; set; }

        [Display(Name = "Correo electrónico")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Se requiere su Correo Electronico")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Display(Name = "Teléfono")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Se requiere su Número de Teléfono")]
        [DataType(DataType.PhoneNumber)]
        public string Phone { get; set; }

        /*[Display(Name = "Dirección")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Se requiere su Dirección")]
        [DataType(DataType.Text)]
        public string Address { get; set; }*/
        [Display(Name = "Provincia")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Provincia no seleccionada.")]
        public string Province { get; set; }

        [Display(Name = "Distrito")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Distrito no seleccionado.")]
        public string District { get; set; }

        [Display(Name = "Cantón")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Cantón no seleccionado.")]
        public string Section { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Se requiere su contraseña")]
        [MembershipPassword(
            MinRequiredNonAlphanumericCharacters = 1,
            MinNonAlphanumericCharactersError = "Su contraseña debe contener al menos un símbolo (!, @, #, etc).",
            ErrorMessage = "Su nueva contraseña debe tener entre 8 y 11 caracteres, y debe contener al menos un símbolo. (!, @, #, etc).",
            MinRequiredPasswordLength = 8
        )]
        //[MaxLength(11, ErrorMessage = "Su contraseña debe tener un máximo de 11 caracteres")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; }

        [Display(Name = "Confirmar Contraseña")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Ambas contraseñas deben coincidir")]
        public string ConfirmPassword { get; set; }
    }
}