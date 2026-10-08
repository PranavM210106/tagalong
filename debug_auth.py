import urllib.request
import urllib.parse
from html.parser import HTMLParser
import http.cookiejar

class TokenParser(HTMLParser):
    def __init__(self):
        super().__init__()
        self.token = None

    def handle_starttag(self, tag, attrs):
        if tag == 'input':
            attrs_dict = dict(attrs)
            if attrs_dict.get('name') == '__RequestVerificationToken':
                self.token = attrs_dict.get('value')

def get_token_and_cookies(url, cookie_jar=None):
    if cookie_jar is None:
        cookie_jar = http.cookiejar.CookieJar()
    opener = urllib.request.build_opener(urllib.request.HTTPCookieProcessor(cookie_jar))
    request = urllib.request.Request(url)
    try:
        response = opener.open(request)
        html = response.read().decode('utf-8')
        parser = TokenParser()
        parser.feed(html)
        return parser.token, cookie_jar, response.geturl(), response.headers
    except urllib.error.HTTPError as e:
        return None, cookie_jar, e.geturl(), e.headers if hasattr(e, 'headers') else {}

def post_form(url, token, cookie_jar, data):
    data['__RequestVerificationToken'] = token
    data_encoded = urllib.parse.urlencode(data).encode('utf-8')
    opener = urllib.request.build_opener(urllib.request.HTTPCookieProcessor(cookie_jar))
    request = urllib.request.Request(url, data=data_encoded, method='POST')
    request.add_header('Content-Type', 'application/x-www-form-urlencoded')
    try:
        response = opener.open(request)
        return response.getcode(), response.read().decode('utf-8'), response.geturl(), response.headers, None
    except urllib.error.HTTPError as e:
        return e.code, e.read().decode('utf-8'), e.geturl(), e.headers if hasattr(e, 'headers') else {}, e

def get_page(url, cookie_jar):
    opener = urllib.request.build_opener(urllib.request.HTTPCookieProcessor(cookie_jar))
    request = urllib.request.Request(url)
    try:
        response = opener.open(request)
        return response.getcode(), response.read().decode('utf-8'), response.geturl(), response.headers
    except urllib.error.HTTPError as e:
        return e.code, e.read().decode('utf-8'), e.geturl(), e.headers if hasattr(e, 'headers') else {}

def main():
    base_url = 'http://localhost:5018'
    cookie_jar = http.cookiejar.CookieJar()

    print('=== Debugging Auth Flow ===')

    # Step 1: Register
    print('\n1. Registering...')
    register_url = base_url + '/Account/Register'
    token, cookie_jar, _, reg_headers = get_token_and_cookies(register_url, cookie_jar)
    print(f'Token: {token[:20] if token else None}')
    print(f'Cookies after GET: {list(cookie_jar)}')

    if not token:
        print('ERROR: No token')
        return

    register_data = {
        'FullName': 'Debug User',
        'Email': f'debug{int(__import__("time").time())}@example.com',
        'PhoneNumber': '1234567890',
        'Password': 'SecurePass123!',
        'ConfirmPassword': 'SecurePass123!',
        'Role': 'Driver'
    }

    status, response_html, redirect_url, post_headers, error = post_form(register_url, token, cookie_jar, register_data)
    print(f'Registration POST status: {status}')
    print(f'Redirect URL: {redirect_url}')
    print(f'Cookies after POST: {list(cookie_jar)}')
    if status in [200, 302]:
        print('Registration successful')
    else:
        print(f'Registration failed: {status}')
        if error:
            print(f'Error: {error}')
        return

    # Step 2: Login
    print('\n2. Logging in...')
    login_url = base_url + '/Account/Login'
    token, cookie_jar, _, login_get_headers = get_token_and_cookies(login_url, cookie_jar)
    print(f'Login GET token: {token[:20] if token else None}')
    print(f'Cookies before login POST: {list(cookie_jar)}')

    if not token:
        print('ERROR: No token for login')
        return

    login_data = {
        'Email': register_data['Email'],
        'Password': register_data['Password'],
        'RememberMe': 'false'
    }

    status, response_html, redirect_url, post_headers, error = post_form(login_url, token, cookie_jar, login_data)
    print(f'Login POST status: {status}')
    print(f'Redirect URL: {redirect_url}')
    print(f'Cookies after login POST: {list(cookie_jar)}')
    print(f'Response headers: {list(post_headers.items())}')
    if status in [200, 302]:
        print('Login successful')
        if status == 302:
            print(f'Redirected to: {redirect_url}')
    else:
        print(f'Login failed: {status}')
        if 'Invalid login attempt' in response_html:
            print('Error: Invalid credentials')
        return

    # Step 3: Access Offer page
    print('\n3. Accessing Ride Offer page...')
    offer_url = base_url + '/Rides/Offer'
    status, response_html, final_url, headers = get_page(offer_url, cookie_jar)
    print(f'Offer page status: {status}')
    print(f'Final URL: {final_url}')
    print(f'Cookies: {list(cookie_jar)}')
    if status == 200:
        print('SUCCESS: Offer page accessible')
        # Check if we see the form
        if '<form' in response_html and 'Offer' in response_html:
            print('Form found in response')
        else:
            print('Warning: Form not clearly visible')
    elif status in [301, 302]:
        print(f'Redirected to: {final_url}')
        if 'Account/Login' in final_url:
            print('REDRECTED TO LOGIN - AUTH FAILED')
            # Check if we have the auth cookie
            auth_cookies = [c for c in cookie_jar if '.AspNetCore.Identity' in c.name or 'Identity' in c.name]
            print(f'Identity cookies: {auth_cookies}')
    else:
        print(f'ERROR: Unexpected status {status}')
        if status == 500:
            print('Internal Server Error - check server logs')
            # Print snippet of response
            print('Response snippet:')
            print(response_html[:500] if response_html else 'No response')

if __name__ == '__main__':
    main()