import os
import re

templates_dir = r"c:\Users\aaa\Pictures\DoAnWeb_CS\DoAnWeb\CVBuilderApp\src\templates"
files = [f for f in os.listdir(templates_dir) if f.endswith('.vue')]

for f in sorted(files):
    path = os.path.join(templates_dir, f)
    with open(path, 'r', encoding='utf-8') as file:
        content = file.read()
    
    # Extract template tag contents
    template_match = re.search(r'<template>(.*?)</template>', content, re.DOTALL)
    if not template_match:
        print(f"{f}: No template tag found")
        continue
    
    tpl = template_match.group(1).strip()
    
    # Find root div tag classes/styles
    root_match = re.search(r'<div\s+([^>]*id=["\']cv-printable-area["\'][^>]*)>', tpl)
    root_info = root_match.group(1) if root_match else "No cv-printable-area"
    
    # Find aside tags
    asides = re.findall(r'<aside\s+([^>]*)>', tpl)
    # Find main tags
    mains = re.findall(r'<main\s+([^>]*)>', tpl)
    
    print(f"File: {f}")
    print(f"  Root: {root_info}")
    if asides:
        for idx, aside in enumerate(asides):
            print(f"  Aside {idx+1}: {aside}")
    if mains:
        for idx, main in enumerate(mains):
            print(f"  Main {idx+1}: {main}")
    print("-" * 50)
