CREATE TABLE Item
(
    Id       VARCHAR(256) NOT NULL PRIMARY KEY,
    Type     VARCHAR(256) NOT NULL,
    Inserted BIGINT       NOT NULL,
    Updated  BIGINT       NOT NULL,
    Enabled  BIT          NOT NULL DEFAULT 1
);

CREATE INDEX Item_Enabled ON Item (Enabled);
CREATE INDEX Item_Type ON Item (Type);