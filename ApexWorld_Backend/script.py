import os
import re

def process_html_file(filepath):
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()

    # We need to find all <input>, <select>, <textarea> tags that have the 'required' attribute.
    # Then we need to find their corresponding <label> and add the red *.
    # And add #name="ngModel" and the [class.is-invalid-custom] and the error div.

    # This is tricky because the label might be before or after the input.
    # Usually it's:
    # <label for="username">Username or Email</label>
    # <input type="text" id="username" name="username" [(ngModel)]="..." required>
    
    # Or in Reactive Forms:
    # <label>Property Title</label>
    # <input formControlName="title" required>

    pass

