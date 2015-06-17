create table nohros_state (
  state_name varchar(8000) not null,
  [state] varchar(8000) not null
)

create unique nonclustered index IX_nohros_state
on nohros_state (
  state_name
) include (
  [state]
)