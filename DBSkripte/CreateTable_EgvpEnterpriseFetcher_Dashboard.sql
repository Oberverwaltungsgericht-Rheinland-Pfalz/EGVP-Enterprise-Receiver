/****** Object:  Table [dbo].[EgvpEnterpriseFetcher_Dashboard]    Script Date: 28.05.2018 11:11:45 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[EgvpEnterpriseFetcher_Dashboard](
	[Id] [int] NOT NULL,
	[Bestaetigt] [int] NULL,
 CONSTRAINT [PK_EgvpEnterpriseFetcher_Dashboard] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO