#!/usr/bin/env bash
set -eo pipefail
# ---------------------------------------------------------------------------
# generate-certs.sh
# Generates an OPC UA user certificate and places it in the correct locations
# for the opc-ua-certificate Docker container and the test project.
#
# Usage: ./generate-certs.sh [OPTIONS]
#   -p, --password    PFX password (default: yourpassword)
#   -o, --output      Output directory for PFX and source files (default: ./Volumes)
#   -k, --pki         PKI directory to mount into the container (default: ./Volumes/pki)
#   -h, --help        Show this help message
# ---------------------------------------------------------------------------

# Force Linux binaries, prevent WSL from picking up Windows openssl or other tools
export PATH="/usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin"

# Defaults
PFX_PASSWORD="yourpassword"
OUTPUT_DIR="./Volumes"
PKI_DIR="./Volumes/pki"

# Parse arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        -p|--password)  PFX_PASSWORD="$2"; shift 2 ;;
        -o|--output)    OUTPUT_DIR="$2";   shift 2 ;;
        -k|--pki)       PKI_DIR="$2";      shift 2 ;;
        -h|--help)
            sed -n '/^# Usage/,/^# ---/p' "$0" | head -n -1
            exit 0
            ;;
        *) echo "Unknown option: $1"; exit 1 ;;
    esac
done

TRUSTED_USER_DIR="$PKI_DIR/trusted-user/certs"
WORK_DIR=$(mktemp -d)
trap 'rm -rf "$WORK_DIR"' EXIT

echo "==> Generating OPC UA user certificate..."

# Write extensions config
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
URI.1 = urn:opcua:client:user
EOF

# Generate private key and self-signed certificate
openssl req -x509 -newkey rsa:2048 -days 365 -nodes \
    -keyout "$WORK_DIR/user.key" \
    -out    "$WORK_DIR/user.crt" \
    -subj   "/CN=opcua-client-user" \
    -extensions v3_ca \
    -config "$WORK_DIR/user_cert_ext.cnf"

echo "==> Verifying certificate extensions..."
CERT_TEXT=$(openssl x509 -in "$WORK_DIR/user.crt" -text -noout)
for REQUIRED in "Digital Signature" "Non Repudiation" "Key Encipherment" "Data Encipherment" "TLS Web Client Authentication" "CA:FALSE" "URI:urn:opcua:client:user"; do
    if ! echo "$CERT_TEXT" | grep -q "$REQUIRED"; then
        echo "ERROR: Required extension not found: $REQUIRED"
        exit 1
    fi
done
echo "    All required extensions present."

# Convert to DER for the server trust store
openssl x509 -in "$WORK_DIR/user.crt" -outform DER -out "$WORK_DIR/user.der"

# Create PFX with unencrypted private key (required by OPC UA SDK)
openssl pkcs12 -export \
    -keypbe NONE -certpbe NONE \
    -name opcua-client-user \
    -in      "$WORK_DIR/user.crt" \
    -inkey   "$WORK_DIR/user.key" \
    -out     "$WORK_DIR/user.pfx" \
    -passout "pass:$PFX_PASSWORD"

echo "==> Creating directory structure..."
mkdir -p "$TRUSTED_USER_DIR"
mkdir -p "$PKI_DIR/own"
mkdir -p "$PKI_DIR/trusted/certs"
mkdir -p "$PKI_DIR/issuer/certs"
mkdir -p "$PKI_DIR/issuer-user/certs"
mkdir -p "$PKI_DIR/rejected/certs"
mkdir -p "$OUTPUT_DIR"

echo "==> Setting permissions on PKI directory..."
chmod -R 777 "$PKI_DIR"

echo "==> Copying DER to trusted-user store: $TRUSTED_USER_DIR"
cp "$WORK_DIR/user.der" "$TRUSTED_USER_DIR/user.der"

echo "==> Copying PFX to output directory: $OUTPUT_DIR"
cp "$WORK_DIR/user.pfx" "$OUTPUT_DIR/user.pfx"

cat > $OUTPUT_DIR/nodesfile.json << 'EOF'
{
  "Folder": "WritableNodes",
  "NodeList": [
    {
      "NodeId": "WriteTest_Boolean",
      "Name": "WriteTest_Boolean",
      "DataType": "Boolean",
      "AccessLevel": "CurrentReadOrWrite"
    },
    {
      "NodeId": "WriteTest_SByte",
      "Name": "WriteTest_SByte",
      "DataType": "SByte",
      "AccessLevel": "CurrentReadOrWrite"
    },
    {
      "NodeId": "WriteTest_Byte",
      "Name": "WriteTest_Byte",
      "DataType": "Byte",
      "AccessLevel": "CurrentReadOrWrite"
    },
    {
      "NodeId": "WriteTest_Int16",
      "Name": "WriteTest_Int16",
      "DataType": "Int16",
      "AccessLevel": "CurrentReadOrWrite"
    },
    {
      "NodeId": "WriteTest_UInt16",
      "Name": "WriteTest_UInt16",
      "DataType": "UInt16",
      "AccessLevel": "CurrentReadOrWrite"
    },
    {
      "NodeId": "WriteTest_Int32",
      "Name": "WriteTest_Int32",
      "DataType": "Int32",
      "AccessLevel": "CurrentReadOrWrite"
    },
    {
      "NodeId": "WriteTest_UInt32",
      "Name": "WriteTest_UInt32",
      "DataType": "UInt32",
      "AccessLevel": "CurrentReadOrWrite"
    },
    {
      "NodeId": "WriteTest_Int64",
      "Name": "WriteTest_Int64",
      "DataType": "Int64",
      "AccessLevel": "CurrentReadOrWrite"
    },
    {
      "NodeId": "WriteTest_UInt64",
      "Name": "WriteTest_UInt64",
      "DataType": "UInt64",
      "AccessLevel": "CurrentReadOrWrite"
    },
    {
      "NodeId": "WriteTest_Float",
      "Name": "WriteTest_Float",
      "DataType": "Float",
      "AccessLevel": "CurrentReadOrWrite"
    },
    {
      "NodeId": "WriteTest_Double",
      "Name": "WriteTest_Double",
      "DataType": "Double",
      "AccessLevel": "CurrentReadOrWrite"
    },
    {
      "NodeId": "WriteTest_String",
      "Name": "WriteTest_String",
      "DataType": "String",
      "AccessLevel": "CurrentReadOrWrite"
    },
    {
      "NodeId": "WriteTest_DateTime",
      "Name": "WriteTest_DateTime",
      "DataType": "DateTime",
      "AccessLevel": "CurrentReadOrWrite"
    },
    {
      "NodeId": "WriteTest_Guid",
      "Name": "WriteTest_Guid",
      "DataType": "Guid",
      "AccessLevel": "CurrentReadOrWrite"
    },
    {
      "NodeId": "WriteTest_ByteString",
      "Name": "WriteTest_ByteString",
      "DataType": "ByteString",
      "AccessLevel": "CurrentReadOrWrite"
    }
  ]
}
EOF

echo ""
echo "==> Done. Summary:"
echo "    PFX file:      $OUTPUT_DIR/user.pfx"
echo "    Trusted cert:  $TRUSTED_USER_DIR/user.der"
echo "    PKI root:      $PKI_DIR"