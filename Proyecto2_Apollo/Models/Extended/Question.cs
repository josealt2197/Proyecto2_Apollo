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
        // private string ddl = "Elige una opción";

        [Display(Name = "Pregunta #1")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Pregunta no seleccionada.")]
        public string UserQuestionOne { get; set; }

        [Display(Name = "Respuesta #1")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Campo de respuesta vacío.")]
        public string AnswerOne { get; set; }

        [Display(Name = "Pregunta #2")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Pregunta no seleccionada.")]
        public string UserQuestionTwo { get; set; }

        [Display(Name = "Respuesta #2")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Campo de respuesta vacío.")]
        public string AnswerTwo { get; set; }

        [Display(Name = "Pregunta #3")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Pregunta no seleccionada.")]
        public string UserQuestionThree { get; set; }

        [Display(Name = "Respuesta #3")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Campo de respuesta vacío.")]
        public string AnswerThree { get; set; }

        [Display(Name = "ID del usuario")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Campo del id del usuario vacío.")]
        public int FUserID { get; set; }
    }
}

