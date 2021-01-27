-- Tables

CREATE TABLE [dbo].[Subscription](
	[QueueAddress] [nvarchar](200) NOT NULL,
	[Endpoint] [nvarchar](200) NOT NULL,
	[Topic] [nvarchar](200) NOT NULL
) ON [PRIMARY]