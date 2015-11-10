update pim
	set PoengArray = '500,300,100,80,50'
--select *
from postimatch pim
inner join post p on pim.Post_PostId = p.PostId
where p.Navn in ('Fredagpost 4', 'Fredagpost 5', 'Fredagpost 6')