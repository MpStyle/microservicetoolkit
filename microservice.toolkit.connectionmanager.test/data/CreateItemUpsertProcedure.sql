CREATE PROCEDURE mt_cm_test_ItemUpsert(
    @Id VARCHAR(256),
    @Type VARCHAR(256),
    @Inserted BIGINT,
    @Updated BIGINT,
    @Enabled BIT
)
AS

BEGIN TRY

INSERT INTO mt_cm_test_Item
    (Id,
    [Type],
    Inserted,
    Updated,
    Enabled)
VALUES
    (@Id,
        @Type,
        @Inserted,
        @Updated,
        @Enabled);

END TRY
BEGIN CATCH

    -- ignore duplicate key errors, throw the rest.
IF ERROR_NUMBER() IN (2601, 2627)
UPDATE mt_cm_test_Item
SET Inserted = @Inserted,
    Updated  = @Updated,
    Enabled  = @Enabled
WHERE Id = @Id
    AND Type = @Type;

END CATCH