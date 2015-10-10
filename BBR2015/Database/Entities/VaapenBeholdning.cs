using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Entities
{
    public class VaapenBeholdning
    {
        [Column(Order = 0), Key, ForeignKey("Lag")]
        public string LagId { get; set; }

        [Column(Order = 1), Key, ForeignKey("Våpen")]
        public string VaapenId { get; set; }
        
        public virtual Lag Lag { get; set; }
        public virtual Vaapen Våpen { get; set; }
    }
}