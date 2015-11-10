
---- Lengst sør
update pim
	set  pim.SynligTilTid = '2015-11-07 12:00:00' 
--select *
from postimatch pim
inner join post p on pim.Post_PostId = p.PostId
where p.Omraade = 'Oscarsborg'

-- Lengst sør
update pim
	set pim.SynligFraTid =  GETDATE()
--select *
from postimatch pim
inner join post p on pim.Post_PostId = p.PostId
where p.Latitude < 59.6746 and p.Omraade = 'Oscarsborg'

-- nord på sørøya
update pim
	set pim.SynligFraTid = '2015-11-07 11:03:00' -- GETDATE()
--select *
from postimatch pim
inner join post p on pim.Post_PostId = p.PostId
where p.Latitude < 59.67773 and p.Latitude > 59.6746 and p.Omraade = 'Oscarsborg'