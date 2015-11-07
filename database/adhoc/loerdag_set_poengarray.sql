update pim
	set PoengArray = '300,200,100,80,50'
--select *
from postimatch pim
inner join post p on pim.Post_PostId = p.PostId
where p.Navn in ('Post 2', 'Post 6')


update pim
	set pim.SynligFraTid = '2015-11-07 12:30:00' -- GETDATE()
--select *
from postimatch pim
inner join post p on pim.Post_PostId = p.PostId
where p.Latitude < 59.6746 and p.Omraade = 'Oscarsborg'

update pim
	set PoengArray = '200,100,80,70,50'
--select *
from postimatch pim
inner join post p on pim.Post_PostId = p.PostId
where p.Navn in ('Post 25')


update pim
	set PoengArray = '300,200,100,80,50'
--select *
from postimatch pim
inner join post p on pim.Post_PostId = p.PostId
where p.Navn in ( 'POST 19', 'POST 17')

update pim
	set pim.SynligTilTid = '2015-11-07 12:30:00' -- GETDATE()
from postimatch pim
inner join post p on pim.Post_PostId = p.PostId
where p.Omraade = 'Oscarsborg'