using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Entities
{
    public class VaapenBeholdning
    {
        [Column(Order = 0), Key, ForeignKey("LagIMatch")]
        public string LagId { get; set; }

        [Column(Order = 1), Key, ForeignKey("LagIMatch")]
        public Guid MatchId { get; set; }

        [Column(Order = 2), Key, ForeignKey("Våpen")]
        public string VaapenId { get; set; }
        
        public virtual LagIMatch LagIMatch { get; set; }
        public virtual Vaapen Våpen { get; set; }
    }
}