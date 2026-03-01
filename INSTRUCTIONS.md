# Instructions to Set Up and Use the License Key Generator

This guide provides the complete, one-time setup process for the license key generator and instructions on how to create license keys for your application.

## Part 1: One-Time Setup

This part is crucial and only needs to be done once. It ensures that the key generator and the application are using a matching pair of private and public keys.

### Step 1: Install Python Dependencies

1.  Open a terminal or command prompt.
2.  Navigate to the `KeyGeneratorTool` directory within your project.
3.  Run the following command to install the necessary library:
    ```bash
    pip install -r requirements.txt
    ```

### Step 2: Generate Your Unique Key Pair

1.  In the same terminal, run this command:
    ```bash
    python generate_private_key.py
    ```
2.  This will do two things:
    *   Create a `private.key` file in the directory. **Keep this file safe and private.**
    *   Print a **Public Key** to the console.

### Step 3: Update the Application's Public Key

1.  Copy the entire public key from the console output, including the `-----BEGIN PUBLIC KEY-----` and `-----END PUBLIC KEY-----` lines.
2.  Open the `SecureLicenseManager.cs` file in your C# project.
3.  Find the `PublicKey` constant at the top of the file.
4.  **Replace the existing hardcoded public key** with the new one you just copied.
5.  Save the `SecureLicenseManager.cs` file and rebuild your application.

Your key generator and your application are now cryptographically linked.

---

## Part 2: Generating License Keys

Once the one-time setup is complete, you can generate as many license keys as you need.

### Step 1: Obtain the User's Machine ID

1.  Launch the Presenter application.
2.  Click the **"Help"** button to open the **Software Activation** window.
3.  Copy the **Machine ID** displayed in the window.

### Step 2: Generate the License Key

1.  Go back to your terminal in the `KeyGeneratorTool` directory.
2.  Use one of the following commands, replacing `USER_MACHINE_ID` with the ID you just copied.

    *   **Permanent License** (unlimited uses, no expiration):
        ```bash
        python generate_license.py USER_MACHINE_ID
        ```

    *   **Time-Limited License** (expires on a specific date):
        ```bash
        python generate_license.py USER_MACHINE_ID --expiry-date YYYY-MM-DD
        ```
        (e.g., `2024-12-31`)

    *   **Usage-Limited License** (valid for a set number of uses):
        ```bash
        python generate_license.py USER_MACHINE_ID --uses 10
        ```
        (e.g., for `10` uses)

3.  The script will output a **License Key**. Copy the entire key.

### Step 3: Activate the Software

1.  Return to the **Software Activation** window in the application.
2.  Paste the generated license key into the text box.
3.  Click **"Activate"**. The application will restart and be activated.
