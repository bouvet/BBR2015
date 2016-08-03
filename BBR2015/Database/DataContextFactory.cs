using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database
{
    public class DataContextFactory
    {
        private OverridableSettings _appSettings;

        public DataContextFactory(OverridableSettings appSettings)
        {
            _appSettings = appSettings;
        }

        public DataContext Create()
        {            
            return new DataContext(_appSettings.DatabaseConnectionString);
            
        }

        /// <summary>
        /// DANGER ZONE
        /// Though useful to run in setup of end-to-end tests in transaction scope - but remember rollback!
        /// </summary>
        public void DeleteAllData()
        {
            using (var context = Create())
            {
                context.VåpenBeholdning.Clear();
                context.PostRegisteringer.Clear();
                context.LagIMatch.Clear();
                context.PosterIMatch.Clear();
                context.Matcher.Clear();
                context.DeltakerPosisjoner.Clear();
                context.Meldinger.Clear();
                context.Deltakere.Clear();
                context.Lag.Clear();
                context.Våpen.Clear();
                context.Poster.Clear();
                context.Achievements.Clear();

                context.SaveChanges();
            }
        }
    }
}
