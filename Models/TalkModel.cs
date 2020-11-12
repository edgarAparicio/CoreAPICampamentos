using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoreCodeCamp.Models
{
    public class TalkModel
    {
        public int TalkId { get; set; }
        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        [StringLength(4000, MinimumLength = 40)]
        public string Abstract { get; set; }

        [Range(100, 300)]
        public int Level { get; set; }

        //Propiedad para mostar el speaker de este modelo 
        public SpeakerModel Speaker { get; set; }
    }
}
