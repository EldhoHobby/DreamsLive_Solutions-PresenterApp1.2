from cryptography.hazmat.primitives.asymmetric import rsa
from cryptography.hazmat.primitives import serialization

# Generate private key
private_key = rsa.generate_private_key(
    public_exponent=65537,
    key_size=2048,
)

# Serialize private key to PEM format
pem_private = private_key.private_bytes(
   encoding=serialization.Encoding.PEM,
   format=serialization.PrivateFormat.PKCS8,
   encryption_algorithm=serialization.NoEncryption()
)

# Save the private key to a file
with open('private.key', 'wb') as f:
    f.write(pem_private)

# Get the public key
public_key = private_key.public_key()

# Serialize public key to PEM format
pem_public = public_key.public_bytes(
   encoding=serialization.Encoding.PEM,
   format=serialization.PublicFormat.SubjectPublicKeyInfo
)

print("Private key saved to private.key")
print("IMPORTANT: Keep this file safe and private. Do not commit it to version control.")
print("\n----- COPY THE PUBLIC KEY BELOW AND PASTE INTO YOUR C# CODE -----\n")
print(pem_public.decode('utf-8'))
