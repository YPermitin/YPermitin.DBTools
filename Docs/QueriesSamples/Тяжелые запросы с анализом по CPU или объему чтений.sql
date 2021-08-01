SELECT
	SQLText,
	SQLTextHash,
	CpuTime,
	LogicalReads * 8 / 1024 AS LogicalReadsMB,
	Duration,
	RowCount,
	Count,
	CpuTime / "CpuTime%" * 100 AS "CpuTimePercent",
	LogicalReads / "LogicalReads%" * 100 AS "LogicalReadsPercent",
	Duration / "Duration%" * 100 AS "DurationTotalPercent",
	RowCount / "RowCount%" * 100 AS "RowCountTotalPercent",
	Count / "Count%" * 100 AS "CountTotalPercent",
	"MinPeriod",
	"MaxPeriod"
FROM
	(select
		SQLText,
		SQLTextHash,
		SUM(CpuTime) AS "CpuTime",
		SUM(LogicalReads) AS "LogicalReads",
		SUM(Duration) AS "Duration",
		SUM(RowCount) AS "RowCount",
		COUNT(*) AS "Count",
		MAX(ttl.CpuTimeTotal) AS "CpuTime%",
		MAX(ttl.LogicalReadsTotal) AS "LogicalReads%",
		MAX(ttl.DurationTotal) AS "Duration%",
		MAX(ttl.RowCountTotal) AS "RowCount%",
		MAX(ttl.CountTotal) AS "Count%",
		MIN(Period) AS "MinPeriod",
		MAX(Period) AS "MaxPeriod"
	from XEventData
	CROSS JOIN (
		select
			SUM(CpuTime) "CpuTimeTotal",
			SUM(LogicalReads) AS "LogicalReadsTotal",
			SUM(Duration) AS "DurationTotal",
			SUM(RowCount) AS "RowCountTotal",
			COUNT(*) AS "CountTotal"
		from XEventData
		WHERE FileName LIKE '%Reads%' -- Фильтр по именам файла (можно и по имени лога)
	) ttl	
	WHERE FileName LIKE '%Reads%' -- Фильтр по именам файла (можно и по имени лога)
	GROUP BY SQLText, SQLTextHash
	) dt
ORDER BY "LogicalReadsMB" DESC
LIMIT 100