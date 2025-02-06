--
-- PostgreSQL database dump
--

-- Dumped from database version 16.4
-- Dumped by pg_dump version 16.4

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

--
-- Name: crm; Type: SCHEMA; Schema: -; Owner: postgres
--

CREATE SCHEMA crm;


ALTER SCHEMA crm OWNER TO postgres;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: companies; Type: TABLE; Schema: crm; Owner: postgres
--

CREATE TABLE crm.companies (
    companies_id integer NOT NULL,
    name character varying,
    email character varying
);


ALTER TABLE crm.companies OWNER TO postgres;

--
-- Name: companies_companies_id_seq; Type: SEQUENCE; Schema: crm; Owner: postgres
--

ALTER TABLE crm.companies ALTER COLUMN companies_id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME crm.companies_companies_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: form; Type: TABLE; Schema: crm; Owner: postgres
--

CREATE TABLE crm.form (
    form_id integer NOT NULL,
    description character varying,
    company integer,
    content json
);


ALTER TABLE crm.form OWNER TO postgres;

--
-- Name: form_form_id_seq; Type: SEQUENCE; Schema: crm; Owner: postgres
--

ALTER TABLE crm.form ALTER COLUMN form_id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME crm.form_form_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: login; Type: TABLE; Schema: crm; Owner: postgres
--

CREATE TABLE crm.login (
    login_id integer NOT NULL,
    email character varying,
    password character varying NOT NULL,
    "user" integer
);


ALTER TABLE crm.login OWNER TO postgres;

--
-- Name: login_login_id_seq; Type: SEQUENCE; Schema: crm; Owner: postgres
--

ALTER TABLE crm.login ALTER COLUMN login_id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME crm.login_login_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: messages; Type: TABLE; Schema: crm; Owner: postgres
--

CREATE TABLE crm.messages (
    message_id integer NOT NULL,
    message character varying,
    ticket_id integer
);


ALTER TABLE crm.messages OWNER TO postgres;

--
-- Name: messages_message_id_seq; Type: SEQUENCE; Schema: crm; Owner: postgres
--

ALTER TABLE crm.messages ALTER COLUMN message_id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME crm.messages_message_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: tickets; Type: TABLE; Schema: crm; Owner: postgres
--

CREATE TABLE crm.tickets (
    ticket_id integer NOT NULL,
    category character varying,
    about character varying,
    title character varying,
    time_posted timestamp without time zone,
    "user" integer,
    email character varying,
    company integer,
    time_closed timestamp without time zone
);


ALTER TABLE crm.tickets OWNER TO postgres;

--
-- Name: tickets_ticket_id_seq; Type: SEQUENCE; Schema: crm; Owner: postgres
--

ALTER TABLE crm.tickets ALTER COLUMN ticket_id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME crm.tickets_ticket_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: users; Type: TABLE; Schema: crm; Owner: postgres
--

CREATE TABLE crm.users (
    users_id integer NOT NULL,
    roles character varying,
    company_id integer,
    email character varying,
    verified boolean
);


ALTER TABLE crm.users OWNER TO postgres;

--
-- Name: users_users_id_seq; Type: SEQUENCE; Schema: crm; Owner: postgres
--

ALTER TABLE crm.users ALTER COLUMN users_id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME crm.users_users_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Data for Name: companies; Type: TABLE DATA; Schema: crm; Owner: postgres
--

COPY crm.companies (companies_id, name, email) FROM stdin;
\.


--
-- Data for Name: form; Type: TABLE DATA; Schema: crm; Owner: postgres
--

COPY crm.form (form_id, description, company, content) FROM stdin;
\.


--
-- Data for Name: login; Type: TABLE DATA; Schema: crm; Owner: postgres
--

COPY crm.login (login_id, email, password, "user") FROM stdin;
\.


--
-- Data for Name: messages; Type: TABLE DATA; Schema: crm; Owner: postgres
--

COPY crm.messages (message_id, message, ticket_id) FROM stdin;
\.


--
-- Data for Name: tickets; Type: TABLE DATA; Schema: crm; Owner: postgres
--

COPY crm.tickets (ticket_id, category, about, title, time_posted, "user", email, company, time_closed) FROM stdin;
\.


--
-- Data for Name: users; Type: TABLE DATA; Schema: crm; Owner: postgres
--

COPY crm.users (users_id, roles, company_id, email, verified) FROM stdin;
\.


--
-- Name: companies_companies_id_seq; Type: SEQUENCE SET; Schema: crm; Owner: postgres
--

SELECT pg_catalog.setval('crm.companies_companies_id_seq', 1, false);


--
-- Name: form_form_id_seq; Type: SEQUENCE SET; Schema: crm; Owner: postgres
--

SELECT pg_catalog.setval('crm.form_form_id_seq', 1, false);


--
-- Name: login_login_id_seq; Type: SEQUENCE SET; Schema: crm; Owner: postgres
--

SELECT pg_catalog.setval('crm.login_login_id_seq', 1, false);


--
-- Name: messages_message_id_seq; Type: SEQUENCE SET; Schema: crm; Owner: postgres
--

SELECT pg_catalog.setval('crm.messages_message_id_seq', 1, false);


--
-- Name: tickets_ticket_id_seq; Type: SEQUENCE SET; Schema: crm; Owner: postgres
--

SELECT pg_catalog.setval('crm.tickets_ticket_id_seq', 1, false);


--
-- Name: users_users_id_seq; Type: SEQUENCE SET; Schema: crm; Owner: postgres
--

SELECT pg_catalog.setval('crm.users_users_id_seq', 1, false);


--
-- Name: companies companies_pk; Type: CONSTRAINT; Schema: crm; Owner: postgres
--

ALTER TABLE ONLY crm.companies
    ADD CONSTRAINT companies_pk PRIMARY KEY (companies_id);


--
-- Name: form form_pk; Type: CONSTRAINT; Schema: crm; Owner: postgres
--

ALTER TABLE ONLY crm.form
    ADD CONSTRAINT form_pk PRIMARY KEY (form_id);


--
-- Name: login login_pk; Type: CONSTRAINT; Schema: crm; Owner: postgres
--

ALTER TABLE ONLY crm.login
    ADD CONSTRAINT login_pk PRIMARY KEY (login_id);


--
-- Name: messages messages_pk; Type: CONSTRAINT; Schema: crm; Owner: postgres
--

ALTER TABLE ONLY crm.messages
    ADD CONSTRAINT messages_pk PRIMARY KEY (message_id);


--
-- Name: tickets tickets_pk; Type: CONSTRAINT; Schema: crm; Owner: postgres
--

ALTER TABLE ONLY crm.tickets
    ADD CONSTRAINT tickets_pk PRIMARY KEY (ticket_id);


--
-- Name: users users_pk; Type: CONSTRAINT; Schema: crm; Owner: postgres
--

ALTER TABLE ONLY crm.users
    ADD CONSTRAINT users_pk PRIMARY KEY (users_id);


--
-- Name: form form_companies_companies_id_fk; Type: FK CONSTRAINT; Schema: crm; Owner: postgres
--

ALTER TABLE ONLY crm.form
    ADD CONSTRAINT form_companies_companies_id_fk FOREIGN KEY (company) REFERENCES crm.companies(companies_id);


--
-- Name: login login_users_users_id_fk; Type: FK CONSTRAINT; Schema: crm; Owner: postgres
--

ALTER TABLE ONLY crm.login
    ADD CONSTRAINT login_users_users_id_fk FOREIGN KEY ("user") REFERENCES crm.users(users_id);


--
-- Name: messages messages_tickets_ticket_id_fk; Type: FK CONSTRAINT; Schema: crm; Owner: postgres
--

ALTER TABLE ONLY crm.messages
    ADD CONSTRAINT messages_tickets_ticket_id_fk FOREIGN KEY (ticket_id) REFERENCES crm.tickets(ticket_id);


--
-- Name: tickets tickets_companies_companies_id_fk; Type: FK CONSTRAINT; Schema: crm; Owner: postgres
--

ALTER TABLE ONLY crm.tickets
    ADD CONSTRAINT tickets_companies_companies_id_fk FOREIGN KEY (company) REFERENCES crm.companies(companies_id);


--
-- Name: tickets tickets_users_users_id_fk; Type: FK CONSTRAINT; Schema: crm; Owner: postgres
--

ALTER TABLE ONLY crm.tickets
    ADD CONSTRAINT tickets_users_users_id_fk FOREIGN KEY ("user") REFERENCES crm.users(users_id);


--
-- Name: users users_companies_companies_id_fk; Type: FK CONSTRAINT; Schema: crm; Owner: postgres
--

ALTER TABLE ONLY crm.users
    ADD CONSTRAINT users_companies_companies_id_fk FOREIGN KEY (company_id) REFERENCES crm.companies(companies_id);


--
-- PostgreSQL database dump complete
--

