CREATE TABLE Item
(
    Id       VARCHAR(256) NOT NULL PRIMARY KEY,
    Type     VARCHAR(256) NOT NULL,
    Inserted BIGINT       NOT NULL,
    Updated  BIGINT       NOT NULL,
    Updater  VARCHAR(256) NOT NULL,
    Enabled  BIT          NOT NULL DEFAULT 1
);

CREATE INDEX Item_Type ON Item (Type);

CREATE TABLE ItemProperty
(
    Id     VARCHAR(256) NOT NULL PRIMARY KEY,
    ItemId VARCHAR(256) NOT NULL, 
    [Key]  VARCHAR(256) NOT NULL,
    StringValue VARCHAR(MAX) ,
    IntValue    INT          ,
    LongValue   BIGINT       ,
    FloatValue  FLOAT        ,
    BoolValue   BIT          ,
    [Order]     INT          DEFAULT 1
);

CREATE INDEX ItemProperty_Key ON ItemProperty ([Key]);