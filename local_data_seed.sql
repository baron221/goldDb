--
-- PostgreSQL database dump
--

\restrict 8XWoE0YXwJfgzKXRC3ylAu6G7S3u74Tg1zeBiEzBsQEe5n0H9UAAUNc4D1ahP0O

-- Dumped from database version 16.14
-- Dumped by pg_dump version 16.14

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
-- Data for Name: __EFMigrationsHistory; Type: TABLE DATA; Schema: goldb; Owner: postgres
--

INSERT INTO goldb."__EFMigrationsHistory" VALUES ('20260707134521_InitialCreate', '8.0.0');
INSERT INTO goldb."__EFMigrationsHistory" VALUES ('20260707144535_AddManufacturerLogistics', '8.0.0');


--
-- Data for Name: articles; Type: TABLE DATA; Schema: goldb; Owner: postgres
--



--
-- Data for Name: companies; Type: TABLE DATA; Schema: goldb; Owner: postgres
--

INSERT INTO goldb.companies VALUES (1, 'Admin Company', 'Admin', 'Toshkent', '111', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, false, 'ADMIN', NULL, NULL, NULL, NULL, NULL, NULL, '2026-07-07 13:46:05.693415', NULL, false);
INSERT INTO goldb.companies VALUES (2, 'MFG Company', 'Mfg', 'Toshkent', '222', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, false, 'MFG', NULL, NULL, NULL, NULL, NULL, NULL, '2026-07-07 13:46:05.693415', NULL, false);
INSERT INTO goldb.companies VALUES (4, 'Market Company', 'Mkt', 'Toshkent', '444', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, false, 'DCC', NULL, NULL, NULL, NULL, NULL, NULL, '2026-07-07 13:46:05.693415', NULL, false);
INSERT INTO goldb.companies VALUES (3, 'RTL Company', 'Rtl', 'Toshkent', '333', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, false, 'RTL', 29, NULL, NULL, NULL, NULL, 1, '2026-07-07 13:46:05.693415', '2026-07-07 15:27:14.977301', false);
INSERT INTO goldb.companies VALUES (24, '테스트 소매점 1', '', '', '', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, false, 'RTL', 29, NULL, NULL, NULL, NULL, 1, '2026-07-07 20:25:25.606767', '2026-07-07 15:41:48.512352', false);
INSERT INTO goldb.companies VALUES (25, '테스트 소매점 2', '', '', '', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, false, 'RTL', NULL, NULL, NULL, NULL, NULL, NULL, '2026-07-07 20:25:25.606767', '2026-07-07 20:25:25.606767', false);
INSERT INTO goldb.companies VALUES (26, '테스트 소매점 3', '', '', '', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, false, 'RTL', NULL, NULL, NULL, NULL, NULL, NULL, '2026-07-07 20:25:25.606767', '2026-07-07 20:25:25.606767', false);
INSERT INTO goldb.companies VALUES (27, '테스트 물류센터 1', '', '', '', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, false, 'DCC', NULL, NULL, NULL, NULL, NULL, NULL, '2026-07-07 20:25:25.606767', '2026-07-07 20:25:25.606767', false);
INSERT INTO goldb.companies VALUES (28, '테스트 물류센터 2', '', '', '', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, false, 'DCC', NULL, NULL, NULL, NULL, NULL, NULL, '2026-07-07 20:25:25.606767', '2026-07-07 20:25:25.606767', false);
INSERT INTO goldb.companies VALUES (29, '테스트 물류센터 3', '', '', '', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, false, 'DCC', NULL, NULL, NULL, NULL, NULL, NULL, '2026-07-07 20:25:25.606767', '2026-07-07 20:25:25.606767', false);
INSERT INTO goldb.companies VALUES (32, '테스트 제조사 3', '', '', '', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, false, 'MFG', NULL, NULL, NULL, NULL, NULL, NULL, '2026-07-07 20:25:25.606767', '2026-07-07 20:25:25.606767', false);
INSERT INTO goldb.companies VALUES (30, 'kreis', '', '', '', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, false, 'MFG', NULL, NULL, NULL, NULL, NULL, NULL, '2026-07-07 20:25:25.606767', '2026-07-07 20:25:25.606767', false);
INSERT INTO goldb.companies VALUES (31, 'ko1111', '', '', '', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, false, 'MFG', NULL, NULL, NULL, NULL, NULL, NULL, '2026-07-07 20:25:25.606767', '2026-07-07 20:25:25.606767', false);


--
-- Data for Name: customers; Type: TABLE DATA; Schema: goldb; Owner: postgres
--



--
-- Data for Name: users; Type: TABLE DATA; Schema: goldb; Owner: postgres
--

INSERT INTO goldb.users VALUES (4, 'market', '$2a$11$4q9.7x5za6BTpYmd5lH/YuvmvLVkpKGIr4Cypm8gtXvIgU2SoHlAK', 'MARKET', NULL, NULL, NULL, NULL, '', NULL, '', false, false, false, 'MARKET', NULL, NULL, NULL, NULL, 0, NULL, NULL, '2026-07-07 13:46:05.834875', NULL, false, true);
INSERT INTO goldb.users VALUES (2, 'manufacturer', '$2a$11$4q9.7x5za6BTpYmd5lH/YuvmvLVkpKGIr4Cypm8gtXvIgU2SoHlAK', 'MANUFACTURER', NULL, NULL, NULL, NULL, '', NULL, '', false, false, false, 'MANUFACTURER', NULL, NULL, '::1', '2026-07-07 15:37:44.416377', 2, NULL, NULL, '2026-07-07 13:46:05.822259', '2026-07-07 15:37:44.416495', false, true);
INSERT INTO goldb.users VALUES (3, 'retail', '$2a$11$4q9.7x5za6BTpYmd5lH/YuvmvLVkpKGIr4Cypm8gtXvIgU2SoHlAK', 'RETAIL', NULL, NULL, NULL, NULL, '', NULL, '', false, false, false, 'RETAIL', NULL, NULL, '::1', '2026-07-07 15:36:06.240647', 4, NULL, NULL, '2026-07-07 13:46:05.831051', '2026-07-07 15:36:06.240759', false, true);
INSERT INTO goldb.users VALUES (34, 'dev_retail3', '.7x5za6BTpYmd5lH/YuvmvLVkpKGIr4Cypm8gtXvIgU2SoHlAK', '테스트 소매 3', NULL, NULL, NULL, NULL, 'avatar.png', NULL, '', false, false, false, 'user', NULL, NULL, NULL, NULL, 0, NULL, NULL, '2026-07-07 20:25:25.607458', '2026-07-07 20:25:25.607458', false, true);
INSERT INTO goldb.users VALUES (35, 'dev_logis1', '.7x5za6BTpYmd5lH/YuvmvLVkpKGIr4Cypm8gtXvIgU2SoHlAK', '테스트 물류 1', NULL, NULL, NULL, NULL, 'avatar.png', NULL, '', false, false, false, 'user', NULL, NULL, NULL, NULL, 0, NULL, NULL, '2026-07-07 20:25:25.607458', '2026-07-07 20:25:25.607458', false, true);
INSERT INTO goldb.users VALUES (36, 'dev_logis2', '.7x5za6BTpYmd5lH/YuvmvLVkpKGIr4Cypm8gtXvIgU2SoHlAK', '테스트 물류 2', NULL, NULL, NULL, NULL, 'avatar.png', NULL, '', false, false, false, 'user', NULL, NULL, NULL, NULL, 0, NULL, NULL, '2026-07-07 20:25:25.607458', '2026-07-07 20:25:25.607458', false, true);
INSERT INTO goldb.users VALUES (37, 'dev_logis3', '.7x5za6BTpYmd5lH/YuvmvLVkpKGIr4Cypm8gtXvIgU2SoHlAK', '테스트 물류 3', NULL, NULL, NULL, NULL, 'avatar.png', NULL, '', false, false, false, 'user', NULL, NULL, NULL, NULL, 0, NULL, NULL, '2026-07-07 20:25:25.607458', '2026-07-07 20:25:25.607458', false, true);
INSERT INTO goldb.users VALUES (1, 'admin', '$2a$11$4q9.7x5za6BTpYmd5lH/YuvmvLVkpKGIr4Cypm8gtXvIgU2SoHlAK', 'ADMIN', NULL, NULL, NULL, NULL, '', NULL, '', false, false, false, 'ADMIN', NULL, NULL, '::1', '2026-07-07 16:50:13.693314', 10, NULL, NULL, '2026-07-07 13:46:05.758679', '2026-07-07 16:50:13.693539', false, true);
INSERT INTO goldb.users VALUES (32, 'dev_retail1', '.7x5za6BTpYmd5lH/YuvmvLVkpKGIr4Cypm8gtXvIgU2SoHlAK', '테스트 소매 1', NULL, NULL, NULL, NULL, 'avatar.png', NULL, '', false, false, false, 'user', NULL, NULL, '::1', '2026-07-07 16:50:52.087795', 2, NULL, 1, '2026-07-07 20:25:25.607458', '2026-07-07 16:50:52.089298', false, true);
INSERT INTO goldb.users VALUES (38, 'dev_mfg1', '.7x5za6BTpYmd5lH/YuvmvLVkpKGIr4Cypm8gtXvIgU2SoHlAK', '테스트 제조 1', NULL, NULL, NULL, NULL, 'avatar.png', NULL, '', false, false, false, 'user', NULL, NULL, '::1', '2026-07-07 16:52:23.603018', 2, NULL, 32, '2026-07-07 20:25:25.607458', '2026-07-07 16:52:23.603137', false, true);
INSERT INTO goldb.users VALUES (33, 'dev_retail2', '.7x5za6BTpYmd5lH/YuvmvLVkpKGIr4Cypm8gtXvIgU2SoHlAK', '테스트 소매 2', NULL, NULL, NULL, NULL, 'avatar.png', NULL, '', false, false, false, 'user', NULL, NULL, '::1', '2026-07-07 15:27:23.22508', 1, NULL, 1, '2026-07-07 20:25:25.607458', '2026-07-07 15:27:23.22568', false, true);
INSERT INTO goldb.users VALUES (39, 'dev_mfg2', '.7x5za6BTpYmd5lH/YuvmvLVkpKGIr4Cypm8gtXvIgU2SoHlAK', '테스트 제조 2', NULL, NULL, NULL, NULL, 'avatar.png', NULL, '', false, false, false, 'user', NULL, NULL, NULL, NULL, 0, NULL, NULL, '2026-07-07 20:25:25.607458', '2026-07-07 20:25:25.607458', false, true);
INSERT INTO goldb.users VALUES (40, 'dev_mfg3', '.7x5za6BTpYmd5lH/YuvmvLVkpKGIr4Cypm8gtXvIgU2SoHlAK', '테스트 제조 3', NULL, NULL, NULL, NULL, 'avatar.png', NULL, '', false, false, false, 'user', NULL, NULL, NULL, NULL, 0, NULL, NULL, '2026-07-07 20:25:25.607458', '2026-07-07 20:25:25.607458', false, true);


--
-- Data for Name: orders; Type: TABLE DATA; Schema: goldb; Owner: postgres
--



--
-- Data for Name: product_sets; Type: TABLE DATA; Schema: goldb; Owner: postgres
--



--
-- Data for Name: products; Type: TABLE DATA; Schema: goldb; Owner: postgres
--

INSERT INTO goldb.products VALUES (1, '0', '0', '4424R', '4424r', '14K', 5.00, 30, 'RING', 'FASHION_RING', NULL, 10000, NULL, NULL, 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, 0, NULL, NULL, true, 0, 'ROSE_GOLD', NULL, 1, NULL, '2026-07-09 15:36:30.376577', '2026-07-09 15:36:30.376577', false);
INSERT INTO goldb.products VALUES (2, '하트', '하트', '6245E', '6245e', '14K', 6.20, 31, 'EARRING', 'FASHION_EARRING', NULL, 30000, NULL, NULL, 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, 0, NULL, NULL, false, 0, 'GOLD_GOLD', NULL, 1, NULL, '2026-07-09 15:36:30.377596', '2026-07-09 15:36:30.377596', false);
INSERT INTO goldb.products VALUES (3, '하트', '하트', '4422R', '4422r', '14K', 5.22, 31, 'RING', 'FASHION_RING', NULL, 10000, NULL, NULL, 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, 0, NULL, NULL, false, 0, 'YELLOW_GOLD', NULL, 1, NULL, '2026-07-09 15:36:30.377969', '2026-07-09 15:36:30.377969', false);
INSERT INTO goldb.products VALUES (4, '0', '0', '3388B', '3388b', '14K', 20.00, 31, 'BRACELET', 'FASHION_BRACELET', NULL, 20000, NULL, NULL, 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, 0, NULL, NULL, false, 0, 'ROSE_GOLD', NULL, 1, NULL, '2026-07-09 15:36:30.37834', '2026-07-09 15:36:30.37834', false);
INSERT INTO goldb.products VALUES (5, '물고기', '물고기', '4438R', '4438r', '14K', 2.50, 31, 'RING', 'FASHION_RING', NULL, 10000, NULL, NULL, 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, 0, NULL, NULL, true, 0, 'ROSE_GOLD', NULL, 1, NULL, '2026-07-09 15:36:30.378736', '2026-07-09 15:36:30.378736', false);
INSERT INTO goldb.products VALUES (6, '물고기', '물고기', '4100', '4100', '18K', 1.32, 2, 'RING', 'CHARACTER_RING', NULL, 10000, NULL, NULL, 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, 0, NULL, NULL, true, 0, 'PINK_GOLD', NULL, 1, NULL, '2026-07-09 15:36:30.379087', '2026-07-09 15:36:30.379087', false);
INSERT INTO goldb.products VALUES (7, '밴드', '밴드', '7080R', '7080r', '18K', 3.77, 31, 'RING', 'FASHION_RING', NULL, 10000, NULL, NULL, 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, 0, NULL, NULL, false, 0, 'ROSE_GOLD', NULL, 1, NULL, '2026-07-09 15:36:30.379504', '2026-07-09 15:36:30.379504', false);


--
-- Data for Name: order_items; Type: TABLE DATA; Schema: goldb; Owner: postgres
--



--
-- Data for Name: stocks; Type: TABLE DATA; Schema: goldb; Owner: postgres
--



--
-- Data for Name: attachments; Type: TABLE DATA; Schema: goldb; Owner: postgres
--



--
-- Data for Name: cart_items; Type: TABLE DATA; Schema: goldb; Owner: postgres
--



--
-- Data for Name: catalogs; Type: TABLE DATA; Schema: goldb; Owner: postgres
--



--
-- Data for Name: catalog_pages; Type: TABLE DATA; Schema: goldb; Owner: postgres
--



--
-- Data for Name: catalog_page_products; Type: TABLE DATA; Schema: goldb; Owner: postgres
--



--
-- Data for Name: common_codes; Type: TABLE DATA; Schema: goldb; Owner: postgres
--

INSERT INTO goldb.common_codes VALUES (1, NULL, 'PRODUCT_CATEGORY', '제품 카테고리', NULL, 1, true, NULL, NULL, NULL, NULL, NULL, NULL, '2026-07-09 15:36:30.360985', '2026-07-09 15:36:30.360985', false);
INSERT INTO goldb.common_codes VALUES (2, NULL, 'MATERIAL_GRADE', '원자재 함량', NULL, 2, true, NULL, NULL, NULL, NULL, NULL, NULL, '2026-07-09 15:36:30.360985', '2026-07-09 15:36:30.360985', false);
INSERT INTO goldb.common_codes VALUES (3, NULL, 'PRODUCT_COLOR', '제품 컬러', NULL, 3, true, NULL, NULL, NULL, NULL, NULL, NULL, '2026-07-09 15:36:30.360985', '2026-07-09 15:36:30.360985', false);
INSERT INTO goldb.common_codes VALUES (4, NULL, 'PRODUCT_SIZE', '제품 사이즈', NULL, 4, true, NULL, NULL, NULL, NULL, NULL, NULL, '2026-07-09 15:36:30.360985', '2026-07-09 15:36:30.360985', false);
INSERT INTO goldb.common_codes VALUES (10, 2, '14K', '14K', NULL, 1, true, NULL, NULL, NULL, NULL, NULL, NULL, '2026-07-09 15:36:30.364833', '2026-07-09 15:36:30.364833', false);
INSERT INTO goldb.common_codes VALUES (11, 2, '18K', '18K', NULL, 2, true, NULL, NULL, NULL, NULL, NULL, NULL, '2026-07-09 15:36:30.364833', '2026-07-09 15:36:30.364833', false);
INSERT INTO goldb.common_codes VALUES (12, 2, '24K', '24K', NULL, 3, true, NULL, NULL, NULL, NULL, NULL, NULL, '2026-07-09 15:36:30.364833', '2026-07-09 15:36:30.364833', false);
INSERT INTO goldb.common_codes VALUES (13, 2, 'PT950', 'PT950', NULL, 4, true, NULL, NULL, NULL, NULL, NULL, NULL, '2026-07-09 15:36:30.364833', '2026-07-09 15:36:30.364833', false);
INSERT INTO goldb.common_codes VALUES (14, 2, 'PT900', 'PT900', NULL, 5, true, NULL, NULL, NULL, NULL, NULL, NULL, '2026-07-09 15:36:30.364833', '2026-07-09 15:36:30.364833', false);
INSERT INTO goldb.common_codes VALUES (15, 2, 'SV925', 'SV925', NULL, 6, true, NULL, NULL, NULL, NULL, NULL, NULL, '2026-07-09 15:36:30.364833', '2026-07-09 15:36:30.364833', false);
INSERT INTO goldb.common_codes VALUES (20, 3, 'ROSE_GOLD', '로즈골드', NULL, 1, true, NULL, NULL, NULL, NULL, NULL, NULL, '2026-07-09 15:36:30.367341', '2026-07-09 15:36:30.367341', false);
INSERT INTO goldb.common_codes VALUES (21, 3, 'WHITE_GOLD', '화이트골드', NULL, 2, true, NULL, NULL, NULL, NULL, NULL, NULL, '2026-07-09 15:36:30.367341', '2026-07-09 15:36:30.367341', false);
INSERT INTO goldb.common_codes VALUES (22, 3, 'YELLOW_GOLD', '옐로우골드', NULL, 3, true, NULL, NULL, NULL, NULL, NULL, NULL, '2026-07-09 15:36:30.367341', '2026-07-09 15:36:30.367341', false);
INSERT INTO goldb.common_codes VALUES (23, 3, 'PINK_GOLD', '핑크골드', NULL, 4, true, NULL, NULL, NULL, NULL, NULL, NULL, '2026-07-09 15:36:30.367341', '2026-07-09 15:36:30.367341', false);
INSERT INTO goldb.common_codes VALUES (24, 3, 'GOLD_GOLD', '골드골드', NULL, 5, true, NULL, NULL, NULL, NULL, NULL, NULL, '2026-07-09 15:36:30.367341', '2026-07-09 15:36:30.367341', false);
INSERT INTO goldb.common_codes VALUES (30, 1, 'RING', '반지', NULL, 1, true, NULL, NULL, NULL, NULL, NULL, NULL, '2026-07-09 15:36:30.368509', '2026-07-09 15:36:30.368509', false);
INSERT INTO goldb.common_codes VALUES (31, 1, 'EARRING', '귀걸이', NULL, 2, true, NULL, NULL, NULL, NULL, NULL, NULL, '2026-07-09 15:36:30.368509', '2026-07-09 15:36:30.368509', false);
INSERT INTO goldb.common_codes VALUES (32, 1, 'BRACELET', '팔찌', NULL, 3, true, NULL, NULL, NULL, NULL, NULL, NULL, '2026-07-09 15:36:30.368509', '2026-07-09 15:36:30.368509', false);
INSERT INTO goldb.common_codes VALUES (33, 1, 'NECKLACE', '목걸이', NULL, 4, true, NULL, NULL, NULL, NULL, NULL, NULL, '2026-07-09 15:36:30.368509', '2026-07-09 15:36:30.368509', false);
INSERT INTO goldb.common_codes VALUES (40, 30, 'FASHION_RING', '패션반지', NULL, 1, true, NULL, NULL, NULL, NULL, NULL, NULL, '2026-07-09 15:36:30.36958', '2026-07-09 15:36:30.36958', false);
INSERT INTO goldb.common_codes VALUES (41, 30, 'CHARACTER_RING', '캐릭터반지', NULL, 2, true, NULL, NULL, NULL, NULL, NULL, NULL, '2026-07-09 15:36:30.36958', '2026-07-09 15:36:30.36958', false);
INSERT INTO goldb.common_codes VALUES (42, 30, 'COUPLE_RING', '커플링', NULL, 3, true, NULL, NULL, NULL, NULL, NULL, NULL, '2026-07-09 15:36:30.36958', '2026-07-09 15:36:30.36958', false);
INSERT INTO goldb.common_codes VALUES (50, 31, 'FASHION_EARRING', '패션귀걸이', NULL, 1, true, NULL, NULL, NULL, NULL, NULL, NULL, '2026-07-09 15:36:30.370687', '2026-07-09 15:36:30.370687', false);
INSERT INTO goldb.common_codes VALUES (51, 31, 'EAR_CUFF', '이어커프', NULL, 2, true, NULL, NULL, NULL, NULL, NULL, NULL, '2026-07-09 15:36:30.370687', '2026-07-09 15:36:30.370687', false);
INSERT INTO goldb.common_codes VALUES (60, 32, 'FASHION_BRACELET', '패션팔찌', NULL, 1, true, NULL, NULL, NULL, NULL, NULL, NULL, '2026-07-09 15:36:30.371566', '2026-07-09 15:36:30.371566', false);
INSERT INTO goldb.common_codes VALUES (61, 32, 'BANGLE', '뱅글', NULL, 2, true, NULL, NULL, NULL, NULL, NULL, NULL, '2026-07-09 15:36:30.371566', '2026-07-09 15:36:30.371566', false);


--
-- Data for Name: contact_messages; Type: TABLE DATA; Schema: goldb; Owner: postgres
--



--
-- Data for Name: diamond_prices; Type: TABLE DATA; Schema: goldb; Owner: postgres
--



--
-- Data for Name: favorites; Type: TABLE DATA; Schema: goldb; Owner: postgres
--



--
-- Data for Name: gold_prices; Type: TABLE DATA; Schema: goldb; Owner: postgres
--



--
-- Data for Name: manufacturer_logistics; Type: TABLE DATA; Schema: goldb; Owner: postgres
--

INSERT INTO goldb.manufacturer_logistics VALUES (1, 30, 29, 1, NULL, '2026-07-07 15:50:14.99737', NULL, false);
INSERT INTO goldb.manufacturer_logistics VALUES (2, 2, 29, 1, 1, '2026-07-07 15:50:21.466739', '2026-07-07 15:50:26.409675', true);
INSERT INTO goldb.manufacturer_logistics VALUES (4, 30, 28, 1, NULL, '2026-07-07 16:14:30.711739', NULL, false);
INSERT INTO goldb.manufacturer_logistics VALUES (3, 2, 29, 1, 1, '2026-07-07 16:14:14.467003', '2026-07-07 16:20:37.947816', true);
INSERT INTO goldb.manufacturer_logistics VALUES (5, 31, 29, 1, NULL, '2026-07-09 15:36:08.774141', NULL, false);
INSERT INTO goldb.manufacturer_logistics VALUES (6, 31, 28, 1, NULL, '2026-07-09 15:36:08.780932', NULL, false);


--
-- Data for Name: menus; Type: TABLE DATA; Schema: goldb; Owner: postgres
--

INSERT INTO goldb.menus VALUES (1, NULL, '/admin', 'Layout', 'Admin', NULL, '관리자 업무', 'Setting', NULL, NULL, NULL, NULL, NULL, NULL, false, 1, NULL, NULL, '2026-07-07 18:52:03.06697', '2026-07-07 18:52:03.06697', false);
INSERT INTO goldb.menus VALUES (2, 1, 'order-management', 'admin/order-management', 'OrderManagement', NULL, '주문 통합 관리', 'List', NULL, NULL, NULL, NULL, NULL, NULL, false, 1, NULL, NULL, '2026-07-07 18:52:03.099213', '2026-07-07 18:52:03.099213', false);
INSERT INTO goldb.menus VALUES (3, 1, 'order-tracking', 'admin/order-tracking', 'OrderTracking', NULL, '주문 이력 추적', 'Clock', NULL, NULL, NULL, NULL, NULL, NULL, false, 2, NULL, NULL, '2026-07-07 18:52:03.100136', '2026-07-07 18:52:03.100136', false);
INSERT INTO goldb.menus VALUES (4, 1, 'partner-retailers', 'admin/partner-retailers', 'PartnerRetailers', NULL, '협력 소매점 관리', 'Connection', NULL, NULL, NULL, NULL, NULL, NULL, false, 3, NULL, NULL, '2026-07-07 18:52:03.100748', '2026-07-07 18:52:03.100748', false);
INSERT INTO goldb.menus VALUES (5, 1, 'logistics-approval', 'admin/logistics-approval', 'LogisticsApproval', NULL, '물류 승인 내역', 'Checked', NULL, NULL, NULL, NULL, NULL, NULL, false, 4, NULL, NULL, '2026-07-07 18:52:03.101296', '2026-07-07 18:52:03.101296', false);
INSERT INTO goldb.menus VALUES (6, 1, 'receivable', 'admin/receivable-management', 'ReceivableManagement', NULL, '미수금/수납 관리', 'Money', NULL, NULL, NULL, NULL, NULL, NULL, false, 5, NULL, NULL, '2026-07-07 18:52:03.101782', '2026-07-07 18:52:03.101782', false);
INSERT INTO goldb.menus VALUES (7, 1, 'factory-request', 'admin/factory-request', 'FactoryRequest', NULL, '공장 의뢰 관리', 'List', NULL, NULL, NULL, NULL, NULL, NULL, false, 6, NULL, NULL, '2026-07-07 18:52:03.102287', '2026-07-07 18:52:03.102287', false);
INSERT INTO goldb.menus VALUES (8, 1, 'inspection-management', 'admin/inspection-management', 'InspectionManagement', NULL, '물류 검수 확인', 'Checked', NULL, NULL, NULL, NULL, NULL, NULL, false, 7, NULL, NULL, '2026-07-07 18:52:03.102632', '2026-07-07 18:52:03.102632', false);
INSERT INTO goldb.menus VALUES (9, 1, 'settlement-management', 'admin/settlement-management', 'SettlementManagement', NULL, '정산 대상 관리', 'Money', NULL, NULL, NULL, NULL, NULL, NULL, false, 8, NULL, NULL, '2026-07-07 18:52:03.102902', '2026-07-07 18:52:03.102902', false);
INSERT INTO goldb.menus VALUES (10, 1, 'settlement-history', 'admin/settlement-history', 'SettlementHistory', NULL, '정산 완료 내역', 'Memo', NULL, NULL, NULL, NULL, NULL, NULL, false, 9, NULL, NULL, '2026-07-07 18:52:03.103165', '2026-07-07 18:52:03.103165', false);
INSERT INTO goldb.menus VALUES (11, NULL, '/sys', 'Layout', 'Sys', NULL, '시스템 관리', 'Setting', NULL, NULL, NULL, NULL, NULL, NULL, false, 2, NULL, NULL, '2026-07-07 18:52:03.103425', '2026-07-07 18:52:03.103425', false);
INSERT INTO goldb.menus VALUES (12, 11, 'user', 'sys/user', 'UserManagement', NULL, '사용자 관리', 'user', NULL, NULL, NULL, NULL, NULL, NULL, false, 1, NULL, NULL, '2026-07-07 18:52:03.10369', '2026-07-07 18:52:03.10369', false);
INSERT INTO goldb.menus VALUES (13, 11, 'company', 'sys/company', 'CompanyManagement', NULL, '업체 관리', 'office-building', NULL, NULL, NULL, NULL, NULL, NULL, false, 2, NULL, NULL, '2026-07-07 18:52:03.104', '2026-07-07 18:52:03.104', false);
INSERT INTO goldb.menus VALUES (14, 11, 'company-mapping', 'sys/company-mapping', 'CompanyMapping', NULL, '물류-소매업 매핑', 'Connection', NULL, NULL, NULL, NULL, NULL, NULL, false, 3, NULL, NULL, '2026-07-07 18:52:03.104267', '2026-07-07 18:52:03.104267', false);
INSERT INTO goldb.menus VALUES (15, 11, 'menu', 'sys/menu', 'MenuManagement', NULL, '메뉴 관리', 'list', NULL, NULL, NULL, NULL, NULL, NULL, false, 4, NULL, NULL, '2026-07-07 18:52:03.104527', '2026-07-07 18:52:03.104527', false);
INSERT INTO goldb.menus VALUES (16, 11, 'code', 'sys/code', 'CodeManagement', NULL, '공통 코드 관리', 'component', NULL, NULL, NULL, NULL, NULL, NULL, false, 5, NULL, NULL, '2026-07-07 18:52:03.1048', '2026-07-07 18:52:03.1048', false);
INSERT INTO goldb.menus VALUES (17, NULL, '/stock', 'Layout', 'Stock', NULL, '재고 관리', 'Management', NULL, NULL, NULL, NULL, NULL, NULL, false, 3, NULL, NULL, '2026-07-07 18:52:03.105072', '2026-07-07 18:52:03.105072', false);
INSERT INTO goldb.menus VALUES (18, 17, 'index', 'stock/index', 'StockMain', NULL, '나의 재고 관리', 'User', NULL, NULL, NULL, NULL, NULL, NULL, false, 1, NULL, NULL, '2026-07-07 18:52:03.105317', '2026-07-07 18:52:03.105317', false);
INSERT INTO goldb.menus VALUES (19, 17, 'direct-stock', 'stock/direct-stock', 'DirectStock', NULL, '직영 소매점 재고', 'OfficeBuilding', NULL, NULL, NULL, NULL, NULL, NULL, false, 2, NULL, NULL, '2026-07-07 18:52:03.105561', '2026-07-07 18:52:03.105561', false);
INSERT INTO goldb.menus VALUES (20, 17, 'settlement', 'stock/settlement', 'RetailerSettlement', NULL, '나의 정산 관리', 'Money', NULL, NULL, NULL, NULL, NULL, NULL, false, 3, NULL, NULL, '2026-07-07 18:52:03.105834', '2026-07-07 18:52:03.105834', false);
INSERT INTO goldb.menus VALUES (21, NULL, '/notice', 'Layout', 'Notice', NULL, '공지 관리', 'message', NULL, NULL, NULL, NULL, NULL, NULL, false, 4, NULL, NULL, '2026-07-07 18:52:03.106137', '2026-07-07 18:52:03.106137', false);
INSERT INTO goldb.menus VALUES (22, 21, 'index', 'notice/index', 'NoticeMain', NULL, '공지 관리', 'message', NULL, NULL, NULL, NULL, NULL, NULL, false, 1, NULL, NULL, '2026-07-07 18:52:03.10648', '2026-07-07 18:52:03.10648', false);
INSERT INTO goldb.menus VALUES (23, NULL, '/logistics', 'Layout', 'Logistics', NULL, '물류업체', 'Connection', NULL, NULL, NULL, NULL, NULL, NULL, false, 5, NULL, NULL, '2026-07-07 18:52:03.106874', '2026-07-07 18:52:03.106874', false);
INSERT INTO goldb.menus VALUES (24, 23, 'index', 'logistics/index', 'LogisticsCompanyList', NULL, '물류업체 현황', 'OfficeBuilding', NULL, NULL, NULL, NULL, NULL, NULL, false, 1, NULL, NULL, '2026-07-07 18:52:03.107174', '2026-07-07 18:52:03.107174', false);
INSERT INTO goldb.menus VALUES (25, NULL, '/prd', 'Layout', 'Prd', NULL, '상품 주문', 'ShoppingCart', NULL, NULL, NULL, NULL, NULL, NULL, false, 6, NULL, NULL, '2026-07-07 18:52:03.107533', '2026-07-07 18:52:03.107533', false);
INSERT INTO goldb.menus VALUES (26, 25, 'market', 'product-market/index', 'ProductMarket', NULL, '상품 마켓', 'Shop', NULL, NULL, NULL, NULL, NULL, NULL, false, 1, NULL, NULL, '2026-07-07 18:52:03.107834', '2026-07-07 18:52:03.107834', false);
INSERT INTO goldb.menus VALUES (27, 25, 'catalog-viewer', 'product-market/catalog-viewer', 'CatalogViewer', NULL, 'E-카탈로그', 'Reading', NULL, NULL, NULL, NULL, NULL, NULL, false, 2, NULL, NULL, '2026-07-07 18:52:03.108174', '2026-07-07 18:52:03.108174', false);
INSERT INTO goldb.menus VALUES (29, 11, 'manufacturer-mapping', 'sys/manufacturer-mapping', 'ManufacturerMapping', NULL, '생산업체-물류센터 매핑', 'Connection', NULL, NULL, NULL, NULL, NULL, NULL, false, 6, NULL, NULL, '2026-07-07 19:58:17.12078', '2026-07-07 19:58:17.12078', false);
INSERT INTO goldb.menus VALUES (30, 2, '', '', '', '', '', '', NULL, false, false, false, NULL, false, false, 1, 1, NULL, '2026-07-07 17:08:28.101988', NULL, false);


--
-- Data for Name: menu_favorites; Type: TABLE DATA; Schema: goldb; Owner: postgres
--



--
-- Data for Name: menu_permissions; Type: TABLE DATA; Schema: goldb; Owner: postgres
--

INSERT INTO goldb.menu_permissions VALUES (2, 29, 'ADMIN', true, true, true, true, true, false, false, false, false, false, false, false, false, NULL, NULL, '2026-07-07 20:03:16.642195', '2026-07-07 20:03:16.642195', false);
INSERT INTO goldb.menu_permissions VALUES (3, 1, 'manufacturer', true, true, true, true, true, false, false, false, false, false, false, false, false, NULL, NULL, '2026-07-07 20:30:21.809782', '2026-07-07 20:30:21.809782', false);
INSERT INTO goldb.menu_permissions VALUES (4, 27, 'retail', true, true, true, true, true, false, false, false, false, false, false, false, false, NULL, NULL, '2026-07-07 20:30:21.809782', '2026-07-07 20:30:21.809782', false);
INSERT INTO goldb.menu_permissions VALUES (5, 22, 'manufacturer', true, true, true, true, true, false, false, false, false, false, false, false, false, NULL, NULL, '2026-07-07 20:30:21.809782', '2026-07-07 20:30:21.809782', false);
INSERT INTO goldb.menu_permissions VALUES (6, 26, 'retail', true, true, true, true, true, false, false, false, false, false, false, false, false, NULL, NULL, '2026-07-07 20:30:21.809782', '2026-07-07 20:30:21.809782', false);
INSERT INTO goldb.menu_permissions VALUES (7, 25, 'retail', true, true, true, true, true, false, false, false, false, false, false, false, false, NULL, NULL, '2026-07-07 20:30:21.809782', '2026-07-07 20:30:21.809782', false);
INSERT INTO goldb.menu_permissions VALUES (8, 9, 'market', true, true, true, true, true, false, false, false, false, false, false, false, false, NULL, NULL, '2026-07-07 20:30:21.809782', '2026-07-07 20:30:21.809782', false);
INSERT INTO goldb.menu_permissions VALUES (9, 24, 'market', true, true, true, true, true, false, false, false, false, false, false, false, false, NULL, NULL, '2026-07-07 20:30:21.809782', '2026-07-07 20:30:21.809782', false);
INSERT INTO goldb.menu_permissions VALUES (10, 21, 'manufacturer', true, true, true, true, true, false, false, false, false, false, false, false, false, NULL, NULL, '2026-07-07 20:30:21.809782', '2026-07-07 20:30:21.809782', false);
INSERT INTO goldb.menu_permissions VALUES (11, 19, 'market', true, true, true, true, true, false, false, false, false, false, false, false, false, NULL, NULL, '2026-07-07 20:30:21.809782', '2026-07-07 20:30:21.809782', false);
INSERT INTO goldb.menu_permissions VALUES (12, 3, 'market', true, true, true, true, true, false, false, false, false, false, false, false, false, NULL, NULL, '2026-07-07 20:30:21.809782', '2026-07-07 20:30:21.809782', false);
INSERT INTO goldb.menu_permissions VALUES (13, 5, 'market', true, true, true, true, true, false, false, false, false, false, false, false, false, NULL, NULL, '2026-07-07 20:30:21.809782', '2026-07-07 20:30:21.809782', false);
INSERT INTO goldb.menu_permissions VALUES (14, 2, 'retail', true, true, true, true, true, false, false, false, false, false, false, false, false, NULL, NULL, '2026-07-07 20:30:21.809782', '2026-07-07 20:30:21.809782', false);
INSERT INTO goldb.menu_permissions VALUES (15, 21, 'market', true, true, true, true, true, false, false, false, false, false, false, false, false, NULL, NULL, '2026-07-07 20:30:21.809782', '2026-07-07 20:30:21.809782', false);
INSERT INTO goldb.menu_permissions VALUES (16, 18, 'retail', true, true, true, true, true, false, false, false, false, false, false, false, false, NULL, NULL, '2026-07-07 20:30:21.809782', '2026-07-07 20:30:21.809782', false);
INSERT INTO goldb.menu_permissions VALUES (17, 1, 'market', true, true, true, true, true, false, false, false, false, false, false, false, false, NULL, NULL, '2026-07-07 20:30:21.809782', '2026-07-07 20:30:21.809782', false);
INSERT INTO goldb.menu_permissions VALUES (18, 7, 'manufacturer', true, true, true, true, true, false, false, false, false, false, false, false, false, NULL, NULL, '2026-07-07 20:30:21.809782', '2026-07-07 20:30:21.809782', false);
INSERT INTO goldb.menu_permissions VALUES (19, 22, 'market', true, true, true, true, true, false, false, false, false, false, false, false, false, NULL, NULL, '2026-07-07 20:30:21.809782', '2026-07-07 20:30:21.809782', false);
INSERT INTO goldb.menu_permissions VALUES (20, 4, 'market', true, true, true, true, true, false, false, false, false, false, false, false, false, NULL, NULL, '2026-07-07 20:30:21.809782', '2026-07-07 20:30:21.809782', false);
INSERT INTO goldb.menu_permissions VALUES (21, 17, 'retail', true, true, true, true, true, false, false, false, false, false, false, false, false, NULL, NULL, '2026-07-07 20:30:21.809782', '2026-07-07 20:30:21.809782', false);
INSERT INTO goldb.menu_permissions VALUES (22, 23, 'market', true, true, true, true, true, false, false, false, false, false, false, false, false, NULL, NULL, '2026-07-07 20:30:21.809782', '2026-07-07 20:30:21.809782', false);
INSERT INTO goldb.menu_permissions VALUES (23, 21, 'retail', true, true, true, true, true, false, false, false, false, false, false, false, false, NULL, NULL, '2026-07-07 20:30:21.809782', '2026-07-07 20:30:21.809782', false);
INSERT INTO goldb.menu_permissions VALUES (24, 6, 'market', true, true, true, true, true, false, false, false, false, false, false, false, false, NULL, NULL, '2026-07-07 20:30:21.809782', '2026-07-07 20:30:21.809782', false);
INSERT INTO goldb.menu_permissions VALUES (25, 2, 'market', true, true, true, true, true, false, false, false, false, false, false, false, false, NULL, NULL, '2026-07-07 20:30:21.809782', '2026-07-07 20:30:21.809782', false);
INSERT INTO goldb.menu_permissions VALUES (26, 18, 'market', true, true, true, true, true, false, false, false, false, false, false, false, false, NULL, NULL, '2026-07-07 20:30:21.809782', '2026-07-07 20:30:21.809782', false);
INSERT INTO goldb.menu_permissions VALUES (27, 1, 'retail', true, true, true, true, true, false, false, false, false, false, false, false, false, NULL, NULL, '2026-07-07 20:30:21.809782', '2026-07-07 20:30:21.809782', false);
INSERT INTO goldb.menu_permissions VALUES (28, 10, 'market', true, true, true, true, true, false, false, false, false, false, false, false, false, NULL, NULL, '2026-07-07 20:30:21.809782', '2026-07-07 20:30:21.809782', false);
INSERT INTO goldb.menu_permissions VALUES (29, 22, 'retail', true, true, true, true, true, false, false, false, false, false, false, false, false, NULL, NULL, '2026-07-07 20:30:21.809782', '2026-07-07 20:30:21.809782', false);
INSERT INTO goldb.menu_permissions VALUES (30, 17, 'market', true, true, true, true, true, false, false, false, false, false, false, false, false, NULL, NULL, '2026-07-07 20:30:21.809782', '2026-07-07 20:30:21.809782', false);
INSERT INTO goldb.menu_permissions VALUES (31, 18, 'manufacturer', true, true, true, true, true, false, false, false, false, false, false, false, false, NULL, NULL, '2026-07-07 20:30:21.809782', '2026-07-07 20:30:21.809782', false);
INSERT INTO goldb.menu_permissions VALUES (32, 20, 'market', true, true, true, true, true, false, false, false, false, false, false, false, false, NULL, NULL, '2026-07-07 20:30:21.809782', '2026-07-07 20:30:21.809782', false);
INSERT INTO goldb.menu_permissions VALUES (33, 19, 'retail', true, true, true, true, true, false, false, false, false, false, false, false, false, NULL, NULL, '2026-07-07 20:30:21.809782', '2026-07-07 20:30:21.809782', false);
INSERT INTO goldb.menu_permissions VALUES (34, 17, 'manufacturer', true, true, true, true, true, false, false, false, false, false, false, false, false, NULL, NULL, '2026-07-07 20:30:21.809782', '2026-07-07 20:30:21.809782', false);
INSERT INTO goldb.menu_permissions VALUES (35, 8, 'market', true, true, true, true, true, false, false, false, false, false, false, false, false, NULL, NULL, '2026-07-07 20:30:21.809782', '2026-07-07 20:30:21.809782', false);
INSERT INTO goldb.menu_permissions VALUES (36, 3, 'retail', true, true, true, true, true, false, false, false, false, false, false, false, false, NULL, NULL, '2026-07-07 20:30:21.809782', '2026-07-07 20:30:21.809782', false);


--
-- Data for Name: mv_admin_dashboard_summaries; Type: TABLE DATA; Schema: goldb; Owner: postgres
--



--
-- Data for Name: mv_daily_order_trends; Type: TABLE DATA; Schema: goldb; Owner: postgres
--



--
-- Data for Name: mv_monthly_order_trends; Type: TABLE DATA; Schema: goldb; Owner: postgres
--



--
-- Data for Name: mv_partner_retailer_stats; Type: TABLE DATA; Schema: goldb; Owner: postgres
--



--
-- Data for Name: mv_product_performances; Type: TABLE DATA; Schema: goldb; Owner: postgres
--



--
-- Data for Name: notices; Type: TABLE DATA; Schema: goldb; Owner: postgres
--



--
-- Data for Name: order_statements; Type: TABLE DATA; Schema: goldb; Owner: postgres
--



--
-- Data for Name: order_status_histories; Type: TABLE DATA; Schema: goldb; Owner: postgres
--



--
-- Data for Name: plaster_orders; Type: TABLE DATA; Schema: goldb; Owner: postgres
--



--
-- Data for Name: product_option_weights; Type: TABLE DATA; Schema: goldb; Owner: postgres
--



--
-- Data for Name: product_photos; Type: TABLE DATA; Schema: goldb; Owner: postgres
--

INSERT INTO goldb.product_photos VALUES (1, 1, '/uploads/4424r.png', NULL, NULL, true, 0, NULL, NULL, '2026-07-09 15:36:30.380168', '2026-07-09 15:36:30.380168', false);
INSERT INTO goldb.product_photos VALUES (2, 2, '/uploads/6245e.png', NULL, NULL, true, 0, NULL, NULL, '2026-07-09 15:36:30.380168', '2026-07-09 15:36:30.380168', false);
INSERT INTO goldb.product_photos VALUES (3, 3, '/uploads/4422r.png', NULL, NULL, true, 0, NULL, NULL, '2026-07-09 15:36:30.380168', '2026-07-09 15:36:30.380168', false);
INSERT INTO goldb.product_photos VALUES (4, 4, '/uploads/3388b.png', NULL, NULL, true, 0, NULL, NULL, '2026-07-09 15:36:30.380168', '2026-07-09 15:36:30.380168', false);
INSERT INTO goldb.product_photos VALUES (5, 5, '/uploads/4438r.png', NULL, NULL, true, 0, NULL, NULL, '2026-07-09 15:36:30.380168', '2026-07-09 15:36:30.380168', false);
INSERT INTO goldb.product_photos VALUES (6, 6, '/uploads/4100.png', NULL, NULL, true, 0, NULL, NULL, '2026-07-09 15:36:30.380168', '2026-07-09 15:36:30.380168', false);
INSERT INTO goldb.product_photos VALUES (7, 7, '/uploads/7080r.png', NULL, NULL, true, 0, NULL, NULL, '2026-07-09 15:36:30.380168', '2026-07-09 15:36:30.380168', false);


--
-- Data for Name: product_set_items; Type: TABLE DATA; Schema: goldb; Owner: postgres
--



--
-- Data for Name: product_set_photos; Type: TABLE DATA; Schema: goldb; Owner: postgres
--



--
-- Data for Name: receivables; Type: TABLE DATA; Schema: goldb; Owner: postgres
--



--
-- Data for Name: roles; Type: TABLE DATA; Schema: goldb; Owner: postgres
--

INSERT INTO goldb.roles VALUES (1, 'admin', 'Admin', NULL, NULL, NULL, '2026-07-07 13:46:05.564932', NULL, false);
INSERT INTO goldb.roles VALUES (2, 'manufacturer', 'Ishlab chiqaruvchi', NULL, NULL, NULL, '2026-07-07 13:46:05.564932', NULL, false);
INSERT INTO goldb.roles VALUES (3, 'retail', 'Chakana savdo', NULL, NULL, NULL, '2026-07-07 13:46:05.564932', NULL, false);
INSERT INTO goldb.roles VALUES (4, 'market', 'Market/Ulgurji', NULL, NULL, NULL, '2026-07-07 13:46:05.564932', NULL, false);


--
-- Data for Name: transactions; Type: TABLE DATA; Schema: goldb; Owner: postgres
--



--
-- Data for Name: user_companies; Type: TABLE DATA; Schema: goldb; Owner: postgres
--

INSERT INTO goldb.user_companies VALUES (1, 1, NULL, NULL, '2026-07-07 13:46:05.822259', NULL, false);
INSERT INTO goldb.user_companies VALUES (2, 2, NULL, NULL, '2026-07-07 13:46:05.831051', NULL, false);
INSERT INTO goldb.user_companies VALUES (3, 3, NULL, NULL, '2026-07-07 13:46:05.834875', NULL, false);
INSERT INTO goldb.user_companies VALUES (4, 4, NULL, NULL, '2026-07-07 13:46:05.837954', NULL, false);
INSERT INTO goldb.user_companies VALUES (32, 24, NULL, NULL, '2026-07-07 20:25:25.607965', '2026-07-07 20:25:25.607965', false);
INSERT INTO goldb.user_companies VALUES (33, 25, NULL, NULL, '2026-07-07 20:25:25.608593', '2026-07-07 20:25:25.608593', false);
INSERT INTO goldb.user_companies VALUES (34, 26, NULL, NULL, '2026-07-07 20:25:25.609464', '2026-07-07 20:25:25.609464', false);
INSERT INTO goldb.user_companies VALUES (35, 27, NULL, NULL, '2026-07-07 20:25:25.609887', '2026-07-07 20:25:25.609887', false);
INSERT INTO goldb.user_companies VALUES (36, 28, NULL, NULL, '2026-07-07 20:25:25.610298', '2026-07-07 20:25:25.610298', false);
INSERT INTO goldb.user_companies VALUES (37, 29, NULL, NULL, '2026-07-07 20:25:25.610708', '2026-07-07 20:25:25.610708', false);
INSERT INTO goldb.user_companies VALUES (38, 30, NULL, NULL, '2026-07-07 20:25:25.611091', '2026-07-07 20:25:25.611091', false);
INSERT INTO goldb.user_companies VALUES (39, 31, NULL, NULL, '2026-07-07 20:25:25.611462', '2026-07-07 20:25:25.611462', false);
INSERT INTO goldb.user_companies VALUES (40, 32, NULL, NULL, '2026-07-07 20:25:25.611852', '2026-07-07 20:25:25.611852', false);


--
-- Data for Name: user_emails; Type: TABLE DATA; Schema: goldb; Owner: postgres
--



--
-- Data for Name: user_menu_settings; Type: TABLE DATA; Schema: goldb; Owner: postgres
--



--
-- Data for Name: user_personal_settings; Type: TABLE DATA; Schema: goldb; Owner: postgres
--



--
-- Data for Name: user_phones; Type: TABLE DATA; Schema: goldb; Owner: postgres
--



--
-- Data for Name: user_photos; Type: TABLE DATA; Schema: goldb; Owner: postgres
--



--
-- Data for Name: user_roles; Type: TABLE DATA; Schema: goldb; Owner: postgres
--

INSERT INTO goldb.user_roles VALUES (1, 1, NULL, NULL, '2026-07-07 13:46:05.822259', NULL, false);
INSERT INTO goldb.user_roles VALUES (2, 2, NULL, NULL, '2026-07-07 13:46:05.831051', NULL, false);
INSERT INTO goldb.user_roles VALUES (3, 3, NULL, NULL, '2026-07-07 13:46:05.834875', NULL, false);
INSERT INTO goldb.user_roles VALUES (4, 4, NULL, NULL, '2026-07-07 13:46:05.837954', NULL, false);
INSERT INTO goldb.user_roles VALUES (32, 3, NULL, NULL, '2026-07-07 20:25:57.482128', '2026-07-07 20:25:57.482128', false);
INSERT INTO goldb.user_roles VALUES (33, 3, NULL, NULL, '2026-07-07 20:25:57.482128', '2026-07-07 20:25:57.482128', false);
INSERT INTO goldb.user_roles VALUES (34, 3, NULL, NULL, '2026-07-07 20:25:57.482128', '2026-07-07 20:25:57.482128', false);
INSERT INTO goldb.user_roles VALUES (38, 2, NULL, NULL, '2026-07-07 20:26:03.536505', '2026-07-07 20:26:03.536505', false);
INSERT INTO goldb.user_roles VALUES (39, 2, NULL, NULL, '2026-07-07 20:26:03.536505', '2026-07-07 20:26:03.536505', false);
INSERT INTO goldb.user_roles VALUES (40, 2, NULL, NULL, '2026-07-07 20:26:03.536505', '2026-07-07 20:26:03.536505', false);
INSERT INTO goldb.user_roles VALUES (35, 4, NULL, NULL, '2026-07-07 20:26:10.5459', '2026-07-07 20:26:10.5459', false);
INSERT INTO goldb.user_roles VALUES (36, 4, NULL, NULL, '2026-07-07 20:26:10.5459', '2026-07-07 20:26:10.5459', false);
INSERT INTO goldb.user_roles VALUES (37, 4, NULL, NULL, '2026-07-07 20:26:10.5459', '2026-07-07 20:26:10.5459', false);


--
-- Name: articles_id_seq; Type: SEQUENCE SET; Schema: goldb; Owner: postgres
--

SELECT pg_catalog.setval('goldb.articles_id_seq', 1, false);


--
-- Name: attachments_id_seq; Type: SEQUENCE SET; Schema: goldb; Owner: postgres
--

SELECT pg_catalog.setval('goldb.attachments_id_seq', 1, false);


--
-- Name: cart_items_id_seq; Type: SEQUENCE SET; Schema: goldb; Owner: postgres
--

SELECT pg_catalog.setval('goldb.cart_items_id_seq', 1, false);


--
-- Name: catalog_pages_id_seq; Type: SEQUENCE SET; Schema: goldb; Owner: postgres
--

SELECT pg_catalog.setval('goldb.catalog_pages_id_seq', 1, false);


--
-- Name: catalogs_id_seq; Type: SEQUENCE SET; Schema: goldb; Owner: postgres
--

SELECT pg_catalog.setval('goldb.catalogs_id_seq', 1, false);


--
-- Name: common_codes_id_seq; Type: SEQUENCE SET; Schema: goldb; Owner: postgres
--

SELECT pg_catalog.setval('goldb.common_codes_id_seq', 100, true);


--
-- Name: companies_id_seq; Type: SEQUENCE SET; Schema: goldb; Owner: postgres
--

SELECT pg_catalog.setval('goldb.companies_id_seq', 32, true);


--
-- Name: contact_messages_id_seq; Type: SEQUENCE SET; Schema: goldb; Owner: postgres
--

SELECT pg_catalog.setval('goldb.contact_messages_id_seq', 1, false);


--
-- Name: customers_id_seq; Type: SEQUENCE SET; Schema: goldb; Owner: postgres
--

SELECT pg_catalog.setval('goldb.customers_id_seq', 1, false);


--
-- Name: diamond_prices_id_seq; Type: SEQUENCE SET; Schema: goldb; Owner: postgres
--

SELECT pg_catalog.setval('goldb.diamond_prices_id_seq', 1, false);


--
-- Name: favorites_id_seq; Type: SEQUENCE SET; Schema: goldb; Owner: postgres
--

SELECT pg_catalog.setval('goldb.favorites_id_seq', 1, false);


--
-- Name: gold_prices_id_seq; Type: SEQUENCE SET; Schema: goldb; Owner: postgres
--

SELECT pg_catalog.setval('goldb.gold_prices_id_seq', 1, false);


--
-- Name: manufacturer_logistics_id_seq; Type: SEQUENCE SET; Schema: goldb; Owner: postgres
--

SELECT pg_catalog.setval('goldb.manufacturer_logistics_id_seq', 6, true);


--
-- Name: menu_favorites_id_seq; Type: SEQUENCE SET; Schema: goldb; Owner: postgres
--

SELECT pg_catalog.setval('goldb.menu_favorites_id_seq', 1, false);


--
-- Name: menu_permissions_id_seq; Type: SEQUENCE SET; Schema: goldb; Owner: postgres
--

SELECT pg_catalog.setval('goldb.menu_permissions_id_seq', 36, true);


--
-- Name: menus_id_seq; Type: SEQUENCE SET; Schema: goldb; Owner: postgres
--

SELECT pg_catalog.setval('goldb.menus_id_seq', 30, true);


--
-- Name: notices_id_seq; Type: SEQUENCE SET; Schema: goldb; Owner: postgres
--

SELECT pg_catalog.setval('goldb.notices_id_seq', 1, false);


--
-- Name: order_items_id_seq; Type: SEQUENCE SET; Schema: goldb; Owner: postgres
--

SELECT pg_catalog.setval('goldb.order_items_id_seq', 1, false);


--
-- Name: order_statements_id_seq; Type: SEQUENCE SET; Schema: goldb; Owner: postgres
--

SELECT pg_catalog.setval('goldb.order_statements_id_seq', 1, false);


--
-- Name: order_status_histories_id_seq; Type: SEQUENCE SET; Schema: goldb; Owner: postgres
--

SELECT pg_catalog.setval('goldb.order_status_histories_id_seq', 1, false);


--
-- Name: orders_id_seq; Type: SEQUENCE SET; Schema: goldb; Owner: postgres
--

SELECT pg_catalog.setval('goldb.orders_id_seq', 1, false);


--
-- Name: plaster_orders_id_seq; Type: SEQUENCE SET; Schema: goldb; Owner: postgres
--

SELECT pg_catalog.setval('goldb.plaster_orders_id_seq', 1, false);


--
-- Name: product_option_weights_id_seq; Type: SEQUENCE SET; Schema: goldb; Owner: postgres
--

SELECT pg_catalog.setval('goldb.product_option_weights_id_seq', 1, false);


--
-- Name: product_photos_id_seq; Type: SEQUENCE SET; Schema: goldb; Owner: postgres
--

SELECT pg_catalog.setval('goldb.product_photos_id_seq', 7, true);


--
-- Name: product_set_photos_id_seq; Type: SEQUENCE SET; Schema: goldb; Owner: postgres
--

SELECT pg_catalog.setval('goldb.product_set_photos_id_seq', 1, false);


--
-- Name: product_sets_id_seq; Type: SEQUENCE SET; Schema: goldb; Owner: postgres
--

SELECT pg_catalog.setval('goldb.product_sets_id_seq', 1, false);


--
-- Name: products_id_seq; Type: SEQUENCE SET; Schema: goldb; Owner: postgres
--

SELECT pg_catalog.setval('goldb.products_id_seq', 8, true);


--
-- Name: receivables_id_seq; Type: SEQUENCE SET; Schema: goldb; Owner: postgres
--

SELECT pg_catalog.setval('goldb.receivables_id_seq', 1, false);


--
-- Name: roles_id_seq; Type: SEQUENCE SET; Schema: goldb; Owner: postgres
--

SELECT pg_catalog.setval('goldb.roles_id_seq', 4, true);


--
-- Name: stocks_id_seq; Type: SEQUENCE SET; Schema: goldb; Owner: postgres
--

SELECT pg_catalog.setval('goldb.stocks_id_seq', 1, false);


--
-- Name: transactions_id_seq; Type: SEQUENCE SET; Schema: goldb; Owner: postgres
--

SELECT pg_catalog.setval('goldb.transactions_id_seq', 1, false);


--
-- Name: user_emails_id_seq; Type: SEQUENCE SET; Schema: goldb; Owner: postgres
--

SELECT pg_catalog.setval('goldb.user_emails_id_seq', 1, false);


--
-- Name: user_menu_settings_id_seq; Type: SEQUENCE SET; Schema: goldb; Owner: postgres
--

SELECT pg_catalog.setval('goldb.user_menu_settings_id_seq', 1, false);


--
-- Name: user_personal_settings_id_seq; Type: SEQUENCE SET; Schema: goldb; Owner: postgres
--

SELECT pg_catalog.setval('goldb.user_personal_settings_id_seq', 1, false);


--
-- Name: user_phones_id_seq; Type: SEQUENCE SET; Schema: goldb; Owner: postgres
--

SELECT pg_catalog.setval('goldb.user_phones_id_seq', 1, false);


--
-- Name: user_photos_id_seq; Type: SEQUENCE SET; Schema: goldb; Owner: postgres
--

SELECT pg_catalog.setval('goldb.user_photos_id_seq', 1, false);


--
-- Name: users_id_seq; Type: SEQUENCE SET; Schema: goldb; Owner: postgres
--

SELECT pg_catalog.setval('goldb.users_id_seq', 40, true);


--
-- PostgreSQL database dump complete
--

\unrestrict 8XWoE0YXwJfgzKXRC3ylAu6G7S3u74Tg1zeBiEzBsQEe5n0H9UAAUNc4D1ahP0O

