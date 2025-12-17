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
  TMP_DIR="./.tmp-e2e-002"
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

# --- Create rocket for flight creation tests ---

echo "Creating rocket for flight tests -> 201"
code="$(http_json POST "$BASE_URL/rockets" '{"name":"Falcon","capacity":3}' "$TMP_DIR/r1.hdr" "$TMP_DIR/r1.body")"
assert_status "$code" "201"

rocket_location="$(read_location_header "$TMP_DIR/r1.hdr")"
[[ -n "$rocket_location" ]] || fail "Expected Location header on 201 Created"
rocket_id="${rocket_location##*/}"
[[ -n "$rocket_id" ]] || fail "Could not parse rocket id from Location: $rocket_location"

# --- Validation failures (400) ---

echo "Testing: POST /flights launchDate not future -> 400"
code="$(http_json POST "$BASE_URL/flights" "{\"rocketId\":\"$rocket_id\",\"launchDate\":\"2000-01-01T00:00:00Z\",\"basePrice\":100}" "$TMP_DIR/t1.hdr" "$TMP_DIR/t1.body")"
assert_status "$code" "400"
assert_body_contains "$TMP_DIR/t1.body" '"error":"launchDate must be in the future"'

echo "Testing: POST /flights basePrice <= 0 -> 400"
code="$(http_json POST "$BASE_URL/flights" "{\"rocketId\":\"$rocket_id\",\"launchDate\":\"2099-01-01T00:00:00Z\",\"basePrice\":0}" "$TMP_DIR/t2.hdr" "$TMP_DIR/t2.body")"
assert_status "$code" "400"
assert_body_contains "$TMP_DIR/t2.body" '"error":"basePrice must be > 0"'

# --- Missing rocket (404) ---

echo "Testing: POST /flights rocketId not found -> 404"
code="$(http_json POST "$BASE_URL/flights" '{"rocketId":"r9999","launchDate":"2099-01-01T00:00:00Z","basePrice":123.45}' "$TMP_DIR/t3.hdr" "$TMP_DIR/t3.body")"
assert_status "$code" "404"

# --- Happy path (201 + default minimumPassengers + state=SCHEDULED) ---

echo "Testing: POST /flights omit minimumPassengers -> 201 with defaults"
code="$(http_json POST "$BASE_URL/flights" "{\"rocketId\":\"$rocket_id\",\"launchDate\":\"2099-01-02T00:00:00Z\",\"basePrice\":123.45}" "$TMP_DIR/t4.hdr" "$TMP_DIR/t4.body")"
assert_status "$code" "201"

flight_location="$(read_location_header "$TMP_DIR/t4.hdr")"
[[ -n "$flight_location" ]] || fail "Expected Location header on 201 Created"
flight_id="${flight_location##*/}"
[[ -n "$flight_id" ]] || fail "Could not parse flight id from Location: $flight_location"

assert_body_contains "$TMP_DIR/t4.body" "\"id\":\"$flight_id\""
assert_body_contains "$TMP_DIR/t4.body" "\"rocketId\":\"$rocket_id\""
assert_body_contains "$TMP_DIR/t4.body" '"minimumPassengers":5'
assert_body_contains "$TMP_DIR/t4.body" '"state":"SCHEDULED"'
assert_body_contains "$TMP_DIR/t4.body" '"basePrice":123.45'
assert_body_contains "$TMP_DIR/t4.body" '"launchDate":"2099-01-02T00:00:00'

echo "E2E OK (002-flight_creation_validation)"
