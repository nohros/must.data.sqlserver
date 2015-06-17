create table nohros_hilo_behavior (
  hilo_key varchar(8000),
  hilo_max_lo int
)

create unique nonclustered index IX_nohros_hilo_behavior_key
on nohros_hilo_behavior (
  hilo_key
) include (
  hilo_max_lo
)