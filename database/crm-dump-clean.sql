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

ALTER TABLE IF EXISTS ONLY public.tickets DROP CONSTRAINT IF EXISTS tickets_users_email_fk;
ALTER TABLE IF EXISTS ONLY public.tickets DROP CONSTRAINT IF EXISTS tickets_companies_company_id_fk;
ALTER TABLE IF EXISTS ONLY public.messages DROP CONSTRAINT IF EXISTS messages_users_email_fk;
ALTER TABLE IF EXISTS ONLY public.messages DROP CONSTRAINT IF EXISTS messages_tickets_ticket_id_fk;
ALTER TABLE IF EXISTS ONLY public.login_credentials DROP CONSTRAINT IF EXISTS login_credentials_users_email_fk;
ALTER TABLE IF EXISTS ONLY public.forms DROP CONSTRAINT IF EXISTS forms_companies_company_id_fk;
ALTER TABLE IF EXISTS ONLY public.categories_x_users DROP CONSTRAINT IF EXISTS categories_x_users_users_email_fk;
ALTER TABLE IF EXISTS ONLY public.categories_x_users DROP CONSTRAINT IF EXISTS categories_x_users_categories_category_fk;
ALTER TABLE IF EXISTS ONLY public.users DROP CONSTRAINT IF EXISTS users_pk_2;
ALTER TABLE IF EXISTS ONLY public.users DROP CONSTRAINT IF EXISTS users_pk;
ALTER TABLE IF EXISTS ONLY public.tickets DROP CONSTRAINT IF EXISTS tickets_pk;
ALTER TABLE IF EXISTS ONLY public.messages DROP CONSTRAINT IF EXISTS messages_pk;
ALTER TABLE IF EXISTS ONLY public.login_credentials DROP CONSTRAINT IF EXISTS login_credentials_pk;
ALTER TABLE IF EXISTS ONLY public.forms DROP CONSTRAINT IF EXISTS forms_pk;
ALTER TABLE IF EXISTS ONLY public.companies DROP CONSTRAINT IF EXISTS companies_pk;
ALTER TABLE IF EXISTS ONLY public.categories_x_users DROP CONSTRAINT IF EXISTS categories_x_users_pk;
ALTER TABLE IF EXISTS ONLY public.categories DROP CONSTRAINT IF EXISTS categories_pk;
DROP TABLE IF EXISTS public.users;
DROP TABLE IF EXISTS public.tickets;
DROP TABLE IF EXISTS public.messages;
DROP TABLE IF EXISTS public.login_credentials;
DROP TABLE IF EXISTS public.forms;
DROP TABLE IF EXISTS public.companies;
DROP TABLE IF EXISTS public.categories_x_users;
DROP TABLE IF EXISTS public.categories;
SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: categories; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.categories (
    category text NOT NULL
);


ALTER TABLE public.categories OWNER TO postgres;

--
-- Name: categories_x_users; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.categories_x_users (
    user_fk text NOT NULL,
    category_fk text NOT NULL
);


ALTER TABLE public.categories_x_users OWNER TO postgres;

--
-- Name: companies; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.companies (
    company_id integer NOT NULL,
    name text NOT NULL
);


ALTER TABLE public.companies OWNER TO postgres;

--
-- Name: companies_company_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.companies ALTER COLUMN company_id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.companies_company_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: forms; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.forms (
    form_id integer NOT NULL,
    company_fk integer NOT NULL,
    form_json json NOT NULL
);


ALTER TABLE public.forms OWNER TO postgres;

--
-- Name: forms_form_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.forms ALTER COLUMN form_id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.forms_form_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: login_credentials; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.login_credentials (
    email text NOT NULL,
    password text NOT NULL
);


ALTER TABLE public.login_credentials OWNER TO postgres;

--
-- Name: messages; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.messages (
    message_id integer NOT NULL,
    message text NOT NULL,
    ticket_id_fk integer NOT NULL,
    title text NOT NULL,
    user_fk text NOT NULL
);


ALTER TABLE public.messages OWNER TO postgres;

--
-- Name: messages_message_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.messages ALTER COLUMN message_id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.messages_message_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: tickets; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.tickets (
    ticket_id integer NOT NULL,
    category text NOT NULL,
    subcategory text NOT NULL,
    title text NOT NULL,
    time_posted timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    time_closed timestamp without time zone,
    user_fk text NOT NULL,
    company_fk integer NOT NULL
);


ALTER TABLE public.tickets OWNER TO postgres;

--
-- Name: tickets_ticket_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.tickets ALTER COLUMN ticket_id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.tickets_ticket_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: users; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.users (
    email text NOT NULL,
    company_fk integer NOT NULL,
    verified boolean DEFAULT false NOT NULL,
    role text NOT NULL
);


ALTER TABLE public.users OWNER TO postgres;

--
-- Data for Name: categories; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.categories (category) FROM stdin;
\.


--
-- Data for Name: categories_x_users; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.categories_x_users (user_fk, category_fk) FROM stdin;
\.


--
-- Data for Name: companies; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.companies (company_id, name) FROM stdin;
1	test company
\.


--
-- Data for Name: forms; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.forms (form_id, company_fk, form_json) FROM stdin;
\.


--
-- Data for Name: login_credentials; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.login_credentials (email, password) FROM stdin;
test1@mail.com	test1
test2@mail.com	test2
test3@mail.com	test3
\.


--
-- Data for Name: messages; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.messages (message_id, message, ticket_id_fk, title, user_fk) FROM stdin;
\.


--
-- Data for Name: tickets; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.tickets (ticket_id, category, subcategory, title, time_posted, time_closed, user_fk, company_fk) FROM stdin;
3	testing	test	test1	2025-02-13 17:35:02.314029	\N	test1@mail.com	1
4	testing	test	test2	2025-02-13 17:35:02.314029	\N	test1@mail.com	1
\.


--
-- Data for Name: users; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.users (email, company_fk, verified, role) FROM stdin;
test1@mail.com	1	f	customer
test2@mail.com	1	f	admin
test3@mail.com	1	f	customerService
\.


--
-- Name: companies_company_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.companies_company_id_seq', 2, true);


--
-- Name: forms_form_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.forms_form_id_seq', 1, false);


--
-- Name: messages_message_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.messages_message_id_seq', 1, false);


--
-- Name: tickets_ticket_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.tickets_ticket_id_seq', 4, true);


--
-- Name: categories categories_pk; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.categories
    ADD CONSTRAINT categories_pk PRIMARY KEY (category);


--
-- Name: categories_x_users categories_x_users_pk; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.categories_x_users
    ADD CONSTRAINT categories_x_users_pk PRIMARY KEY (user_fk, category_fk);


--
-- Name: companies companies_pk; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.companies
    ADD CONSTRAINT companies_pk PRIMARY KEY (company_id);


--
-- Name: forms forms_pk; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.forms
    ADD CONSTRAINT forms_pk PRIMARY KEY (form_id);


--
-- Name: login_credentials login_credentials_pk; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.login_credentials
    ADD CONSTRAINT login_credentials_pk PRIMARY KEY (email);


--
-- Name: messages messages_pk; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.messages
    ADD CONSTRAINT messages_pk PRIMARY KEY (message_id);


--
-- Name: tickets tickets_pk; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.tickets
    ADD CONSTRAINT tickets_pk PRIMARY KEY (ticket_id);


--
-- Name: users users_pk; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_pk PRIMARY KEY (email);


--
-- Name: users users_pk_2; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_pk_2 UNIQUE (email);


--
-- Name: categories_x_users categories_x_users_categories_category_fk; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.categories_x_users
    ADD CONSTRAINT categories_x_users_categories_category_fk FOREIGN KEY (category_fk) REFERENCES public.categories(category);


--
-- Name: categories_x_users categories_x_users_users_email_fk; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.categories_x_users
    ADD CONSTRAINT categories_x_users_users_email_fk FOREIGN KEY (user_fk) REFERENCES public.users(email);


--
-- Name: forms forms_companies_company_id_fk; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.forms
    ADD CONSTRAINT forms_companies_company_id_fk FOREIGN KEY (company_fk) REFERENCES public.companies(company_id);


--
-- Name: login_credentials login_credentials_users_email_fk; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.login_credentials
    ADD CONSTRAINT login_credentials_users_email_fk FOREIGN KEY (email) REFERENCES public.users(email);


--
-- Name: messages messages_tickets_ticket_id_fk; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.messages
    ADD CONSTRAINT messages_tickets_ticket_id_fk FOREIGN KEY (ticket_id_fk) REFERENCES public.tickets(ticket_id);


--
-- Name: messages messages_users_email_fk; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.messages
    ADD CONSTRAINT messages_users_email_fk FOREIGN KEY (user_fk) REFERENCES public.users(email);


--
-- Name: tickets tickets_companies_company_id_fk; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.tickets
    ADD CONSTRAINT tickets_companies_company_id_fk FOREIGN KEY (company_fk) REFERENCES public.companies(company_id);


--
-- Name: tickets tickets_users_email_fk; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.tickets
    ADD CONSTRAINT tickets_users_email_fk FOREIGN KEY (user_fk) REFERENCES public.users(email);


--
-- PostgreSQL database dump complete
--

