using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Entities
{
    public class Melding
    {
        public Melding(string deltakerId, string lagId, string meldingstekst)
        {
            DeltakerId = deltakerId;
            LagId = lagId;
            Tekst = meldingstekst;
        }

        public Melding() { }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string DeltakerId { get; set; }

        [Required]
        public string LagId { get; set; }

        [Required]
        public long SekvensId { get; set; }

        public DateTime Tidspunkt { get; set; }

        [MaxLength(256)]
        public string Tekst { get; private set; }
    }
}
