import os
import re

templates_dir = r"c:\Users\aaa\Pictures\DoAnWeb_CS\DoAnWeb\CVBuilderApp\src\templates"
files = [f for f in os.listdir(templates_dir) if f.endswith('.vue')]

out_path = r"c:\Users\aaa\Pictures\DoAnWeb_CS\DoAnWeb\scratch\analyze_children.txt"
with open(out_path, 'w', encoding='utf-8') as out:
    for f in sorted(files):
        path = os.path.join(templates_dir, f)
        with open(path, 'r', encoding='utf-8') as file:
            content = file.read()
        
        # Extract template tag contents
        template_match = re.search(r'<template>(.*?)</template>', content, re.DOTALL)
        if not template_match:
            continue
        
        tpl = template_match.group(1).strip()
        
        # Find root div tag and its content
        # We find <div id="cv-printable-area"...> and parse its direct children
        # To simplify, let's find all tags matching <aside...> <main...> or <div...> that are top-level inside the root div.
        # Let's search for tags that have specific common classes or attributes.
        lines = tpl.split('\n')
        out.write(f"File: {f}\n")
        for line in lines:
            if '<aside' in line or '<main' in line:
                out.write(f"  Tag: {line.strip()}\n")
            elif 'class=' in line and ('sidebar' in line or 'column' in line or 'main' in line or 'content' in line or 'wrapper' in line):
                out.write(f"  Line: {line.strip()}\n")
        out.write("-" * 50 + "\n")
print("Done writing to analyze_children.txt")
