CREATE PROCEDURE ItemBulkUpsert @UpdateRecords ItemType READONLY
AS
BEGIN
MERGE INTO Item AS Target
    USING @UpdateRecords AS Source
    ON Target.Id = Source.Id
    WHEN MATCHED THEN
        UPDATE
            SET Target.Type = Source.Type,
                Target.Inserted = Source.Inserted,
                Target.Updated = Source.Updated,
                Target.Updater = Source.Updater,
                Target.Enabled = Source.Enabled
    WHEN NOT MATCHED THEN
        INSERT (Id,
                Type,
                Inserted,
                Updated,
                Updater,
                Enabled)
            VALUES (Source.Id,
                    Source.Type,
                    Source.Inserted,
                    Source.Updated,
                    Source.Updater,
                    Source.Enabled);
END