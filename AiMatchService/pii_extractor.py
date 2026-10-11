import re
import pymupdf as fitz  # PyMuPDF mới nhất

# Danh sách mẫu nguyên âm tiếng Việt để phát hiện ngôn ngữ
VIETNAMESE_CHARS_PATTERN = re.compile(
    r'[àáảãạăằắẳẵặâầấẩẫậèéẻẽẹêềếểễệìíỉĩịòóỏõọôồốổỗộơờớởỡợùúủũụưừứửữựỳýỷỹỵđ]',
    re.IGNORECASE
)

# Biểu thức chính quy phát hiện và che giấu thông tin cá nhân (PII)
EMAIL_PATTERN = re.compile(
    r'\b[A-Za-z0-9._%+-]+(?:\s*[\n\r]\s*)?@(?:\s*[\n\r]\s*)?[A-Za-z0-9.-]+(?:\s*[\n\r]\s*)?\.[A-Za-z]{2,}\b'
)
PHONE_PATTERN = re.compile(
    r'(?:\+84|84|0)(?:\s*[\.\-\s]?\s*\d){9,10}\b'
)
LINK_PATTERN = re.compile(
    r'https?://[^\s]+|(?:www\.)?[a-zA-Z0-9.-]+\.(?:com|org|vn|edu|io|net|github\.io)(?:/[^\s]*)?',
    re.IGNORECASE
)


def extract_text_adaptive(pdf_bytes: bytes) -> str:
    """
    Bóc tách văn bản từ byte stream PDF:
    - Tự động nhận diện CV 2 cột (chia theo toạ độ trục x) hoặc CV 1 cột truyền thống.
    - Duyệt và ghép văn bản theo luồng đọc tự nhiên, tránh bị trộn lẫn hàng ngang giữa các cột.
    """
    doc = fitz.open(stream=pdf_bytes, filetype="pdf")
    all_pages_text = []

    for page in doc:
        page_width = page.rect.width
        blocks = page.get_text("blocks")
        
        # Chỉ giữ lại các khối văn bản hợp lệ (type == 0)
        text_blocks = [b for b in blocks if b[4].strip() and b[6] == 0]
        if not text_blocks:
            continue

        # Thử tìm ranh giới chia 2 cột tiềm năng trong khoảng 28% đến 42% chiều rộng trang
        split_cand = None
        for cand_pct in [0.30, 0.33, 0.35, 0.38, 0.40]:
            cand_x = page_width * cand_pct
            left_count = 0
            right_count = 0
            crossing_count = 0

            for b in text_blocks:
                bx0, bx1 = b[0], b[2]
                if bx1 <= cand_x + 15:
                    left_count += 1
                elif bx0 >= cand_x - 15:
                    right_count += 1
                else:
                    crossing_count += 1

            # Điều kiện xác định 2 cột rõ rệt: Cả 2 bên đều có nội dung và ít khối cắt ngang
            if left_count >= 2 and right_count >= 2 and crossing_count <= 2:
                split_cand = cand_x
                break

        if split_cand is not None:
            # Layout 2 cột: Gom cột trái trước (sắp xếp trên -> dưới), sau đó đến cột phải
            left_blocks = [b for b in text_blocks if b[2] <= split_cand + 15]
            right_blocks = [b for b in text_blocks if b[0] >= split_cand - 15]
            cross_blocks = [b for b in text_blocks if b not in left_blocks and b not in right_blocks]

            left_blocks.sort(key=lambda b: (b[1], b[0]))
            right_blocks.sort(key=lambda b: (b[1], b[0]))
            cross_blocks.sort(key=lambda b: (b[1], b[0]))

            page_content = []
            for b in left_blocks:
                page_content.append(b[4].strip())
            for b in right_blocks:
                page_content.append(b[4].strip())
            for b in cross_blocks:
                page_content.append(b[4].strip())

            all_pages_text.append("\n".join(page_content))
        else:
            # Layout 1 cột chuẩn: Duyệt tự nhiên từ trên xuống dưới theo toạ độ y
            text_blocks.sort(key=lambda b: (b[1], b[0]))
            page_content = [b[4].strip() for b in text_blocks]
            all_pages_text.append("\n".join(page_content))

    doc.close()
    return "\n\n".join(all_pages_text).strip()


def anonymize_text(raw_text: str) -> dict:
    """
    Ẩn danh hóa các trường thông tin định danh cá nhân (PII) và phân loại ngôn ngữ.
    """
    if not raw_text:
        return {
            "clean_text": "",
            "language": "unknown",
            "word_count": 0
        }

    # 1. Che giấu Email (kể cả trường hợp email bị ngắt dòng mềm bởi PDF)
    clean_text = EMAIL_PATTERN.sub("[ANONYMIZED_EMAIL]", raw_text)

    # 2. Che giấu Số điện thoại
    clean_text = PHONE_PATTERN.sub("[ANONYMIZED_PHONE]", clean_text)

    # 3. Che giấu Đường dẫn liên kết cá nhân (Github, Portfolio, Social)
    clean_text = LINK_PATTERN.sub("[ANONYMIZED_LINK]", clean_text)

    # 4. Chuẩn hóa khoảng trắng và dòng trống dư thừa
    clean_text = re.sub(r'[ \t]+', ' ', clean_text)
    clean_text = re.sub(r'\n{3,}', '\n\n', clean_text).strip()

    # 5. Phân loại ngôn ngữ dựa trên mật độ ký tự tiếng Việt
    words = clean_text.split()
    word_count = len(words)
    vi_matches = VIETNAMESE_CHARS_PATTERN.findall(clean_text)

    if word_count == 0:
        language = "unknown"
    else:
        vi_density = len(vi_matches) / max(word_count, 1)
        if vi_density > 0.12:
            language = "vi"
        elif vi_density < 0.02:
            language = "en"
        else:
            language = "mixed"

    return {
        "clean_text": clean_text,
        "language": language,
        "word_count": word_count
    }
