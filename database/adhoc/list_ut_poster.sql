 select *
from postimatch pim
inner join post p on pim.Post_PostId = p.PostId
where p.Omraade = 'Oscarsborg'