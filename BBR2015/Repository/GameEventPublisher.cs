using Database;

namespace Repository
{
    public class GameEventPublisher
    {
        private readonly MeldingService _meldingService;
        private readonly TilgangsKontroll _tilgangsKontroll;

        private const string AdminLagId = Constants.AdminLagId;

        public GameEventPublisher(MeldingService meldingService, TilgangsKontroll tilgangsKontroll)
        {
            _meldingService = meldingService;
            _tilgangsKontroll = tilgangsKontroll;
        }

        public void PoengScoret(string lagId, string deltakerId, int poeng)
        {
            var lagNavn = _tilgangsKontroll.HentLagNavn(lagId);
            var deltakerNavn = _tilgangsKontroll.HentDeltakerNavn(deltakerId);

            var melding = string.Format("{0} ({1}) fikk {2} poeng", deltakerNavn, lagNavn, poeng);

            _meldingService.PostMelding(deltakerId, lagId, melding);
            _meldingService.PostMelding(deltakerId, AdminLagId, melding);
        }

        public void Utl�steFelle(string lagId, string deltakerId, int poeng, string riggetAvLag)
        {
            var lagNavn = _tilgangsKontroll.HentLagNavn(lagId);
            var deltakerNavn = _tilgangsKontroll.HentDeltakerNavn(deltakerId);
            var riggetAvLagNavn = _tilgangsKontroll.HentLagNavn(riggetAvLag);

            var melding = string.Format("{0} ({1}) utl�ste en felle rigget av {2} og mistet {3} poeng.", deltakerNavn, lagNavn, riggetAvLagNavn, poeng);

            _meldingService.PostMelding(deltakerId, lagId, melding);
            _meldingService.PostMelding(deltakerId, riggetAvLag, melding);
            _meldingService.PostMelding(deltakerId, AdminLagId, melding);
        }

        public void AlleredeRegistrert(string lagId, string deltakerId)
        {
            var lagNavn = _tilgangsKontroll.HentLagNavn(lagId);
            var deltakerNavn = _tilgangsKontroll.HentDeltakerNavn(deltakerId);

            var melding = string.Format("{0} ({1}) pr�vde � registrere en post som laget allerede har registrert.", deltakerNavn, lagNavn);

            _meldingService.PostMelding(deltakerId, lagId, melding);
            _meldingService.PostMelding(deltakerId, AdminLagId, melding);
        }

        public void BrukteBombe(string lagId, string deltakerId)
        {
            var lagNavn = _tilgangsKontroll.HentLagNavn(lagId);
            var deltakerNavn = _tilgangsKontroll.HentDeltakerNavn(deltakerId);

            var melding = string.Format("{0} ({1}) sprengte en post i lufta med en bombe.", deltakerNavn, lagNavn);

            _meldingService.PostMelding(deltakerId, lagId, melding);
            _meldingService.PostMelding(deltakerId, AdminLagId, melding);
        }

        public void RiggetEnFelle(string lagId, string deltakerId)
        {
            var lagNavn = _tilgangsKontroll.HentLagNavn(lagId);
            var deltakerNavn = _tilgangsKontroll.HentDeltakerNavn(deltakerId);

            var melding = string.Format("{0} ({1}) rigget en felle.", deltakerNavn, lagNavn);

            _meldingService.PostMelding(deltakerId, lagId, melding);
            _meldingService.PostMelding(deltakerId, AdminLagId, melding);
        }

        public void Pr�vde�RegistrereEnPostMedFeilKode(string lagId, string deltakerId)
        {
            var lagNavn = _tilgangsKontroll.HentLagNavn(lagId);
            var deltakerNavn = _tilgangsKontroll.HentDeltakerNavn(deltakerId);

            var melding = string.Format("{0} ({1}) pr�vde � registrere en post, men brukte feil kode.", deltakerNavn, lagNavn);

            _meldingService.PostMelding(deltakerId, lagId, melding);
            _meldingService.PostMelding(deltakerId, AdminLagId, melding);
        }

        public void Pr�vde�RegistrereEnPostSomIkkeErSynlig(string lagId, string deltakerId)
        {
            var lagNavn = _tilgangsKontroll.HentLagNavn(lagId);
            var deltakerNavn = _tilgangsKontroll.HentDeltakerNavn(deltakerId);

            var melding = string.Format("{0} ({1}) pr�vde � registrere en post som ikke er synlig.", deltakerNavn, lagNavn);

            _meldingService.PostMelding(deltakerId, lagId, melding);
            _meldingService.PostMelding(deltakerId, AdminLagId, melding);
        }
    }
}