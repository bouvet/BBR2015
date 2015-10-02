using Database.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database
{
    public class InitialDataCreator
    {
        private DataContextFactory _dataContextFactory;

        public InitialDataCreator(DataContextFactory dataContextFactory)
        {
            _dataContextFactory = dataContextFactory;
        }
    
        public void FyllDatabasen()
        {
            using (var context = _dataContextFactory.Create())
            {
                var alleLag = context.Lag.Include(x => x.Deltakere).ToList();

                if (alleLag.Count > 0)
                    return;

                var bbr1 = new Lag("BBR1", "BBR #1", "00FF00", "abc1.gif");
                bbr1.LeggTilDeltaker(new Deltaker("BBR1-A", "BBR1-A"));
                bbr1.LeggTilDeltaker(new Deltaker("BBR1-B", "BBR1-B"));
                bbr1.LeggTilDeltaker(new Deltaker("BBR1-C", "BBR1-C"));
                context.Lag.Add(bbr1);

                var bbr2 = new Lag("BBR2", "BBR #2", "FFFF00", "abc2.gif");
                bbr2.LeggTilDeltaker(new Deltaker("BBR2-A", "BBR2-A"));
                bbr2.LeggTilDeltaker(new Deltaker("BBR2-B", "BBR2-B"));
                bbr2.LeggTilDeltaker(new Deltaker("BBR2-C", "BBR2-C"));
                context.Lag.Add(bbr2);

                var bbr3 = new Lag("BBR3", "BBR #3", "00FFFF", "abc1.gif");
                bbr1.LeggTilDeltaker(new Deltaker("BBR3-A", "BBR3-A"));
                bbr1.LeggTilDeltaker(new Deltaker("BBR3-B", "BBR3-B"));
                bbr1.LeggTilDeltaker(new Deltaker("BBR3-C", "BBR3-C"));
                context.Lag.Add(bbr3);

                var match = new Match()
                {
                    MatchId = Guid.NewGuid(),
                    Navn = "Treningsrunde",
                    StartUTC = new DateTime(2015, 10, 01),
                    SluttUTC = new DateTime(2015, 11, 01)
                };

                context.SaveChanges();
            }
        }
    }
}
