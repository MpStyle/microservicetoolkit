CREATE TABLE mt_cm_test_Item
(
    Id VARCHAR(256) NOT NULL PRIMARY KEY,
    Type VARCHAR(256) NOT NULL,
    Inserted BIGINT NOT NULL,
    Updated BIGINT NOT NULL,
    Enabled BIT NOT NULL DEFAULT 1
);

CREATE INDEX Item_Enabled ON mt_cm_test_Item (Enabled);
CREATE INDEX Item_Type ON mt_cm_test_Item (Type);