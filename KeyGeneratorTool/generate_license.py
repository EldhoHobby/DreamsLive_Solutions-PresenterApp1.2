import argparse
import base64
from cryptography.hazmat.primitives import hashes
from cryptography.hazmat.primitives.asymmetric import padding
from cryptography.hazmat.primitives.serialization import load_pem_private_key

def generate_license(machine_id, expiry_date=None, uses=None):
    try:
        with open("private.key", "rb") as key_file:
            private_key = load_pem_private_key(
                key_file.read(),
                password=None,
            )

        data = f"MachineID:{machine_id}\n"
        if expiry_date:
            data += f"ExpiryDate:{expiry_date}\n"
        if uses:
            data += f"Uses:{uses}\n"

        signature = private_key.sign(
            data.encode('utf-8'),
            padding.PKCS1v15(),
            hashes.SHA256()
        )

        encoded_signature = base64.b64encode(signature).decode('utf-8')
        license_key = f"{base64.b64encode(data.encode('utf-8')).decode('utf-8')}---SIGNATURE---{encoded_signature}"

        print("License Key:")
        print(license_key)

    except FileNotFoundError:
        print("Error: private.key not found.")
    except Exception as e:
        print(f"An error occurred: {e}")

if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="Generate a license key.")
    parser.add_argument("machine_id", help="The machine ID to license.")
    parser.add_argument("--expiry-date", default=None, help="Optional expiry date in YYYY-MM-DD format.")
    parser.add_argument("--uses", type=int, default=None, help="Optional number of uses for the license.")
    args = parser.parse_args()

    generate_license(args.machine_id, args.expiry_date, args.uses)
