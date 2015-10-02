using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Entities
{
    public class LagIMatch
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public virtual Lag Lag { get; set; }

        public int PoengSum { get; set; }

        public virtual Match Match { get; set; }

        public virtual List<PostRegistrering> PostRegistreringer { get; set; }

        public virtual List<Vaapen> VåpenBeholdning { get; set; }

    }
}
