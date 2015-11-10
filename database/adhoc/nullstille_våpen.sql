update v set v.BruktIPostRegistrering_Id = null
--select * 
from VaapenBeholdning v
inner join LagIMatch lim on v.LagIMatchId = lim.Id
inner join Match m on lim.Match_MatchId = m.MatchId
where m.Navn = 'Bouvet Battle Royale 2015' and v.VaapenId = 'BOMBE'

