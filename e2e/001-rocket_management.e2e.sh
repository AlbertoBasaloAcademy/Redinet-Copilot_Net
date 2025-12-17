#!/usr/bin/env bash
set -euo pipefail

BASE_URL="${BASE_URL:-http://localhost:5277}"
PROJECT_PATH="${PROJECT_PATH:-./Redinet-Copilot_Net.csproj}"

fail() {
  echo "FAIL: $*" >&2
  exit 1
}

require_cmd() {
  command -v "$1" >/dev/null 2>&1 || fail "Missing required command: $1"
}

assert_status() {
  local actual="$1"
  local expected="$2"
  [[ "$actual" == "$expected" ]] || fail "Expected HTTP $expected, got $actual"
}

assert_body_contains() {
  local file="$1"
  local needle="$2"
  grep -Fq "$needle" "$file" || {
    echo "--- response body ---" >&2
    cat "$file" >&2
    echo "---------------------" >&2
    fail "Expected body to contain: $needle"
  }
}

http_json() {
  local method="$1"
  local url="$2"
  local json_body="$3"
  local header_out="$4"
  local body_out="$5"

  curl -sS -D "$header_out" -o "$body_out" -w "%{http_code}" \
    -H "Content-Type: application/json" \
    -X "$method" \
    --data "$json_body" \
    "$url"
}

http_get() {
  local url="$1"
  local header_out="$2"
  local body_out="$3"

  curl -sS -D "$header_out" -o "$body_out" -w "%{http_code}" "$url"
}

read_location_header() {
  local header_file="$1"
  awk -F': ' 'tolower($1)=="location" {print $2}' "$header_file" | tr -d '\r' | tail -n 1
}

cleanup() {
  if [[ -n "${APP_PID:-}" ]]; then
    kill "$APP_PID" >/dev/null 2>&1 || true

    # Fallback for Git Bash on Windows
    if command -v taskkill.exe >/dev/null 2>&1; then
      taskkill.exe /PID "$APP_PID" /T /F >/dev/null 2>&1 || true
    fi

    wait "$APP_PID" >/dev/null 2>&1 || true
  fi

  if [[ -n "${TMP_DIR:-}" && -d "${TMP_DIR:-}" ]]; then
    rm -rf "$TMP_DIR" || true
  fi
}

trap cleanup EXIT

require_cmd curl
require_cmd dotnet

TMP_DIR="$(mktemp -d 2>/dev/null || echo "")"
if [[ -z "$TMP_DIR" || ! -d "$TMP_DIR" ]]; then
  TMP_DIR="./.tmp-e2e-001"
  rm -rf "$TMP_DIR"
  mkdir -p "$TMP_DIR"
fi

echo "Building..."
dotnet build "$PROJECT_PATH" >/dev/null

echo "Starting API at $BASE_URL ..."
ASPNETCORE_URLS="$BASE_URL" dotnet run --no-build --project "$PROJECT_PATH" >/dev/null 2>&1 &
APP_PID=$!

echo "Waiting for API readiness..."
ready=0
for _ in $(seq 1 60); do
  hdr="$TMP_DIR/ready.hdr"
  body="$TMP_DIR/ready.body"
  code="$(http_get "$BASE_URL/" "$hdr" "$body" || true)"
  if [[ "$code" == "200" ]] && grep -Fq "Hello World!" "$body"; then
    ready=1
    break
  fi
  sleep 0.5
done
[[ "$ready" == "1" ]] || fail "API did not become ready at $BASE_URL"

# --- Negative create cases (400 + exact error messages) ---

echo "Testing: POST /rockets missing name -> 400"
code="$(http_json POST "$BASE_URL/rockets" '{"capacity":1}' "$TMP_DIR/t1.hdr" "$TMP_DIR/t1.body")"
assert_status "$code" "400"
assert_body_contains "$TMP_DIR/t1.body" '"error":"Name is required"'

echo "Testing: POST /rockets missing capacity -> 400"
code="$(http_json POST "$BASE_URL/rockets" '{"name":"Falcon"}' "$TMP_DIR/t2.hdr" "$TMP_DIR/t2.body")"
assert_status "$code" "400"
assert_body_contains "$TMP_DIR/t2.body" '"error":"Capacity must be > 0 and <= 10"'

echo "Testing: POST /rockets capacity > 10 -> 400"
code="$(http_json POST "$BASE_URL/rockets" '{"name":"Falcon","capacity":11}' "$TMP_DIR/t3.hdr" "$TMP_DIR/t3.body")"
assert_status "$code" "400"
assert_body_contains "$TMP_DIR/t3.body" '"error":"Capacity must be > 0 and <= 10"'

echo "Testing: POST /rockets invalid range -> 400"
code="$(http_json POST "$BASE_URL/rockets" '{"name":"Falcon","capacity":1,"range":"PLUTO"}' "$TMP_DIR/t4.hdr" "$TMP_DIR/t4.body")"
assert_status "$code" "400"
assert_body_contains "$TMP_DIR/t4.body" '"error":"Range must be one of: LEO, MOON, MARS"'

# --- Happy path create (201 + Location + response mapping) ---

echo "Testing: POST /rockets valid -> 201"
code="$(http_json POST "$BASE_URL/rockets" '{"name":"  Falcon  ","capacity":3,"speed":123,"range":"moon"}' "$TMP_DIR/t5.hdr" "$TMP_DIR/t5.body")"
assert_status "$code" "201"

location="$(read_location_header "$TMP_DIR/t5.hdr")"
[[ -n "$location" ]] || fail "Expected Location header on 201 Created"
rocket_id="${location##*/}"
[[ -n "$rocket_id" ]] || fail "Could not parse rocket id from Location: $location"

assert_body_contains "$TMP_DIR/t5.body" "\"id\":\"$rocket_id\""
assert_body_contains "$TMP_DIR/t5.body" '"name":"Falcon"'
assert_body_contains "$TMP_DIR/t5.body" '"capacity":3'
assert_body_contains "$TMP_DIR/t5.body" '"speed":123'
assert_body_contains "$TMP_DIR/t5.body" '"range":"MOON"'

# --- List rockets (200) ---

echo "Testing: GET /rockets -> 200"
code="$(http_get "$BASE_URL/rockets" "$TMP_DIR/t6.hdr" "$TMP_DIR/t6.body")"
assert_status "$code" "200"
assert_body_contains "$TMP_DIR/t6.body" "\"id\":\"$rocket_id\""

# --- Get by id (200 / 404) ---

echo "Testing: GET /rockets/{id} existing -> 200"
code="$(http_get "$BASE_URL/rockets/$rocket_id" "$TMP_DIR/t7.hdr" "$TMP_DIR/t7.body")"
assert_status "$code" "200"
assert_body_contains "$TMP_DIR/t7.body" "\"id\":\"$rocket_id\""

echo "Testing: GET /rockets/{id} missing -> 404"
code="$(http_get "$BASE_URL/rockets/r9999" "$TMP_DIR/t8.hdr" "$TMP_DIR/t8.body")"
assert_status "$code" "404"

echo "E2E OK (001-rocket_management)"
