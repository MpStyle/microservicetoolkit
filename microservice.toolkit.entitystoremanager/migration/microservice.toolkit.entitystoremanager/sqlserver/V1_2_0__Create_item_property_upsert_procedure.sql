CREATE PROCEDURE ItemPropertyUpsert(
    @Id VARCHAR(256),
    @ItemId VARCHAR(256),
    @Key VARCHAR(256),
    @StringValue VARCHAR(MAX),
    @IntValue INT,
    @LongValue BIGINT,
    @FloatValue FLOAT,
    @BoolValue BIT,
    @Order INT
)
AS

BEGIN TRY

    INSERT INTO ItemProperty (Id,
                              ItemId,
                              [Key],
                              StringValue,
                              IntValue,
                              LongValue,
                              FloatValue,
                              BoolValue,
                              [Order])
    VALUES (@Id,
            @ItemId,
            @Key,
            @StringValue,
            @IntValue,
            @LongValue,
            @FloatValue,
            @BoolValue,
            @Order);

END TRY
BEGIN CATCH

    -- ignore duplicate key errors, throw the rest.
    IF ERROR_NUMBER() IN (2601, 2627)
        UPDATE ItemProperty
        SET ItemId      = @ItemId,
            [Key]         = @Key,
            StringValue = @StringValue,
            IntValue    = @IntValue,
            LongValue   = @LongValue,
            FloatValue  = @FloatValue,
            BoolValue   = @BoolValue,
            [Order]       = @Order
        WHERE Id = @Id;

END CATCH