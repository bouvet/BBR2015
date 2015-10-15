using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Entities
{
    public class VaapenBeholdning
    {
        // Surrogatnøkkel for å kunne ha flere våpen av samme type
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey("LagIMatch")]        
        public int LagIMatchId { get; set; }

        [ForeignKey("Våpen")]
        public string VaapenId { get; set; }

        [Required]
        public virtual LagIMatch LagIMatch { get; set; }

        [Required]
        public virtual Vaapen Våpen { get; set; }

        public virtual PostRegistrering BruktIPostRegistrering {get;set;}
    }
}