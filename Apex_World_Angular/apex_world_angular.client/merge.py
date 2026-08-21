import json

app_ts_path = 'C:/Users/kirth/source/repos/ApexWorld_Backend/Apex_World_Angular/apex_world_angular.client/src/app/app.ts'
app_js_path = 'c:/Users/kirth/Downloads/frontend_temp/frontend/assets/js/app.js'

with open(app_js_path, 'r', encoding='utf-8') as f:
    app_js_content = f.read()

# Replace 'document.addEventListener("DOMContentLoaded", () => {' with just a function
app_js_content = app_js_content.replace('document.addEventListener("DOMContentLoaded", () => {', 'function initAppJs() {')
app_js_content = app_js_content.replace('});', '}', 1) # Close initAppJs

# We need to make it valid TS, so we might need some basic any types, but since it's JS, TS compiler might complain if strict is on.
# Let's add @ts-nocheck at the top to make TS ignore the vanilla JS file errors if we put it in a separate file.

with open('C:/Users/kirth/source/repos/ApexWorld_Backend/Apex_World_Angular/apex_world_angular.client/src/app/app-logic.ts', 'w', encoding='utf-8') as f:
    f.write('// @ts-nocheck\n' + app_js_content + '\nexport function runVanillaLogic() { initAppJs(); }\n')

with open(app_ts_path, 'r', encoding='utf-8') as f:
    app_ts = f.read()

if 'runVanillaLogic()' not in app_ts:
    app_ts = app_ts.replace('import { Component', 'import { runVanillaLogic } from "./app-logic";\nimport { Component')
    app_ts = app_ts.replace('ngAfterViewInit() {', 'ngAfterViewInit() {\n    setTimeout(() => { runVanillaLogic(); }, 100);')
    
    with open(app_ts_path, 'w', encoding='utf-8') as f:
        f.write(app_ts)

print("Merged app.js logic into app-logic.ts and imported it into app.ts")
