CREATE PROCEDURE ItemUpsert(
    @Id VARCHAR(256),
    @Type VARCHAR(256),
    @Inserted BIGINT,
    @Updated BIGINT,
    @Updater VARCHAR(256),
    @Enabled BIT
)
AS

BEGIN TRY

    INSERT INTO Item (Id,
                      [Type],
                      Inserted,
                      Updated,
                      Updater,
                      Enabled)
    VALUES (@Id,
            @Type,
            @Inserted,
            @Updated,
            @Updater,
            @Enabled);

END TRY
BEGIN CATCH

    -- ignore duplicate key errors, throw the rest.
    IF ERROR_NUMBER() IN (2601, 2627)
        UPDATE Item
        SET Inserted = @Inserted,
            Updated  = @Updated,
            Updater  = @Updater,
            Enabled  = @Enabled
        WHERE Id = @Id
          AND Type = @Type;

END CATCH