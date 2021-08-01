SELECT
	JSONExtractString(FieldsData, 'message') as "Message",
	Username,
	UsernameNT,
	DatabaseName,
	DatabaseId,
	SQLText,
	MIN(Period) AS "From",
	MAX(Period) AS "To",
	COUNT(*) AS "EventsCount"
FROM XEventData
WHERE FileName LIKE 'Errors_%'
GROUP BY 
	JSONExtractString(FieldsData, 'message'),
	Username,
	UsernameNT,
	DatabaseName,
	DatabaseId,
	SQLText
ORDER BY "EventsCount" DESC