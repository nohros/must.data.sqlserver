create table nohros_objectversion (
  objectname varchar(120) not null,
  objectversion int not null
)

alter table nohros_objectversion
add constraint PK_nohros_objectversion
primary key (
  objectname
)
go