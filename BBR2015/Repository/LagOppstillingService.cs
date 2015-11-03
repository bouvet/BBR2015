using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Database;
using Database.Entities;

namespace Repository
{
    public class LagOppstillingService
    {
        private readonly DataContextFactory _dataContextFactory;
        private ConcurrentDictionary<string, Lag> _alleLag;

        public LagOppstillingService(DataContextFactory dataContextFactory)
        {
            _dataContextFactory = dataContextFactory;
        }

        public Lag Get(string lagId)
        {
            if (_alleLag == null)
                HentFraDatabasen();

            return _alleLag[lagId];
        }

        private void HentFraDatabasen()
        {
            using (var context = _dataContextFactory.Create())
            {
                _alleLag = new ConcurrentDictionary<string, Lag>(context.Lag.ToDictionary(x => x.LagId, x => x));
            }
        }
    }
}
