# Key Generator Tool

This tool is for the developer to generate license keys for users of the PresenterApp.

## Setup

1.  **Install prerequisites**:
    ```bash
    pip install cryptography
    ```
2.  **Generate a private key**:
    Run the `generate_private_key.py` script to create your unique `private.key` file.
    ```bash
    python generate_private_key.py
    ```
    **IMPORTANT**: Keep this `private.key` file safe and private. **DO NOT** commit it to version control or distribute it.

## Usage

Run the script from the command line, providing the user's machine ID and the desired license type. The script will print the full license key to the console. Copy this key and send it to your user for them to paste into the activation form.

### Generate a permanent license:
This license never expires and has no usage limit.
```bash
python generate_license.py <machine_id>
```

### Generate a time-limited license:
This license will expire after the specified date.
```bash
python generate_license.py <machine_id> --expiry-date YYYY-MM-DD
```

### Generate a usage-limited license:
This license will expire after the specified number of uses.
```bash
python generate_license.py <machine_id> --uses 500
```
