CREATE TABLE mt_cm_test_ItemProperty
(
    Id VARCHAR(256) NOT NULL PRIMARY KEY,
    ItemId VARCHAR(256) NOT NULL,
    [Key] VARCHAR(256) NOT NULL,
    StringValue VARCHAR(MAX) ,
    IntValue INT          ,
    LongValue BIGINT       ,
    FloatValue FLOAT        ,
    BoolValue BIT          ,
    [Order] INT DEFAULT 1
);