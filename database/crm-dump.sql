--
-- PostgreSQL database dump
--

-- Dumped from database version 16.8
-- Dumped by pg_dump version 16.8

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

ALTER TABLE IF EXISTS ONLY public.users DROP CONSTRAINT IF EXISTS users_companies_id_fk;
ALTER TABLE IF EXISTS ONLY public.tickets DROP CONSTRAINT IF EXISTS tickets_users_id_fk;
ALTER TABLE IF EXISTS ONLY public.tickets DROP CONSTRAINT IF EXISTS tickets_subcategories_id_fk;
ALTER TABLE IF EXISTS ONLY public.tickets DROP CONSTRAINT IF EXISTS tickets_companies_id_fk;
ALTER TABLE IF EXISTS ONLY public.subcategories DROP CONSTRAINT IF EXISTS subcategories_categories_id_fk;
ALTER TABLE IF EXISTS ONLY public.messages DROP CONSTRAINT IF EXISTS messages_users_id_fk;
ALTER TABLE IF EXISTS ONLY public.messages DROP CONSTRAINT IF EXISTS messages_tickets_id_fk;
ALTER TABLE IF EXISTS ONLY public.feedback DROP CONSTRAINT IF EXISTS feedback_users_id_fk_2;
ALTER TABLE IF EXISTS ONLY public.feedback DROP CONSTRAINT IF EXISTS feedback_users_id_fk;
ALTER TABLE IF EXISTS ONLY public.feedback DROP CONSTRAINT IF EXISTS feedback_tickets_id_fk;
ALTER TABLE IF EXISTS ONLY public.categories DROP CONSTRAINT IF EXISTS categories_companies_id_fk;
ALTER TABLE IF EXISTS ONLY public.users DROP CONSTRAINT IF EXISTS users_pk;
ALTER TABLE IF EXISTS ONLY public.tickets DROP CONSTRAINT IF EXISTS tickets_pk;
ALTER TABLE IF EXISTS ONLY public.ticket_access_links DROP CONSTRAINT IF EXISTS ticket_access_links_pk;
ALTER TABLE IF EXISTS ONLY public.subcategories DROP CONSTRAINT IF EXISTS subcategories_pk;
ALTER TABLE IF EXISTS ONLY public.messages DROP CONSTRAINT IF EXISTS messages_pk;
ALTER TABLE IF EXISTS ONLY public.feedback DROP CONSTRAINT IF EXISTS feedback_pk;
ALTER TABLE IF EXISTS ONLY public.companies DROP CONSTRAINT IF EXISTS companies_pk;
ALTER TABLE IF EXISTS ONLY public.categories DROP CONSTRAINT IF EXISTS categories_pk;
ALTER TABLE IF EXISTS ONLY public.assigned_categories DROP CONSTRAINT IF EXISTS assigned_categories_pk;
DROP TABLE IF EXISTS public.users;
DROP TABLE IF EXISTS public.tickets;
DROP TABLE IF EXISTS public.ticket_access_links;
DROP TABLE IF EXISTS public.subcategories;
DROP TABLE IF EXISTS public.messages;
DROP TABLE IF EXISTS public.feedback;
DROP TABLE IF EXISTS public.companies;
DROP TABLE IF EXISTS public.categories;
DROP TABLE IF EXISTS public.assigned_categories;
DROP TYPE IF EXISTS public.status;
DROP TYPE IF EXISTS public.role;
--
-- Name: role; Type: TYPE; Schema: public; Owner: postgres
--

CREATE TYPE public.role AS ENUM (
    'customer',
    'support',
    'admin',
    'superadmin'
);


ALTER TYPE public.role OWNER TO postgres;

--
-- Name: status; Type: TYPE; Schema: public; Owner: postgres
--

CREATE TYPE public.status AS ENUM (
    'pending',
    'ongoing',
    'answered',
    'closed'
);


ALTER TYPE public.status OWNER TO postgres;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: assigned_categories; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.assigned_categories (
    user_id integer NOT NULL,
    category_id integer NOT NULL
);


ALTER TABLE public.assigned_categories OWNER TO postgres;

--
-- Name: categories; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.categories (
    id integer NOT NULL,
    name text NOT NULL,
    company_id integer NOT NULL
);


ALTER TABLE public.categories OWNER TO postgres;

--
-- Name: categories_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.categories ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.categories_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: companies; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.companies (
    id integer NOT NULL,
    name text NOT NULL
);


ALTER TABLE public.companies OWNER TO postgres;

--
-- Name: companies_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.companies ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.companies_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: feedback; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.feedback (
    id integer NOT NULL,
    rating integer NOT NULL,
    comment text NOT NULL,
    written timestamp without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    "from" integer NOT NULL,
    target integer NOT NULL,
    ticket_id integer NOT NULL
);


ALTER TABLE public.feedback OWNER TO postgres;

--
-- Name: feedback_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.feedback ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.feedback_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: messages; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.messages (
    id integer NOT NULL,
    title text,
    message text NOT NULL,
    ticket_id integer NOT NULL,
    user_id integer NOT NULL
);


ALTER TABLE public.messages OWNER TO postgres;

--
-- Name: messages_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.messages ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.messages_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: subcategories; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.subcategories (
    id integer NOT NULL,
    name text NOT NULL,
    main_category_id integer NOT NULL
);


ALTER TABLE public.subcategories OWNER TO postgres;

--
-- Name: subcategories_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.subcategories ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.subcategories_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: ticket_access_links; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.ticket_access_links (
    id integer NOT NULL,
    access_link text NOT NULL
);


ALTER TABLE public.ticket_access_links OWNER TO postgres;

--
-- Name: ticket_access_links_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.ticket_access_links ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.ticket_access_links_id_seq
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
    id integer NOT NULL,
    title text NOT NULL,
    status public.status DEFAULT 'pending'::public.status NOT NULL,
    subcategory_id integer NOT NULL,
    posted timestamp without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    closed timestamp without time zone,
    user_id integer NOT NULL,
    company_id integer NOT NULL,
    elevated boolean DEFAULT false NOT NULL
);


ALTER TABLE public.tickets OWNER TO postgres;

--
-- Name: tickets_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.tickets ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.tickets_id_seq
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
    id integer NOT NULL,
    company_id integer NOT NULL,
    role public.role DEFAULT 'customer'::public.role NOT NULL,
    email text NOT NULL,
    verified boolean DEFAULT false NOT NULL,
    password text
);


ALTER TABLE public.users OWNER TO postgres;

--
-- Name: users_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.users ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.users_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Data for Name: assigned_categories; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.assigned_categories (user_id, category_id) FROM stdin;
5	1
6	4
\.


--
-- Data for Name: categories; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.categories (id, name, company_id) FROM stdin;
1	billing	1
2	assembly	1
3	billing	4
4	tech	4
\.


--
-- Data for Name: companies; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.companies (id, name) FROM stdin;
1	ikea
2	doofenshmirtz
3	ånga
4	micromjuk kant
\.


--
-- Data for Name: feedback; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.feedback (id, rating, comment, written, "from", target, ticket_id) FROM stdin;
\.


--
-- Data for Name: messages; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.messages (id, title, message, ticket_id, user_id) FROM stdin;
1	messagetitle1	text	1	8
2	messagetitle2	text	2	7
\.


--
-- Data for Name: subcategories; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.subcategories (id, name, main_category_id) FROM stdin;
1	refund	1
2	payment	1
3	instructions	2
4	delivery	2
5	refund	3
6	payment	3
7	account	4
8	software	4
\.


--
-- Data for Name: ticket_access_links; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.ticket_access_links (id, access_link) FROM stdin;
\.


--
-- Data for Name: tickets; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.tickets (id, title, status, subcategory_id, posted, closed, user_id, company_id, elevated) FROM stdin;
1	test1	pending	2	2025-03-03 10:46:41.665726	\N	8	1	f
2	test2	pending	8	2025-03-03 10:46:41.665726	\N	7	4	f
\.


--
-- Data for Name: users; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.users (id, company_id, role, email, verified, password) FROM stdin;
1	1	admin	ikeaadmin@gmail.com	f	123
2	2	admin	doofenshmirtzadmin@gmail.com	f	123
3	3	admin	angaadmin@gmail.com	f	123
4	4	admin	micromjukkantadmin@gmail.com	f	123
5	1	support	ikeasupport@gmail.com	f	123
6	4	support	micromjuksupport@gmail.com	f	123
7	4	customer	test@gmail.com	f	123
8	1	customer	test@gmail.com	f	123
\.


--
-- Name: categories_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.categories_id_seq', 4, true);


--
-- Name: companies_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.companies_id_seq', 4, true);


--
-- Name: feedback_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.feedback_id_seq', 1, false);


--
-- Name: messages_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.messages_id_seq', 2, true);


--
-- Name: subcategories_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.subcategories_id_seq', 8, true);


--
-- Name: ticket_access_links_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.ticket_access_links_id_seq', 1, false);


--
-- Name: tickets_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.tickets_id_seq', 2, true);


--
-- Name: users_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.users_id_seq', 8, true);


--
-- Name: assigned_categories assigned_categories_pk; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.assigned_categories
    ADD CONSTRAINT assigned_categories_pk PRIMARY KEY (user_id, category_id);


--
-- Name: categories categories_pk; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.categories
    ADD CONSTRAINT categories_pk PRIMARY KEY (id);


--
-- Name: companies companies_pk; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.companies
    ADD CONSTRAINT companies_pk PRIMARY KEY (id);


--
-- Name: feedback feedback_pk; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.feedback
    ADD CONSTRAINT feedback_pk PRIMARY KEY (id);


--
-- Name: messages messages_pk; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.messages
    ADD CONSTRAINT messages_pk PRIMARY KEY (id);


--
-- Name: subcategories subcategories_pk; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.subcategories
    ADD CONSTRAINT subcategories_pk PRIMARY KEY (id);


--
-- Name: ticket_access_links ticket_access_links_pk; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ticket_access_links
    ADD CONSTRAINT ticket_access_links_pk PRIMARY KEY (id);


--
-- Name: tickets tickets_pk; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.tickets
    ADD CONSTRAINT tickets_pk PRIMARY KEY (id);


--
-- Name: users users_pk; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_pk PRIMARY KEY (id);


--
-- Name: categories categories_companies_id_fk; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.categories
    ADD CONSTRAINT categories_companies_id_fk FOREIGN KEY (company_id) REFERENCES public.companies(id);


--
-- Name: feedback feedback_tickets_id_fk; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.feedback
    ADD CONSTRAINT feedback_tickets_id_fk FOREIGN KEY (ticket_id) REFERENCES public.tickets(id);


--
-- Name: feedback feedback_users_id_fk; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.feedback
    ADD CONSTRAINT feedback_users_id_fk FOREIGN KEY (target) REFERENCES public.users(id);


--
-- Name: feedback feedback_users_id_fk_2; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.feedback
    ADD CONSTRAINT feedback_users_id_fk_2 FOREIGN KEY ("from") REFERENCES public.users(id);


--
-- Name: messages messages_tickets_id_fk; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.messages
    ADD CONSTRAINT messages_tickets_id_fk FOREIGN KEY (ticket_id) REFERENCES public.tickets(id);


--
-- Name: messages messages_users_id_fk; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.messages
    ADD CONSTRAINT messages_users_id_fk FOREIGN KEY (user_id) REFERENCES public.users(id);


--
-- Name: subcategories subcategories_categories_id_fk; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.subcategories
    ADD CONSTRAINT subcategories_categories_id_fk FOREIGN KEY (main_category_id) REFERENCES public.categories(id);


--
-- Name: tickets tickets_companies_id_fk; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.tickets
    ADD CONSTRAINT tickets_companies_id_fk FOREIGN KEY (company_id) REFERENCES public.companies(id);


--
-- Name: tickets tickets_subcategories_id_fk; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.tickets
    ADD CONSTRAINT tickets_subcategories_id_fk FOREIGN KEY (subcategory_id) REFERENCES public.subcategories(id);


--
-- Name: tickets tickets_users_id_fk; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.tickets
    ADD CONSTRAINT tickets_users_id_fk FOREIGN KEY (user_id) REFERENCES public.users(id);


--
-- Name: users users_companies_id_fk; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_companies_id_fk FOREIGN KEY (company_id) REFERENCES public.companies(id);


--
-- PostgreSQL database dump complete
--

