CREATE PROCEDURE ItemPropertyUpsert(
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

    INSERT INTO ItemProperty (ItemId,
                              [Key],
                              StringValue,
                              IntValue,
                              LongValue,
                              FloatValue,
                              BoolValue,
                              [Order])
    VALUES (@ItemId,
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
        SET StringValue = @StringValue,
            IntValue    = @IntValue,
            LongValue   = @LongValue,
            FloatValue  = @FloatValue,
            BoolValue   = @BoolValue
        WHERE ItemId = @ItemId AND [Key] = @Key AND [Order] = @Order;

END CATCH