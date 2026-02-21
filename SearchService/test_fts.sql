-- Test Full-Text Search query
SELECT TOP 5 
    d.Title, 
    d.Description,
    fts.[RANK] AS Score
FROM CONTAINSTABLE(SearchableDocuments, (Title, Description, Tags), '"financial*"') AS fts
INNER JOIN SearchableDocuments d ON fts.[KEY] = d.Id
WHERE d.IsDeleted = 0
ORDER BY fts.[RANK] DESC;
