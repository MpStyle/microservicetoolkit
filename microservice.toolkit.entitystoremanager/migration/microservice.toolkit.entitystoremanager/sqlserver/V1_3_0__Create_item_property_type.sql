CREATE TYPE ItemPropertyType AS TABLE
(
    ItemId      VARCHAR(256) NOT NULL,
    [Key]       VARCHAR(256) NOT NULL,
    StringValue VARCHAR(MAX) ,
    IntValue    INT          ,
    LongValue   BIGINT       ,
    FloatValue  FLOAT        ,
    BoolValue   BIT          ,
    [Order]     INT          NOT NULL DEFAULT 0
);