using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Components.DictionaryAdapter;
using Castle.Windsor;
using Database;
using Database.Entities;
using RestApi.Tests.Infrastructure;

namespace RestApi.Tests
{
    public class Given
    {
        private IWindsorContainer _container;

        private DataContextFactory _dataContextFactory;

        public Given(IWindsorContainer _container)
        {
            this._container = _container;
            _dataContextFactory = _container.Resolve<DataContextFactory>();           
        }

        public List<Lag> ATwoTeamWithTwoPlayers()
        {
            using (var context = _dataContextFactory.Create())
            {
                var lag1 = SettOppEtLagMedDeltakere(1, 2);
                var lag2 = SettOppEtLagMedDeltakere(2, 2);
                context.Lag.Add(lag1);
                context.Lag.Add(lag2);

                context.SaveChanges();

                return new List<Lag> {lag1, lag2};
            }
        }

        private Lag SettOppEtLagMedDeltakere(int lagIndex, int antallDeltakere)
        {
            var lag = new Lag
            {
                LagId = string.Format("Lag{0}", lagIndex),
                Navn = string.Format("LagNavn{0}", lagIndex)
            };

            for (int i = 1; i <= antallDeltakere; i++)
            {
                lag.LeggTilDeltaker(new Deltaker(string.Format("Deltaker{0}-{1}", lagIndex, i), string.Format("DeltakerNavn{0}-{1}", lagIndex, i)));
            }

            return lag;
        }
    }
}
