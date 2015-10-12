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

        public virtual Deltaker RegistrertAvDeltaker { get; set; }

        public virtual LagIMatch RegistertForLag { get; set; }
       
        public virtual PostIMatch RegistertPost { get; set; }

        [ForeignKey("BruktVaapen")]
        public virtual string BruktVaapenId { get; set; }

        public virtual Vaapen BruktVaapen { get; set; }
    }
}
