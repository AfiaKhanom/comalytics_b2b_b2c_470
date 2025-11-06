-- Step 1: Create temporary table for duplicates (non-deleted only)
IF OBJECT_ID('tempdb..#DuplicateGroups') IS NOT NULL DROP TABLE #DuplicateGroups;

SELECT 
    sa.ShipToCode,
    map.ErpAccount_Id,
    COUNT(*) as DuplicateCount
INTO #DuplicateGroups
FROM Erp_ShipToAddress sa
INNER JOIN Erp_ShiptoAddress_Erp_Account_Map map ON sa.Id = map.ErpShiptoAddress_Id
WHERE map.ErpShipToAddressCreatedByTypeId = 10
    AND sa.ShipToCode IS NOT NULL 
    AND LTRIM(RTRIM(sa.ShipToCode)) <> ''
GROUP BY sa.ShipToCode, map.ErpAccount_Id
HAVING COUNT(*) > 1;

PRINT 'Step 1 Complete: Found ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' duplicate groups';

-- Step 2: Rank records within each duplicate group
IF OBJECT_ID('tempdb..#RankedDuplicates') IS NOT NULL DROP TABLE #RankedDuplicates;

SELECT
    sa.Id,
    sa.ShipToCode,
    sa.IsActive,
    sa.CreatedOnUtc,
    sa.UpdatedOnUtc,
    map.ErpAccount_Id,
    dg.DuplicateCount,
    -- Ranking: Active records first, then by latest update date
    ROW_NUMBER() OVER (
        PARTITION BY sa.ShipToCode, map.ErpAccount_Id

        ORDER BY sa.IsActive DESC, sa.IsDeleted ASC, sa.UpdatedOnUtc DESC
    ) as RecordRank
INTO #RankedDuplicates
FROM Erp_ShipToAddress sa
INNER JOIN Erp_ShiptoAddress_Erp_Account_Map map ON sa.Id = map.ErpShiptoAddress_Id
INNER JOIN #DuplicateGroups dg ON sa.ShipToCode = dg.ShipToCode AND map.ErpAccount_Id = dg.ErpAccount_Id
WHERE
 map.ErpShipToAddressCreatedByTypeId = 10 AND
	sa.ShipToCode IS NOT NULL 
    AND LTRIM(RTRIM(sa.ShipToCode)) <> '';

PRINT 'Step 2 Complete: Ranked ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' duplicate records';

-- Step 3: Analysis - Show what will be kept vs processed
PRINT '=== ANALYSIS RESULTS ===';

-- Records to KEEP (Rank = 1)
SELECT 
    'KEEP' as Action,
    ShipToCode,
    ErpAccount_Id,
    Id,
    IsActive,
    CreatedOnUtc,
    UpdatedOnUtc,
    DuplicateCount
FROM #RankedDuplicates 
WHERE RecordRank = 1
ORDER BY ShipToCode, ErpAccount_Id;

-- Records to PROCESS (Rank > 1)
SELECT
    'PROCESS' as Action,
    ShipToCode,
    ErpAccount_Id,
    Id,
    IsActive,
    CreatedOnUtc,
    UpdatedOnUtc,
    RecordRank
FROM #RankedDuplicates
WHERE RecordRank > 1
ORDER BY ShipToCode, ErpAccount_Id, RecordRank;

-- Summary Statistics
PRINT '=== SUMMARY ===';
SELECT
    COUNT(CASE WHEN RecordRank = 1 THEN 1 END) as RecordsToKeep,
    COUNT(CASE WHEN RecordRank > 1 THEN 1 END) as RecordsToProcess,
    COUNT(*) as TotalDuplicateRecords
FROM #RankedDuplicates;

-- Step 4: Apply Process_Duplicates logic (only to records with Rank > 1)
UPDATE sa
SET
    ShipToCode = sa.ShipToCode + '_deleted',
    IsActive = 0,
    IsDeleted = 1,
    UpdatedOnUtc = GETUTCDATE()
FROM Erp_ShipToAddress sa
INNER JOIN #RankedDuplicates rd ON sa.Id = rd.Id
WHERE rd.RecordRank > 1;

PRINT 'Step 4 Complete: Processed ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' duplicate records';

-- Step 5: Verification
PRINT '=== VERIFICATION ===';

-- Check processed records
SELECT
    'PROCESSED' as Status,
    ShipToCode,
    IsActive,
    IsDeleted,
    UpdatedOnUtc
FROM Erp_ShipToAddress
WHERE ShipToCode LIKE '%_deleted'
    AND UpdatedOnUtc >= DATEADD(MINUTE, -5, GETUTCDATE())
ORDER BY UpdatedOnUtc DESC;

-- Verify no more duplicates exist (should return 0 rows) with ship-to-address IDs shown as comma-separated values
SELECT 
    sa.ShipToCode, 
    map.ErpAccount_Id, 
    COUNT(*) as RemainingCount,
    STRING_AGG(CAST(sa.Id AS VARCHAR), ', ') as ShipToAddressIds
FROM Erp_ShipToAddress sa 
INNER JOIN Erp_ShiptoAddress_Erp_Account_Map map ON sa.Id = map.ErpShiptoAddress_Id 
WHERE map.ErpShipToAddressCreatedByTypeId = 10 
  AND sa.ShipToCode IS NOT NULL 
  AND LTRIM(RTRIM(sa.ShipToCode)) <> '' 
  AND sa.ShipToCode NOT LIKE '%_deleted' 
GROUP BY sa.ShipToCode, map.ErpAccount_Id 
HAVING COUNT(*) > 1
ORDER BY sa.ShipToCode, map.ErpAccount_Id;

-- Clean up temporary tables
DROP TABLE #DuplicateGroups;
DROP TABLE #RankedDuplicates;

PRINT '=== SCRIPT COMPLETED ===';


-- for shipToAddresses having empty/null shipToCode
UPDATE Erp_ShipToAddress
SET ShipToCode = CONCAT('M-', Id)
WHERE ShipToCode IS NULL OR LTRIM(RTRIM(ShipToCode)) = '';
