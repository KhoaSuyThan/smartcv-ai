import os
import re

templates_dir = r"c:\Users\aaa\Pictures\DoAnWeb_CS\DoAnWeb\CVBuilderApp\src\templates"
files = [f for f in os.listdir(templates_dir) if f.endswith('.vue')]

out_path = r"c:\Users\aaa\Pictures\DoAnWeb_CS\DoAnWeb\scratch\analyze_results.txt"
with open(out_path, 'w', encoding='utf-8') as out:
    for f in sorted(files):
        path = os.path.join(templates_dir, f)
        with open(path, 'r', encoding='utf-8') as file:
            content = file.read()
        
        # Extract template tag contents
        template_match = re.search(r'<template>(.*?)</template>', content, re.DOTALL)
        if not template_match:
            out.write(f"{f}: No template tag found\n")
            continue
        
        tpl = template_match.group(1).strip()
        
        # Find root div tag classes/styles
        root_match = re.search(r'<div\s+([^>]*id=["\']cv-printable-area["\'][^>]*)>', tpl)
        root_info = root_match.group(1) if root_match else "No cv-printable-area"
        
        # Find aside tags
        asides = re.findall(r'<aside\s+([^>]*)>', tpl)
        # Find main tags
        mains = re.findall(r'<main\s+([^>]*)>', tpl)
        
        out.write(f"File: {f}\n")
        out.write(f"  Root: {root_info}\n")
        if asides:
            for idx, aside in enumerate(asides):
                out.write(f"  Aside {idx+1}: {aside}\n")
        if mains:
            for idx, main in enumerate(mains):
                out.write(f"  Main {idx+1}: {main}\n")
        out.write("-" * 50 + "\n")
print("Done writing to analyze_results.txt")
