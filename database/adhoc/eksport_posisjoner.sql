declare @start datetime = '2015-11-07 10:30'
declare @end datetime = '2015-11-07 11:30'
select 
	p.Latitude, 
	p.Longitude, 
	p.DeltakerId as Name, 
	l.Farge as LineStringColor, 
	p.Tidspunkt as TimeBegin,
	isnull((select MIN(tidspunkt) from DeltakerPosisjon where DeltakerId = p.deltakerid and p.id < id), @end) as TimeEnd
from DeltakerPosisjon p
inner join Lag l on p.LagId = l.LagId
where p.Tidspunkt between @start and @end
