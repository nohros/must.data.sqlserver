create table nohros_loglevel (
  loglevelid int not null,
  levelname varchar(15) not null,
  [description] varchar(800) null
)

alter table nohros_loglevel
add constraint PK_nohros_loglevel
primary key (
  loglevelid
)

create table nohros_logging (
  loggingid int identity(1,1) not null,
  loglevelid int not null,
  procname varchar(128) not null,
  logmessage varchar(8000) not null,
  created datetime not null
)

alter table nohros_logging
add constraint PK_nohros_logging
primary key (
  loggingid
)

alter table nohros_logging
add constraint FK_nohros_loglevel_logging
foreign key (
  loglevelid
) references nohros_loglevel (
  loglevelid
)

alter table nohros_logging
add constraint DF_nohros_logging
default (
  getdate()
) for created

create nonclustered index IX_nohros_logging_loglevel
on nohros_logging (
  created, loglevelid
) include (
  logmessage
)
go

insert into nohros_loglevel(loglevelid, levelname) values(1, 'DEBUG')
insert into nohros_loglevel(loglevelid, levelname) values(2, 'ERROR')
insert into nohros_loglevel(loglevelid, levelname) values(3, 'INFO')
insert into nohros_loglevel(loglevelid, levelname) values(4, 'WARNING')
