PRINT N'Trusting [AhoCorasick.SqlClr]...';


GO
-- Since SQL Server 2017 'clr strict security' is on by default and rejects an unsigned assembly
-- even with PERMISSION_SET = SAFE. Dynamic SQL because sys.trusted_assemblies does not exist on
-- SQL Server 2016 and earlier, where nothing needs to be trusted.
IF OBJECT_ID('sys.sp_add_trusted_assembly') IS NOT NULL
    EXEC sp_executesql N'IF NOT EXISTS (SELECT 1 FROM sys.trusted_assemblies WHERE [hash] = {ASSEMBLY_SHA512})
    EXEC sp_add_trusted_assembly {ASSEMBLY_SHA512}, N''AhoCorasick.SqlClr'';';


GO
PRINT N'Creating [AhoCorasick.SqlClr]...';


GO
CREATE ASSEMBLY [AhoCorasick.SqlClr]
    AUTHORIZATION [dbo]
    FROM {ASSEMBLY_HEX};


GO
ALTER ASSEMBLY [AhoCorasick.SqlClr]
    DROP FILE ALL
    ADD FILE FROM {PDB_HEX} AS N'AhoCorasick.SqlClr.pdb';


GO
PRINT N'Creating [dbo].[ContainsWords]...';


GO
CREATE FUNCTION [dbo].[ContainsWords]
(@xml XML, @text NVARCHAR (MAX), @culture NVARCHAR (MAX))
RETURNS BIT
AS
 EXTERNAL NAME [AhoCorasick.SqlClr].[UserDefinedFunctions].[ContainsWords]


GO
PRINT N'Creating [dbo].[CreateAhoCorasick]...';


GO
CREATE FUNCTION [dbo].[CreateAhoCorasick]
(@xml XML, @culture NVARCHAR (MAX))
RETURNS NVARCHAR (MAX)
AS
 EXTERNAL NAME [AhoCorasick.SqlClr].[UserDefinedFunctions].[CreateAhoCorasick]


GO
PRINT N'Creating [dbo].[DeleteAhoCorasick]...';


GO
CREATE FUNCTION [dbo].[DeleteAhoCorasick]
(@obj NVARCHAR (MAX))
RETURNS BIT
AS
 EXTERNAL NAME [AhoCorasick.SqlClr].[UserDefinedFunctions].[DeleteAhoCorasick]


GO
PRINT N'Creating [dbo].[ClearAhoCorasick]...';


GO
CREATE FUNCTION [dbo].[ClearAhoCorasick]
( )
RETURNS BIT
AS
 EXTERNAL NAME [AhoCorasick.SqlClr].[UserDefinedFunctions].[ClearAhoCorasick]


GO
PRINT N'Creating [dbo].[ContainsWordsByObject]...';


GO
CREATE FUNCTION [dbo].[ContainsWordsByObject]
(@text NVARCHAR (MAX), @obj NVARCHAR (MAX))
RETURNS BIT
AS
 EXTERNAL NAME [AhoCorasick.SqlClr].[UserDefinedFunctions].[ContainsWordsByObject]


GO
PRINT N'Creating [dbo].[ContainsWordsBoundedByObject]...';


GO
CREATE FUNCTION [dbo].[ContainsWordsBoundedByObject]
(@text NVARCHAR (MAX), @obj NVARCHAR (MAX))
RETURNS BIT
AS
 EXTERNAL NAME [AhoCorasick.SqlClr].[UserDefinedFunctions].[ContainsWordsBoundedByObject]


GO
PRINT N'Creating [dbo].[ContainsWordsTable]...';


GO
CREATE FUNCTION [dbo].[ContainsWordsTable]
(@xml XML, @text NVARCHAR (MAX), @culture NVARCHAR (MAX))
RETURNS 
     TABLE (
        [Index] INT            NULL,
        [Word]  NVARCHAR (MAX) NULL)
AS
 EXTERNAL NAME [AhoCorasick.SqlClr].[UserDefinedFunctions].[ContainsWordsTable]


GO
PRINT N'Creating [dbo].[ContainsWordsTableByObject]...';


GO
CREATE FUNCTION [dbo].[ContainsWordsTableByObject]
(@text NVARCHAR (MAX), @obj NVARCHAR (MAX))
RETURNS 
     TABLE (
        [Index] INT            NULL,
        [Word]  NVARCHAR (MAX) NULL)
AS
 EXTERNAL NAME [AhoCorasick.SqlClr].[UserDefinedFunctions].[ContainsWordsTableByObject]


GO
PRINT N'Creating [dbo].[ContainsWordsBoundedTableByObject]...';


GO
CREATE FUNCTION [dbo].[ContainsWordsBoundedTableByObject]
(@text NVARCHAR (MAX), @obj NVARCHAR (MAX))
RETURNS 
     TABLE (
        [Index] INT            NULL,
        [Word]  NVARCHAR (MAX) NULL)
AS
 EXTERNAL NAME [AhoCorasick.SqlClr].[UserDefinedFunctions].[ContainsWordsBoundedTableByObject]


GO
PRINT N'Creating [dbo].[ListAhoCorasick]...';


GO
CREATE FUNCTION [dbo].[ListAhoCorasick]
( )
RETURNS 
     TABLE (
        [Hash] NVARCHAR (MAX) NULL)
AS
 EXTERNAL NAME [AhoCorasick.SqlClr].[UserDefinedFunctions].[ListAhoCorasick]


GO
PRINT N'Update complete.';


GO
