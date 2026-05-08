#!/usr/bin/env bash
set -euo pipefail

# ---------------------------------------------------------------------------
# generate-certs.sh (FIXED for Git Bash / MSYS2 + Windows OpenSSL)
# ---------------------------------------------------------------------------

PFX_PASSWORD="yourpassword"
OUTPUT_DIR="./Volumes"
PKI_DIR="./Volumes/pki"

while [[ $# -gt 0 ]]; do
    case $1 in
        -p|--password) PFX_PASSWORD="$2"; shift 2 ;;
        -o|--output)   OUTPUT_DIR="$2"; shift 2 ;;
        -k|--pki)      PKI_DIR="$2"; shift 2 ;;
        -h|--help)
            sed -n '/^# Usage/,/^# ---/p' "$0" | head -n -1
            exit 0
            ;;
        *) echo "Unknown option: $1"; exit 1 ;;
    esac
done

TRUSTED_USER_DIR="$PKI_DIR/trusted-user/certs"

# -----------------------------
# FIX: avoid mktemp /tmp issues
# -----------------------------
WORK_DIR="$PWD/.tmp-opcua-certs"
rm -rf "$WORK_DIR"
mkdir -p "$WORK_DIR"

trap 'rm -rf "$WORK_DIR"' EXIT

echo "==> Using WORK_DIR: $WORK_DIR"

# -----------------------------
# Path conversion helper
# -----------------------------
to_winpath() {
    if command -v cygpath >/dev/null 2>&1; then
        cygpath -w "$1"
    else
        echo "$1"
    fi
}

echo "==> Generating OPC UA user certificate..."

# Write OpenSSL config
cat > "$WORK_DIR/user_cert_ext.cnf" << 'EOF'
[req]
req_extensions = v3_req
distinguished_name = req_distinguished_name
x509_extensions = v3_ca

[req_distinguished_name]

[v3_ca]
subjectKeyIdentifier = hash
authorityKeyIdentifier = keyid:always,issuer
basicConstraints = critical, CA:FALSE
keyUsage = critical, digitalSignature, nonRepudiation, keyEncipherment, dataEncipherment
extendedKeyUsage = clientAuth
subjectAltName = @alt_names

[v3_req]
subjectKeyIdentifier = hash
basicConstraints = critical, CA:FALSE
keyUsage = critical, digitalSignature, nonRepudiation, keyEncipherment, dataEncipherment
extendedKeyUsage = clientAuth
subjectAltName = @alt_names

[alt_names]
DNS.1 = localhost
URI.1 = urn:opcua:client:user
EOF

CFG=$(to_winpath "$WORK_DIR/user_cert_ext.cnf")
KEY=$(to_winpath "$WORK_DIR/user.key")
CRT=$(to_winpath "$WORK_DIR/user.crt")

# Generate key + cert
MSYS_NO_PATHCONV=1 openssl req -x509 -newkey rsa:2048 -days 365 -nodes \
    -keyout "$KEY" \
    -out "$CRT" \
    -subj "/CN=opcua-client-user" \
    -extensions v3_ca \
    -config "$CFG"

echo "==> Verifying certificate extensions..."
CERT_TEXT=$(openssl x509 -in "$WORK_DIR/user.crt" -text -noout)

for REQUIRED in "Digital Signature" "Non Repudiation" "Key Encipherment" "Data Encipherment" "TLS Web Client Authentication" "CA:FALSE" "urn:opcua:client:user"; do
    if ! echo "$CERT_TEXT" | grep -q "$REQUIRED"; then
        echo "ERROR: Missing extension: $REQUIRED"
        exit 1
    fi
done

# DER conversion
DER=$(to_winpath "$WORK_DIR/user.der")
openssl x509 -in "$CRT" -outform DER -out "$DER"

# PFX export
PFX=$(to_winpath "$WORK_DIR/user.pfx")
openssl pkcs12 -export \
    -keypbe NONE -certpbe NONE \
    -name opcua-client-user \
    -in "$CRT" \
    -inkey "$KEY" \
    -out "$PFX" \
    -passout "pass:$PFX_PASSWORD"

echo "==> Creating directories..."
mkdir -p "$TRUSTED_USER_DIR"
mkdir -p "$PKI_DIR/own"
mkdir -p "$PKI_DIR/trusted/certs"
mkdir -p "$PKI_DIR/issuer/certs"
mkdir -p "$PKI_DIR/issuer-user/certs"
mkdir -p "$PKI_DIR/rejected/certs"
mkdir -p "$OUTPUT_DIR"

echo "==> Copying files..."

cp "$WORK_DIR/user.der" "$TRUSTED_USER_DIR/user.der"
cp "$WORK_DIR/user.pfx" "$OUTPUT_DIR/user.pfx"

echo ""
echo "==> DONE"
echo "    PFX password : $PFX_PASSWORD"
echo "    PFX file     : $OUTPUT_DIR/user.pfx"
echo "    DER file     : $TRUSTED_USER_DIR/user.der"

echo ""
echo "==> Thumbprint:"
openssl x509 -in "$WORK_DIR/user.crt" -noout -fingerprint -sha1 | cut -d= -f2

echo ""
echo "==> Ready for Docker:"
echo "    docker compose up -d"