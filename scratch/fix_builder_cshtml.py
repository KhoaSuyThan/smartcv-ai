import os

file_path = r"c:\Users\aaa\Pictures\DoAnWeb_CS\DoAnWeb\Views\Resume\Builder.cshtml"

with open(file_path, "r", encoding="utf-8") as f:
    content = f.read()

# 1. Fix the double-selector nesting syntax error for #cv-printable-area
bad_printable_area = """            #cv-printable-area { 
            #cv-printable-area:not(.keep-print-height) { 
                height: auto !important;
            }
                width: 210mm !important; 
                min-height: 297mm !important;
                overflow: visible !important; 
                margin: 0 !important; 
                box-shadow: none !important; 
                transform: none !important; 
                position: relative !important; 
                background: white !important; 
                border: none !important;
            }"""

good_printable_area = """            #cv-printable-area:not(.keep-print-height) { 
                height: auto !important;
            }
            #cv-printable-area { 
                width: 210mm !important; 
                min-height: 297mm !important;
                overflow: visible !important; 
                margin: 0 !important; 
                box-shadow: none !important; 
                transform: none !important; 
                position: relative !important; 
                background: white !important; 
                border: none !important;
            }"""

# Try to replace it, handling possible CRLF/LF mismatch
normalized_content = content.replace("\r\n", "\n")
normalized_bad = bad_printable_area.replace("\r\n", "\n")
normalized_good = good_printable_area.replace("\r\n", "\n")

if normalized_bad in normalized_content:
    normalized_content = normalized_content.replace(normalized_bad, normalized_good)
    print("Successfully replaced #cv-printable-area block!")
else:
    # Let's try replacing line by line or check what's there
    print("Warning: bad #cv-printable-area block not found directly, trying custom replacement...")
    # Let's find: #cv-printable-area { \n            #cv-printable-area:not(.keep-print-height) {
    # and clean it up.
    import re
    pattern = r"(\s*)#cv-printable-area\s*\{\s*\n(\s*)#cv-printable-area:not\(\.keep-print-height\)\s*\{\s*\n(\s*)height:\s*auto\s*!important;\s*\n\s*\}\s*\n(\s*)width:\s*210mm\s*!important;"
    match = re.search(pattern, normalized_content)
    if match:
        # Reconstruct properly
        replacement = "\\1#cv-printable-area:not(.keep-print-height) {\n\\3height: auto !important;\n\\1}\n\\1#cv-printable-area {\n\\4"
        normalized_content = re.sub(pattern, replacement, normalized_content)
        print("Regex replaced #cv-printable-area block successfully!")
    else:
        print("Regex replacement failed too.")

# 2. Fix the children selector to exclude .keep-print-height
bad_children = """            /* CRITICAL: Children của template (main, sidebar columns, flex columns) */
            #cv-printable-area > *:not(.deco-element):not(.no-print),
            #cv-printable-area main,
            #cv-printable-area .left-sidebar,
            #cv-printable-area .right-main,
            #cv-printable-area .cv-sidebar,
            #cv-printable-area .cv-main-content {
                overflow: visible !important;
                height: auto !important;
                max-height: none !important;
            }"""

good_children = """            /* CRITICAL: Children của template (main, sidebar columns, flex columns) */
            #cv-printable-area > *:not(.deco-element):not(.no-print):not(.keep-print-height),
            #cv-printable-area main:not(.keep-print-height),
            #cv-printable-area .left-sidebar:not(.keep-print-height),
            #cv-printable-area .right-main:not(.keep-print-height),
            #cv-printable-area .cv-sidebar:not(.keep-print-height),
            #cv-printable-area .cv-main-content:not(.keep-print-height) {
                overflow: visible !important;
                height: auto !important;
                max-height: none !important;
            }"""

normalized_bad_children = bad_children.replace("\r\n", "\n")
normalized_good_children = good_children.replace("\r\n", "\n")

if normalized_bad_children in normalized_content:
    normalized_content = normalized_content.replace(normalized_bad_children, normalized_good_children)
    print("Successfully replaced children block!")
else:
    print("Warning: bad children block not found, trying regex...")
    pattern_children = r"#cv-printable-area\s*>\s*\*:\s*not\(\.deco-element\):\s*not\(\.no-print\),"
    if re.search(pattern_children, normalized_content):
        # Let's replace the whole block by finding the start and end of it
        # Actually let's just do a string replacement of the specific lines
        lines_bad = [
            "#cv-printable-area > *:not(.deco-element):not(.no-print),",
            "#cv-printable-area main,",
            "#cv-printable-area .left-sidebar,",
            "#cv-printable-area .right-main,",
            "#cv-printable-area .cv-sidebar,",
            "#cv-printable-area .cv-main-content {"
        ]
        lines_good = [
            "#cv-printable-area > *:not(.deco-element):not(.no-print):not(.keep-print-height),",
            "#cv-printable-area main:not(.keep-print-height),",
            "#cv-printable-area .left-sidebar:not(.keep-print-height),",
            "#cv-printable-area .right-main:not(.keep-print-height),",
            "#cv-printable-area .cv-sidebar:not(.keep-print-height),",
            "#cv-printable-area .cv-main-content:not(.keep-print-height) {"
        ]
        temp_content = normalized_content
        found_all = True
        for b, g in zip(lines_bad, lines_good):
            if b in temp_content:
                temp_content = temp_content.replace(b, g)
            else:
                found_all = False
                break
        if found_all:
            normalized_content = temp_content
            print("Successfully replaced children block line by line!")
        else:
            print("Line-by-line replacement failed.")

# Save back with original CRLF line endings
content_to_write = normalized_content.replace("\n", "\r\n")
with open(file_path, "w", encoding="utf-8") as f:
    f.write(content_to_write)
print("Done writing back!")
