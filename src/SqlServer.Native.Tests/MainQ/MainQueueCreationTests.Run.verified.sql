-- Tables

CREATE TABLE [dbo].[MainQueueCreationTests](
	[Id] [uniqueidentifier] NOT NULL,
	[CorrelationId] [varchar](255) NULL,
	[ReplyToAddress] [varchar](255) NULL,
	[Recoverable] [bit] NOT NULL,
	[Expires] [datetime] NULL,
	[Headers] [nvarchar](max) NOT NULL,
	[BodyString]  AS (CONVERT([varchar](max),[Body])),
	[Body] [varbinary](max) NULL,
	[RowVersion] [bigint] IDENTITY(1,1) NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

CREATE NONCLUSTERED INDEX [Index_Expires] ON [dbo].[MainQueueCreationTests]
(
	[Expires] ASC
)
INCLUDE([Id],[RowVersion]) 
WHERE ([Expires] IS NOT NULL)
WITH () ON [PRIMARY]
CREATE NONCLUSTERED INDEX [Index_RowVersion] ON [dbo].[MainQueueCreationTests]
(
	[RowVersion] ASC
) ON [PRIMARY]