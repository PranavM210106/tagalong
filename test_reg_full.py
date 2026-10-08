import urllib.request
import urllib.parse
from html.parser import HTMLParser
import http.cookiejar
import sys
import time

class MyHTMLParser(HTMLParser):
    def __init__(self):
        super().__init__()
        self.token = None

    def handle_starttag(self, tag, attrs):
        if tag == 'input':
            attrs_dict = dict(attrs)
            if attrs_dict.get('name') == '__RequestVerificationToken':
                self.token = attrs_dict.get('value')

def get_token_and_cookies(url):
    cookie_jar = http.cookiejar.CookieJar()
    opener = urllib.request.build_opener(urllib.request.HTTPCookieProcessor(cookie_jar))
    request = urllib.request.Request(url)
    response = opener.open(request)
    html = response.read().decode('utf-8')
    parser = MyHTMLParser()
    parser.feed(html)
    return parser.token, cookie_jar

def post_registration(url, token, cookie_jar, data):
    data['__RequestVerificationToken'] = token
    data_encoded = urllib.parse.urlencode(data).encode('utf-8')
    opener = urllib.request.build_opener(urllib.request.HTTPCookieProcessor(cookie_jar))
    request = urllib.request.Request(url, data=data_encoded, method='POST')
    request.add_header('Content-Type', 'application/x-www-form-urlencoded')
    try:
        response = opener.open(request)
        return response.getcode(), response.read().decode('utf-8'), response.geturl()
    except urllib.error.HTTPError as e:
        return e.code, e.read().decode('utf-8'), e.geturl()

def get_page(url, cookie_jar):
    opener = urllib.request.build_opener(urllib.request.HTTPCookieProcessor(cookie_jar))
    request = urllib.request.Request(url)
    try:
        response = opener.open(request)
        return response.getcode(), response.read().decode('utf-8'), response.geturl()
    except urllib.error.HTTPError as e:
        return e.code, e.read().decode('utf-8'), e.geturl()

if __name__ == '__main__':
    base_url = 'http://localhost:5018'
    register_url = base_url + '/Account/Register'

    print('Step 1: GET registration page')
    token, cookie_jar = get_token_and_cookies(register_url)
    if token is None:
        print('ERROR: Could not find anti-forgery token')
        sys.exit(1)
    print(f'Token: {token[:20]}...')

    # Prepare registration data with a unique email
    timestamp = str(int(time.time()))
    email = f'test{timestamp}@example.com'
    data = {
        'FullName': 'Test User',
        'Email': email,
        'PhoneNumber': '1234567890',
        'Password': 'SecurePass123!',
        'ConfirmPassword': 'SecurePass123!',
        'Role': 'Rider'
    }

    print(f'Step 2: POST registration for {email}')
    status, response_html, redirect_url = post_registration(register_url, token, cookie_jar, data)
    print(f'Status: {status}')
    print(f'Redirect URL: {redirect_url}')

    if status == 302:
        print('SUCCESS: Redirected after registration')
        # Follow redirect to home
        home_status, home_html, home_url = get_page(redirect_url, cookie_jar)
        print(f'Home page status: {home_status}')
        print(f'Home page URL: {home_url}')
        if home_status == 200:
            # Check for signs of being logged in (e.g., logout link, user name)
            if 'Logout' in home_html or '/Account/Logout' in home_html:
                print('SUCCESS: Found logout link in home page - user is logged in')
            elif 'Hello' in home_html or 'Welcome' in home_html:
                print('SUCCESS: Found greeting in home page')
            else:
                print('INFO: No obvious login indicators, but registration redirected to home')
        else:
            print(f'ERROR: Home page returned status {home_status}')
    elif status == 200:
        # Check for validation errors
        if 'validation-summary-errors' in response_html or 'field-validation-error' in response_html:
            print('FAILURE: Validation errors present')
            # Extract errors for display
            class ErrorParser(HTMLParser):
                def __init__(self):
                    super().__init__()
                    self.in_error = False
                    self.errors = []
                def handle_starttag(self, tag, attrs):
                    if tag == 'span':
                        attrs_dict = dict(attrs)
                        if 'field-validation-error' in attrs_dict.get('class', ''):
                            self.in_error = True
                def handle_data(self, data):
                    if self.in_error:
                        self.errors.append(data.strip())
                        self.in_error = False
            parser = ErrorParser()
            parser.feed(response_html)
            if parser.errors:
                print('Errors:', parser.errors)
        else:
            print('INFO: Registration form returned 200 with no validation errors')
            # Might be a success page with message
            if 'Thank you' in response_html or 'Welcome' in response_html or 'registered' in response_html.lower():
                print('SUCCESS: Found success message in response')
            else:
                print('INFO: No success message, but no errors either')
    else:
        print(f'Unexpected status: {status}')
        print('Response snippet:')
        print(response_html[:500])