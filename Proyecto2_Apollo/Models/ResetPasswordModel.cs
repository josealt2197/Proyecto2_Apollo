using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace Proyecto2_Apollo.Models
{
    public class ResetPasswordModel
    {
        [Display(Name = "Contraseña")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Se requiere su contraseña")]
        [MembershipPassword(
            MinRequiredNonAlphanumericCharacters = 1,
            MinNonAlphanumericCharactersError = "Su nueva contraseña debe contener al menos un símbolo (!, @, #, etc).",
            ErrorMessage = "Su nueva contraseña debe tener entre 8 y 11 caracteres, y debe contener al menos un símbolo. (!, @, #, etc).",
            MinRequiredPasswordLength = 8
        )]
        //[MaxLength(11, ErrorMessage = "Su contraseña debe tener un máximo de 11 caracteres")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Display(Name = "Confirmar Contraseña")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Ambas contraseñas deben coincidir")]
        public string ConfirmPassword { get; set; }

        [Required]
        public string ResetCode { get; set; }
    }
}