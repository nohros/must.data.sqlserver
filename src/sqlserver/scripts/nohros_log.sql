/* Checks if the current database version is greater
 * than the version of this patch. To use this in the
 * SSMS the SQLCMD mode should be enabled.
 */
:on error exit

declare @continue bit,
  @objectname varchar(120),
  @objectversion int

set @objectname = 'nohros_log' /* the name of the object related with the script */
set @objectversion = 1 /* the current object version */

exec @continue = nohros_updateversion @objectname=@objectname, @objectversion=@objectversion
if @continue != 1
begin /* version guard */
  raiserror(
    'The version of the database is greater than the version of this script. The batch will be stopped', 11, 1
  )
end /* version guard */

/* create and empty procedure with the name [@pbjectname].So, we
 * can use the [alter proc [@objectname] statement]. This simulates
 * the behavior of the ALTER OR REPLACE statement that exists in other
 * datbases products. */
exec nohros_createproc @name = @objectname
go

alter proc nohros_log (
  @loglevel varchar(15),
  @procname varchar(128),
  @logmessage varchar(8000)
)
as
declare @logleveid int

select @logleveid = loglevelid
from nohros_loglevel
where levelname = @loglevel

if @logleveid is null
begin
  set @logleveid = 0
end

insert into nohros_logging(loglevelid, procname, logmessage)
values(@logleveid, @procname, @logmessage)