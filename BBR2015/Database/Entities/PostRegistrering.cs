using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Entities
{
    public class PostRegistrering
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int PoengForRegistrering { get; set; }

        [Required]
        public virtual Deltaker RegistrertAvDeltaker { get; set; }

        [Required]
        public virtual LagIMatch RegistertForLag { get; set; }

        [Column(Order = 0), Key, ForeignKey("RegistertPost")]
        public Guid PostId { get; set; }

        [Column(Order = 1), Key, ForeignKey("RegistertPost")]
        public Guid MatchId { get; set; }

        [Required]
        public virtual PostIMatch RegistertPost { get; set; }

        [ForeignKey("BruktVaapen")]
        public virtual string BruktVaapenId { get; set; }

        public virtual Vaapen BruktVaapen { get; set; }
    }
}
