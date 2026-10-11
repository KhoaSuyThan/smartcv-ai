import re
from typing import Dict, List, Any, Tuple
import datetime

# Các từ khóa tiêu đề phân đoạn thường gặp trong CV
SKILL_HEADERS = re.compile(r'(kỹ\s*năng|skills|chuyên\s*môn|technologies|tech\s*stack)', re.IGNORECASE)
EXPERIENCE_HEADERS = re.compile(r'(kinh\s*nghiệm|experience|dự\s*án|projects|lịch\s*sử\s*làm\s*việc|employment)', re.IGNORECASE)
EDUCATION_HEADERS = re.compile(r'(học\s*vấn|education|bằng\s*cấp|đào\s*tạo|qualifications)', re.IGNORECASE)

# Mẫu regex nhận diện khoảng năm kinh nghiệm (vd: 2020 - 2024, 2021 - Hiện tại, 06/2019 - 12/2022)
YEAR_RANGE_PATTERN = re.compile(
    r'(?:\b(?:0?[1-9]|1[0-2])[/-])?((?:19|20)\d{2})\s*(?:[-–—]|đến|to)\s*(?:(?:\b(?:0?[1-9]|1[0-2])[/-])?((?:19|20)\d{2})|hiện\s*tại|nay|present|current)',
    re.IGNORECASE
)

# Từ điển chuẩn hóa các biến thể kỹ năng công nghệ phổ biến
SKILL_SYNONYMS = {
    "c#": ["c#", "csharp", "c-sharp", ".net", "dotnet", ".net core", "asp.net"],
    ".net core": [".net core", ".net", "dotnet", "asp.net core", "asp.net", "c#"],
    "asp.net core": ["asp.net core", "asp.net", ".net core", "c#"],
    "vue.js": ["vue.js", "vue", "vuejs", "vue 3", "vue2", "nuxt", "nuxtjs"],
    "react": ["react", "react.js", "reactjs", "next.js", "nextjs"],
    "sql server": ["sql server", "mssql", "t-sql", "tsql", "sql"],
    "postgresql": ["postgresql", "postgres", "pgsql", "psql"],
    "docker": ["docker", "container", "dockerfile", "docker-compose"],
    "kubernetes": ["kubernetes", "k8s", "k8s cluster"],
    "python": ["python", "python3", "py"],
    "fastapi": ["fastapi", "fast-api"],
    "java": ["java", "java 8", "java 11", "java 17", "spring", "spring boot"],
    "spring boot": ["spring boot", "spring", "spring framework"],
    "node.js": ["node.js", "nodejs", "node", "express.js", "nestjs"],
    "git": ["git", "github", "gitlab", "bitbucket"],
    "microservices": ["microservices", "microservice", "kiến trúc microservice"]
}


def parse_cv_sections(cv_text: str) -> Tuple[str, str, str]:
    """
    Tách nội dung CV thành 3 vùng logic:
    1. Vùng kỹ năng (Skills section)
    2. Vùng kinh nghiệm / Dự án thực tế (Experience & Projects)
    3. Vùng học vấn (Education)
    """
    lines = cv_text.split('\n')
    skills_text = []
    exp_text = []
    edu_text = []
    
    current_section = "general"
    
    for line in lines:
        line_clean = line.strip()
        if not line_clean:
            continue
            
        # Kiểm tra tiêu đề phân đoạn
        if SKILL_HEADERS.search(line_clean) and len(line_clean) < 40:
            current_section = "skills"
            continue
        elif EXPERIENCE_HEADERS.search(line_clean) and len(line_clean) < 40:
            current_section = "experience"
            continue
        elif EDUCATION_HEADERS.search(line_clean) and len(line_clean) < 40:
            current_section = "education"
            continue
            
        if current_section == "skills":
            skills_text.append(line_clean)
        elif current_section == "experience":
            exp_text.append(line_clean)
        elif current_section == "education":
            edu_text.append(line_clean)
        else:
            # Nếu chưa phân đoạn rõ ràng, dồn vào kinh nghiệm để kiểm tra
            exp_text.append(line_clean)

    return "\n".join(skills_text), "\n".join(exp_text), "\n".join(edu_text)


def estimate_experience_years(cv_text: str) -> float:
    """
    Ước lượng tổng số năm kinh nghiệm bằng cách trích xuất các mốc thời gian (vd: 2021 - 2024).
    """
    current_year = datetime.datetime.now().year
    matches = YEAR_RANGE_PATTERN.findall(cv_text)
    
    total_months = 0
    intervals = []
    
    for m in matches:
        start_year = int(m[0])
        end_str = m[1].strip()
        end_year = int(end_str) if end_str.isdigit() else current_year
        
        # Lọc các mốc vô lý (nhỏ hơn 2000 hoặc start > end)
        if 2000 <= start_year <= current_year and start_year <= end_year <= current_year + 1:
            intervals.append((start_year, end_year))

    if not intervals:
        # Fallback: Tìm các từ khóa trực tiếp dạng 'X năm kinh nghiệm'
        direct_match = re.search(r'(\d+)\s*\+?\s*(?:năm|years?)\s*(?:kinh\s*nghiệm|experience)', cv_text, re.IGNORECASE)
        if direct_match:
            return float(direct_match.group(1))
        return 1.0  # Mặc định tối thiểu 1 năm nếu không xác định được

    # Hợp nhất các khoảng thời gian bị trùng lặp
    intervals.sort()
    merged = []
    for interval in intervals:
        if not merged:
            merged.append(interval)
        else:
            prev_start, prev_end = merged[-1]
            if interval[0] <= prev_end:
                merged[-1] = (prev_start, max(prev_end, interval[1]))
            else:
                merged.append(interval)

    total_years = sum(end - start for start, end in merged)
    return max(float(total_years), 0.5)


def check_single_skill_match(skill_name: str, text: str) -> bool:
    """
    Kiểm tra một kỹ năng có xuất hiện trong đoạn văn bản hay không (hỗ trợ từ đồng nghĩa/viết tắt).
    """
    skill_lower = skill_name.lower().strip()
    synonyms = SKILL_SYNONYMS.get(skill_lower, [skill_lower])
    
    for syn in synonyms:
        # Regex tìm từ nguyên vẹn để tránh so khớp nhầm (vd: 'C' trong 'CSS')
        pattern = r'(?<![a-zA-Z0-9#+])' + re.escape(syn) + r'(?![a-zA-Z0-9#+])'
        if re.search(pattern, text, re.IGNORECASE):
            return True
    return False


def score_ats_candidate(
    cv_text: str,
    jd_data: Dict[str, Any],
    semantic_score: float
) -> Dict[str, Any]:
    """
    Chấm điểm hồ sơ ứng viên theo chuẩn ATS thế hệ mới với 5 cơ chế thông minh:
    1. Chống nhồi từ khóa (Proven Skills vs Unverified Skills)
    2. Quy tắc loại trừ Knockout (Must-have vs Nice-to-have)
    3. So khớp thực thể kỹ năng đa biến thể
    4. Phân tích số năm kinh nghiệm thực tế
    5. Đánh giá cấp bậc và xuất nhận xét có cấu trúc
    """
    # 1. Phân tách CV thành các phân vùng logic
    skills_sec, exp_sec, edu_sec = parse_cv_sections(cv_text)
    full_lower = cv_text.lower()
    exp_lower = exp_sec.lower()

    # 2. Phân loại Kỹ năng JD: Must-have (3-4 kỹ năng đầu) và Nice-to-have
    jd_hard_skills = jd_data.get("hard_skills", [])
    if len(jd_hard_skills) <= 4:
        must_have_skills = jd_hard_skills
        nice_to_have_skills = []
    else:
        must_have_skills = jd_hard_skills[:4]
        nice_to_have_skills = jd_hard_skills[4:]

    # 3. Đánh giá Kỹ năng: Proven (100% điểm) vs Unverified (50% điểm) vs Missing (0%)
    matched_skills = []
    proven_skills = []
    unverified_skills = []
    missing_skills = []
    must_have_matched = 0

    for skill in jd_hard_skills:
        # Kiểm tra xuất hiện trong toàn bộ CV
        in_full = check_single_skill_match(skill, full_lower)
        # Kiểm tra xuất hiện trong phần Dự án / Kinh nghiệm thực tế (Chống nhồi từ khóa)
        in_exp = check_single_skill_match(skill, exp_lower)

        if in_full:
            matched_skills.append(skill)
            if in_exp:
                proven_skills.append(skill)
            else:
                unverified_skills.append(skill)
            
            if skill in must_have_skills:
                must_have_matched += 1
        else:
            missing_skills.append(skill)

    # Tính điểm kỹ năng có trọng số chống gian lận
    # Proven skill được 1.0 điểm, Unverified skill chỉ được 0.5 điểm
    skill_earned_points = (len(proven_skills) * 1.0) + (len(unverified_skills) * 0.5)
    total_skill_points = max(len(jd_hard_skills), 1)
    hard_skills_score = min((skill_earned_points / total_skill_points) * 100.0, 100.0)

    # 4. Kiểm tra Quy tắc Knockout Rule cho Must-have
    must_have_ratio = must_have_matched / max(len(must_have_skills), 1)
    knockout_passed = must_have_ratio >= 0.75  # Cần khớp ít nhất 75% kỹ năng bắt buộc
    knockout_factor = 1.0 if knockout_passed else 0.65  # Phạt 35% nếu trượt knockout

    # 5. Đánh giá Số năm kinh nghiệm
    exp_years = estimate_experience_years(cv_text)
    required_years = float(jd_data.get("experience_years_required", 2))
    
    if exp_years >= required_years:
        exp_score = 100.0
        exp_eval = f"{exp_years:.1f} năm (Đáp ứng tốt yêu cầu {required_years} năm)"
    else:
        exp_score = max((exp_years / required_years) * 100.0, 40.0)
        exp_eval = f"{exp_years:.1f} năm (Chưa đạt tối thiểu {required_years} năm)"

    # 6. Đánh giá Bằng cấp & Học vấn
    edu_lower = (edu_sec + "\n" + cv_text).lower()
    has_degree = bool(re.search(r'(đại\s*học|cử\s*nhân|kỹ\s*sư|bachelor|master|thạc\s*sĩ|university)', edu_lower))
    edu_score = 100.0 if has_degree else 75.0

    # 7. Tính Điểm Tổng Hợp ATS (Composite Weighted Score)
    # Trọng số: 30% Ngữ nghĩa + 35% Kỹ năng + 15% Điểm cộng + 10% Kinh nghiệm + 10% Học vấn
    sem_score_100 = semantic_score * 100.0
    composite_raw = (
        (0.30 * sem_score_100) +
        (0.35 * hard_skills_score) +
        (0.15 * (must_have_ratio * 100.0)) +
        (0.10 * exp_score) +
        (0.10 * edu_score)
    )
    final_score = int(round(composite_raw * knockout_factor))
    final_score = max(min(final_score, 99), 30)

    # 8. Đánh giá Cấp bậc (Seniority Alignment)
    jd_level = jd_data.get("level", "Middle")
    if exp_years < 2.0:
        candidate_level = "Junior"
    elif exp_years <= 4.0:
        candidate_level = "Middle"
    else:
        candidate_level = "Senior"

    if candidate_level.lower() in jd_level.lower():
        seniority_fit = f"{candidate_level} (Khớp chuẩn vị trí tuyển)"
    elif exp_years < required_years:
        seniority_fit = f"{candidate_level} (Cần thêm kinh nghiệm thực chiến)"
    else:
        seniority_fit = f"{candidate_level} (Ứng viên giàu kinh nghiệm)"

    # 9. Sinh Nhận xét & Đề xuất (Recommendations)
    if final_score >= 80 and knockout_passed:
        recommendation = "strong_match"
        summary = f"Ứng viên rất sáng giá cho vị trí {jd_data.get('title')}. Sở hữu {len(proven_skills)} kỹ năng thực chiến được chứng minh trong dự án."
    elif final_score >= 65 and knockout_passed:
        recommendation = "good_match"
        summary = f"Ứng viên tiềm năng tốt. Đáp ứng đa số yêu cầu cốt lõi nhưng cần đào tạo bổ sung thêm một số kỹ năng nâng cao."
    elif final_score >= 50:
        recommendation = "partial_match"
        summary = f"Ứng viên có sự liên quan về ngành nghề nhưng thiếu các kỹ năng chuyên sâu bắt buộc."
    else:
        recommendation = "weak_match"
        summary = f"Hồ sơ chưa đạt tiêu chuẩn tối thiểu cho vị trí này (Trượt bài kiểm tra kỹ năng bắt buộc)."

    # Điểm mạnh nổi bật
    strengths_list = []
    if proven_skills:
        strengths_list.append(f"Kỹ năng thực chiến tốt: {', '.join(proven_skills[:3])}")
    if exp_years >= required_years:
        strengths_list.append(f"Kinh nghiệm làm việc vững chắc ({exp_years:.1f} năm)")
    if has_degree:
        strengths_list.append("Học vấn và nền tảng đào tạo chuyên ngành bài bản")
    strengths = "; ".join(strengths_list) if strengths_list else "Có nền tảng làm việc cơ bản"

    # Gợi ý cải thiện
    if missing_skills:
        suggestions = f"Nên bổ sung kiến thức hoặc dự án thực hành liên quan đến: {', '.join(missing_skills[:3])}."
    else:
        suggestions = "Hồ sơ rất đầy đủ, nên bổ sung thêm các số liệu định lượng về kết quả dự án."

    return {
        "match_score": final_score,
        "matched_skills": matched_skills,
        "missing_skills": missing_skills,
        "proven_skills": proven_skills,
        "unverified_skills": unverified_skills,
        "knockout_passed": knockout_passed,
        "experience_eval": exp_eval,
        "seniority_fit": seniority_fit,
        "strengths": strengths,
        "suggestions": suggestions,
        "summary": summary,
        "recommendation": recommendation
    }
