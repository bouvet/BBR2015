using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Entities
{
    public class Achievement
    {
        [Required]
        public virtual string LagLagId{ get; set; }

        [Required]
        public string AchievementType { get; set; }

        [Required]
        public int Score { get; set; }
    }
}
