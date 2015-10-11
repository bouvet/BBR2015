using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Entities
{
    public class LagIMatch
    {
        [Column(Order = 0), Key, ForeignKey("Lag")]
        public string LagId { get; set; }

        [Column(Order = 1), Key, ForeignKey("Match")]
        public Guid MatchId { get; set; }

        public virtual Lag Lag { get; set; }

        public int PoengSum { get; set; }

        public virtual Match Match { get; set; }

        public virtual List<PostRegistrering> PostRegistreringer { get; set; }

        public virtual List<VaapenBeholdning> VåpenBeholdning { get; set; }

    }
}
