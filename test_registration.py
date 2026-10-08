import requests
import re
from bs4 import BeautifulSoup

# Base URL
base_url = "http://localhost:5018"

# Session to maintain cookies
session = requests.Session()

def test_registration():
    # Step 1: GET the registration page to get anti-forgery token
    print("Step 1: GET /Account/Register")
    register_page = session.get(f"{base_url}/Account/Register")
    print(f"Status Code: {register_page.status_code}")

    if register_page.status_code != 200:
        print(f"Failed to get registration page: {register_page.status_code}")
        return

    # Parse HTML to extract anti-forgery token
    soup = BeautifulSoup(register_page.text, 'html.parser')
    token_input = soup.find('input', {'name': '__RequestVerificationToken'})

    if not token_input:
        print("Could not find anti-forgery token")
        print("First 500 chars of response:")
        print(register_page.text[:500])
        return

    token_value = token_input.get('value')
    print(f"Anti-forgery token: {token_value[:50]}...")

    # Step 2: POST registration data with token
    print("\nStep 2: POST /Account/Register with registration data")
    registration_data = {
        'FullName': 'Test User',
        'Email': 'test@example.com',
        'PhoneNumber': '1234567890',
        'Password': 'SecurePass123!',
        'ConfirmPassword': 'SecurePass123!',
        'Role': 'Rider',
        '__RequestVerificationToken': token_value
    }

    response = session.post(
        f"{base_url}/Account/Register",
        data=registration_data,
        headers={'Content-Type': 'application/x-www-form-urlencoded'}
    )

    print(f"Status Code: {response.status_code}")
    print(f"Response URL: {response.url}")

    if response.status_code == 200:
        print("Registration form returned (likely validation error or success page)")
        # Check if we're redirected or if there are errors
        if 'Index' in response.url or 'Home' in response.url:
            print("SUCCESS: Redirected to home page after registration")
        else:
            # Check for validation errors
            soup = BeautifulSoup(response.text, 'html.parser')
            validation_errors = soup.find_all('span', class_='text-danger')
            if validation_errors:
                print("Validation errors found:")
                for error in validation_errors:
                    print(f"  - {error.get_text()}")
            else:
                print("No validation errors found - might be successful registration")
                print("First 1000 chars of response:")
                print(response.text[:1000])
    elif response.status_code == 302:
        print("SUCCESS: Redirected after registration (status 302)")
        print(f"Redirect location: {response.headers.get('Location')}")
    else:
        print(f"Unexpected status code: {response.status_code}")
        print("Response headers:")
        for key, value in response.headers.items():
            print(f"  {key}: {value}")
        print("First 500 chars of response:")
        print(response.text[:500])

if __name__ == "__main__":
    test_registration()