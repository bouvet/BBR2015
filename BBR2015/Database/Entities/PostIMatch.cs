using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Entities
{
    public class PostIMatch
    {
        [Column(Order = 0), Key, ForeignKey("Post")]
        public Guid PostId { get; set; }

        [Column(Order = 1), Key, ForeignKey("Match")]
        public Guid MatchId { get; set; }

        public virtual Post Post { get; set; }

        public virtual Match Match { get; set; }

        public int CurrentPoengIndex { get; set; }

        public string PoengArray { get; set; }

        public DateTime SynligFraUTC { get; set; }
        public DateTime SynligTilUTC { get; set; }

        public bool ErSynlig { get
            {
                return SynligFraUTC < DateTime.UtcNow && DateTime.UtcNow < SynligTilUTC;
            }
        }

        public int[] GetPoengArray()
        {
            return SplitOgParse(PoengArray);
        }

        public static int[] SplitOgParse(string poengArray)
        {
            return poengArray.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                             .Select(x => Convert.ToInt32(x))
                             .ToArray();
        }

        public int HentPoengOgInkrementerIndex()
        {
            var poeng = GetPoengArray()[CurrentPoengIndex];
            CurrentPoengIndex++;

            return poeng;
        }
    }
}
