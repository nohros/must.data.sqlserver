create proc nohros_updateversion (
  @objectname varchar(120),
  @objectversion int
)
as

declare @objectname_t varchar(120),
  @objectversion_t int,
  @objectisupdatable bit

/* this proc is pesimistic and by default
 * does deny any object update attempt */
set @objectisupdatable = 0

select @objectname_t = objectname
  ,@objectversion_t = objectversion
from nohros_objectversion
where objectname = @objectname

/* If the object does not exists the update should
 * be allowed */
if @objectname_t is null
begin
  insert into nohros_objectversion(objectname, objectversion)
  values(@objectname, @objectversion)

  set @objectisupdatable = 1
end
else
begin
  /* If the specified object version is greater than the current
   * object update the object version and return 1(true), allowing
   * the caller to update the object; otherwise, return 0(false),
   * denying the object update. */
  if @objectversion >= @objectversion_t
  begin
    update nohros_objectversion
    set objectversion = @objectversion
    where objectname = @objectname

    set @objectisupdatable = 1
  end
end

return @objectisupdatable