 

namespace Vit.Db.DbMng.MsSql
{
 
    public partial class MsSqlDbMng 
    {

        public static string DataBaseStructBuilder = @"

-------------------
--生成建库语句
--1.生成 表字段、字段备注、默认值约束 、unique约束、primary key约束、索引的创建语句
--2.生成触发器、函数、存储过程、视图的创建语句
-- by lith on 2021-03-22 v2.5
------------------- 


-- ------------------------------------------------------------------------------------
-- 1.构建表头  备份时间、数据库版本、数据库名称 

select '
-- (x.1)备份信息
/* ';

select '
  备份时间      ：',CONVERT(varchar(100), GETDATE(), 120);

select '
  SqlServer版本 ：',CONVERT(varchar(1000),@@version);

--select '
-- 数据库名称   ：',(Select Name From Master..SysDataBases Where DbId=(Select Dbid From Master..SysProcesses Where Spid = @@spid)) as dbName;

select '
*/
';

-- ------------------------------------------------------------------------------------
-- 2.生成 表字段、字段备注、默认值约束 、unique约束、primary key约束、索引的创建语句

select ('


/* (x.2)表 */
') comment;

--(x.1)创建表用来存储数据库表的结构

create table #Proc_S_TableStruct_ColInfo([col_id] int,[col_name] varchar(200),[col_typename] varchar(200),[col_len] int,[col_prec] varchar(200),[col_scale] varchar(200),[col_identity] int,[col_seed] int,[col_increment] int,[collation] varchar(200),[col_null] int,[col_DefaultValue] varchar(2000),[ConstraintName_DefaultValue]  varchar(200),[ExtendedProperty] varchar(4000),[ConstraintName_PrimaryKey] varchar(200),[ConstraintName_Unique] varchar(200))
 
create table #Proc_S_TableStruct_SqlCreateTb([id] int identity(1,1),sql varchar(700));

select  [Name] into #Proc_S_TableStruct_tbName from sysobjects where [type] = 'U'  and [Name]!='dtproperties';


 
--(x.2)获取所有的索引
SELECT
SCHEMA_NAME(t.schema_id) AS [架构名称],
t.name AS [数据表名称],
i.name AS [索引名称],
i.type_desc as [索引类型],
i.is_primary_key as [是否主键],
i.is_unique as [是否唯一],
i.is_unique_constraint as [是否外键],
STUFF(REPLACE(REPLACE((
SELECT QUOTENAME(c.name) + CASE WHEN ic.is_descending_key = 1 THEN ' DESC' ELSE '' END AS [data()] 
FROM sys.index_columns AS ic
INNER JOIN sys.columns AS c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
WHERE ic.object_id = i.object_id AND ic.index_id = i.index_id AND ic.is_included_column = 0
ORDER BY ic.key_ordinal
FOR XML PATH
), '<row>', ', '), '</row>', ''), 1, 2, '') AS [索引键列表],
STUFF(REPLACE(REPLACE((
SELECT QUOTENAME(c.name) AS [data()]
FROM sys.index_columns AS ic
INNER JOIN sys.columns AS c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
WHERE ic.object_id = i.object_id AND ic.index_id = i.index_id AND ic.is_included_column = 1
ORDER BY ic.index_column_id
FOR XML PATH
), '<row>', ', '), '</row>', ''), 1, 2, '') AS [包含列信息]
 
-- ,u.user_seeks
-- ,u.user_scans
-- ,u.user_lookups
-- ,u.user_updates 
into #Proc_S_TableStruct_Index
FROM sys.tables AS t
INNER JOIN sys.indexes AS i ON t.object_id = i.object_id
-- LEFT JOIN sys.dm_db_index_usage_stats AS u ON i.object_id = u.object_id AND i.index_id = u.index_id and u.database_id= db_id()
WHERE t.is_ms_shipped = 0
AND i.type <> 0;



--(x.3)循环处理各个表
declare @tbName varchar(100); 
declare @tbCount int; 
declare @tbIndex int; 

set @tbCount=(select count(*) from #Proc_S_TableStruct_tbName);
set @tbIndex=0;

while 1=1
begin

    --(x.x.1)
	set @tbIndex=@tbIndex+1;


    --(x.x.2)获取表信息
	set @tbName=( SELECT top 1  [Name] from #Proc_S_TableStruct_tbName)
	if @tbName is null 
		break;	 
	delete  #Proc_S_TableStruct_tbName  where [Name]=@tbName;


	--(x.x.2.1) 获取字段基础信息 
	SELECT 
	  C.column_id  [col_id],
	  C.[name] [col_name],
	  T.[name] col_typename, 
	  COLUMNPROPERTY(C.[object_id],C.[name],'PRECISION')   col_len, 
	  C.[precision] col_prec,
	  COLUMNPROPERTY(C.[object_id],C.[name],'Scale')  col_scale, 
	  C.is_identity col_identity, 
	  IDENT_SEED (@tbName)  col_seed,
	  IDENT_INCR (@tbName)  col_increment,
	  C.collation_name collation,
	  C.is_nullable col_null,
	  D.[definition] as col_DefaultValue,
	  D.[name] [ConstraintName_DefaultValue]  
		into #Proc_S_TableStruct_Col
	FROM sys.columns C
	INNER JOIN sys.types T ON C.user_type_id=T.user_type_id
	LEFT JOIN sys.default_constraints D ON C.[object_id]=D.parent_object_id AND C.column_id=D.parent_column_id AND C.default_object_id=D.[object_id]  
	WHERE C.[object_id]=(select top 1 [object_id] from  sys.objects    where [name]=@tbName and [type]='U' AND [is_ms_shipped]=0)
	ORDER BY  C.column_id
 


	--（x.x.2.2）  获取字段的备注
	select objname [col_name],[value] [ExtendedProperty] 
	into #Proc_S_TableStruct_Property 
	from ::fn_listextendedproperty(null,N'user',N'dbo',N'table',@tbName,N'column',null) 
	where 1=1;


	--（x.x.2.3）主码 和 唯一 约束 。 CONSTRAINT_TYPE：  'PRIMARY KEY' 和 'UNIQUE'
	select  t1.COLUMN_NAME [col_name],t2.CONSTRAINT_TYPE ,t1.Constraint_Name
	into #Proc_S_TableStruct_Constraint
	from information_schema.key_column_usage t1 
	left join information_schema.table_constraints t2 on t1.Constraint_Name=t2.Constraint_Name 
	where t1.TABLE_NAME=@tbName;


	--（x.x.2.4） 合并最终结构数据
	insert into #Proc_S_TableStruct_ColInfo
	select c.*
	,convert(varchar(8000) , p.[ExtendedProperty])
	,conPrimary.Constraint_Name [ConstraintName_PrimaryKey]
	,conUnique.Constraint_Name [ConstraintName_Unique]
	from  #Proc_S_TableStruct_Col c
	left join #Proc_S_TableStruct_Property p on c.[col_name] Collate Database_Default =p.[col_name]
	left join #Proc_S_TableStruct_Constraint conPrimary on c.[col_name]=conPrimary.[col_name] and conPrimary.[CONSTRAINT_TYPE]='PRIMARY KEY'
	left join #Proc_S_TableStruct_Constraint conUnique on c.[col_name]=conUnique.[col_name]  and conUnique.[CONSTRAINT_TYPE]='UNIQUE';

 

	--(x.x.2.5)清理数据
	drop  table #Proc_S_TableStruct_Col;
	drop  table #Proc_S_TableStruct_Property;
	drop  table #Proc_S_TableStruct_Constraint;



	--(x.x.3)输出
	select ('




/* ['+  CONVERT(varchar(10),@tbIndex)+'/'+ CONVERT(varchar(10),@tbCount) +']创建表 '+@tbName+' */
') comment;



	--(x.x.x.1)建表
	select ('
/* 创建表字段 */') comment;

	insert into #Proc_S_TableStruct_SqlCreateTb(sql)
	select '
create table [dbo].['+@tbName+'] ( ';


	insert into #Proc_S_TableStruct_SqlCreateTb(sql) 
	select 

	-- [col_name] [类型]
	' ['+[col_name]+'] ['+[col_typename]+']'

	-- (长度)
	+(
	  -- char
	  case when(0!=charindex('char',col_typename)) then (case when [col_len]<=0 then '(MAX)' else ' ('+convert(varchar(100),[col_len])+')' end)
	  -- decimal
	  when [col_typename]='decimal' OR [col_typename]='numeric'  then ' ('+col_prec+','+col_scale+')'
	  else '' end
	 )  

	-- IDENTITY(2010,100)
	+(case when(1=col_identity) then ' IDENTITY('+convert(varchar(100),[col_seed]) +','+ convert(varchar(100),[col_increment]) +')' else '' end)  

	-- COLLATE Chinese_PRC_CI
	+(case when(collation is not null) then ' COLLATE '+[collation] else '' end)  

	-- NOT NULL  
	+(case when(1=col_null) then ' NULL' else ' NOT NULL' end)  
	+'
	,'
	from #Proc_S_TableStruct_ColInfo;


	update #Proc_S_TableStruct_SqlCreateTb set sql=(isnull(left(sql,len(sql)-1),'')+'); ') where [ID]= (select max([ID]) from #Proc_S_TableStruct_SqlCreateTb);


	select sql from #Proc_S_TableStruct_SqlCreateTb;
	truncate table #Proc_S_TableStruct_SqlCreateTb;    



 


	--(x.x.x.2)默认值约束
	select ('

/* 默认值约束 */') comment;
	select ('
alter table '+ quotename(@tbName)
	+' add constraint '+quotename(ConstraintName_DefaultValue)
	+' default'+col_DefaultValue
	+' for ' + quotename([col_name])
	+';') [sql]
	from #Proc_S_TableStruct_ColInfo
	where  ConstraintName_DefaultValue is not null;

 
 
	--(x.x.x.3)unique约束              ALTER TABLE table_a ADD unique(aID);
	select ('

/* unique约束 */') comment;
	select ('
alter table '+ quotename(@tbName)
	+' add unique( '+quotename([col_name])+')'
	+';') [sql]
	from #Proc_S_TableStruct_ColInfo
	where  ConstraintName_Unique is not null


 
	--(x.x.x.4)primary key约束
	--'ALTER TABLE ['+@tbName+'] ADD CONSTRAINT PK__'+@tbName+'_'+@colName+'__lit17032317 PRIMARY KEY CLUSTERED (['+@colName+']) '
	select ('

/* primary key约束 */') comment;
	select('
alter table '+ quotename(@tbName)
	+' add constraint '+quotename(ConstraintName_PrimaryKey)
	+' PRIMARY KEY CLUSTERED ('+quotename([col_name])+')'
	+';') [sql]
	from #Proc_S_TableStruct_ColInfo
	where  ConstraintName_PrimaryKey is not null;
	


    --(x.x.x.5)索引
	-- CREATE NONCLUSTERED INDEX IX_Tileset_File ON dbo.Tileset_File
	-- (professionalTreeId,	creator DESC,floorId)
	-- WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	select ('

/* 索引 */') comment;
	select('
CREATE NONCLUSTERED INDEX '+ quotename([索引名称])
	+' ON '+quotename([架构名称])+'.'+quotename([数据表名称])
	+'
('+[索引键列表]+')'
	+'
WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY];
') [sql]
	from #Proc_S_TableStruct_Index
    where [索引类型]='NONCLUSTERED' and [数据表名称]=@tbName;
	


	truncate table #Proc_S_TableStruct_ColInfo;


end


select '
GO
';

drop table #Proc_S_TableStruct_Index;
drop table #Proc_S_TableStruct_tbName;
drop table #Proc_S_TableStruct_ColInfo;
drop table #Proc_S_TableStruct_SqlCreateTb;

 



-- ------------------------------------------------------------------------------------
-- 3.生成触发器、函数、存储过程、视图的创建语句




--(x.1)获取数据的语句

declare @IDStart int;
declare @IDNext int;
--定义text 指针   
declare @ptrval BINARY(16)
declare @sqlNext varchar(8000)


SELECT  Identity(int,1,1) [ID],
    o.xtype,
(CASE o.xtype WHEN 'X' THEN '扩展存储过程' WHEN 'TR' THEN '触发器' WHEN 'PK' THEN '主键' WHEN 'F' THEN '外键' WHEN 'C' THEN '约束' WHEN 'V' THEN '视图' WHEN 'FN' THEN '函数-标量' WHEN 'IF' THEN '函数-内嵌' WHEN 'TF' THEN '函数-表值' ELSE '存储过程' END)
 AS [类型]
, o.name AS [对象名]
, o.crdate AS [创建时间]
, o.refdate AS [更改时间]
,convert(text,c.[text]) AS [声明语句]
into #tb
FROM dbo.sysobjects o LEFT OUTER JOIN
dbo.syscomments c ON o.id = c.id
WHERE (o.xtype IN ('X', 'TR', 'C', 'V', 'F', 'IF', 'TF', 'FN', 'P', 'PK')) AND
(OBJECTPROPERTY(o.id, N'IsMSShipped') = 0)
--order BY  o.xtype



while(1=1)
begin
set @IDStart=null;
 	select top 1 @IDStart=start.[ID], @IDNext=nex.[ID], @ptrval=TEXTPTR(start.[声明语句]),@sqlNext=convert(varchar(8000),nex.[声明语句])        
	from #tb start,#tb nex where start.[ID]<nex.[ID] and  start.[xtype]=nex.[xtype] and  start.[对象名]=nex.[对象名];

 	if( @IDStart is null) break;

	UPDATETEXT #tb.[声明语句] @ptrval NULL 0 @sqlNext;
	delete #tb where [id]=@IDNext;

end



  
--(x.3)触发器
select ('


/* (x.3)触发器 */
') comment;

select [声明语句],'
GO
'  from #tb   where xtype='TR';






--(x.4)函数
select ('


/* (x.4)函数 */
') comment;

select [声明语句],'
GO
'  from #tb   where xtype='FN';

select [声明语句] ,'
GO
' from #tb   where xtype='TF';






--(x.5)存储过程
select ('


/* (x.5)存储过程 */
') comment;

select [声明语句],'
GO
'   from #tb   where xtype='P';




--(x.6)视图 （考虑 依附关系）
select ('


/* (x.6)视图 */
') comment;

select identity(int,1,1) [id],[对象名] [name], convert(smallint,null) SortCode  into #tmp_Enty  from #tb   where xtype='V'; 

 
SELECT distinct  o.[name],  p.[name]  dependOn
into #tmp_R
FROM sysobjects o 
INNER JOIN sysdepends d     ON d.id = o.id   
INNER JOIN sysobjects p     ON d.depid = p.id  and p.xtype='v'  and exists(select 1 from #tmp_Enty where p.[name] = #tmp_Enty.[name] )
where  o.xtype='v' and exists(select 1 from #tmp_Enty where o.[name] = #tmp_Enty.[name] );

 
declare @sc int;
set @sc=1;

while 1=1
begin
	set @sc=@sc+1;
	update #tmp_Enty  set SortCode=@sc  from #tmp_Enty enty   where SortCode is null 
	and not exists(select 1 from #tmp_R r inner join  #tmp_Enty parent on  r.[dependOn]=parent.[name]  where r.[name]=enty.[name] and parent.SortCode is null  )	
	if(0=@@ROWCOUNT)
	 	break;
	
end
update #tmp_Enty   set SortCode=@sc+1  where SortCode is null;
 
select [声明语句],'
GO
'  from #tb inner join #tmp_Enty on  #tb.对象名=#tmp_Enty.[name] order by SortCode;

drop table #tmp_Enty;
drop table #tmp_R;
 

 


drop table #tb;






";

    }
}
