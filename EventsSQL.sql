drop table if exists [Events];

-- Create Events table
CREATE TABLE [Events] (
    [EventID] NVARCHAR(10) NOT NULL,
    [EventTitle] NVARCHAR(100) NOT NULL,
    [EventDate] DATETIME2 NOT NULL,
    [EventLocation] NVARCHAR(100) NOT NULL,
    [EventDescription] NVARCHAR(500) NULL,
    [EventPhotoURL] NVARCHAR(500) NULL,
    
    CONSTRAINT [PK_Events] PRIMARY KEY ([EventID])
);

-- Create indexes for better performance
CREATE INDEX [IX_Events_EventDate] ON [Events] ([EventDate]);
