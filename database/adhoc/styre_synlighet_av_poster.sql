
update pim
	set  pim.SynligFraTid = '2015-11-07 10:30:00', pim.SynligTilTid = '2015-11-07 11:17:13' 
--select *
from postimatch pim
inner join post p on pim.Post_PostId = p.PostId
where p.Omraade = 'Oscarsborg' and pim.Match_MatchId = '4A82121D-71A6-45AE-AFD8-41574086B55C'

-- Lengst sør
update pim
	set pim.SynligFraTid =  '2015-11-07 10:42:27'
--select *
from postimatch pim
inner join post p on pim.Post_PostId = p.PostId
where p.Latitude < 59.6746 and p.Omraade = 'Oscarsborg' and pim.Match_MatchId = '4A82121D-71A6-45AE-AFD8-41574086B55C'

-- nord på sørøya
update pim
	set pim.SynligFraTid = '2015-11-07 11:03:00' -- GETDATE()
--select *
from postimatch pim
inner join post p on pim.Post_PostId = p.PostId
where p.Latitude < 59.67773 and p.Latitude > 59.6746 and p.Omraade = 'Oscarsborg' and pim.Match_MatchId = '4A82121D-71A6-45AE-AFD8-41574086B55C'