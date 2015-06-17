/* Checks if the current database version is greater
 * than the version of this patch.
 */
:on error exit

declare @continue bit,
  @objectname varchar(120),
  @objectversion int

set @objectname = 'nohros_hilo_get_next_hi' /* the name of the object related with the script */
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

/**
 * Copyright (c) 2011 by Nohros Inc, All rights reserved.
 */
alter proc nohros_hilo_get_next_hi (
  @key varchar(8000)
)
as

declare @hilo_current_hi bigint,
  @hilo_max_lo int

select @hilo_current_hi = hilo_current_hi
from nohros_hilo with(updlock)
where hilo_key = @key

select @hilo_max_lo = hilo_max_lo
from nohros_hilo_behavior
where hilo_key = @key
  or hilo_key = '*'

if (@hilo_max_lo is null)
begin
  set @hilo_max_lo = 100
end

if (@hilo_current_hi is null)
begin
  insert into nohros_hilo(hilo_key, hilo_current_hi)
  values(@key, 1)
  
  select cast(1 as bigint) as hilo_current_hi, @hilo_max_lo as hilo_max_lo
end
else
begin
  update nohros_hilo
  set hilo_current_hi = hilo_current_hi+@hilo_max_lo+1
  output inserted.hilo_current_hi
    ,@hilo_max_lo as hilo_max_lo
  where hilo_key = @key
end