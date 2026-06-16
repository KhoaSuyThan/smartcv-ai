import os
import re

template_dir = r"c:\Users\aaa\Pictures\DoAnWeb_CS\DoAnWeb\ReactCVBuilder\src\components\Templates"

for filename in os.listdir(template_dir):
    if not filename.endswith(".jsx"):
        continue
    filepath = os.path.join(template_dir, filename)
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()

    # Look for: <img src={resumeData?.avatarUrl || "..."} ... /> or similar
    # We want to replace it with:
    # {resumeData?.avatarUrl && <img src={resumeData.avatarUrl} ... />}
    # Let's handle different patterns:
    # 1. <img src={resumeData?.avatarUrl || "..."} className="..." />
    # 2. <img className="..." src={resumeData?.avatarUrl || "..."} />
    
    # Let's find any img tag containing resumeData?.avatarUrl
    img_pattern = r'<img\s+[^>]*?src=\{resumeData\?.avatarUrl\s*\|\|\s*"[^"]*"\}[^>]*?>'
    
    def repl(match):
        img_tag = match.group(0)
        # Remove the || "..." fallback from src
        fixed_src = re.sub(r'src=\{resumeData\?.avatarUrl\s*\|\|\s*"[^"]*"\}', 'src={resumeData.avatarUrl}', img_tag)
        return f"{{resumeData?.avatarUrl && {fixed_src}}}"
        
    # Also handle if src is after class
    img_pattern_alt = r'<img\s+[^>]*?src=\{resumeData\?.avatarUrl\s*\|\|\s*"[^"]*"\}[^>]*?/>'
    
    new_content = re.sub(img_pattern, repl, content)
    new_content = re.sub(img_pattern_alt, repl, new_content)
    
    if new_content != content:
        with open(filepath, 'w', encoding='utf-8') as f:
            f.write(new_content)
        print(f"Fixed avatar in {filename}")

print("Completed fixing avatars.")
