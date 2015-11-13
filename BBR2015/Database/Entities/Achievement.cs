using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Entities
{
    public class Achievement
    {
        [Required]
        [Column(Order = 0), Key]
        public string LagId{ get; set; }

        [Required]
        [Column(Order = 1), Key]
        public string AchievementType { get; set; }

        [Required]
        public int Score { get; set; }

        [ForeignKey("LagId")]
        public virtual Lag Lag { get; set; }

        public DateTime Tidspunkt { get; set; }
    }
}
