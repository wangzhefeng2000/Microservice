﻿CREATE TABLE [Boundary].[Poll]
(
	[Id] BIGINT NOT NULL IDENTITY(1,1) PRIMARY KEY,
	[TimeStamp] DATETIME2 NOT NULL DEFAULT(SYSUTCDATETIME()),
	[ServiceName] VARCHAR(50) NOT NULL,
	[ServiceId] VARCHAR(50) NOT NULL,
	[BatchId] UNIQUEIDENTIFIER NOT NULL,

	[Requested] INT NOT NULL,
	[Returned] INT NOT NULL, 
    [ChannelId] SMALLINT NULL,
)