import os
import re

template_dir = r'c:\Users\aaa\Pictures\DoAnWeb_CS\DoAnWeb\ReactCVBuilder\src\components\Templates'

for filename in os.listdir(template_dir):
    if not filename.endswith('.jsx'):
        continue
    filepath = os.path.join(template_dir, filename)
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()

    # Fix src="{...}"
    content = re.sub(r'src="({resumeData[^}]*})"', r'src=\1', content)
    
    # Fix HTML comments
    content = re.sub(r'<!--(.*?)-->', r'{/*\1*/}', content, flags=re.DOTALL)
    
    # Close unclosed tags
    content = re.sub(r'<br\s*>', '<br/>', content)
    content = re.sub(r'<hr\s*>', '<hr/>', content)
    
    # Fix missing export default
    template_name = filename[:-4]
    if f"export default {template_name}" not in content:
        content += f"\nexport default {template_name};\n"

    with open(filepath, 'w', encoding='utf-8') as f:
        f.write(content)

print('Fixed JSX errors')
