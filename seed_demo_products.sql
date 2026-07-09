SET search_path TO goldb;

-- Truncate existing product-related tables
TRUNCATE TABLE products RESTART IDENTITY CASCADE;
TRUNCATE TABLE product_photos RESTART IDENTITY CASCADE;
TRUNCATE TABLE product_option_weights RESTART IDENTITY CASCADE;

-- Insert Common Codes (if not already seeded)
-- Let's clean and seed them to make sure they are in a good state
TRUNCATE TABLE common_codes RESTART IDENTITY CASCADE;

-- Root groups
INSERT INTO common_codes (id, parent_id, code, name, sort_order, is_active, is_deleted, created_at, updated_at)
VALUES 
(1, NULL, 'PRODUCT_CATEGORY', '제품 카테고리', 1, true, false, NOW(), NOW()),
(2, NULL, 'MATERIAL_GRADE', '원자재 함량', 2, true, false, NOW(), NOW()),
(3, NULL, 'PRODUCT_COLOR', '제품 컬러', 3, true, false, NOW(), NOW()),
(4, NULL, 'PRODUCT_SIZE', '제품 사이즈', 4, true, false, NOW(), NOW());

-- Material Grades (Purity)
INSERT INTO common_codes (id, parent_id, code, name, sort_order, is_active, is_deleted, created_at, updated_at)
VALUES 
(10, 2, '14K', '14K', 1, true, false, NOW(), NOW()),
(11, 2, '18K', '18K', 2, true, false, NOW(), NOW()),
(12, 2, '24K', '24K', 3, true, false, NOW(), NOW()),
(13, 2, 'PT950', 'PT950', 4, true, false, NOW(), NOW()),
(14, 2, 'PT900', 'PT900', 5, true, false, NOW(), NOW()),
(15, 2, 'SV925', 'SV925', 6, true, false, NOW(), NOW());

-- Colors
INSERT INTO common_codes (id, parent_id, code, name, sort_order, is_active, is_deleted, created_at, updated_at)
VALUES 
(20, 3, 'ROSE_GOLD', '로즈골드', 1, true, false, NOW(), NOW()),
(21, 3, 'WHITE_GOLD', '화이트골드', 2, true, false, NOW(), NOW()),
(22, 3, 'YELLOW_GOLD', '옐로우골드', 3, true, false, NOW(), NOW()),
(23, 3, 'PINK_GOLD', '핑크골드', 4, true, false, NOW(), NOW()),
(24, 3, 'GOLD_GOLD', '골드골드', 5, true, false, NOW(), NOW());

-- Categories Large
INSERT INTO common_codes (id, parent_id, code, name, sort_order, is_active, is_deleted, created_at, updated_at)
VALUES 
(30, 1, 'RING', '반지', 1, true, false, NOW(), NOW()),
(31, 1, 'EARRING', '귀걸이', 2, true, false, NOW(), NOW()),
(32, 1, 'BRACELET', '팔찌', 3, true, false, NOW(), NOW()),
(33, 1, 'NECKLACE', '목걸이', 4, true, false, NOW(), NOW());

-- Categories Medium (RING)
INSERT INTO common_codes (id, parent_id, code, name, sort_order, is_active, is_deleted, created_at, updated_at)
VALUES 
(40, 30, 'FASHION_RING', '패션반지', 1, true, false, NOW(), NOW()),
(41, 30, 'CHARACTER_RING', '캐릭터반지', 2, true, false, NOW(), NOW()),
(42, 30, 'COUPLE_RING', '커플링', 3, true, false, NOW(), NOW());

-- Categories Medium (EARRING)
INSERT INTO common_codes (id, parent_id, code, name, sort_order, is_active, is_deleted, created_at, updated_at)
VALUES 
(50, 31, 'FASHION_EARRING', '패션귀걸이', 1, true, false, NOW(), NOW()),
(51, 31, 'EAR_CUFF', '이어커프', 2, true, false, NOW(), NOW());

-- Categories Medium (BRACELET)
INSERT INTO common_codes (id, parent_id, code, name, sort_order, is_active, is_deleted, created_at, updated_at)
VALUES 
(60, 32, 'FASHION_BRACELET', '패션팔찌', 1, true, false, NOW(), NOW()),
(61, 32, 'BANGLE', '뱅글', 2, true, false, NOW(), NOW());

-- Update common_codes sequence
SELECT setval('common_codes_id_seq', 100);


-- Update company names to match the user's setup
UPDATE companies SET name = 'kreis' WHERE id = 30;
UPDATE companies SET name = 'ko1111' WHERE id = 31;

-- Add manufacturer_logistics links for ko1111 (31) if they don't exist
INSERT INTO manufacturer_logistics (manufacturer_id, logistics_id, created_by, created_at, is_deleted)
SELECT 31, 29, 1, NOW(), false WHERE NOT EXISTS (SELECT 1 FROM manufacturer_logistics WHERE manufacturer_id = 31 AND logistics_id = 29);

INSERT INTO manufacturer_logistics (manufacturer_id, logistics_id, created_by, created_at, is_deleted)
SELECT 31, 28, 1, NOW(), false WHERE NOT EXISTS (SELECT 1 FROM manufacturer_logistics WHERE manufacturer_id = 31 AND logistics_id = 28);


-- Insert Products
-- Product 1: 4424R / 0 / kreis (14K 로즈골드, 5.00g, RING/FASHION_RING, 10000, Public)
INSERT INTO products (id, name, normalized_name, product_no, normalized_product_no, purity, weight, company_id, category_large, category_medium, factory_price, labor_cost, colors, is_public, is_deleted, created_at, updated_at, created_by, basic_loss, center_stone_count, side_stone_count)
VALUES (1, '0', '0', '4424R', '4424r', '14K', 5.00, 30, 'RING', 'FASHION_RING', 10000, 0, 'ROSE_GOLD', true, false, NOW(), NOW(), 1, 0, 0, 0);

-- Product 2: 6245E / 하트 / ko1111 (14K 골드골드, 6.20g, EARRING/FASHION_EARRING, 30000, Private)
INSERT INTO products (id, name, normalized_name, product_no, normalized_product_no, purity, weight, company_id, category_large, category_medium, factory_price, labor_cost, colors, is_public, is_deleted, created_at, updated_at, created_by, basic_loss, center_stone_count, side_stone_count)
VALUES (2, '하트', '하트', '6245E', '6245e', '14K', 6.20, 31, 'EARRING', 'FASHION_EARRING', 30000, 0, 'GOLD_GOLD', false, false, NOW(), NOW(), 1, 0, 0, 0);

-- Product 3: 4422R / 하트 / ko1111 (14K 옐로우골드, 5.22g, RING/FASHION_RING, 10000, Private)
INSERT INTO products (id, name, normalized_name, product_no, normalized_product_no, purity, weight, company_id, category_large, category_medium, factory_price, labor_cost, colors, is_public, is_deleted, created_at, updated_at, created_by, basic_loss, center_stone_count, side_stone_count)
VALUES (3, '하트', '하트', '4422R', '4422r', '14K', 5.22, 31, 'RING', 'FASHION_RING', 10000, 0, 'YELLOW_GOLD', false, false, NOW(), NOW(), 1, 0, 0, 0);

-- Product 4: 3388B / 0 / ko1111 (14K 로즈골드, 20.00g, BRACELET/FASHION_BRACELET, 20000, Private)
INSERT INTO products (id, name, normalized_name, product_no, normalized_product_no, purity, weight, company_id, category_large, category_medium, factory_price, labor_cost, colors, is_public, is_deleted, created_at, updated_at, created_by, basic_loss, center_stone_count, side_stone_count)
VALUES (4, '0', '0', '3388B', '3388b', '14K', 20.00, 31, 'BRACELET', 'FASHION_BRACELET', 20000, 0, 'ROSE_GOLD', false, false, NOW(), NOW(), 1, 0, 0, 0);

-- Product 5: 4438R / 물고기 / ko1111 (14K 로즈골드, 2.50g, RING/FASHION_RING, 10000, Public)
INSERT INTO products (id, name, normalized_name, product_no, normalized_product_no, purity, weight, company_id, category_large, category_medium, factory_price, labor_cost, colors, is_public, is_deleted, created_at, updated_at, created_by, basic_loss, center_stone_count, side_stone_count)
VALUES (5, '물고기', '물고기', '4438R', '4438r', '14K', 2.50, 31, 'RING', 'FASHION_RING', 10000, 0, 'ROSE_GOLD', true, false, NOW(), NOW(), 1, 0, 0, 0);

-- Product 6: 4100 / 물고기 / MFG Company (18K 핑크골드, 1.32g, RING/CHARACTER_RING, 10000, Public)
INSERT INTO products (id, name, normalized_name, product_no, normalized_product_no, purity, weight, company_id, category_large, category_medium, factory_price, labor_cost, colors, is_public, is_deleted, created_at, updated_at, created_by, basic_loss, center_stone_count, side_stone_count)
VALUES (6, '물고기', '물고기', '4100', '4100', '18K', 1.32, 2, 'RING', 'CHARACTER_RING', 10000, 0, 'PINK_GOLD', true, false, NOW(), NOW(), 1, 0, 0, 0);

-- Product 7: 7080R / 밴드 / ko1111 (18K 로즈골드, 3.77g, RING/FASHION_RING, 10000, Private)
INSERT INTO products (id, name, normalized_name, product_no, normalized_product_no, purity, weight, company_id, category_large, category_medium, factory_price, labor_cost, colors, is_public, is_deleted, created_at, updated_at, created_by, basic_loss, center_stone_count, side_stone_count)
VALUES (7, '밴드', '밴드', '7080R', '7080r', '18K', 3.77, 31, 'RING', 'FASHION_RING', 10000, 0, 'ROSE_GOLD', false, false, NOW(), NOW(), 1, 0, 0, 0);

SELECT setval('products_id_seq', 8);


-- Insert Product Photos (Placeholders)
INSERT INTO product_photos (product_id, sort_order, is_main, is_deleted, created_at, updated_at, photo_url)
VALUES 
(1, 0, true, false, NOW(), NOW(), '/uploads/4424r.png'),
(2, 0, true, false, NOW(), NOW(), '/uploads/6245e.png'),
(3, 0, true, false, NOW(), NOW(), '/uploads/4422r.png'),
(4, 0, true, false, NOW(), NOW(), '/uploads/3388b.png'),
(5, 0, true, false, NOW(), NOW(), '/uploads/4438r.png'),
(6, 0, true, false, NOW(), NOW(), '/uploads/4100.png'),
(7, 0, true, false, NOW(), NOW(), '/uploads/7080r.png');

