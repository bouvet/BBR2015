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
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public virtual Post Post { get; set; }

        [Required]
        public virtual Match Match { get; set; }

        public int CurrentPoengIndex { get; set; }

        public string PoengArray { get; set; }

        public DateTime SynligFraUTC { get; set; }
        public DateTime SynligTilUTC { get; set; }

        public bool ErSynlig 
        { get
            {
                return SynligFraUTC < TimeService.Now && TimeService.Now < SynligTilUTC;
            }
        }
        public static int[] SplitOgParse(string poengArray)
        {
            return poengArray.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                             .Select(x => Convert.ToInt32(x))
                             .ToArray();
        }

        public int HentPoengOgInkrementerIndex()
        {
            var poeng = BeregnPoengForNesteRegistrering(PoengArray, CurrentPoengIndex);
            CurrentPoengIndex++;

            return poeng;
        }

        public static int BeregnPoengForNesteRegistrering(string poengArray, int currentIndex)
        {
            var poengListe = SplitOgParse(poengArray);
            var index = currentIndex;
            if (currentIndex > poengListe.Length - 1)
                index = poengListe.Length - 1;

            return poengListe[index];
        }
    }
}
