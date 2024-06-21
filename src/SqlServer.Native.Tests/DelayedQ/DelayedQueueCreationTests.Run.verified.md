## Tables

### DelayedQueueCreationTests

```sql
CREATE TABLE [dbo].[DelayedQueueCreationTests](
	[Headers] [nvarchar](max) NOT NULL,
	[BodyString]  AS (CONVERT([varchar](max),[Body])),
	[Body] [varbinary](max) NULL,
	[Due] [datetime] NOT NULL,
	[RowVersion] [bigint] IDENTITY(1,1) NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

CREATE NONCLUSTERED INDEX [Index_Due] ON [dbo].[DelayedQueueCreationTests]
(
	[Due] ASC
) ON [PRIMARY]
```