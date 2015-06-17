create table nohros_hilo (
  hilo_key varchar(8000) not null,
  hilo_current_hi bigint not null
)

alter table nohros_hilo
add constraint CK_nohros_hilo
check (
  hilo_current_hi > 0
)

create unique nonclustered index IX_nohros_hilo_key
on nohros_hilo (
  hilo_key
) include (
  hilo_current_hi
)