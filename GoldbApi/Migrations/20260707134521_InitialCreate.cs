using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GoldbApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "goldb");

            migrationBuilder.CreateTable(
                name: "articles",
                schema: "goldb",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false, comment: "게시글 ID")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    timestamp = table.Column<long>(type: "bigint", nullable: false, comment: "타임스탬프"),
                    author = table.Column<string>(type: "text", nullable: false, comment: "작성자"),
                    reviewer = table.Column<string>(type: "text", nullable: true, comment: "검토자"),
                    title = table.Column<string>(type: "text", nullable: false, comment: "제목"),
                    content_short = table.Column<string>(type: "text", nullable: true, comment: "요약"),
                    content = table.Column<string>(type: "text", nullable: false, comment: "내용"),
                    forecast = table.Column<double>(type: "double precision", nullable: false, comment: "예측 수치"),
                    importance = table.Column<int>(type: "integer", nullable: false, comment: "중요도"),
                    type = table.Column<string>(type: "text", nullable: false, comment: "타입"),
                    status = table.Column<string>(type: "text", nullable: false, comment: "상태"),
                    display_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "표시 시간"),
                    comment_disabled = table.Column<bool>(type: "boolean", nullable: false, comment: "댓글 금지 여부"),
                    pageviews = table.Column<int>(type: "integer", nullable: false, comment: "조회수"),
                    image_uri = table.Column<string>(type: "text", nullable: true, comment: "이미지 URI"),
                    source_uri = table.Column<string>(type: "text", nullable: true, comment: "소스 URI"),
                    platforms = table.Column<string[]>(type: "text[]", nullable: false, comment: "플랫폼"),
                    created_by = table.Column<int>(type: "integer", nullable: true, comment: "생성자 ID"),
                    updated_by = table.Column<int>(type: "integer", nullable: true, comment: "수정자 ID"),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "생성 일시"),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "수정 일시"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, comment: "삭제 여부")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_articles", x => x.id);
                },
                comment: "게시글 정보");

            migrationBuilder.CreateTable(
                name: "catalogs",
                schema: "goldb",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false, comment: "ID")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "책제목"),
                    security_class = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, comment: "보안분류"),
                    issue_no = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "발행호"),
                    photo_url = table.Column<string>(type: "text", nullable: false, comment: "책자사진"),
                    total_pages = table.Column<int>(type: "integer", nullable: false, comment: "전체페이지수"),
                    created_by = table.Column<int>(type: "integer", nullable: true, comment: "생성자 ID"),
                    updated_by = table.Column<int>(type: "integer", nullable: true, comment: "수정자 ID"),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "생성 일시"),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "수정 일시"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, comment: "삭제 여부")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_catalogs", x => x.id);
                },
                comment: "카탈로그 정보");

            migrationBuilder.CreateTable(
                name: "common_codes",
                schema: "goldb",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false, comment: "코드 ID")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    parent_id = table.Column<int>(type: "integer", nullable: true, comment: "상위 코드 ID"),
                    code = table.Column<string>(type: "text", nullable: false, comment: "코드값"),
                    name = table.Column<string>(type: "text", nullable: false, comment: "코드명"),
                    description = table.Column<string>(type: "text", nullable: true, comment: "코드 설명"),
                    sort_order = table.Column<int>(type: "integer", nullable: false, comment: "정렬 순서"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, comment: "활성화 여부"),
                    custom1 = table.Column<string>(type: "text", nullable: true, comment: "커스텀 필드 1"),
                    custom2 = table.Column<string>(type: "text", nullable: true, comment: "커스텀 필드 2"),
                    custom3 = table.Column<string>(type: "text", nullable: true, comment: "커스텀 필드 3"),
                    custom4 = table.Column<string>(type: "text", nullable: true, comment: "커스텀 필드 4"),
                    created_by = table.Column<int>(type: "integer", nullable: true, comment: "생성자 ID"),
                    updated_by = table.Column<int>(type: "integer", nullable: true, comment: "수정자 ID"),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "생성 일시"),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "수정 일시"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, comment: "삭제 여부")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_common_codes", x => x.id);
                    table.ForeignKey(
                        name: "fk_common_codes_common_codes_parent_id",
                        column: x => x.parent_id,
                        principalSchema: "goldb",
                        principalTable: "common_codes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "공통 코드 정보");

            migrationBuilder.CreateTable(
                name: "companies",
                schema: "goldb",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false, comment: "업체 ID")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false, comment: "상호명"),
                    ceo = table.Column<string>(type: "text", nullable: false, comment: "대표자명"),
                    region = table.Column<string>(type: "text", nullable: false, comment: "지역"),
                    business_number = table.Column<string>(type: "text", nullable: false, comment: "사업자 등록번호"),
                    corporate_number = table.Column<string>(type: "text", nullable: true, comment: "법인번호"),
                    business_license_file_url = table.Column<string>(type: "text", nullable: true, comment: "사업자 등록증 파일"),
                    business_type = table.Column<string>(type: "text", nullable: true, comment: "업태"),
                    business_category = table.Column<string>(type: "text", nullable: true, comment: "종목"),
                    phone = table.Column<string>(type: "text", nullable: true, comment: "전화번호"),
                    address_base = table.Column<string>(type: "text", nullable: true, comment: "기본 주소"),
                    address_detail = table.Column<string>(type: "text", nullable: true, comment: "상세 주소"),
                    zip_code = table.Column<string>(type: "text", nullable: true, comment: "우편번호"),
                    logistics_code = table.Column<string>(type: "text", nullable: true, comment: "물류코드"),
                    center_number = table.Column<string>(type: "text", nullable: true, comment: "센터번호"),
                    is_direct_management = table.Column<bool>(type: "boolean", nullable: false, comment: "직영 여부"),
                    category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "업체 구분"),
                    logistics_company_id = table.Column<int>(type: "integer", nullable: true, comment: "물류센터 ID"),
                    bank_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "은행명"),
                    bank_account = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, comment: "계좌번호"),
                    account_holder = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "예금주명"),
                    created_by = table.Column<int>(type: "integer", nullable: true, comment: "생성자 ID"),
                    updated_by = table.Column<int>(type: "integer", nullable: true, comment: "수정자 ID"),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "생성 일시"),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "수정 일시"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, comment: "삭제 여부")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_companies", x => x.id);
                    table.ForeignKey(
                        name: "fk_companies_companies_logistics_company_id",
                        column: x => x.logistics_company_id,
                        principalSchema: "goldb",
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "업체 정보");

            migrationBuilder.CreateTable(
                name: "contact_messages",
                schema: "goldb",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false, comment: "이름"),
                    email = table.Column<string>(type: "text", nullable: false, comment: "이메일"),
                    phone = table.Column<string>(type: "text", nullable: true, comment: "전화번호"),
                    subject = table.Column<string>(type: "text", nullable: false, comment: "주제"),
                    message = table.Column<string>(type: "text", nullable: false, comment: "메시지 내용"),
                    is_processed = table.Column<bool>(type: "boolean", nullable: false, comment: "처리 여부"),
                    processed_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "처리 일시"),
                    processed_by = table.Column<int>(type: "integer", nullable: true, comment: "처리자 ID"),
                    admin_memo = table.Column<string>(type: "text", nullable: true, comment: "관리자 메모"),
                    created_by = table.Column<int>(type: "integer", nullable: true, comment: "생성자 ID"),
                    updated_by = table.Column<int>(type: "integer", nullable: true, comment: "수정자 ID"),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "생성 일시"),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "수정 일시"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, comment: "삭제 여부")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_contact_messages", x => x.id);
                },
                comment: "고객 문의 메시지");

            migrationBuilder.CreateTable(
                name: "diamond_prices",
                schema: "goldb",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false, comment: "ID")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    price_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, comment: "가격 구분"),
                    size = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, comment: "사이즈"),
                    virgin_price = table.Column<decimal>(type: "numeric", nullable: false, comment: "버진 가격"),
                    wooshin_price = table.Column<decimal>(type: "numeric", nullable: false, comment: "우신 가격"),
                    hyundai_price = table.Column<decimal>(type: "numeric", nullable: false, comment: "현대 가격"),
                    gukje_price = table.Column<decimal>(type: "numeric", nullable: false, comment: "국제 가격"),
                    gukbo_price = table.Column<decimal>(type: "numeric", nullable: false, comment: "국보 가격"),
                    other_price = table.Column<decimal>(type: "numeric", nullable: false, comment: "기타 가격"),
                    announced_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "고시 일시"),
                    created_by = table.Column<int>(type: "integer", nullable: true, comment: "생성자 ID"),
                    updated_by = table.Column<int>(type: "integer", nullable: true, comment: "수정자 ID"),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "생성 일시"),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "수정 일시"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, comment: "삭제 여부")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_diamond_prices", x => x.id);
                },
                comment: "다이아몬드 시세 정보");

            migrationBuilder.CreateTable(
                name: "gold_prices",
                schema: "goldb",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false, comment: "ID")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    announced_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "고시 일시"),
                    platinum = table.Column<decimal>(type: "numeric", nullable: false, comment: "백금 시세"),
                    pure_gold = table.Column<decimal>(type: "numeric", nullable: false, comment: "순금 시세"),
                    gold18_k = table.Column<decimal>(type: "numeric", nullable: false, comment: "18K 시세"),
                    gold14_k = table.Column<decimal>(type: "numeric", nullable: false, comment: "14K 시세"),
                    silver = table.Column<decimal>(type: "numeric", nullable: false, comment: "실버 시세"),
                    created_by = table.Column<int>(type: "integer", nullable: true, comment: "생성자 ID"),
                    updated_by = table.Column<int>(type: "integer", nullable: true, comment: "수정자 ID"),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "생성 일시"),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "수정 일시"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, comment: "삭제 여부")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_gold_prices", x => x.id);
                },
                comment: "일반 시세 정보");

            migrationBuilder.CreateTable(
                name: "menus",
                schema: "goldb",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false, comment: "메뉴 ID")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    parent_id = table.Column<int>(type: "integer", nullable: true, comment: "상위 메뉴 ID"),
                    path = table.Column<string>(type: "text", nullable: false, comment: "메뉴 경로"),
                    component = table.Column<string>(type: "text", nullable: true, comment: "컴포넌트 경로"),
                    name = table.Column<string>(type: "text", nullable: true, comment: "메뉴 이름"),
                    redirect = table.Column<string>(type: "text", nullable: true, comment: "리다이렉트 경로"),
                    title = table.Column<string>(type: "text", nullable: true, comment: "메뉴 제목"),
                    icon = table.Column<string>(type: "text", nullable: true, comment: "아이콘"),
                    no_cache = table.Column<bool>(type: "boolean", nullable: true, comment: "캐시 사용 여부"),
                    affix = table.Column<bool>(type: "boolean", nullable: true, comment: "태그바 고정 여부"),
                    hidden = table.Column<bool>(type: "boolean", nullable: true, comment: "숨김 여부"),
                    always_show = table.Column<bool>(type: "boolean", nullable: true, comment: "항상 표시 여부"),
                    active_menu = table.Column<string>(type: "text", nullable: true, comment: "활성 메뉴"),
                    allow_duplicate = table.Column<bool>(type: "boolean", nullable: true, comment: "중복화면 허용 여부"),
                    is_mobile = table.Column<bool>(type: "boolean", nullable: false, comment: "모바일 여부"),
                    sort_order = table.Column<int>(type: "integer", nullable: false, comment: "정렬 순서"),
                    created_by = table.Column<int>(type: "integer", nullable: true, comment: "생성자 ID"),
                    updated_by = table.Column<int>(type: "integer", nullable: true, comment: "수정자 ID"),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "생성 일시"),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "수정 일시"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, comment: "삭제 여부")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_menus", x => x.id);
                    table.ForeignKey(
                        name: "fk_menus_menus_parent_id",
                        column: x => x.parent_id,
                        principalSchema: "goldb",
                        principalTable: "menus",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "메뉴 정보");

            migrationBuilder.CreateTable(
                name: "mv_admin_dashboard_summaries",
                schema: "goldb",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    total_users = table.Column<int>(type: "integer", nullable: false),
                    total_companies = table.Column<int>(type: "integer", nullable: false),
                    total_products = table.Column<int>(type: "integer", nullable: false),
                    total_orders = table.Column<int>(type: "integer", nullable: false),
                    total_revenue = table.Column<decimal>(type: "numeric", nullable: false),
                    pending_approval_count = table.Column<int>(type: "integer", nullable: false),
                    unassigned_company_user_count = table.Column<int>(type: "integer", nullable: false),
                    unassigned_logistics_retailer_count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "mv_daily_order_trends",
                schema: "goldb",
                columns: table => new
                {
                    order_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    order_count = table.Column<int>(type: "integer", nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "mv_monthly_order_trends",
                schema: "goldb",
                columns: table => new
                {
                    order_month = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    order_count = table.Column<int>(type: "integer", nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "mv_partner_retailer_stats",
                schema: "goldb",
                columns: table => new
                {
                    company_id = table.Column<int>(type: "integer", nullable: false),
                    company_name = table.Column<string>(type: "text", nullable: false),
                    is_direct_management = table.Column<bool>(type: "boolean", nullable: false),
                    ceo = table.Column<string>(type: "text", nullable: false),
                    region = table.Column<string>(type: "text", nullable: false),
                    logistics_company_id = table.Column<int>(type: "integer", nullable: true),
                    total_stock_count = table.Column<int>(type: "integer", nullable: false),
                    total_stock_weight = table.Column<decimal>(type: "numeric", nullable: false),
                    total_order_count = table.Column<int>(type: "integer", nullable: false),
                    total_order_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    monthly_order_count = table.Column<int>(type: "integer", nullable: false),
                    monthly_order_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    pending_order_count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "mv_product_performances",
                schema: "goldb",
                columns: table => new
                {
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    product_name = table.Column<string>(type: "text", nullable: false),
                    product_no = table.Column<string>(type: "text", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "notices",
                schema: "goldb",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false, comment: "공지사항 ID")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "text", nullable: false, comment: "공지 제목"),
                    content = table.Column<string>(type: "text", nullable: false, comment: "공지 내용"),
                    is_login_required = table.Column<bool>(type: "boolean", nullable: false, comment: "로그인 필요 여부"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, comment: "활성화 여부"),
                    view_count = table.Column<int>(type: "integer", nullable: false, comment: "조회수"),
                    start_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "노출 시작일"),
                    end_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "노출 종료일"),
                    created_by = table.Column<int>(type: "integer", nullable: true, comment: "생성자 ID"),
                    updated_by = table.Column<int>(type: "integer", nullable: true, comment: "수정자 ID"),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "생성 일시"),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "수정 일시"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, comment: "삭제 여부")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notices", x => x.id);
                },
                comment: "공지사항 정보");

            migrationBuilder.CreateTable(
                name: "roles",
                schema: "goldb",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false, comment: "역할 ID")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    key = table.Column<string>(type: "text", nullable: false, comment: "역할 키"),
                    name = table.Column<string>(type: "text", nullable: false, comment: "역할 이름"),
                    description = table.Column<string>(type: "text", nullable: true, comment: "역할 설명"),
                    created_by = table.Column<int>(type: "integer", nullable: true, comment: "생성자 ID"),
                    updated_by = table.Column<int>(type: "integer", nullable: true, comment: "수정자 ID"),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "생성 일시"),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "수정 일시"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, comment: "삭제 여부")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_roles", x => x.id);
                },
                comment: "역할 정보");

            migrationBuilder.CreateTable(
                name: "transactions",
                schema: "goldb",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false, comment: "거래 ID")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    order_no = table.Column<string>(type: "text", nullable: false, comment: "주문 번호"),
                    timestamp = table.Column<long>(type: "bigint", nullable: false, comment: "타임스탬프"),
                    username = table.Column<string>(type: "text", nullable: false, comment: "사용자명"),
                    price = table.Column<decimal>(type: "numeric", nullable: false, comment: "가격"),
                    status = table.Column<string>(type: "text", nullable: false, comment: "상태"),
                    created_by = table.Column<int>(type: "integer", nullable: true, comment: "생성자 ID"),
                    updated_by = table.Column<int>(type: "integer", nullable: true, comment: "수정자 ID"),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "생성 일시"),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "수정 일시"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, comment: "삭제 여부")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_transactions", x => x.id);
                },
                comment: "거래 내역 정보");

            migrationBuilder.CreateTable(
                name: "users",
                schema: "goldb",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false, comment: "사용자 ID")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    username = table.Column<string>(type: "text", nullable: false, comment: "로그인 계정"),
                    password = table.Column<string>(type: "text", nullable: false, comment: "비밀번호"),
                    name = table.Column<string>(type: "text", nullable: false, comment: "사용자 이름"),
                    ssn = table.Column<string>(type: "text", nullable: true, comment: "주민등록번호"),
                    zip_code = table.Column<string>(type: "text", nullable: true, comment: "우편번호"),
                    address_base = table.Column<string>(type: "text", nullable: true, comment: "기본 주소"),
                    address_detail = table.Column<string>(type: "text", nullable: true, comment: "상세 주소"),
                    avatar = table.Column<string>(type: "text", nullable: false, comment: "아바타 이미지 URL"),
                    avatar_id = table.Column<int>(type: "integer", nullable: true, comment: "아바타 첨부파일 ID"),
                    introduction = table.Column<string>(type: "text", nullable: false, comment: "자기 소개"),
                    sms_allowed = table.Column<bool>(type: "boolean", nullable: false, comment: "SMS 수신 동의 여부"),
                    kakao_allowed = table.Column<bool>(type: "boolean", nullable: false, comment: "카카오톡 수신 동의 여부"),
                    email_allowed = table.Column<bool>(type: "boolean", nullable: false, comment: "이메일 수신 동의 여부"),
                    user_type = table.Column<string>(type: "text", nullable: false, comment: "사용자 구분"),
                    reset_token = table.Column<string>(type: "text", nullable: true, comment: "비밀번호 재설정 토큰"),
                    reset_token_expires = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "비밀번호 재설정 토큰 만료 시간"),
                    last_login_ip = table.Column<string>(type: "text", nullable: true, comment: "마지막 로그인 IP"),
                    last_login_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "마지막 로그인 일시"),
                    login_count = table.Column<int>(type: "integer", nullable: false, comment: "로그인 횟수"),
                    created_by = table.Column<int>(type: "integer", nullable: true, comment: "생성자 ID"),
                    updated_by = table.Column<int>(type: "integer", nullable: true, comment: "수정자 ID"),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "생성 일시"),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "수정 일시"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, comment: "삭제 여부")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                },
                comment: "사용자 정보");

            migrationBuilder.CreateTable(
                name: "catalog_pages",
                schema: "goldb",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false, comment: "ID")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    catalog_id = table.Column<int>(type: "integer", nullable: false, comment: "카탈로그 ID"),
                    page_number = table.Column<int>(type: "integer", nullable: false, comment: "페이지 번호"),
                    photo_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false, comment: "페이지 사진"),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false, comment: "페이지 설명"),
                    company_id = table.Column<int>(type: "integer", nullable: true, comment: "제조사 ID"),
                    category_large = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "대분류"),
                    category_medium = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "중분류"),
                    category_small = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "소분류"),
                    created_by = table.Column<int>(type: "integer", nullable: true, comment: "생성자 ID"),
                    updated_by = table.Column<int>(type: "integer", nullable: true, comment: "수정자 ID"),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "생성 일시"),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "수정 일시"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, comment: "삭제 여부")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_catalog_pages", x => x.id);
                    table.ForeignKey(
                        name: "fk_catalog_pages_catalogs_catalog_id",
                        column: x => x.catalog_id,
                        principalSchema: "goldb",
                        principalTable: "catalogs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_catalog_pages_companies_company_id",
                        column: x => x.company_id,
                        principalSchema: "goldb",
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                },
                comment: "카탈로그 페이지 정보");

            migrationBuilder.CreateTable(
                name: "customers",
                schema: "goldb",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false, comment: "고객 ID")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "이름"),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, comment: "연락처"),
                    email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, comment: "이메일"),
                    birth_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "생년월일"),
                    note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true, comment: "비고"),
                    company_id = table.Column<int>(type: "integer", nullable: true, comment: "담당 업체 ID"),
                    created_by = table.Column<int>(type: "integer", nullable: true, comment: "생성자 ID"),
                    updated_by = table.Column<int>(type: "integer", nullable: true, comment: "수정자 ID"),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "생성 일시"),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "수정 일시"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, comment: "삭제 여부")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_customers", x => x.id);
                    table.ForeignKey(
                        name: "fk_customers_companies_company_id",
                        column: x => x.company_id,
                        principalSchema: "goldb",
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "고객 정보");

            migrationBuilder.CreateTable(
                name: "plaster_orders",
                schema: "goldb",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false, comment: "ID")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ordering_company_id = table.Column<int>(type: "integer", nullable: true, comment: "발주사 ID"),
                    manufacturer_id = table.Column<int>(type: "integer", nullable: true, comment: "발주 제조사 ID"),
                    quantity = table.Column<int>(type: "integer", nullable: false, comment: "수량"),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, comment: "상태"),
                    order_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "발주일자"),
                    is_cancelled = table.Column<bool>(type: "boolean", nullable: false, comment: "취소 여부"),
                    remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false, comment: "비고"),
                    created_by = table.Column<int>(type: "integer", nullable: true, comment: "생성자 ID"),
                    updated_by = table.Column<int>(type: "integer", nullable: true, comment: "수정자 ID"),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "생성 일시"),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "수정 일시"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, comment: "삭제 여부")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_plaster_orders", x => x.id);
                    table.ForeignKey(
                        name: "fk_plaster_orders_companies_manufacturer_id",
                        column: x => x.manufacturer_id,
                        principalSchema: "goldb",
                        principalTable: "companies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_plaster_orders_companies_ordering_company_id",
                        column: x => x.ordering_company_id,
                        principalSchema: "goldb",
                        principalTable: "companies",
                        principalColumn: "id");
                },
                comment: "석고 발주 정보");

            migrationBuilder.CreateTable(
                name: "product_sets",
                schema: "goldb",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false, comment: "ID")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "세트 제목"),
                    set_no = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, comment: "세트 번호"),
                    description = table.Column<string>(type: "text", nullable: true, comment: "세트 설명"),
                    photo_url = table.Column<string>(type: "text", nullable: false, comment: "세트 사진"),
                    is_public = table.Column<bool>(type: "boolean", nullable: false, comment: "공개여부"),
                    factory_price = table.Column<decimal>(type: "numeric", nullable: false, comment: "공장도가격"),
                    labor_cost = table.Column<decimal>(type: "numeric", nullable: false, comment: "수공비"),
                    company_id = table.Column<int>(type: "integer", nullable: true, comment: "생산업체 ID"),
                    basic_component_count = table.Column<int>(type: "integer", nullable: false, comment: "기본 구성품 수"),
                    category_large = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "대분류"),
                    category_medium = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "중분류"),
                    category_small = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "소분류"),
                    created_by = table.Column<int>(type: "integer", nullable: true, comment: "생성자 ID"),
                    updated_by = table.Column<int>(type: "integer", nullable: true, comment: "수정자 ID"),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "생성 일시"),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "수정 일시"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, comment: "삭제 여부")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_sets", x => x.id);
                    table.ForeignKey(
                        name: "fk_product_sets_companies_company_id",
                        column: x => x.company_id,
                        principalSchema: "goldb",
                        principalTable: "companies",
                        principalColumn: "id");
                },
                comment: "세트 제품 정보");

            migrationBuilder.CreateTable(
                name: "products",
                schema: "goldb",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false, comment: "ID")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "상품이름"),
                    normalized_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "정규화된 상품이름"),
                    product_no = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, comment: "제품번호"),
                    normalized_product_no = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, comment: "정규화된 제품번호"),
                    purity = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "함량"),
                    weight = table.Column<decimal>(type: "numeric", nullable: false, comment: "중량"),
                    company_id = table.Column<int>(type: "integer", nullable: true, comment: "생산업체 ID"),
                    category_large = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "제품 대분류"),
                    category_medium = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "제품 중분류"),
                    category_small = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "제품 소분류"),
                    factory_price = table.Column<decimal>(type: "numeric", nullable: false, comment: "공장도가격"),
                    design_notice = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true, comment: "디자인고시"),
                    product_size = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, comment: "제품크기"),
                    basic_loss = table.Column<decimal>(type: "numeric", nullable: false, comment: "기본감량"),
                    center_stone = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, comment: "중앙석"),
                    center_stone_shape = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, comment: "중앙석형태"),
                    center_stone_size = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, comment: "중앙석 사이즈"),
                    center_stone_count = table.Column<int>(type: "integer", nullable: false, comment: "중앙석 개수"),
                    side_stone = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, comment: "보조석"),
                    side_stone_shape = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, comment: "보조석 형태"),
                    side_stone_size = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, comment: "보조석 사이즈"),
                    side_stone_count = table.Column<int>(type: "integer", nullable: false, comment: "보조석 개수"),
                    description = table.Column<string>(type: "text", nullable: true, comment: "제품설명"),
                    special_note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true, comment: "특이사항"),
                    is_public = table.Column<bool>(type: "boolean", nullable: false, comment: "공개 여부"),
                    labor_cost = table.Column<decimal>(type: "numeric", nullable: false, comment: "수공비"),
                    colors = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true, comment: "제품 컬러"),
                    sizes = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true, comment: "제품 사이즈"),
                    created_by = table.Column<int>(type: "integer", nullable: true, comment: "생성자 ID"),
                    updated_by = table.Column<int>(type: "integer", nullable: true, comment: "수정자 ID"),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "생성 일시"),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "수정 일시"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, comment: "삭제 여부")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_products", x => x.id);
                    table.ForeignKey(
                        name: "fk_products_companies_company_id",
                        column: x => x.company_id,
                        principalSchema: "goldb",
                        principalTable: "companies",
                        principalColumn: "id");
                },
                comment: "제품 정보");

            migrationBuilder.CreateTable(
                name: "menu_permissions",
                schema: "goldb",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false, comment: "권한 ID")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    menu_id = table.Column<int>(type: "integer", nullable: false, comment: "메뉴 ID"),
                    role_key = table.Column<string>(type: "text", nullable: false, comment: "역할 키"),
                    can_search = table.Column<bool>(type: "boolean", nullable: false, comment: "조회 권한"),
                    can_create = table.Column<bool>(type: "boolean", nullable: false, comment: "추가 권한"),
                    can_delete = table.Column<bool>(type: "boolean", nullable: false, comment: "삭제 권한"),
                    can_save = table.Column<bool>(type: "boolean", nullable: false, comment: "저장 권한"),
                    can_print = table.Column<bool>(type: "boolean", nullable: false, comment: "출력 권한"),
                    custom1 = table.Column<bool>(type: "boolean", nullable: false, comment: "커스텀 권한 1"),
                    custom2 = table.Column<bool>(type: "boolean", nullable: false, comment: "커스텀 권한 2"),
                    custom3 = table.Column<bool>(type: "boolean", nullable: false, comment: "커스텀 권한 3"),
                    custom4 = table.Column<bool>(type: "boolean", nullable: false, comment: "커스텀 권한 4"),
                    custom5 = table.Column<bool>(type: "boolean", nullable: false, comment: "커스텀 권한 5"),
                    custom6 = table.Column<bool>(type: "boolean", nullable: false, comment: "커스텀 권한 6"),
                    custom7 = table.Column<bool>(type: "boolean", nullable: false, comment: "커스텀 권한 7"),
                    custom8 = table.Column<bool>(type: "boolean", nullable: false, comment: "커스텀 권한 8"),
                    created_by = table.Column<int>(type: "integer", nullable: true, comment: "생성자 ID"),
                    updated_by = table.Column<int>(type: "integer", nullable: true, comment: "수정자 ID"),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "생성 일시"),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "수정 일시"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, comment: "삭제 여부")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_menu_permissions", x => x.id);
                    table.ForeignKey(
                        name: "fk_menu_permissions_menus_menu_id",
                        column: x => x.menu_id,
                        principalSchema: "goldb",
                        principalTable: "menus",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "메뉴 권한 정보");

            migrationBuilder.CreateTable(
                name: "menu_favorites",
                schema: "goldb",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false, comment: "ID")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false, comment: "사용자 ID"),
                    menu_id = table.Column<int>(type: "integer", nullable: false, comment: "메뉴 ID"),
                    sort_order = table.Column<int>(type: "integer", nullable: false, comment: "정렬 순서"),
                    created_by = table.Column<int>(type: "integer", nullable: true, comment: "생성자 ID"),
                    updated_by = table.Column<int>(type: "integer", nullable: true, comment: "수정자 ID"),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "생성 일시"),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "수정 일시"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, comment: "삭제 여부")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_menu_favorites", x => x.id);
                    table.ForeignKey(
                        name: "fk_menu_favorites_menus_menu_id",
                        column: x => x.menu_id,
                        principalSchema: "goldb",
                        principalTable: "menus",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_menu_favorites_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "goldb",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "메뉴 즐겨찾기 정보");

            migrationBuilder.CreateTable(
                name: "user_companies",
                schema: "goldb",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "integer", nullable: false, comment: "사용자 ID"),
                    company_id = table.Column<int>(type: "integer", nullable: false, comment: "업체 ID"),
                    created_by = table.Column<int>(type: "integer", nullable: true, comment: "생성자 ID"),
                    updated_by = table.Column<int>(type: "integer", nullable: true, comment: "수정자 ID"),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "생성 일시"),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "수정 일시"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, comment: "삭제 여부")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_companies", x => new { x.user_id, x.company_id });
                    table.ForeignKey(
                        name: "fk_user_companies_companies_company_id",
                        column: x => x.company_id,
                        principalSchema: "goldb",
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_companies_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "goldb",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "사용자-업체 연결");

            migrationBuilder.CreateTable(
                name: "user_emails",
                schema: "goldb",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false, comment: "이메일 ID")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false, comment: "사용자 ID"),
                    email = table.Column<string>(type: "text", nullable: false, comment: "이메일 주소"),
                    is_primary = table.Column<bool>(type: "boolean", nullable: false, comment: "대표 여부"),
                    created_by = table.Column<int>(type: "integer", nullable: true, comment: "생성자 ID"),
                    updated_by = table.Column<int>(type: "integer", nullable: true, comment: "수정자 ID"),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "생성 일시"),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "수정 일시"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, comment: "삭제 여부")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_emails", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_emails_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "goldb",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "사용자 이메일 정보");

            migrationBuilder.CreateTable(
                name: "user_menu_settings",
                schema: "goldb",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    menu_id = table.Column<int>(type: "integer", nullable: false),
                    affix = table.Column<bool>(type: "boolean", nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    created_by = table.Column<int>(type: "integer", nullable: true, comment: "생성자 ID"),
                    updated_by = table.Column<int>(type: "integer", nullable: true, comment: "수정자 ID"),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "생성 일시"),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "수정 일시"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, comment: "삭제 여부")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_menu_settings", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_menu_settings_menus_menu_id",
                        column: x => x.menu_id,
                        principalSchema: "goldb",
                        principalTable: "menus",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_menu_settings_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "goldb",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "사용자별 메뉴 설정");

            migrationBuilder.CreateTable(
                name: "user_personal_settings",
                schema: "goldb",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false, comment: "사용자 ID"),
                    size = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, comment: "글자 크기"),
                    tags_view = table.Column<bool>(type: "boolean", nullable: false, comment: "Tags-View 사용 여부"),
                    fixed_header = table.Column<bool>(type: "boolean", nullable: false, comment: "Header 고정 여부"),
                    sidebar_logo = table.Column<bool>(type: "boolean", nullable: false, comment: "사이드바 로고 보기 여부"),
                    second_menu_popup = table.Column<bool>(type: "boolean", nullable: false, comment: "사이드바 하위 메뉴 팝업 여부"),
                    created_by = table.Column<int>(type: "integer", nullable: true, comment: "생성자 ID"),
                    updated_by = table.Column<int>(type: "integer", nullable: true, comment: "수정자 ID"),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "생성 일시"),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "수정 일시"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, comment: "삭제 여부")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_personal_settings", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_personal_settings_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "goldb",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "사용자 개인 설정");

            migrationBuilder.CreateTable(
                name: "user_phones",
                schema: "goldb",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false, comment: "전화번호 ID")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false, comment: "사용자 ID"),
                    phone_number = table.Column<string>(type: "text", nullable: false, comment: "전화번호"),
                    is_primary = table.Column<bool>(type: "boolean", nullable: false, comment: "대표 여부"),
                    created_by = table.Column<int>(type: "integer", nullable: true, comment: "생성자 ID"),
                    updated_by = table.Column<int>(type: "integer", nullable: true, comment: "수정자 ID"),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "생성 일시"),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "수정 일시"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, comment: "삭제 여부")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_phones", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_phones_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "goldb",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "사용자 전화번호 정보");

            migrationBuilder.CreateTable(
                name: "user_photos",
                schema: "goldb",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false, comment: "사진 ID")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false, comment: "사용자 ID"),
                    photo_url = table.Column<string>(type: "text", nullable: false, comment: "사진 URL"),
                    attachment_id = table.Column<int>(type: "integer", nullable: true, comment: "첨부파일 ID"),
                    sort_order = table.Column<int>(type: "integer", nullable: false, comment: "정렬 순서"),
                    created_by = table.Column<int>(type: "integer", nullable: true, comment: "생성자 ID"),
                    updated_by = table.Column<int>(type: "integer", nullable: true, comment: "수정자 ID"),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "생성 일시"),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "수정 일시"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, comment: "삭제 여부")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_photos", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_photos_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "goldb",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "사용자 사진 정보");

            migrationBuilder.CreateTable(
                name: "user_roles",
                schema: "goldb",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "integer", nullable: false, comment: "사용자 ID"),
                    role_id = table.Column<int>(type: "integer", nullable: false, comment: "역할 ID"),
                    created_by = table.Column<int>(type: "integer", nullable: true, comment: "생성자 ID"),
                    updated_by = table.Column<int>(type: "integer", nullable: true, comment: "수정자 ID"),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "생성 일시"),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "수정 일시"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, comment: "삭제 여부")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_roles", x => new { x.user_id, x.role_id });
                    table.ForeignKey(
                        name: "fk_user_roles_roles_role_id",
                        column: x => x.role_id,
                        principalSchema: "goldb",
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_roles_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "goldb",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "사용자-역할 매핑");

            migrationBuilder.CreateTable(
                name: "orders",
                schema: "goldb",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false, comment: "ID")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    order_no = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "주문번호"),
                    group_order_no = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "그룹주문번호"),
                    user_id = table.Column<int>(type: "integer", nullable: false, comment: "사용자 ID"),
                    customer_id = table.Column<int>(type: "integer", nullable: true, comment: "고객 ID"),
                    total_amount = table.Column<decimal>(type: "numeric", nullable: false, comment: "총 주문 금액"),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "주문 상태"),
                    order_memo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true, comment: "주문 메모"),
                    factory_remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true, comment: "공장 추가 메시지"),
                    logistics_remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true, comment: "물류 추가 메시지"),
                    inspection_remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true, comment: "검수 요청 메시지"),
                    work_order_remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true, comment: "작업서 작성 메시지"),
                    logistics_company_id = table.Column<int>(type: "integer", nullable: true, comment: "물류업체 ID"),
                    cancellation_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true, comment: "취소 사유"),
                    settlement_method = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "정산 방법"),
                    settlement_remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true, comment: "정산확인요청 메모"),
                    settlement_start_memo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true, comment: "정산시작 메모"),
                    settlement_amount = table.Column<decimal>(type: "numeric", nullable: true, comment: "정산 금액"),
                    modification_memo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true, comment: "수정가공 의뢰 메모"),
                    close_memo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true, comment: "검수불가 종결 메모"),
                    delivery_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "납기일"),
                    created_by = table.Column<int>(type: "integer", nullable: true, comment: "생성자 ID"),
                    updated_by = table.Column<int>(type: "integer", nullable: true, comment: "수정자 ID"),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "생성 일시"),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "수정 일시"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, comment: "삭제 여부")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_orders", x => x.id);
                    table.ForeignKey(
                        name: "fk_orders_companies_logistics_company_id",
                        column: x => x.logistics_company_id,
                        principalSchema: "goldb",
                        principalTable: "companies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_orders_customers_customer_id",
                        column: x => x.customer_id,
                        principalSchema: "goldb",
                        principalTable: "customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_orders_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "goldb",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "주문 정보");

            migrationBuilder.CreateTable(
                name: "cart_items",
                schema: "goldb",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false, comment: "ID")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false, comment: "사용자 ID"),
                    product_id = table.Column<int>(type: "integer", nullable: true, comment: "제품 ID"),
                    product_set_id = table.Column<int>(type: "integer", nullable: true, comment: "세트 제품 ID"),
                    quantity = table.Column<int>(type: "integer", nullable: false, comment: "수량"),
                    purity = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true, comment: "함량"),
                    color = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "컬러"),
                    custom_factory_price = table.Column<decimal>(type: "numeric", nullable: true, comment: "사용자 정의 공장도 가격"),
                    custom_labor_cost = table.Column<decimal>(type: "numeric", nullable: true, comment: "사용자 정의 수공비"),
                    created_by = table.Column<int>(type: "integer", nullable: true, comment: "생성자 ID"),
                    updated_by = table.Column<int>(type: "integer", nullable: true, comment: "수정자 ID"),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "생성 일시"),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "수정 일시"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, comment: "삭제 여부")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cart_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_cart_items_product_sets_product_set_id",
                        column: x => x.product_set_id,
                        principalSchema: "goldb",
                        principalTable: "product_sets",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_cart_items_products_product_id",
                        column: x => x.product_id,
                        principalSchema: "goldb",
                        principalTable: "products",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_cart_items_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "goldb",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "장바구니 아이템 정보");

            migrationBuilder.CreateTable(
                name: "catalog_page_products",
                schema: "goldb",
                columns: table => new
                {
                    catalog_page_id = table.Column<int>(type: "integer", nullable: false, comment: "카탈로그 페이지 ID"),
                    product_id = table.Column<int>(type: "integer", nullable: false, comment: "제품 ID"),
                    created_by = table.Column<int>(type: "integer", nullable: true, comment: "생성자 ID"),
                    updated_by = table.Column<int>(type: "integer", nullable: true, comment: "수정자 ID"),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "생성 일시"),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "수정 일시"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, comment: "삭제 여부")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_catalog_page_products", x => new { x.catalog_page_id, x.product_id });
                    table.ForeignKey(
                        name: "fk_catalog_page_products_catalog_pages_catalog_page_id",
                        column: x => x.catalog_page_id,
                        principalSchema: "goldb",
                        principalTable: "catalog_pages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_catalog_page_products_products_product_id",
                        column: x => x.product_id,
                        principalSchema: "goldb",
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "카탈로그 페이지 제품 연결 정보");

            migrationBuilder.CreateTable(
                name: "favorites",
                schema: "goldb",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false, comment: "ID")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false, comment: "사용자 ID"),
                    product_id = table.Column<int>(type: "integer", nullable: true, comment: "제품 ID"),
                    product_set_id = table.Column<int>(type: "integer", nullable: true, comment: "세트 제품 ID"),
                    created_by = table.Column<int>(type: "integer", nullable: true, comment: "생성자 ID"),
                    updated_by = table.Column<int>(type: "integer", nullable: true, comment: "수정자 ID"),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "생성 일시"),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "수정 일시"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, comment: "삭제 여부")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_favorites", x => x.id);
                    table.ForeignKey(
                        name: "fk_favorites_product_sets_product_set_id",
                        column: x => x.product_set_id,
                        principalSchema: "goldb",
                        principalTable: "product_sets",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_favorites_products_product_id",
                        column: x => x.product_id,
                        principalSchema: "goldb",
                        principalTable: "products",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_favorites_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "goldb",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "즐겨찾기 정보");

            migrationBuilder.CreateTable(
                name: "product_option_weights",
                schema: "goldb",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false, comment: "ID")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    product_id = table.Column<int>(type: "integer", nullable: false, comment: "제품 ID"),
                    purity = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "함량"),
                    color = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "컬러"),
                    weight = table.Column<decimal>(type: "numeric", nullable: false, comment: "중량"),
                    created_by = table.Column<int>(type: "integer", nullable: true, comment: "생성자 ID"),
                    updated_by = table.Column<int>(type: "integer", nullable: true, comment: "수정자 ID"),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "생성 일시"),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "수정 일시"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, comment: "삭제 여부")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_option_weights", x => x.id);
                    table.ForeignKey(
                        name: "fk_product_option_weights_products_product_id",
                        column: x => x.product_id,
                        principalSchema: "goldb",
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "제품 옵션 조합별 중량 정보");

            migrationBuilder.CreateTable(
                name: "product_set_items",
                schema: "goldb",
                columns: table => new
                {
                    product_set_id = table.Column<int>(type: "integer", nullable: false, comment: "세트 ID"),
                    product_id = table.Column<int>(type: "integer", nullable: false, comment: "제품 ID"),
                    created_by = table.Column<int>(type: "integer", nullable: true, comment: "생성자 ID"),
                    updated_by = table.Column<int>(type: "integer", nullable: true, comment: "수정자 ID"),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "생성 일시"),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "수정 일시"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, comment: "삭제 여부")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_set_items", x => new { x.product_set_id, x.product_id });
                    table.ForeignKey(
                        name: "fk_product_set_items_product_sets_product_set_id",
                        column: x => x.product_set_id,
                        principalSchema: "goldb",
                        principalTable: "product_sets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_product_set_items_products_product_id",
                        column: x => x.product_id,
                        principalSchema: "goldb",
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "세트 구성 제품 정보");

            migrationBuilder.CreateTable(
                name: "order_items",
                schema: "goldb",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false, comment: "ID")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    order_id = table.Column<int>(type: "integer", nullable: false, comment: "주문 ID"),
                    product_id = table.Column<int>(type: "integer", nullable: true, comment: "제품 ID"),
                    product_set_id = table.Column<int>(type: "integer", nullable: true, comment: "세트 제품 ID"),
                    parent_id = table.Column<int>(type: "integer", nullable: true, comment: "상위 주문 상세 ID"),
                    quantity = table.Column<int>(type: "integer", nullable: false, comment: "수량"),
                    price = table.Column<decimal>(type: "numeric", nullable: false, comment: "가격"),
                    factory_price = table.Column<decimal>(type: "numeric", nullable: false, comment: "공장도 가격"),
                    labor_cost = table.Column<decimal>(type: "numeric", nullable: false, comment: "수공비"),
                    factory_input_material_cost = table.Column<decimal>(type: "numeric", nullable: true, comment: "공장 입력 재료비"),
                    factory_input_labor_cost = table.Column<decimal>(type: "numeric", nullable: true, comment: "공장 입력 수공비"),
                    retailer_confirm_material_cost = table.Column<decimal>(type: "numeric", nullable: true, comment: "소매점 확인용 재료비"),
                    retailer_confirm_labor_cost = table.Column<decimal>(type: "numeric", nullable: true, comment: "소매점 확인용 수공비"),
                    production_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "제작생산일"),
                    order_weight = table.Column<decimal>(type: "numeric", nullable: true, comment: "주문 당시 중량"),
                    actual_weight = table.Column<decimal>(type: "numeric", nullable: true, comment: "실중량"),
                    inspection_memo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true, comment: "검수 메모"),
                    purity = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true, comment: "항량순도"),
                    color = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "컬러"),
                    is_as_order = table.Column<bool>(type: "boolean", nullable: false, comment: "AS 주문 여부"),
                    confirmed_weight = table.Column<decimal>(type: "numeric", nullable: true, comment: "물류 확인 중량"),
                    logistics_memo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true, comment: "물류 검수 메모"),
                    approved_weight = table.Column<decimal>(type: "numeric", nullable: true, comment: "물류 승인 중량"),
                    approved_memo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true, comment: "물류 승인 메모"),
                    requested_weight = table.Column<decimal>(type: "numeric", nullable: true, comment: "공장 의뢰 중량"),
                    requested_memo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true, comment: "공장 의뢰 메모"),
                    settlement_ratio = table.Column<decimal>(type: "numeric", nullable: false, comment: "정산 비율"),
                    settlement_amount = table.Column<decimal>(type: "numeric", nullable: true, comment: "정산 금액"),
                    settlement_memo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true, comment: "정산 메모"),
                    created_by = table.Column<int>(type: "integer", nullable: true, comment: "생성자 ID"),
                    updated_by = table.Column<int>(type: "integer", nullable: true, comment: "수정자 ID"),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "생성 일시"),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "수정 일시"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, comment: "삭제 여부")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_order_items_order_items_parent_id",
                        column: x => x.parent_id,
                        principalSchema: "goldb",
                        principalTable: "order_items",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_order_items_orders_order_id",
                        column: x => x.order_id,
                        principalSchema: "goldb",
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_order_items_product_sets_product_set_id",
                        column: x => x.product_set_id,
                        principalSchema: "goldb",
                        principalTable: "product_sets",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_order_items_products_product_id",
                        column: x => x.product_id,
                        principalSchema: "goldb",
                        principalTable: "products",
                        principalColumn: "id");
                },
                comment: "주문 상세 정보");

            migrationBuilder.CreateTable(
                name: "order_statements",
                schema: "goldb",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    order_id = table.Column<int>(type: "integer", nullable: false),
                    snapshot_data = table.Column<string>(type: "text", nullable: false),
                    pdf_binary = table.Column<byte[]>(type: "bytea", nullable: true),
                    created_by = table.Column<int>(type: "integer", nullable: true, comment: "생성자 ID"),
                    updated_by = table.Column<int>(type: "integer", nullable: true, comment: "수정자 ID"),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "생성 일시"),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "수정 일시"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, comment: "삭제 여부")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order_statements", x => x.id);
                    table.ForeignKey(
                        name: "fk_order_statements_orders_order_id",
                        column: x => x.order_id,
                        principalSchema: "goldb",
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "기본 베이스 모델");

            migrationBuilder.CreateTable(
                name: "order_status_histories",
                schema: "goldb",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false, comment: "ID")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    order_id = table.Column<int>(type: "integer", nullable: false, comment: "주문 ID"),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "변경된 주문 상태"),
                    user_id = table.Column<int>(type: "integer", nullable: true, comment: "처리자 사용자 ID"),
                    remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true, comment: "메모"),
                    created_by = table.Column<int>(type: "integer", nullable: true, comment: "생성자 ID"),
                    updated_by = table.Column<int>(type: "integer", nullable: true, comment: "수정자 ID"),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "생성 일시"),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "수정 일시"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, comment: "삭제 여부")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order_status_histories", x => x.id);
                    table.ForeignKey(
                        name: "fk_order_status_histories_orders_order_id",
                        column: x => x.order_id,
                        principalSchema: "goldb",
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_order_status_histories_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "goldb",
                        principalTable: "users",
                        principalColumn: "id");
                },
                comment: "주문 상태 변경 이력");

            migrationBuilder.CreateTable(
                name: "receivables",
                schema: "goldb",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    order_id = table.Column<int>(type: "integer", nullable: true),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    amount = table.Column<decimal>(type: "numeric", nullable: false),
                    remaining_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    memo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    settlement_method = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    created_by = table.Column<int>(type: "integer", nullable: true, comment: "생성자 ID"),
                    updated_by = table.Column<int>(type: "integer", nullable: true, comment: "수정자 ID"),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "생성 일시"),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "수정 일시"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, comment: "삭제 여부")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_receivables", x => x.id);
                    table.ForeignKey(
                        name: "fk_receivables_orders_order_id",
                        column: x => x.order_id,
                        principalSchema: "goldb",
                        principalTable: "orders",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_receivables_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "goldb",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "미수금 내역");

            migrationBuilder.CreateTable(
                name: "stocks",
                schema: "goldb",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false, comment: "ID")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    product_id = table.Column<int>(type: "integer", nullable: true, comment: "제품 ID"),
                    product_set_id = table.Column<int>(type: "integer", nullable: true, comment: "세트 제품 ID"),
                    parent_stock_id = table.Column<int>(type: "integer", nullable: true, comment: "상위 재고 ID"),
                    company_id = table.Column<int>(type: "integer", nullable: true, comment: "소속 업체 ID"),
                    stock_no = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "재고번호"),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "상태"),
                    purity = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true, comment: "함량"),
                    color = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "컬러"),
                    actual_weight = table.Column<decimal>(type: "numeric", nullable: false, comment: "실중량"),
                    production_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "제작생산일"),
                    renter_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, comment: "대여자명"),
                    rent_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "대여일"),
                    return_due_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "반납예정일"),
                    note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true, comment: "비고"),
                    source_order_id = table.Column<int>(type: "integer", nullable: true, comment: "원천 주문 ID"),
                    source_order_item_id = table.Column<int>(type: "integer", nullable: true, comment: "원천 주문 상세 ID"),
                    retailer_confirm_material_cost = table.Column<decimal>(type: "numeric", nullable: true, comment: "소매점 확인용 재료비"),
                    retailer_confirm_labor_cost = table.Column<decimal>(type: "numeric", nullable: true, comment: "소매점 확인용 수공비"),
                    is_exhausted = table.Column<bool>(type: "boolean", nullable: false, comment: "소진 여부"),
                    exhaustion_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "소진일"),
                    exhaustion_order_id = table.Column<int>(type: "integer", nullable: true, comment: "소진 주문 ID"),
                    exhaustion_order_item_id = table.Column<int>(type: "integer", nullable: true, comment: "소진 주문 상세 ID"),
                    created_by = table.Column<int>(type: "integer", nullable: true, comment: "생성자 ID"),
                    updated_by = table.Column<int>(type: "integer", nullable: true, comment: "수정자 ID"),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "생성 일시"),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "수정 일시"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, comment: "삭제 여부")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_stocks", x => x.id);
                    table.ForeignKey(
                        name: "FK_stocks_order_items_exhaustion_order_item_id",
                        column: x => x.exhaustion_order_item_id,
                        principalSchema: "goldb",
                        principalTable: "order_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_stocks_order_items_source_order_item_id",
                        column: x => x.source_order_item_id,
                        principalSchema: "goldb",
                        principalTable: "order_items",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_stocks_companies_company_id",
                        column: x => x.company_id,
                        principalSchema: "goldb",
                        principalTable: "companies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_stocks_orders_exhaustion_order_id",
                        column: x => x.exhaustion_order_id,
                        principalSchema: "goldb",
                        principalTable: "orders",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_stocks_orders_source_order_id",
                        column: x => x.source_order_id,
                        principalSchema: "goldb",
                        principalTable: "orders",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_stocks_product_sets_product_set_id",
                        column: x => x.product_set_id,
                        principalSchema: "goldb",
                        principalTable: "product_sets",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_stocks_products_product_id",
                        column: x => x.product_id,
                        principalSchema: "goldb",
                        principalTable: "products",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_stocks_stocks_parent_stock_id",
                        column: x => x.parent_stock_id,
                        principalSchema: "goldb",
                        principalTable: "stocks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "재고 정보");

            migrationBuilder.CreateTable(
                name: "attachments",
                schema: "goldb",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false, comment: "첨부파일 ID")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    file_name = table.Column<string>(type: "text", nullable: false, comment: "저장된 파일명"),
                    original_name = table.Column<string>(type: "text", nullable: false, comment: "원본 파일명"),
                    extension = table.Column<string>(type: "text", nullable: false, comment: "파일 확장자"),
                    mime_type = table.Column<string>(type: "text", nullable: false, comment: "MIME 타입"),
                    file_size = table.Column<long>(type: "bigint", nullable: false, comment: "파일 크기"),
                    file_path = table.Column<string>(type: "text", nullable: false, comment: "저장 경로"),
                    sub_directory = table.Column<string>(type: "text", nullable: true, comment: "하위 디렉토리"),
                    is_main = table.Column<bool>(type: "boolean", nullable: false, comment: "대표 사진 여부"),
                    stock_id = table.Column<int>(type: "integer", nullable: true),
                    created_by = table.Column<int>(type: "integer", nullable: true, comment: "생성자 ID"),
                    updated_by = table.Column<int>(type: "integer", nullable: true, comment: "수정자 ID"),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "생성 일시"),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "수정 일시"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, comment: "삭제 여부")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_attachments", x => x.id);
                    table.ForeignKey(
                        name: "fk_attachments_stocks_stock_id",
                        column: x => x.stock_id,
                        principalSchema: "goldb",
                        principalTable: "stocks",
                        principalColumn: "id");
                },
                comment: "첨부 파일 및 미디어 정보");

            migrationBuilder.CreateTable(
                name: "product_photos",
                schema: "goldb",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false, comment: "ID")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    product_id = table.Column<int>(type: "integer", nullable: false, comment: "제품 ID"),
                    photo_url = table.Column<string>(type: "text", nullable: false, comment: "사진 URL"),
                    attachment_id = table.Column<int>(type: "integer", nullable: true, comment: "첨부파일 ID"),
                    AttachmentId1 = table.Column<int>(type: "integer", nullable: true),
                    is_main = table.Column<bool>(type: "boolean", nullable: false, comment: "대표 사진 여부"),
                    sort_order = table.Column<int>(type: "integer", nullable: false, comment: "정렬 순서"),
                    created_by = table.Column<int>(type: "integer", nullable: true, comment: "생성자 ID"),
                    updated_by = table.Column<int>(type: "integer", nullable: true, comment: "수정자 ID"),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "생성 일시"),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "수정 일시"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, comment: "삭제 여부")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_photos", x => x.id);
                    table.ForeignKey(
                        name: "FK_product_photos_attachments_AttachmentId1",
                        column: x => x.AttachmentId1,
                        principalSchema: "goldb",
                        principalTable: "attachments",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_product_photos_attachments_attachment_id",
                        column: x => x.attachment_id,
                        principalSchema: "goldb",
                        principalTable: "attachments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_product_photos_products_product_id",
                        column: x => x.product_id,
                        principalSchema: "goldb",
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "제품 사진 정보");

            migrationBuilder.CreateTable(
                name: "product_set_photos",
                schema: "goldb",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false, comment: "ID")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    product_set_id = table.Column<int>(type: "integer", nullable: false, comment: "세트 제품 ID"),
                    photo_url = table.Column<string>(type: "text", nullable: true, comment: "사진 URL"),
                    attachment_id = table.Column<int>(type: "integer", nullable: true, comment: "첨부파일 ID"),
                    AttachmentId1 = table.Column<int>(type: "integer", nullable: true),
                    is_main = table.Column<bool>(type: "boolean", nullable: false, comment: "대표 사진 여부"),
                    sort_order = table.Column<int>(type: "integer", nullable: false, comment: "정렬 순서"),
                    created_by = table.Column<int>(type: "integer", nullable: true, comment: "생성자 ID"),
                    updated_by = table.Column<int>(type: "integer", nullable: true, comment: "수정자 ID"),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "생성 일시"),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "수정 일시"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, comment: "삭제 여부")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_set_photos", x => x.id);
                    table.ForeignKey(
                        name: "FK_product_set_photos_attachments_AttachmentId1",
                        column: x => x.AttachmentId1,
                        principalSchema: "goldb",
                        principalTable: "attachments",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_product_set_photos_attachments_attachment_id",
                        column: x => x.attachment_id,
                        principalSchema: "goldb",
                        principalTable: "attachments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_product_set_photos_product_sets_product_set_id",
                        column: x => x.product_set_id,
                        principalSchema: "goldb",
                        principalTable: "product_sets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "세트 제품 사진 정보");

            migrationBuilder.CreateIndex(
                name: "ix_attachments_stock_id",
                schema: "goldb",
                table: "attachments",
                column: "stock_id");

            migrationBuilder.CreateIndex(
                name: "ix_cart_items_product_id",
                schema: "goldb",
                table: "cart_items",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_cart_items_product_set_id",
                schema: "goldb",
                table: "cart_items",
                column: "product_set_id");

            migrationBuilder.CreateIndex(
                name: "ix_cart_items_user_id",
                schema: "goldb",
                table: "cart_items",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_catalog_page_products_product_id",
                schema: "goldb",
                table: "catalog_page_products",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_catalog_pages_catalog_id",
                schema: "goldb",
                table: "catalog_pages",
                column: "catalog_id");

            migrationBuilder.CreateIndex(
                name: "ix_catalog_pages_company_id",
                schema: "goldb",
                table: "catalog_pages",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "ix_common_codes_parent_id",
                schema: "goldb",
                table: "common_codes",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "ix_companies_logistics_company_id",
                schema: "goldb",
                table: "companies",
                column: "logistics_company_id");

            migrationBuilder.CreateIndex(
                name: "ix_customers_company_id",
                schema: "goldb",
                table: "customers",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "ix_favorites_product_id",
                schema: "goldb",
                table: "favorites",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_favorites_product_set_id",
                schema: "goldb",
                table: "favorites",
                column: "product_set_id");

            migrationBuilder.CreateIndex(
                name: "ix_favorites_user_id",
                schema: "goldb",
                table: "favorites",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_menu_favorites_menu_id",
                schema: "goldb",
                table: "menu_favorites",
                column: "menu_id");

            migrationBuilder.CreateIndex(
                name: "ix_menu_favorites_user_id",
                schema: "goldb",
                table: "menu_favorites",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_menu_permissions_menu_id",
                schema: "goldb",
                table: "menu_permissions",
                column: "menu_id");

            migrationBuilder.CreateIndex(
                name: "ix_menus_parent_id",
                schema: "goldb",
                table: "menus",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_items_order_id",
                schema: "goldb",
                table: "order_items",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_items_parent_id",
                schema: "goldb",
                table: "order_items",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_items_product_id",
                schema: "goldb",
                table: "order_items",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_items_product_set_id",
                schema: "goldb",
                table: "order_items",
                column: "product_set_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_statements_order_id",
                schema: "goldb",
                table: "order_statements",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_status_histories_order_id",
                schema: "goldb",
                table: "order_status_histories",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_status_histories_user_id",
                schema: "goldb",
                table: "order_status_histories",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_orders_customer_id",
                schema: "goldb",
                table: "orders",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_orders_logistics_company_id",
                schema: "goldb",
                table: "orders",
                column: "logistics_company_id");

            migrationBuilder.CreateIndex(
                name: "ix_orders_user_id",
                schema: "goldb",
                table: "orders",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_plaster_orders_manufacturer_id",
                schema: "goldb",
                table: "plaster_orders",
                column: "manufacturer_id");

            migrationBuilder.CreateIndex(
                name: "ix_plaster_orders_ordering_company_id",
                schema: "goldb",
                table: "plaster_orders",
                column: "ordering_company_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_option_weights_product_id",
                schema: "goldb",
                table: "product_option_weights",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_photos_attachment_id",
                schema: "goldb",
                table: "product_photos",
                column: "attachment_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_photos_AttachmentId1",
                schema: "goldb",
                table: "product_photos",
                column: "AttachmentId1");

            migrationBuilder.CreateIndex(
                name: "ix_product_photos_product_id",
                schema: "goldb",
                table: "product_photos",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_set_items_product_id",
                schema: "goldb",
                table: "product_set_items",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_set_photos_attachment_id",
                schema: "goldb",
                table: "product_set_photos",
                column: "attachment_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_set_photos_AttachmentId1",
                schema: "goldb",
                table: "product_set_photos",
                column: "AttachmentId1");

            migrationBuilder.CreateIndex(
                name: "ix_product_set_photos_product_set_id",
                schema: "goldb",
                table: "product_set_photos",
                column: "product_set_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_sets_company_id",
                schema: "goldb",
                table: "product_sets",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "ix_products_company_id",
                schema: "goldb",
                table: "products",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "ix_receivables_order_id",
                schema: "goldb",
                table: "receivables",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_receivables_user_id",
                schema: "goldb",
                table: "receivables",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_stocks_company_id",
                schema: "goldb",
                table: "stocks",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "ix_stocks_exhaustion_order_id",
                schema: "goldb",
                table: "stocks",
                column: "exhaustion_order_id");

            migrationBuilder.CreateIndex(
                name: "IX_stocks_exhaustion_order_item_id",
                schema: "goldb",
                table: "stocks",
                column: "exhaustion_order_item_id");

            migrationBuilder.CreateIndex(
                name: "ix_stocks_parent_stock_id",
                schema: "goldb",
                table: "stocks",
                column: "parent_stock_id");

            migrationBuilder.CreateIndex(
                name: "ix_stocks_product_id",
                schema: "goldb",
                table: "stocks",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_stocks_product_set_id",
                schema: "goldb",
                table: "stocks",
                column: "product_set_id");

            migrationBuilder.CreateIndex(
                name: "ix_stocks_source_order_id",
                schema: "goldb",
                table: "stocks",
                column: "source_order_id");

            migrationBuilder.CreateIndex(
                name: "IX_stocks_source_order_item_id",
                schema: "goldb",
                table: "stocks",
                column: "source_order_item_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_companies_company_id",
                schema: "goldb",
                table: "user_companies",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_emails_user_id",
                schema: "goldb",
                table: "user_emails",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_menu_settings_menu_id",
                schema: "goldb",
                table: "user_menu_settings",
                column: "menu_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_menu_settings_user_id",
                schema: "goldb",
                table: "user_menu_settings",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_personal_settings_user_id",
                schema: "goldb",
                table: "user_personal_settings",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_phones_user_id",
                schema: "goldb",
                table: "user_phones",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_photos_user_id",
                schema: "goldb",
                table: "user_photos",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_roles_role_id",
                schema: "goldb",
                table: "user_roles",
                column: "role_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "articles",
                schema: "goldb");

            migrationBuilder.DropTable(
                name: "cart_items",
                schema: "goldb");

            migrationBuilder.DropTable(
                name: "catalog_page_products",
                schema: "goldb");

            migrationBuilder.DropTable(
                name: "common_codes",
                schema: "goldb");

            migrationBuilder.DropTable(
                name: "contact_messages",
                schema: "goldb");

            migrationBuilder.DropTable(
                name: "diamond_prices",
                schema: "goldb");

            migrationBuilder.DropTable(
                name: "favorites",
                schema: "goldb");

            migrationBuilder.DropTable(
                name: "gold_prices",
                schema: "goldb");

            migrationBuilder.DropTable(
                name: "menu_favorites",
                schema: "goldb");

            migrationBuilder.DropTable(
                name: "menu_permissions",
                schema: "goldb");

            migrationBuilder.DropTable(
                name: "mv_admin_dashboard_summaries",
                schema: "goldb");

            migrationBuilder.DropTable(
                name: "mv_daily_order_trends",
                schema: "goldb");

            migrationBuilder.DropTable(
                name: "mv_monthly_order_trends",
                schema: "goldb");

            migrationBuilder.DropTable(
                name: "mv_partner_retailer_stats",
                schema: "goldb");

            migrationBuilder.DropTable(
                name: "mv_product_performances",
                schema: "goldb");

            migrationBuilder.DropTable(
                name: "notices",
                schema: "goldb");

            migrationBuilder.DropTable(
                name: "order_statements",
                schema: "goldb");

            migrationBuilder.DropTable(
                name: "order_status_histories",
                schema: "goldb");

            migrationBuilder.DropTable(
                name: "plaster_orders",
                schema: "goldb");

            migrationBuilder.DropTable(
                name: "product_option_weights",
                schema: "goldb");

            migrationBuilder.DropTable(
                name: "product_photos",
                schema: "goldb");

            migrationBuilder.DropTable(
                name: "product_set_items",
                schema: "goldb");

            migrationBuilder.DropTable(
                name: "product_set_photos",
                schema: "goldb");

            migrationBuilder.DropTable(
                name: "receivables",
                schema: "goldb");

            migrationBuilder.DropTable(
                name: "transactions",
                schema: "goldb");

            migrationBuilder.DropTable(
                name: "user_companies",
                schema: "goldb");

            migrationBuilder.DropTable(
                name: "user_emails",
                schema: "goldb");

            migrationBuilder.DropTable(
                name: "user_menu_settings",
                schema: "goldb");

            migrationBuilder.DropTable(
                name: "user_personal_settings",
                schema: "goldb");

            migrationBuilder.DropTable(
                name: "user_phones",
                schema: "goldb");

            migrationBuilder.DropTable(
                name: "user_photos",
                schema: "goldb");

            migrationBuilder.DropTable(
                name: "user_roles",
                schema: "goldb");

            migrationBuilder.DropTable(
                name: "catalog_pages",
                schema: "goldb");

            migrationBuilder.DropTable(
                name: "attachments",
                schema: "goldb");

            migrationBuilder.DropTable(
                name: "menus",
                schema: "goldb");

            migrationBuilder.DropTable(
                name: "roles",
                schema: "goldb");

            migrationBuilder.DropTable(
                name: "catalogs",
                schema: "goldb");

            migrationBuilder.DropTable(
                name: "stocks",
                schema: "goldb");

            migrationBuilder.DropTable(
                name: "order_items",
                schema: "goldb");

            migrationBuilder.DropTable(
                name: "orders",
                schema: "goldb");

            migrationBuilder.DropTable(
                name: "product_sets",
                schema: "goldb");

            migrationBuilder.DropTable(
                name: "products",
                schema: "goldb");

            migrationBuilder.DropTable(
                name: "customers",
                schema: "goldb");

            migrationBuilder.DropTable(
                name: "users",
                schema: "goldb");

            migrationBuilder.DropTable(
                name: "companies",
                schema: "goldb");
        }
    }
}
