CREATE TABLE MyCustomItem
(
    Id       VARCHAR(256) NOT NULL PRIMARY KEY,
    Type     VARCHAR(256) NOT NULL,
    Inserted BIGINT       NOT NULL,
    Updated  BIGINT       NOT NULL,
    Updater  VARCHAR(256) NOT NULL,
    Enabled  BIT          NOT NULL DEFAULT 1
);

CREATE INDEX MyCustomItem_Type ON MyCustomItem (Type);

CREATE TABLE MyCustomItemProperty
(
    ItemId VARCHAR(256) NOT NULL, 
    [Key]  VARCHAR(256) NOT NULL,
    StringValue VARCHAR(MAX) ,
    IntValue    INT          ,
    LongValue   BIGINT       ,
    FloatValue  FLOAT        ,
    BoolValue   BIT          ,
    [Order]     INT          DEFAULT 1,
    CONSTRAINT pk_my_custom_item_property PRIMARY KEY (ItemId, [Key], [Order])
);

CREATE INDEX MyCustomItemProperty_Key ON MyCustomItemProperty ([Key]);