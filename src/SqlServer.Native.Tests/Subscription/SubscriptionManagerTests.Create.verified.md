## Tables

### Subscription

```sql
CREATE TABLE [dbo].[Subscription](
	[QueueAddress] [nvarchar](200) NOT NULL,
	[Endpoint] [nvarchar](200) NOT NULL,
	[Topic] [nvarchar](200) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Endpoint] ASC,
	[Topic] ASC
) ON [PRIMARY]
) ON [PRIMARY]
```