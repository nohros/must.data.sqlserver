/* Checks if the current database version is greater
 * than the version of this patch.
 */
:on error exit

declare @continue bit,
  @objectname varchar(120),
  @objectversion int

set @objectname = 'nohros_createproc' /* the name of the object related with the script */
set @objectversion = 1 /* the current object version */

exec @continue = nohros_updateversion @objectname=@objectname, @objectversion=@objectversion
if @continue != 1
begin /* version guard */
  raiserror(
    'The version of the database is greater than the version of this script. The batch will be stopped', 11, 1
  )
end /* version guard */

if exists(
  select [name]
  from sys.objects
  where [name] = @objectname
    and [type] = 'P'
)
  exec('drop proc ' + @objectname)
go

create proc nohros_createproc (
  @name varchar(300)
)
as
if not exists(
  select [name]
  from sys.objects
  where [name] = @name
    and [type] = 'P'
)
exec('create proc ' + @name + ' as')