--
-- PostgreSQL database dump
--

\restrict CeDkCEsVoWaHSb7bLR89FPVspD5bkgdc92sVJt0u5f4yEH9CZBNKLSYkm2zUdCb

-- Dumped from database version 18.4
-- Dumped by pg_dump version 18.4

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET transaction_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

ALTER TABLE IF EXISTS ONLY public.projects DROP CONSTRAINT IF EXISTS projects_media_id_fkey;
ALTER TABLE IF EXISTS ONLY public.posts DROP CONSTRAINT IF EXISTS posts_thumbnail_media_id_fkey;
ALTER TABLE IF EXISTS ONLY public.posts DROP CONSTRAINT IF EXISTS posts_og_image_media_id_fkey;
ALTER TABLE IF EXISTS ONLY public.posts DROP CONSTRAINT IF EXISTS posts_category_id_fkey;
ALTER TABLE IF EXISTS ONLY public.post_tags DROP CONSTRAINT IF EXISTS post_tags_tag_id_fkey;
ALTER TABLE IF EXISTS ONLY public.post_tags DROP CONSTRAINT IF EXISTS post_tags_post_id_fkey;
ALTER TABLE IF EXISTS ONLY public.post_reviews DROP CONSTRAINT IF EXISTS post_reviews_session_id_fkey;
ALTER TABLE IF EXISTS ONLY public.post_reviews DROP CONSTRAINT IF EXISTS post_reviews_post_id_fkey;
ALTER TABLE IF EXISTS ONLY public.categories DROP CONSTRAINT IF EXISTS categories_parent_id_fkey;
ALTER TABLE IF EXISTS ONLY public.banners DROP CONSTRAINT IF EXISTS banners_media_id_fkey;
DROP TRIGGER IF EXISTS trg_set_category_level ON public.categories;
DROP TRIGGER IF EXISTS trg_posts_touch_updated_at ON public.posts;
DROP TRIGGER IF EXISTS trg_post_reviews_touch_updated_at ON public.post_reviews;
DROP TRIGGER IF EXISTS trg_check_post_category_level ON public.posts;
DROP INDEX IF EXISTS public.uq_slogans_single_active;
DROP INDEX IF EXISTS public.idx_visitor_logs_visited_at;
DROP INDEX IF EXISTS public.idx_subscribers_status;
DROP INDEX IF EXISTS public.idx_review_sessions_expires_at;
DROP INDEX IF EXISTS public.idx_review_sessions_email;
DROP INDEX IF EXISTS public.idx_projects_media_id;
DROP INDEX IF EXISTS public.idx_posts_thumbnail_media_id;
DROP INDEX IF EXISTS public.idx_posts_status;
DROP INDEX IF EXISTS public.idx_posts_published_at;
DROP INDEX IF EXISTS public.idx_posts_og_image_media_id;
DROP INDEX IF EXISTS public.idx_posts_category_id;
DROP INDEX IF EXISTS public.idx_post_tags_tag_id;
DROP INDEX IF EXISTS public.idx_post_reviews_status;
DROP INDEX IF EXISTS public.idx_post_reviews_session_id;
DROP INDEX IF EXISTS public.idx_post_reviews_post_id;
DROP INDEX IF EXISTS public.idx_otp_verifications_ip_created_at;
DROP INDEX IF EXISTS public.idx_otp_verifications_email_created_at;
DROP INDEX IF EXISTS public.idx_cookie_consents_visitor_token;
DROP INDEX IF EXISTS public.idx_categories_parent_id;
DROP INDEX IF EXISTS public.idx_banners_media_id;
ALTER TABLE IF EXISTS ONLY public.visitor_stats_monthly DROP CONSTRAINT IF EXISTS visitor_stats_monthly_pkey;
ALTER TABLE IF EXISTS ONLY public.visitor_stats_monthly DROP CONSTRAINT IF EXISTS visitor_stats_monthly_month_key;
ALTER TABLE IF EXISTS ONLY public.visitor_logs DROP CONSTRAINT IF EXISTS visitor_logs_pkey;
ALTER TABLE IF EXISTS ONLY public.tags DROP CONSTRAINT IF EXISTS tags_slug_key;
ALTER TABLE IF EXISTS ONLY public.tags DROP CONSTRAINT IF EXISTS tags_pkey;
ALTER TABLE IF EXISTS ONLY public.tags DROP CONSTRAINT IF EXISTS tags_name_key;
ALTER TABLE IF EXISTS ONLY public.subscribers DROP CONSTRAINT IF EXISTS subscribers_pkey;
ALTER TABLE IF EXISTS ONLY public.subscribers DROP CONSTRAINT IF EXISTS subscribers_email_key;
ALTER TABLE IF EXISTS ONLY public.smtp_settings DROP CONSTRAINT IF EXISTS smtp_settings_pkey;
ALTER TABLE IF EXISTS ONLY public.slogans DROP CONSTRAINT IF EXISTS slogans_pkey;
ALTER TABLE IF EXISTS ONLY public.seo_settings DROP CONSTRAINT IF EXISTS seo_settings_pkey;
ALTER TABLE IF EXISTS ONLY public.review_sessions DROP CONSTRAINT IF EXISTS review_sessions_pkey;
ALTER TABLE IF EXISTS ONLY public.projects DROP CONSTRAINT IF EXISTS projects_pkey;
ALTER TABLE IF EXISTS ONLY public.posts DROP CONSTRAINT IF EXISTS posts_slug_key;
ALTER TABLE IF EXISTS ONLY public.posts DROP CONSTRAINT IF EXISTS posts_pkey;
ALTER TABLE IF EXISTS ONLY public.post_tags DROP CONSTRAINT IF EXISTS post_tags_pkey;
ALTER TABLE IF EXISTS ONLY public.post_reviews DROP CONSTRAINT IF EXISTS post_reviews_pkey;
ALTER TABLE IF EXISTS ONLY public.otp_verifications DROP CONSTRAINT IF EXISTS otp_verifications_pkey;
ALTER TABLE IF EXISTS ONLY public.media_library DROP CONSTRAINT IF EXISTS media_library_pkey;
ALTER TABLE IF EXISTS ONLY public.cookie_consents DROP CONSTRAINT IF EXISTS cookie_consents_pkey;
ALTER TABLE IF EXISTS ONLY public.contact_info DROP CONSTRAINT IF EXISTS contact_info_pkey;
ALTER TABLE IF EXISTS ONLY public.categories DROP CONSTRAINT IF EXISTS categories_slug_key;
ALTER TABLE IF EXISTS ONLY public.categories DROP CONSTRAINT IF EXISTS categories_pkey;
ALTER TABLE IF EXISTS ONLY public.banners DROP CONSTRAINT IF EXISTS banners_pkey;
ALTER TABLE IF EXISTS ONLY public.ads DROP CONSTRAINT IF EXISTS ads_position_key;
ALTER TABLE IF EXISTS ONLY public.ads DROP CONSTRAINT IF EXISTS ads_pkey;
ALTER TABLE IF EXISTS ONLY public.admin_users DROP CONSTRAINT IF EXISTS admin_users_username_key;
ALTER TABLE IF EXISTS ONLY public.admin_users DROP CONSTRAINT IF EXISTS admin_users_pkey;
ALTER TABLE IF EXISTS ONLY public.admin_users DROP CONSTRAINT IF EXISTS admin_users_email_key;
DROP TABLE IF EXISTS public.visitor_stats_monthly;
DROP TABLE IF EXISTS public.visitor_logs;
DROP TABLE IF EXISTS public.tags;
DROP TABLE IF EXISTS public.subscribers;
DROP TABLE IF EXISTS public.smtp_settings;
DROP TABLE IF EXISTS public.slogans;
DROP TABLE IF EXISTS public.seo_settings;
DROP TABLE IF EXISTS public.review_sessions;
DROP TABLE IF EXISTS public.projects;
DROP TABLE IF EXISTS public.posts;
DROP TABLE IF EXISTS public.post_tags;
DROP TABLE IF EXISTS public.post_reviews;
DROP TABLE IF EXISTS public.otp_verifications;
DROP TABLE IF EXISTS public.media_library;
DROP TABLE IF EXISTS public.cookie_consents;
DROP TABLE IF EXISTS public.contact_info;
DROP TABLE IF EXISTS public.categories;
DROP TABLE IF EXISTS public.banners;
DROP TABLE IF EXISTS public.ads;
DROP TABLE IF EXISTS public.admin_users;
DROP FUNCTION IF EXISTS public.fn_touch_updated_at();
DROP FUNCTION IF EXISTS public.fn_set_category_level();
DROP FUNCTION IF EXISTS public.fn_check_post_category_level();
DROP TYPE IF EXISTS public.subscriber_status;
DROP TYPE IF EXISTS public.review_status;
DROP TYPE IF EXISTS public.post_status;
DROP TYPE IF EXISTS public.otp_purpose;
DROP TYPE IF EXISTS public.media_usage_type;
DROP TYPE IF EXISTS public.cookie_choice;
DROP TYPE IF EXISTS public.banner_type;
DROP TYPE IF EXISTS public.ad_position;
--
-- Name: ad_position; Type: TYPE; Schema: public; Owner: postgres
--

CREATE TYPE public.ad_position AS ENUM (
    'home_mid',
    'post_detail'
);


ALTER TYPE public.ad_position OWNER TO postgres;

--
-- Name: banner_type; Type: TYPE; Schema: public; Owner: postgres
--

CREATE TYPE public.banner_type AS ENUM (
    'banner',
    'slider'
);


ALTER TYPE public.banner_type OWNER TO postgres;

--
-- Name: cookie_choice; Type: TYPE; Schema: public; Owner: postgres
--

CREATE TYPE public.cookie_choice AS ENUM (
    'accepted',
    'declined'
);


ALTER TYPE public.cookie_choice OWNER TO postgres;

--
-- Name: media_usage_type; Type: TYPE; Schema: public; Owner: postgres
--

CREATE TYPE public.media_usage_type AS ENUM (
    'post_thumbnail',
    'post_content',
    'hero_banner',
    'project_image',
    'og_image',
    'other'
);


ALTER TYPE public.media_usage_type OWNER TO postgres;

--
-- Name: otp_purpose; Type: TYPE; Schema: public; Owner: postgres
--

CREATE TYPE public.otp_purpose AS ENUM (
    'review_verification',
    'admin_password_reset'
);


ALTER TYPE public.otp_purpose OWNER TO postgres;

--
-- Name: post_status; Type: TYPE; Schema: public; Owner: postgres
--

CREATE TYPE public.post_status AS ENUM (
    'published',
    'hidden'
);


ALTER TYPE public.post_status OWNER TO postgres;

--
-- Name: review_status; Type: TYPE; Schema: public; Owner: postgres
--

CREATE TYPE public.review_status AS ENUM (
    'visible',
    'hidden',
    'flagged'
);


ALTER TYPE public.review_status OWNER TO postgres;

--
-- Name: subscriber_status; Type: TYPE; Schema: public; Owner: postgres
--

CREATE TYPE public.subscriber_status AS ENUM (
    'pending',
    'active',
    'unsubscribed'
);


ALTER TYPE public.subscriber_status OWNER TO postgres;

--
-- Name: fn_check_post_category_level(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.fn_check_post_category_level() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
DECLARE
  v_level SMALLINT;
BEGIN
  SELECT level INTO v_level FROM categories WHERE id = NEW.category_id;

  IF v_level IS NULL THEN
    RAISE EXCEPTION 'category_id % không tồn tại', NEW.category_id;
  ELSIF v_level != 3 THEN
    RAISE EXCEPTION 'category_id % phải là danh mục cấp 3, hiện đang là cấp %', NEW.category_id, v_level;
  END IF;

  RETURN NEW;
END;
$$;


ALTER FUNCTION public.fn_check_post_category_level() OWNER TO postgres;

--
-- Name: fn_set_category_level(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.fn_set_category_level() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
DECLARE
  v_parent_level SMALLINT;
BEGIN
  IF NEW.parent_id IS NULL THEN
    NEW.level := 1;
  ELSE
    SELECT level INTO v_parent_level FROM categories WHERE id = NEW.parent_id;

    IF v_parent_level IS NULL THEN
      RAISE EXCEPTION 'parent_id % không tồn tại trong categories', NEW.parent_id;
    END IF;

    IF v_parent_level >= 3 THEN
      RAISE EXCEPTION 'Không thể tạo danh mục cấp 4 — parent_id % đã ở cấp %, tối đa chỉ 3 cấp',
        NEW.parent_id, v_parent_level;
    END IF;

    NEW.level := v_parent_level + 1;
  END IF;

  RETURN NEW;
END;
$$;


ALTER FUNCTION public.fn_set_category_level() OWNER TO postgres;

--
-- Name: fn_touch_updated_at(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.fn_touch_updated_at() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
BEGIN
  NEW.updated_at := now();
  RETURN NEW;
END;
$$;


ALTER FUNCTION public.fn_touch_updated_at() OWNER TO postgres;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: admin_users; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.admin_users (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    username character varying NOT NULL,
    password_hash character varying NOT NULL,
    email character varying NOT NULL,
    last_login_at timestamp without time zone,
    created_at timestamp without time zone DEFAULT now()
);


ALTER TABLE public.admin_users OWNER TO postgres;

--
-- Name: TABLE admin_users; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON TABLE public.admin_users IS 'Chỉ 1 dòng theo yêu cầu "1 admin duy nhất" — không ép bằng DB, chỉ vì UI không có màn hình tạo thêm admin. Quên mật khẩu tra otp_verifications theo email + purpose=admin_password_reset.';


--
-- Name: ads; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.ads (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    "position" public.ad_position NOT NULL,
    adsense_slot_id character varying NOT NULL,
    start_date date,
    end_date date,
    is_active boolean DEFAULT true
);


ALTER TABLE public.ads OWNER TO postgres;

--
-- Name: TABLE ads; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON TABLE public.ads IS 'Dùng Google AdSense thật — adsense_slot_id là ad unit Google cấp, không tự upload ảnh quảng cáo.';


--
-- Name: banners; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.banners (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    media_id uuid NOT NULL,
    type public.banner_type NOT NULL,
    link_url character varying,
    sort_order integer DEFAULT 0,
    is_active boolean DEFAULT true,
    created_at timestamp without time zone DEFAULT now()
);


ALTER TABLE public.banners OWNER TO postgres;

--
-- Name: TABLE banners; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON TABLE public.banners IS 'type phân biệt banner trang chủ (không link, tối đa 3 ảnh) và slider (có thể có link).';


--
-- Name: categories; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.categories (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    name character varying NOT NULL,
    slug character varying NOT NULL,
    parent_id uuid,
    level smallint NOT NULL,
    sort_order integer DEFAULT 0,
    created_at timestamp without time zone DEFAULT now(),
    CONSTRAINT categories_level_check CHECK (((level >= 1) AND (level <= 3)))
);


ALTER TABLE public.categories OWNER TO postgres;

--
-- Name: TABLE categories; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON TABLE public.categories IS 'Cây 3 cấp qua parent_id (self-reference). level tự tính bằng trigger, không nhập tay.';


--
-- Name: contact_info; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.contact_info (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    email character varying NOT NULL,
    location character varying,
    social_links jsonb,
    updated_at timestamp without time zone DEFAULT now()
);


ALTER TABLE public.contact_info OWNER TO postgres;

--
-- Name: TABLE contact_info; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON TABLE public.contact_info IS 'Bảng singleton — quy ước chỉ 1 dòng tồn tại, ràng buộc ở tầng ứng dụng.';


--
-- Name: cookie_consents; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.cookie_consents (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    visitor_token character varying NOT NULL,
    choice public.cookie_choice NOT NULL,
    consented_at timestamp without time zone DEFAULT now()
);


ALTER TABLE public.cookie_consents OWNER TO postgres;

--
-- Name: TABLE cookie_consents; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON TABLE public.cookie_consents IS 'Chỉ ghi dòng khi: (1) bấm Đồng ý lần đầu, hoặc (2) chủ động thu hồi sau này qua trang cài đặt. Bấm Từ chối lần đầu KHÔNG tạo dòng nào — không có gì cần chứng minh. Không unique trên visitor_token — nhiều dòng theo thời gian nếu đổi ý, lấy consented_at mới nhất để biết hiện tại.';


--
-- Name: media_library; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.media_library (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    file_name character varying NOT NULL,
    url character varying NOT NULL,
    width integer NOT NULL,
    height integer NOT NULL,
    file_size_bytes bigint,
    alt_text character varying,
    usage_type public.media_usage_type NOT NULL,
    uploaded_at timestamp without time zone DEFAULT now()
);


ALTER TABLE public.media_library OWNER TO postgres;

--
-- Name: TABLE media_library; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON TABLE public.media_library IS 'Quản lý ảnh tập trung — width/height/usage_type bắt buộc để đối chiếu kích thước chuẩn khi upload.';


--
-- Name: otp_verifications; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.otp_verifications (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    email character varying NOT NULL,
    otp_code_hash character varying NOT NULL,
    purpose public.otp_purpose NOT NULL,
    ip_address character varying,
    is_used boolean DEFAULT false,
    expires_at timestamp without time zone NOT NULL,
    created_at timestamp without time zone DEFAULT now(),
    CONSTRAINT chk_otp_ip_required_for_review CHECK ((((purpose = 'review_verification'::public.otp_purpose) AND (ip_address IS NOT NULL)) OR (purpose = 'admin_password_reset'::public.otp_purpose)))
);


ALTER TABLE public.otp_verifications OWNER TO postgres;

--
-- Name: TABLE otp_verifications; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON TABLE public.otp_verifications IS 'Không FK sang bảng khác — khớp theo email ở tầng ứng dụng. Đếm số dòng theo email/ip trong 24h gần nhất để áp giới hạn 5 lần/email/24h.';


--
-- Name: post_reviews; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.post_reviews (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    post_id uuid NOT NULL,
    session_id uuid,
    email character varying NOT NULL,
    rating integer NOT NULL,
    comment text,
    status public.review_status DEFAULT 'visible'::public.review_status NOT NULL,
    created_at timestamp without time zone DEFAULT now(),
    updated_at timestamp without time zone DEFAULT now(),
    CONSTRAINT post_reviews_rating_check CHECK (((rating >= 1) AND (rating <= 5)))
);


ALTER TABLE public.post_reviews OWNER TO postgres;

--
-- Name: TABLE post_reviews; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON TABLE public.post_reviews IS 'email không hiển thị công khai, chỉ dùng nội bộ xác thực chủ sở hữu khi hết session.';


--
-- Name: post_tags; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.post_tags (
    post_id uuid NOT NULL,
    tag_id uuid NOT NULL
);


ALTER TABLE public.post_tags OWNER TO postgres;

--
-- Name: TABLE post_tags; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON TABLE public.post_tags IS 'CASCADE cả 2 chiều — xoá post hoặc tag chỉ xoá dòng liên kết, không đụng bảng còn lại.';


--
-- Name: posts; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.posts (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    category_id uuid NOT NULL,
    thumbnail_media_id uuid NOT NULL,
    og_image_media_id uuid,
    title character varying NOT NULL,
    slug character varying NOT NULL,
    excerpt text,
    content_html text,
    status public.post_status DEFAULT 'published'::public.post_status NOT NULL,
    view_count integer DEFAULT 0,
    avg_rating numeric(2,1) DEFAULT 0,
    rating_count integer DEFAULT 0,
    reading_time_minutes integer,
    meta_title character varying,
    meta_description character varying,
    published_at timestamp without time zone DEFAULT now() NOT NULL,
    updated_at timestamp without time zone DEFAULT now()
);


ALTER TABLE public.posts OWNER TO postgres;

--
-- Name: TABLE posts; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON TABLE public.posts IS 'category_id bắt buộc là danh mục cấp 3 — ép bằng trigger (xem trg_check_post_category_level) vì Postgres CHECK không tham chiếu được sang bảng khác. Không có draft — chỉ published/hidden.';


--
-- Name: projects; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.projects (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    media_id uuid NOT NULL,
    title character varying NOT NULL,
    short_description text,
    tag character varying,
    sort_order integer DEFAULT 0
);


ALTER TABLE public.projects OWNER TO postgres;

--
-- Name: review_sessions; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.review_sessions (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    email character varying NOT NULL,
    created_at timestamp without time zone DEFAULT now(),
    expires_at timestamp without time zone NOT NULL
);


ALTER TABLE public.review_sessions OWNER TO postgres;

--
-- Name: TABLE review_sessions; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON TABLE public.review_sessions IS 'Sống 24h sau xác thực OTP. Cron job xoá vật lý dòng hết hạn mỗi đêm — vì vậy post_reviews.session_id phải là ON DELETE SET NULL (không phải CASCADE/RESTRICT).';


--
-- Name: seo_settings; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.seo_settings (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    default_meta_title character varying,
    default_meta_description character varying,
    robots_txt_content text,
    sitemap_generated_at timestamp without time zone
);


ALTER TABLE public.seo_settings OWNER TO postgres;

--
-- Name: TABLE seo_settings; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON TABLE public.seo_settings IS 'Cấu hình SEO toàn site (fallback khi 1 trang không tự đặt SEO riêng). sitemap.xml sinh tự động mỗi khi có bài viết mới/sửa/ẩn/xoá — sitemap_generated_at tự cập nhật.';


--
-- Name: slogans; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.slogans (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    content character varying NOT NULL,
    is_active boolean DEFAULT false,
    created_at timestamp without time zone DEFAULT now()
);


ALTER TABLE public.slogans OWNER TO postgres;

--
-- Name: TABLE slogans; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON TABLE public.slogans IS 'Chỉ tối đa 1 dòng is_active=true — ép bằng partial unique index uq_slogans_single_active.';


--
-- Name: smtp_settings; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.smtp_settings (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    host character varying,
    port integer,
    username character varying,
    password_encrypted character varying,
    sender_email character varying,
    sender_name character varying,
    daily_limit integer DEFAULT 50,
    emails_sent_today integer DEFAULT 0,
    last_reset_at timestamp without time zone
);


ALTER TABLE public.smtp_settings OWNER TO postgres;

--
-- Name: TABLE smtp_settings; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON TABLE public.smtp_settings IS 'Bảng singleton. password_encrypted dùng MÃ HOÁ 2 CHIỀU (khác hash ở admin_users/otp_verifications) vì cần lấy lại mật khẩu gốc để đăng nhập SMTP. Khoá mã hoá lưu ở biến môi trường, KHÔNG lưu trong DB.';


--
-- Name: subscribers; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.subscribers (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    email character varying NOT NULL,
    status public.subscriber_status DEFAULT 'pending'::public.subscriber_status NOT NULL,
    confirm_token character varying,
    confirm_token_expires_at timestamp without time zone,
    confirmed_at timestamp without time zone,
    unsubscribed_at timestamp without time zone,
    created_at timestamp without time zone DEFAULT now()
);


ALTER TABLE public.subscribers OWNER TO postgres;

--
-- Name: TABLE subscribers; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON TABLE public.subscribers IS 'Double opt-in: pending -> active (bấm link, có hạn 7 ngày qua confirm_token_expires_at) -> unsubscribed. Đăng ký lại email đang pending -> UPDATE token cũ, không tạo dòng mới.';


--
-- Name: tags; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.tags (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    name character varying NOT NULL,
    slug character varying NOT NULL
);


ALTER TABLE public.tags OWNER TO postgres;

--
-- Name: visitor_logs; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.visitor_logs (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    ip_address character varying,
    user_agent character varying,
    device_type character varying,
    country character varying,
    city character varying,
    page_url character varying,
    referrer character varying,
    visited_at timestamp without time zone DEFAULT now()
);


ALTER TABLE public.visitor_logs OWNER TO postgres;

--
-- Name: TABLE visitor_logs; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON TABLE public.visitor_logs IS 'Chỉ insert khi client đồng ý cookie (kiểm tra ở app, không FK ràng buộc). Giữ 30 ngày — cron xoá dòng cũ hơn, PHẢI chạy SAU job tổng hợp visitor_stats_monthly.';


--
-- Name: visitor_stats_monthly; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.visitor_stats_monthly (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    month date NOT NULL,
    total_visits integer NOT NULL,
    unique_visitors integer NOT NULL,
    created_at timestamp without time zone DEFAULT now()
);


ALTER TABLE public.visitor_stats_monthly OWNER TO postgres;

--
-- Name: TABLE visitor_stats_monthly; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON TABLE public.visitor_stats_monthly IS 'month luôn là ngày 01 của tháng đó (quy ước, VD 2026-08-01 = "tháng 8/2026"). Cron tổng hợp tháng cũ TRƯỚC khi xoá visitor_logs quá 30 ngày — thứ tự bắt buộc.';


--
-- Data for Name: admin_users; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.admin_users (id, username, password_hash, email, last_login_at, created_at) FROM stdin;
18554769-c353-4fbd-ac6f-ab332da02939	admin	$2a$11$M4wyEQATjexSEMhLB0GikujGzPSeubF5fN6s6yECEUT0/35R9PPvW	admin@example.com	2026-10-06 11:50:48.943497	2026-09-08 18:17:39.716622
\.


--
-- Data for Name: ads; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.ads (id, "position", adsense_slot_id, start_date, end_date, is_active) FROM stdin;
f3687cfd-662b-4a79-8ad0-031ae3bd4892	home_mid	12345	\N	\N	t
81e8ffdb-c695-4ab4-a2b2-b94c6d67e666	post_detail	1234567890123901321	\N	\N	t
\.


--
-- Data for Name: banners; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.banners (id, media_id, type, link_url, sort_order, is_active, created_at) FROM stdin;
912fffcd-1236-4bd1-bb45-b83c5d0561aa	3e864ee8-0c75-4318-bccc-17979f2fdd73	banner	https://www.youtube.com/shorts/gQFUwF0Sp5s	1	t	2026-09-29 19:06:54.9923
22d55e12-8381-4170-b6a8-926de1c56582	6349f36b-6527-46a7-bdb4-a8d5aef49da4	banner	https://www.youtube.com/shorts/gQFUwF0Sp5s	1	t	2026-09-29 19:07:06.623128
03512f12-85b9-4b11-b2f9-63e7a05d55f6	4c905a11-c74c-47ce-b516-7b5da6efb10f	banner	https://www.youtube.com/shorts/gQFUwF0Sp5s	1	t	2026-09-29 19:06:39.599666
\.


--
-- Data for Name: categories; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.categories (id, name, slug, parent_id, level, sort_order, created_at) FROM stdin;
03d0085e-b601-434b-81fa-962658ef6564	test1	test1	86fd1f00-155a-40ab-9383-87f208e738a3	2	1	2026-09-23 09:50:31.951049
9b3f8bfd-d0b9-42f9-b321-6df5f0297bcc	test2	test2	\N	1	1	2026-10-06 10:38:01.411764
86fd1f00-155a-40ab-9383-87f208e738a3	test	test	\N	1	1	2026-09-22 18:22:54.510972
f1a2b3c4-d5e6-7890-abcd-ef1234567890	Lap trinh C#	lap-trinh-csharp	03d0085e-b601-434b-81fa-962658ef6564	3	1	2026-09-25 07:51:54.519688
\.


--
-- Data for Name: contact_info; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.contact_info (id, email, location, social_links, updated_at) FROM stdin;
62d59884-3a3e-48d5-9a5e-b51fcae7bc4b	quockhai2904@gmail.com	499/23 Quang Trung, Go Vap, Ho Chi Minh	{"bio": "Developer & Blogger", "about": "Welcome to my personal blog", "links": [{"url": "https://www.facebook.com/josnguyen0294/", "icon": null, "platform": "Facebook"}, {"url": "https://github.com/GiGaChadl", "icon": null, "platform": "GitHub"}, {"url": "https://www.linkedin.com/in/josnguyen294/", "icon": null, "platform": "LinkedIn"}], "phone": "0987654321", "slogan": "Ghi lai nhung gi that", "siteName": "Nhật Ký Của Tôi", "copyright": "2026 My Blog. All rights reserved.", "authorName": "Jos Nguyen", "faviconUrl": "/uploads/e18b2ae562314eb080c131e9f34bdcde.jpg", "siteLogoUrl": "/uploads/85d414a4019c45a1b1f8189eea498813.jpg", "defaultOgImage": "/uploads/f9fdec05dda0488a81c2a5e11ca8c278.png", "defaultKeywords": "test", "googleSiteVerification": ""}	2026-09-30 18:03:29.560175
\.


--
-- Data for Name: cookie_consents; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.cookie_consents (id, visitor_token, choice, consented_at) FROM stdin;
7aa2fc67-6e35-487e-9b21-7f0e9a858f92	v_6prncz9sxet_mumjd07g	accepted	2026-09-29 17:30:55.623009
\.


--
-- Data for Name: media_library; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.media_library (id, file_name, url, width, height, file_size_bytes, alt_text, usage_type, uploaded_at) FROM stdin;
e2b3c4d5-e6f7-8901-abcd-ef2345678901	sample-thumbnail.jpg	https://picsum.photos/seed/blog/800/450	800	450	102400	Anh bia mau	post_thumbnail	2026-09-25 07:52:17.018864
67653d18-1d1b-4ccb-b9de-9bc2ecfcd761	ChatGPT Image 08_46_21 25 thg 9, 2026.png	/uploads/59e44d920011404593cd59afcb4efaaa.png	1359	1158	2034113	\N	post_thumbnail	2026-09-29 10:18:23.865651
121a6499-6bee-41d7-88a8-d815f7da1b65	ChatGPT Image 08_46_21 25 thg 9, 2026.png	/uploads/89bf2307ecfe47aabcc3df7bbb70406f.png	1359	1158	2034113	\N	post_thumbnail	2026-09-29 11:19:02.149568
3e864ee8-0c75-4318-bccc-17979f2fdd73	779261997_27932094556430350_7413348939150634134_n.jpg	/uploads/2cb04394ace546cdba2410cc07b9d4db.jpg	1280	1707	266874	test	post_thumbnail	2026-09-29 19:06:51.916689
e36ac880-d68f-45fd-8164-b6f5d4fbeabf	779261997_27932094556430350_7413348939150634134_n.jpg	/uploads/0192085db05445338f4871d50d3675e7.jpg	1280	1707	266874	\N	post_thumbnail	2026-09-29 19:06:35.474209
6349f36b-6527-46a7-bdb4-a8d5aef49da4	f96570cf57bbd7e58eaa.jpg	/uploads/d2676fa269254c83b5a582cfbf9a013d.jpg	1920	2560	1360634	\N	post_thumbnail	2026-10-06 10:38:42.924317
e07e8881-0f6c-4738-b058-fa0e2fcb159e	f96570cf57bbd7e58eaa.jpg	/uploads/3bfe01fc043a481495f411f641d06180.jpg	1920	2560	1360634	\N	post_thumbnail	2026-10-06 10:40:12.709415
4c905a11-c74c-47ce-b516-7b5da6efb10f	Scunchie hồng.jpg	/uploads/8c941871e495401fa950d49d70162412.jpg	1280	1280	94609	\N	post_thumbnail	2026-10-06 12:33:49.841146
e585757a-a51f-4a59-bce3-a4dc181578cb	1wupuknkemoqcrpljvw9embsjuyhxlufonntof93ve6pfnu0nbhrzegp8jlgdgkjkxm1.jpg	/uploads/cd312a4df2de412fb3ab2374d8dcf56d.jpg	1920	2560	927991	\N	post_thumbnail	2026-10-06 12:34:15.09614
\.


--
-- Data for Name: otp_verifications; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.otp_verifications (id, email, otp_code_hash, purpose, ip_address, is_used, expires_at, created_at) FROM stdin;
\.


--
-- Data for Name: post_reviews; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.post_reviews (id, post_id, session_id, email, rating, comment, status, created_at, updated_at) FROM stdin;
805bcfc5-68f5-45bd-939a-2aaa29dd6014	784e9bc6-7fe6-4c6a-8dec-a2ec8c53944e	\N	josnguyen2904@gmail.com	5	tuyệt	visible	2026-09-30 10:21:00.910847	2026-10-06 10:36:45.282568
5a5615e0-c31e-4cbf-851a-2035b3e26d94	784e9bc6-7fe6-4c6a-8dec-a2ec8c53944e	\N	quockhai2904@gmail.com	5	tuyệt vời cmnl	visible	2026-09-29 13:41:56.143589	2026-10-06 10:36:45.994557
\.


--
-- Data for Name: post_tags; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.post_tags (post_id, tag_id) FROM stdin;
784e9bc6-7fe6-4c6a-8dec-a2ec8c53944e	608bed38-fae9-47d7-acab-08ffa7f8a238
\.


--
-- Data for Name: posts; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.posts (id, category_id, thumbnail_media_id, og_image_media_id, title, slug, excerpt, content_html, status, view_count, avg_rating, rating_count, reading_time_minutes, meta_title, meta_description, published_at, updated_at) FROM stdin;
784e9bc6-7fe6-4c6a-8dec-a2ec8c53944e	f1a2b3c4-d5e6-7890-abcd-ef1234567890	6349f36b-6527-46a7-bdb4-a8d5aef49da4	121a6499-6bee-41d7-88a8-d815f7da1b65	có bài mới	co-bai-moi	có bài mới	<p>có bài mới</p><img src="/uploads/59e44d920011404593cd59afcb4efaaa.png" alt="ChatGPT Image 08_46_21 25 thg 9, 2026.png" containerstyle="" wrapperstyle="display: flex; margin: 0;"><p></p>	published	31	5.0	2	1	có bài mới	có bài mới	2026-09-29 12:01:14.947666	2026-10-06 12:33:32.759566
6a0f26fc-3d7d-4c66-828f-7219eb42574a	f1a2b3c4-d5e6-7890-abcd-ef1234567890	e2b3c4d5-e6f7-8901-abcd-ef2345678901	\N	Bài Viết Hướng Dẫn C# Mới	bai-viet-huong-dan-c-sharp-moi	Tóm tắt bài viết hướng dẫn C#	Nội dung chi tiết bài viết hướng dẫn C# rất hay.	published	23	0.0	0	1	\N	\N	2026-09-25 08:03:43.335752	2026-09-29 18:15:17.20615
\.


--
-- Data for Name: projects; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.projects (id, media_id, title, short_description, tag, sort_order) FROM stdin;
98f06705-78aa-4122-9840-91ebf848a3a1	e07e8881-0f6c-4738-b058-fa0e2fcb159e	test2	test	test	3
73c8a72f-852a-4d12-81ee-9d7063e22d36	e585757a-a51f-4a59-bce3-a4dc181578cb	test	test	test	2
\.


--
-- Data for Name: review_sessions; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.review_sessions (id, email, created_at, expires_at) FROM stdin;
\.


--
-- Data for Name: seo_settings; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.seo_settings (id, default_meta_title, default_meta_description, robots_txt_content, sitemap_generated_at) FROM stdin;
f6842d4c-a978-4f20-a84c-a30c0942d15d	JosNguyen — Chia sẻ kiến thức lập trình & công nghệ	test 1	test	\N
\.


--
-- Data for Name: slogans; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.slogans (id, content, is_active, created_at) FROM stdin;
0c5010c4-d495-4f79-a99c-ae98390d3b2f	hello	t	2026-09-23 14:18:11.13866
\.


--
-- Data for Name: smtp_settings; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.smtp_settings (id, host, port, username, password_encrypted, sender_email, sender_name, daily_limit, emails_sent_today, last_reset_at) FROM stdin;
8bc17a95-3c54-41d8-b74d-0353bf086d92	smtp.gmail.com	465	quockhai2904@gmail.com	z/8SFxe3B6nGpUmwBXPty4cNACeo6SDEnKeikRgyLjypxQrNoFkcrH2E3Xq/CZSN	quockhai2904@gmail.com	Nhật ký của tôi	50	0	2026-10-06 09:05:01.608951
\.


--
-- Data for Name: subscribers; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.subscribers (id, email, status, confirm_token, confirm_token_expires_at, confirmed_at, unsubscribed_at, created_at) FROM stdin;
a7de8272-497c-406e-819e-3c9bb1f120ed	bolaxem5t6@gmail.com	active	\N	\N	2026-09-29 12:32:07.58177	\N	2026-09-29 12:20:46.089177
37e56fb9-6ce4-4559-9067-2047a5d09c56	khai.nq1584@aptechlearning.edu.vn	active	\N	\N	2026-09-29 13:02:42.018108	\N	2026-09-29 13:01:31.445116
71a906be-7093-4993-9edb-baa6ae8f734c	josnguyen2904@gmail.com	unsubscribed	\N	\N	2026-09-29 12:00:02.547678	2026-09-29 18:16:18.515279	2026-09-29 09:04:14.890368
\.


--
-- Data for Name: tags; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.tags (id, name, slug) FROM stdin;
608bed38-fae9-47d7-acab-08ffa7f8a238	ASP.NET Core	aspnet-core
cea5ac90-89b4-410b-8772-7162144edf48	test	test
\.


--
-- Data for Name: visitor_logs; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.visitor_logs (id, ip_address, user_agent, device_type, country, city, page_url, referrer, visited_at) FROM stdin;
\.


--
-- Data for Name: visitor_stats_monthly; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.visitor_stats_monthly (id, month, total_visits, unique_visitors, created_at) FROM stdin;
\.


--
-- Name: admin_users admin_users_email_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.admin_users
    ADD CONSTRAINT admin_users_email_key UNIQUE (email);


--
-- Name: admin_users admin_users_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.admin_users
    ADD CONSTRAINT admin_users_pkey PRIMARY KEY (id);


--
-- Name: admin_users admin_users_username_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.admin_users
    ADD CONSTRAINT admin_users_username_key UNIQUE (username);


--
-- Name: ads ads_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ads
    ADD CONSTRAINT ads_pkey PRIMARY KEY (id);


--
-- Name: ads ads_position_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ads
    ADD CONSTRAINT ads_position_key UNIQUE ("position");


--
-- Name: banners banners_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.banners
    ADD CONSTRAINT banners_pkey PRIMARY KEY (id);


--
-- Name: categories categories_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.categories
    ADD CONSTRAINT categories_pkey PRIMARY KEY (id);


--
-- Name: categories categories_slug_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.categories
    ADD CONSTRAINT categories_slug_key UNIQUE (slug);


--
-- Name: contact_info contact_info_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.contact_info
    ADD CONSTRAINT contact_info_pkey PRIMARY KEY (id);


--
-- Name: cookie_consents cookie_consents_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cookie_consents
    ADD CONSTRAINT cookie_consents_pkey PRIMARY KEY (id);


--
-- Name: media_library media_library_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.media_library
    ADD CONSTRAINT media_library_pkey PRIMARY KEY (id);


--
-- Name: otp_verifications otp_verifications_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.otp_verifications
    ADD CONSTRAINT otp_verifications_pkey PRIMARY KEY (id);


--
-- Name: post_reviews post_reviews_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.post_reviews
    ADD CONSTRAINT post_reviews_pkey PRIMARY KEY (id);


--
-- Name: post_tags post_tags_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.post_tags
    ADD CONSTRAINT post_tags_pkey PRIMARY KEY (post_id, tag_id);


--
-- Name: posts posts_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.posts
    ADD CONSTRAINT posts_pkey PRIMARY KEY (id);


--
-- Name: posts posts_slug_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.posts
    ADD CONSTRAINT posts_slug_key UNIQUE (slug);


--
-- Name: projects projects_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.projects
    ADD CONSTRAINT projects_pkey PRIMARY KEY (id);


--
-- Name: review_sessions review_sessions_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.review_sessions
    ADD CONSTRAINT review_sessions_pkey PRIMARY KEY (id);


--
-- Name: seo_settings seo_settings_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.seo_settings
    ADD CONSTRAINT seo_settings_pkey PRIMARY KEY (id);


--
-- Name: slogans slogans_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.slogans
    ADD CONSTRAINT slogans_pkey PRIMARY KEY (id);


--
-- Name: smtp_settings smtp_settings_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.smtp_settings
    ADD CONSTRAINT smtp_settings_pkey PRIMARY KEY (id);


--
-- Name: subscribers subscribers_email_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.subscribers
    ADD CONSTRAINT subscribers_email_key UNIQUE (email);


--
-- Name: subscribers subscribers_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.subscribers
    ADD CONSTRAINT subscribers_pkey PRIMARY KEY (id);


--
-- Name: tags tags_name_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.tags
    ADD CONSTRAINT tags_name_key UNIQUE (name);


--
-- Name: tags tags_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.tags
    ADD CONSTRAINT tags_pkey PRIMARY KEY (id);


--
-- Name: tags tags_slug_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.tags
    ADD CONSTRAINT tags_slug_key UNIQUE (slug);


--
-- Name: visitor_logs visitor_logs_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.visitor_logs
    ADD CONSTRAINT visitor_logs_pkey PRIMARY KEY (id);


--
-- Name: visitor_stats_monthly visitor_stats_monthly_month_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.visitor_stats_monthly
    ADD CONSTRAINT visitor_stats_monthly_month_key UNIQUE (month);


--
-- Name: visitor_stats_monthly visitor_stats_monthly_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.visitor_stats_monthly
    ADD CONSTRAINT visitor_stats_monthly_pkey PRIMARY KEY (id);


--
-- Name: idx_banners_media_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_banners_media_id ON public.banners USING btree (media_id);


--
-- Name: idx_categories_parent_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_categories_parent_id ON public.categories USING btree (parent_id);


--
-- Name: idx_cookie_consents_visitor_token; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_cookie_consents_visitor_token ON public.cookie_consents USING btree (visitor_token);


--
-- Name: idx_otp_verifications_email_created_at; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_otp_verifications_email_created_at ON public.otp_verifications USING btree (email, created_at DESC);


--
-- Name: idx_otp_verifications_ip_created_at; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_otp_verifications_ip_created_at ON public.otp_verifications USING btree (ip_address, created_at DESC);


--
-- Name: idx_post_reviews_post_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_post_reviews_post_id ON public.post_reviews USING btree (post_id);


--
-- Name: idx_post_reviews_session_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_post_reviews_session_id ON public.post_reviews USING btree (session_id);


--
-- Name: idx_post_reviews_status; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_post_reviews_status ON public.post_reviews USING btree (status);


--
-- Name: idx_post_tags_tag_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_post_tags_tag_id ON public.post_tags USING btree (tag_id);


--
-- Name: idx_posts_category_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_posts_category_id ON public.posts USING btree (category_id);


--
-- Name: idx_posts_og_image_media_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_posts_og_image_media_id ON public.posts USING btree (og_image_media_id);


--
-- Name: idx_posts_published_at; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_posts_published_at ON public.posts USING btree (published_at DESC);


--
-- Name: idx_posts_status; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_posts_status ON public.posts USING btree (status);


--
-- Name: idx_posts_thumbnail_media_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_posts_thumbnail_media_id ON public.posts USING btree (thumbnail_media_id);


--
-- Name: idx_projects_media_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_projects_media_id ON public.projects USING btree (media_id);


--
-- Name: idx_review_sessions_email; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_review_sessions_email ON public.review_sessions USING btree (email);


--
-- Name: idx_review_sessions_expires_at; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_review_sessions_expires_at ON public.review_sessions USING btree (expires_at);


--
-- Name: idx_subscribers_status; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_subscribers_status ON public.subscribers USING btree (status);


--
-- Name: idx_visitor_logs_visited_at; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_visitor_logs_visited_at ON public.visitor_logs USING btree (visited_at);


--
-- Name: uq_slogans_single_active; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX uq_slogans_single_active ON public.slogans USING btree (is_active) WHERE (is_active = true);


--
-- Name: posts trg_check_post_category_level; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER trg_check_post_category_level BEFORE INSERT OR UPDATE OF category_id ON public.posts FOR EACH ROW EXECUTE FUNCTION public.fn_check_post_category_level();


--
-- Name: post_reviews trg_post_reviews_touch_updated_at; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER trg_post_reviews_touch_updated_at BEFORE UPDATE ON public.post_reviews FOR EACH ROW EXECUTE FUNCTION public.fn_touch_updated_at();


--
-- Name: posts trg_posts_touch_updated_at; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER trg_posts_touch_updated_at BEFORE UPDATE ON public.posts FOR EACH ROW EXECUTE FUNCTION public.fn_touch_updated_at();


--
-- Name: categories trg_set_category_level; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER trg_set_category_level BEFORE INSERT OR UPDATE OF parent_id ON public.categories FOR EACH ROW EXECUTE FUNCTION public.fn_set_category_level();


--
-- Name: banners banners_media_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.banners
    ADD CONSTRAINT banners_media_id_fkey FOREIGN KEY (media_id) REFERENCES public.media_library(id) ON DELETE RESTRICT;


--
-- Name: categories categories_parent_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.categories
    ADD CONSTRAINT categories_parent_id_fkey FOREIGN KEY (parent_id) REFERENCES public.categories(id) ON DELETE RESTRICT;


--
-- Name: post_reviews post_reviews_post_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.post_reviews
    ADD CONSTRAINT post_reviews_post_id_fkey FOREIGN KEY (post_id) REFERENCES public.posts(id) ON DELETE CASCADE;


--
-- Name: post_reviews post_reviews_session_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.post_reviews
    ADD CONSTRAINT post_reviews_session_id_fkey FOREIGN KEY (session_id) REFERENCES public.review_sessions(id) ON DELETE SET NULL;


--
-- Name: post_tags post_tags_post_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.post_tags
    ADD CONSTRAINT post_tags_post_id_fkey FOREIGN KEY (post_id) REFERENCES public.posts(id) ON DELETE CASCADE;


--
-- Name: post_tags post_tags_tag_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.post_tags
    ADD CONSTRAINT post_tags_tag_id_fkey FOREIGN KEY (tag_id) REFERENCES public.tags(id) ON DELETE CASCADE;


--
-- Name: posts posts_category_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.posts
    ADD CONSTRAINT posts_category_id_fkey FOREIGN KEY (category_id) REFERENCES public.categories(id) ON DELETE RESTRICT;


--
-- Name: posts posts_og_image_media_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.posts
    ADD CONSTRAINT posts_og_image_media_id_fkey FOREIGN KEY (og_image_media_id) REFERENCES public.media_library(id) ON DELETE RESTRICT;


--
-- Name: posts posts_thumbnail_media_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.posts
    ADD CONSTRAINT posts_thumbnail_media_id_fkey FOREIGN KEY (thumbnail_media_id) REFERENCES public.media_library(id) ON DELETE RESTRICT;


--
-- Name: projects projects_media_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.projects
    ADD CONSTRAINT projects_media_id_fkey FOREIGN KEY (media_id) REFERENCES public.media_library(id) ON DELETE RESTRICT;


--
-- PostgreSQL database dump complete
--

\unrestrict CeDkCEsVoWaHSb7bLR89FPVspD5bkgdc92sVJt0u5f4yEH9CZBNKLSYkm2zUdCb

