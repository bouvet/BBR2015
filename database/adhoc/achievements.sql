
--insert into Achievement (LagId, AchievementType, Score, Tidspunkt)

select vi.lagid, vi.kode, score.poeng, getdate() from
(
select 'KODE', 'LAGID_1' union all
select 'KODE', 'LAGID_2' 
) VI (kode, lagid)
inner join 
(
	select 'I_SEE_LIVE_PEOPLE', 200 UNION ALL
	select 'POSTMANN_PAT', 200
)score(kode, poeng) on vi.kode = score.kode
left join Achievement a on vi.lagid = a.LagId and vi.kode = a.AchievementType
where a.LagId is null