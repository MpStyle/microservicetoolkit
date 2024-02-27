CREATE PROCEDURE ItemPropertyBulkUpsert @UpdateRecords ItemPropertyType READONLY
AS
BEGIN
    MERGE INTO ItemProperty AS Target
    USING @UpdateRecords AS Source
    ON Target.ItemId = Source.ItemId AND Target.[Key] = Source.[Key] AND Target.[Order] = Source.[Order]
    WHEN MATCHED THEN
        UPDATE
        SET Target.StringValue=Source.StringValue,
            Target.IntValue=Source.IntValue,
            Target.LongValue=Source.LongValue,
            Target.FloatValue=Source.FloatValue,
            Target.BoolValue=Source.BoolValue
    WHEN NOT MATCHED THEN
        INSERT (ItemId,
                [Key],
                StringValue,
                IntValue,
                LongValue,
                FloatValue,
                BoolValue,
                [Order])
        VALUES (Source.ItemId,
                Source.[Key],
                Source.StringValue,
                Source.IntValue,
                Source.LongValue,
                Source.FloatValue,
                Source.BoolValue,
                Source.[Order]);
END