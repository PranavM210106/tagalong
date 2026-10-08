import urllib.request
import urllib.parse
from html.parser import HTMLParser
import http.cookiejar
import sys
import time
import json

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
        return parser.token, cookie_jar, response.geturl()
    except urllib.error.HTTPError as e:
        return None, cookie_jar, e.geturl()

def post_form(url, token, cookie_jar, data):
    data['__RequestVerificationToken'] = token
    data_encoded = urllib.parse.urlencode(data).encode('utf-8')
    opener = urllib.request.build_opener(urllib.request.HTTPCookieProcessor(cookie_jar))
    request = urllib.request.Request(url, data=data_encoded, method='POST')
    request.add_header('Content-Type', 'application/x-www-form-urlencoded')
    try:
        response = opener.open(request)
        return response.getcode(), response.read().decode('utf-8'), response.geturl(), None
    except urllib.error.HTTPError as e:
        return e.code, e.read().decode('utf-8'), e.geturl(), e

def get_page(url, cookie_jar):
    opener = urllib.request.build_opener(urllib.request.HTTPCookieProcessor(cookie_jar))
    request = urllib.request.Request(url)
    try:
        response = opener.open(request)
        return response.getcode(), response.read().decode('utf-8'), response.geturl()
    except urllib.error.HTTPError as e:
        return e.code, e.read().decode('utf-8'), e.geturl()

def main():
    base_url = 'http://localhost:5019'
    cookie_jar = http.cookiejar.CookieJar()

    print('=== Testing Complete Ride Flow ===')

    # Step 1: Register a new user (Driver)
    print('\n1. Registering new driver...')
    register_url = base_url + '/Account/Register'
    timestamp = str(int(time.time()))
    email = f'driver{timestamp}@example.com'
    register_data = {
        'FullName': 'Test Driver',
        'Email': email,
        'PhoneNumber': '1234567890',
        'Password': 'SecurePass123!',
        'ConfirmPassword': 'SecurePass123!',
        'Role': 'Driver'
    }

    token, cookie_jar, _ = get_token_and_cookies(register_url, cookie_jar)
    if not token:
        print('ERROR: Could not get token for registration')
        return

    status, response_html, redirect_url, error = post_form(register_url, token, cookie_jar, register_data.copy())
    if status in [200, 302]:
        print(f'SUCCESS: Registration successful (status {status})')
        if status == 302:
            # Follow redirect to home
            _, _, home_url = get_page(redirect_url, cookie_jar)
            print(f'Redirected to: {home_url}')
    else:
        print(f'ERROR: Registration failed with status {status}')
        if error:
            print(f'Error: {error}')
        return

    # Step 2: Login with the same user (to ensure we have valid session)
    print('\n2. Logging in...')
    login_url = base_url + '/Account/Login'
    token, cookie_jar, _ = get_token_and_cookies(login_url, cookie_jar)
    if not token:
        print('ERROR: Could not get token for login')
        return

    login_data = {
        'Email': email,
        'Password': 'SecurePass123!',
        'RememberMe': 'false'
    }

    status, response_html, redirect_url, error = post_form(login_url, token, cookie_jar, login_data)
    if status in [200, 302]:
        print(f'SUCCESS: Login successful (status {status})')
        if status == 302:
            print(f'Redirected to: {redirect_url}')
    else:
        print(f'ERROR: Login failed with status {status}')
        if 'Invalid login attempt' in response_html:
            print('Error: Invalid credentials')
        return

    # Step 3: Access ride offer page (should work now that we're logged in as Driver)
    print('\n3. Accessing ride offer page...')
    offer_url = base_url + '/Rides/Offer'
    status, response_html, final_url = get_page(offer_url, cookie_jar)
    if status == 200 and '/Rides/Offer' in final_url:
        print('SUCCESS: Ride offer page accessible')
        # Extract token for form submission
        token, _, _ = get_token_and_cookies(offer_url, cookie_jar)
        if not token:
            print('ERROR: Could not get token for offer form')
            return
    else:
        print(f'ERROR: Could not access offer page. Status: {status}, URL: {final_url}')
        if status in [301, 302]:
            print(f'Redirected to: {final_url}')
        return

    # Step 4: Submit ride offer
    print('\n4. Submitting ride offer...')
    offer_data = {
        'StartLocation': 'New York, NY',
        'StartLat': '40.7128',
        'StartLng': '-74.0060',
        'EndLocation': 'Boston, MA',
        'EndLat': '42.3601',
        'EndLng': '-71.0589',
        'DepartureTime': '2026-10-01T10:00',
        'SeatsAvailable': '3',
        'PricePerSeat': '25.00',
        'VehicleId': ''  # We don't have a vehicle yet, but let's see what happens
    }

    status, response_html, redirect_url, error = post_form(offer_url, token, cookie_jar, offer_data)
    print(f'Offer submission status: {status}')
    if status == 302:
        print(f'SUCCESS: Ride offer submitted, redirected to: {redirect_url}')
    elif status == 200:
        if 'validation-summary-errors' in response_html:
            print('FORM HAS VALIDATION ERRORS:')
            # Extract errors
            from html.parser import HTMLParser
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
                for err in parser.errors:
                    print(f'  - {err}')
        else:
            print('Offer form returned 200 with no validation errors')
    else:
        print(f'ERROR: Unexpected status {status}')
        if error:
            print(f'Error: {error}')

    # Step 5: Add a vehicle first, then try ride offer again
    print('\n5. Adding a vehicle...')
    vehicle_url = base_url + '/Profile/AddVehicle'
    status, response_html, final_url = get_page(vehicle_url, cookie_jar)
    if status == 200 and '/Profile/AddVehicle' in final_url:
        print('SUCCESS: Add vehicle page accessible')
        token, _, _ = get_token_and_cookies(vehicle_url, cookie_jar)
        if not token:
            print('ERROR: Could not get token for vehicle form')
            return
    else:
        print(f'ERROR: Could not access add vehicle page. Status: {status}, URL: {final_url}')
        return

    vehicle_data = {
        'Make': 'Toyota',
        'Model': 'Camry',
        'Year': '2020',
        'Color': 'Blue',
        'LicensePlate': 'ABC123',
        'Seats': '5'
    }

    status, response_html, redirect_url, error = post_form(vehicle_url, token, cookie_jar, vehicle_data)
    print(f'Vehicle submission status: {status}')
    if status == 302:
        print(f'SUCCESS: Vehicle added, redirected to: {redirect_url}')
    elif status == 200:
        if 'validation-summary-errors' in response_html:
            print('VEHICLE FORM HAS VALIDATION ERRORS')
        else:
            print('Vehicle form returned 200 with no validation errors')
    else:
        print(f'ERROR: Unexpected status {status}')

    # Step 6: Now try to offer a ride with the vehicle
    print('\n6. Offering ride with vehicle...')
    offer_url = base_url + '/Rides/Offer'
    status, response_html, final_url = get_page(offer_url, cookie_jar)
    if status == 200 and '/Rides/Offer' in final_url:
        print('SUCCESS: Ride offer page accessible')
        token, _, _ = get_token_and_cookies(offer_url, cookie_jar)
        if not token:
            print('ERROR: Could not get token for offer form')
            return

        # We need to get the list of vehicles to get a valid VehicleId
        # Let's parse the page to see if vehicles are listed
        if 'Toyota' in response_html and 'Camry' in response_html:
            print('INFO: Vehicle appears to be in the dropdown')
        else:
            print('WARNING: Vehicle may not be in dropdown')

        # Get vehicles via API or profile page
        profile_url = base_url + '/Profile/Index'
        status, profile_html, profile_url = get_page(profile_url, cookie_jar)
        if status == 200:
            # Extract vehicle ID from profile page if possible
            import re
            # Look for vehicle IDs in the HTML
            vehicle_matches = re.findall(r'value="(\d+)"', profile_html)
            if vehicle_matches:
                vehicle_id = vehicle_matches[0]
                print(f'Found vehicle ID: {vehicle_id}')
                offer_data['VehicleId'] = vehicle_id
            else:
                print('Could not extract vehicle ID, trying empty')
                offer_data['VehicleId'] = ''
        else:
            print('Could not access profile page')
            offer_data['VehicleId'] = ''

        status, response_html, redirect_url, error = post_form(offer_url, token, cookie_jar, offer_data)
        print(f'Offer submission status: {status}')
        if status == 302:
            print(f'SUCCESS: Ride offer submitted with vehicle, redirected to: {redirect_url}')
        elif status == 200:
            if 'validation-summary-errors' in response_html:
                print('FORM HAS VALIDATION ERRORS:')
                from html.parser import HTMLParser
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
                    for err in parser.errors:
                        print(f'  - {err}')
            else:
                print('Offer form returned 200 with no validation errors')
        else:
            print(f'ERROR: Unexpected status {status}')
            if error:
                print(f'Error: {error}')

    print('\n=== Test Complete ===')

if __name__ == '__main__':
    main()