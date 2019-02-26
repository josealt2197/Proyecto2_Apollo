using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Proyecto2_Apollo.Models
{
    [MetadataType(typeof(QuestionMetadata))]
    public partial class Question
    {

    }

    public class QuestionMetadata
    {
        [Display(Name = "Pregunta #1")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Pregunta no seleccionada.")]
        public string UserQuestion { get; set; }

        [Display(Name = "Respuesta")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Campo de respuesta vacío.")]
        public string Answers { get; set; }

        [Display(Name = "UserID")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Campo de UserID vacío.")]
        public int FUserID { get; set; }
    }
}
