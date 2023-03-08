CREATE PROCEDURE ItemPropertyBulkUpsert @UpdateRecords ItemPropertyType READONLY
AS
BEGIN
    MERGE INTO ItemProperty AS Target
    USING @UpdateRecords AS Source
    ON Target.Id = Source.Id
    WHEN MATCHED THEN
        UPDATE
        SET Target.Id=Source.Id,
            Target.ItemId=Source.ItemId,
            Target.[Key]=Source.[Key],
            Target.StringValue=Source.StringValue,
            Target.IntValue=Source.IntValue,
            Target.LongValue=Source.LongValue,
            Target.FloatValue=Source.FloatValue,
            Target.BoolValue=Source.BoolValue,
            Target.[Order]=Source.[Order]
    WHEN NOT MATCHED THEN
        INSERT (Id,
                ItemId,
                [Key],
                StringValue,
                IntValue,
                LongValue,
                FloatValue,
                BoolValue,
                [Order])
        VALUES (Source.Id,
                Source.ItemId,
                Source.[Key],
                Source.StringValue,
                Source.IntValue,
                Source.LongValue,
                Source.FloatValue,
                Source.BoolValue,
                Source.[Order]);
END