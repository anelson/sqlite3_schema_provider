PRAGMA auto_vacuum = 1;

/* create a simple table with no primary key, that exercises all
the data types */
drop table if exists simple_type_menagerie;
create table simple_type_menagerie (
	text_column text null,
	numeric_column numeric null,
	integer_column integer null,
	typeless_column none null
);

create index stm_txt on simple_type_menagerie(text_column);
create index stm_multi on simple_type_menagerie(numeric_column, integer_column, typeless_column);


/* a table with an integer primary key */
drop table if exists int_pkey;
create table int_pkey (
	id integer primary key,
	something_else text not null
);

create unique index ip_id on int_pkey(id);
create index idx_ip_se on int_pkey(something_else);

/* A table with a text primary key */
drop table if exists txt_pkey;
create table txt_pkey (
	name text primary key,
	something_else text not null,
	some_other_thing integer null
);

create index idx_tp_se on txt_pkey(something_else);

/* A table with a typeless primary key */
drop table if exists typeless_pkey;
create table typeless_pkey (
	name none primary key,
	something_else text not null,
	some_other_thing integer null
);

create index idx_tlp_se on typeless_pkey(something_else);

/* an auto-increment integer */
drop table if exists int_ai_pkey;
create table int_ai_pkey (
	id integer primary key autoincrement,
	something_else text null
);

/* a series of tables w/ primary and foreign key constraints */
drop table if exists customers;
create table customers (
	id integer primary key autoincrement,
	name text not null,
	address text null
);

drop table if exists orders;
create table orders (
	id integer primary key autoincrement,
	customer_id integer not null constraint fk_orders_customers references customers(id) ,
	order_date text not null,
	order_total numeric not null
);

create index idx_orders_customer_id on orders(customer_id);

drop table if exists order_items;
create table order_items (
	id integer primary key autoincrement,
	order_id integer not null constraint fk_order_items_orders references orders(id),
	description text not null,
	unit_price numeric not null,
	quantity integer not null,
	total_price numeric not null
);

create index idx_order_items_order_id on order_items(order_id);

/* Create views atop each of these tables */
drop view if exists v_simple_type_menagerie;
create view v_simple_type_menagerie as select * from simple_type_menagerie;
drop view if exists v_int_pkey;
create view v_int_pkey as select * from int_pkey;
drop view if exists v_txt_pkey;
create view v_txt_pkey as select * from txt_pkey;
drop view if exists v_typeless_pkey;
create view v_typeless_pkey as select * from typeless_pkey;
drop view if exists v_int_ai_pkey;
create view v_int_ai_pkey as select * from int_ai_pkey;
drop view if exists v_customers;
create view v_customers as select * from customers;
drop view if exists v_orders;
create view v_orders as select * from orders;
drop view if exists v_order_items;
create view v_order_items as select * from order_items;

