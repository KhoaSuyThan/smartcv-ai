import os
import re

template_dir = r"c:\Users\aaa\Pictures\DoAnWeb_CS\DoAnWeb\ReactCVBuilder\src\components\Templates"

# Standard replacements for tokens
token_map = {
    "{{FullName}}": "{resumeData?.fullName || 'HỌ TÊN'}",
    "{{JobTitle}}": "{resumeData?.jobTitle || 'VỊ TRÍ'}",
    "{{Phone}}": "{resumeData?.phone || 'SĐT'}",
    "{{Email}}": "{resumeData?.email || 'Email'}",
    "{{Address}}": "{resumeData?.address || 'Địa chỉ'}",
    "{{Website}}": "{resumeData?.website || 'Website'}",
    "{{BirthDate}}": "{resumeData?.birthDate || 'Ngày sinh'}",
    "{{Summary}}": "{resumeData?.summary || 'Mục tiêu nghề nghiệp'}",
}

def generate_loop(token_name, array_name, inner_jsx):
    return f"{{resumeData?.{array_name}?.map((item, idx) => (\n{inner_jsx}\n))}}"

loops = {
    "{{Experience}}": generate_loop("Experience", "experiences", """
        <div className="exp-item" key={idx}>
            <span className="exp-year">• {item.time}</span>
            <div className="exp-content">
                <div className="info-line"><strong>Công ty:</strong> {item.company}</div>
                <div className="info-line"><strong>Vị trí:</strong> {item.role}</div>
                <div className="desc-text" dangerouslySetInnerHTML={{__html: item.desc}}></div>
            </div>
        </div>"""),
    "{{Education}}": generate_loop("Education", "educations", """
        <div className="exp-item" key={idx}>
            <span className="exp-year">• {item.year}</span>
            <div className="exp-content">
                <strong>{item.school}</strong><br/>
                {item.major}<br/>
                {item.gradType ? `Xếp loại: ${item.gradType}` : ''}
            </div>
        </div>"""),
    "{{Projects}}": generate_loop("Projects", "projects", """
        <div className="exp-item" key={idx}>
            <span className="exp-year">• {item.time}</span>
            <div className="exp-content">
                <div className="info-line"><strong>Dự án:</strong> {item.name}</div>
                <div className="info-line"><strong>Vai trò:</strong> {item.role}</div>
                <div className="desc-text" dangerouslySetInnerHTML={{__html: item.desc}}></div>
            </div>
        </div>"""),
    "{{Activities}}": generate_loop("Activities", "activities", """
        <div className="exp-item" key={idx}>
            <div className="exp-header">
                <span className="company-name">{item.name}</span>
                <span className="date-badge">{item.time}</span>
            </div>
            <div className="exp-desc" dangerouslySetInnerHTML={{__html: item.desc}}></div>
        </div>"""),
    "{{Skills}}": "<ul className=\"skill-list-items\">" + generate_loop("Skills", "skills", "<li>• {item.name}: {item.level}</li>") + "</ul>",
    "{{Languages}}": "<ul className=\"lang-list-items\">" + generate_loop("Languages", "languages", "<li style={{display:'flex', justifyContent:'space-between'}}><span>• {item.name}</span><span style={{fontStyle:'italic', opacity:0.8}}>{item.level}</span></li>") + "</ul>",
    "{{OtherSkills}}": "<ul className=\"other-skill-list-items\">" + generate_loop("OtherSkills", "otherSkills", "<li>• {item.name} {item.level ? `(${item.level})` : ''}</li>") + "</ul>",
    "{{Hobbies}}": "<ul className=\"hobby-list-items\">" + generate_loop("Hobbies", "hobbies", "<li>• {item.name}</li>") + "</ul>",
    "{{Awards}}": "<ul style={{paddingLeft:'15px', margin:0}}>" + generate_loop("Awards", "awards", "<li>{item.name}</li>") + "</ul>",
    "{{References}}": generate_loop("References", "references", "<p style={{marginBottom:'5px', fontSize:'12px'}}>• {item.info}</p>"),
    "{{Certifications}}": generate_loop("Certifications", "certifications", """
        <div style={{marginBottom:'8px'}}>
            <div className="cert-year-div" style={{fontWeight:'bold', fontSize:'11px', color:'#634c46'}}>{item.year}</div>
            <div className="cert-name-div" style={{fontSize:'12.5px'}}>{item.name}</div>
        </div>"""),
}

for filename in os.listdir(template_dir):
    if not filename.endswith(".jsx"):
        continue
    filepath = os.path.join(template_dir, filename)
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()

    # If it's already processed, skip (check if dangerouslySetInnerHTML exists in loops)
    if "dangerouslySetInnerHTML" in content:
        continue

    # Find the CSS part. It usually starts after the main wrapper div closes.
    # A heuristic: find the first `@import` or `/*` or CSS selector after the HTML.
    # The user pasted it like this:
    # </div>
    # @import url...
    # /* CSS ... */
    # </div>
    # );
    
    # Let's extract everything between `<div className="cv-template-...` and the `</div>\n    );\n};`
    match = re.search(r'(<div className="cv-template-.*?>\s*)(.*?)(\s*</div>\s*\);\s*};)', content, re.DOTALL)
    if not match:
        print(f"Failed to match {filename}")
        continue
    
    prefix = match.group(1)
    body = match.group(2)
    suffix = match.group(3)

    # In body, the CSS usually starts at `@import` or `/*`. 
    # Or we can just find where the HTML ends. HTML ends with `</div>` (the main wrapper).
    # Since there are many divs, let's use regex to find CSS blocks.
    # Typically, CSS has `{ ... }`. We can wrap the non-HTML part in <style>{`...`}</style>.
    # Actually, we can split by looking for `@import` or `/* CẤU TRÚC` or `/* Reset`
    css_start_idx = -1
    for marker in ['@import', '/* CẤU TRÚC', '/* Layout', '/* KHUNG', '/* RESET', '/* Cấu trúc', '/* Reset', '/* TỔNG THỂ', ':root {', '/* KHUNG BAO', '/* CẤU TRÚC A4']:
        idx = body.find(marker)
        if idx != -1:
            if css_start_idx == -1 or idx < css_start_idx:
                css_start_idx = idx
    
    if css_start_idx != -1:
        html_part = body[:css_start_idx]
        css_part = body[css_start_idx:]
    else:
        # If no marker, maybe CSS is just everything after the last `</div>`
        last_div = body.rfind("</div>")
        if last_div != -1:
            html_part = body[:last_div+6]
            css_part = body[last_div+6:]
        else:
            html_part = body
            css_part = ""

    # Fix HTML part
    html_part = html_part.replace('class=', 'className=')
    html_part = html_part.replace('for=', 'htmlFor=')
    # Close img tags
    html_part = re.sub(r'(<img[^>]*?[^/])>', r'\1 />', html_part)
    html_part = html_part.replace('{{AvatarUrl}}', '{resumeData?.avatarUrl || "https://i.imgur.com/8Km9tLL.png"}')
    html_part = html_part.replace("onerror=\"this.src=''/images/no-avatar.png''\"", "")

    # Clean up { and } in html that might break JSX
    # Replace tokens
    for k, v in token_map.items():
        html_part = html_part.replace(k, v)
    for k, v in loops.items():
        html_part = html_part.replace(k, v)

    # Some templates have inline style that are strings. Convert to object.
    # e.g., style="margin-top: 15px;" -> style={{marginTop: '15px'}}
    def repl_style(m):
        style_str = m.group(1)
        # simplistic conversion
        props = []
        for prop in style_str.split(';'):
            if ':' not in prop: continue
            k, v = prop.split(':', 1)
            k = k.strip()
            # to camelCase
            k = re.sub(r'-([a-z])', lambda m2: m2.group(1).upper(), k)
            v = v.strip().replace("'", '"')
            props.append(f"{k}: '{v}'")
        return "style={{" + ", ".join(props) + "}}"
    html_part = re.sub(r'style="([^"]*)"', repl_style, html_part)

    # Wrap CSS part
    css_part = css_part.strip()
    if css_part:
        # escape backticks and ${}
        css_part = css_part.replace('`', r'\`').replace('${', r'\${')
        css_part = f"\n<style>{{`\n{css_part}\n`}}</style>\n"

    new_content = content[:match.start()] + prefix + html_part + css_part + suffix

    with open(filepath, 'w', encoding='utf-8') as f:
        f.write(new_content)

print("Done")
