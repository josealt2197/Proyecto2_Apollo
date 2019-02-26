using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Proyecto2_Apollo.Models
{
    public class TwoStepCodeModel
    {
        [Required(ErrorMessage = "Code required", AllowEmptyStrings = false)]
        public string codes { get; set; }

        [Required]
        public string CodeSet { get; set; }
    }
}