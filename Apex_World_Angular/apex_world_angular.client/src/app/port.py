import os
import re

html_path = 'C:/Users/kirth/Downloads/frontend_temp/frontend/features/site-visits/admin-site-visits/site_visit_management.html'
with open(html_path, 'r', encoding='utf-8') as f:
    html = f.read()

# Extract header
header_match = re.search(r'(<header.*?</header>)', html, re.DOTALL)
header = header_match.group(1) if header_match else ''

# Extract content
content_match = re.search(r'(<div class=\"cm-content-area\">.*?</div>\s*</div>)', html, re.DOTALL)
content = content_match.group(1) if content_match else ''

# Extract modal
modal_match = re.search(r'(<div class=\"cm-modal-overlay\" id=\"deny-modal\">.*?</div>\s*</div>)', html, re.DOTALL)
modal = modal_match.group(1) if modal_match else ''

toast = '<div id=\"toast\"></div>'

final_html = header + '\n' + content + '\n' + modal + '\n' + toast
# Fix icons
final_html = final_html.replace('â ‖ \"', '🔍').replace('â\x80\x9C', '🔍').replace('🔍', '🔍')

# also replace bell and question mark if needed
final_html = final_html.replace('🔔', '🔔').replace('❓', '❓')

with open('C:/Users/kirth/source/repos/ApexWorld_Backend/Apex_World_Angular/apex_world_angular.client/src/app/features/site-visits/admin-site-visits/admin-site-visit-management/admin-site-visit-management.html', 'w', encoding='utf-8') as f:
    f.write(final_html)

css_path = 'C:/Users/kirth/Downloads/frontend_temp/frontend/features/site-visits/admin-site-visits/site_visit_management.css'
with open(css_path, 'r', encoding='utf-8') as f:
    css = f.read()

css = re.sub(r'\.kpi-value\s*\{[^}]*\}', 'h2.kpi-value { font-size: 1.4rem !important; }', css)
css = re.sub(r'max-width:\s*1200px', 'width: 100%; max-width: 100%;', css)
css += '\n.kpi-row { width: 100%; }\n'
css += '.toast { position: fixed; top: 32px; right: 32px; background: #1E293B; color: #fff; padding: 14px 24px; border-radius: 10px; z-index: 2000; opacity: 0; transition: all 0.3s ease; transform: translateY(-20px); pointer-events: none; } .toast.show { opacity: 1; transform: translateY(0); } .toast.success { background: #10B981; } .toast.danger { background: #EF4444; }'

with open('C:/Users/kirth/source/repos/ApexWorld_Backend/Apex_World_Angular/apex_world_angular.client/src/app/features/site-visits/admin-site-visits/admin-site-visit-management/admin-site-visit-management.css', 'w', encoding='utf-8') as f:
    f.write(css)
print('HTML and CSS ported')
