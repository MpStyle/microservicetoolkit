CREATE TYPE ItemType AS TABLE
(
    Id       VARCHAR(256) NOT NULL,
    Type     VARCHAR(256) NOT NULL,
    Inserted BIGINT       NOT NULL,
    Updated  BIGINT       NOT NULL,
    Updater  VARCHAR(256) NOT NULL,
    Enabled  BIT          NOT NULL
);