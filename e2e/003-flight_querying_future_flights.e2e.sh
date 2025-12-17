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

assert_body_not_contains() {
  local file="$1"
  local needle="$2"
  if grep -Fq "$needle" "$file"; then
    echo "--- response body ---" >&2
    cat "$file" >&2
    echo "---------------------" >&2
    fail "Expected body NOT to contain: $needle"
  fi
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

http_post() {
  local url="$1"
  local header_out="$2"
  local body_out="$3"

  curl -sS -D "$header_out" -o "$body_out" -w "%{http_code}" \
    -X POST \
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
require_cmd date

TMP_DIR="$(mktemp -d 2>/dev/null || echo "")"
if [[ -z "$TMP_DIR" || ! -d "$TMP_DIR" ]]; then
  TMP_DIR="./.tmp-e2e-003"
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

# --- Setup: create rocket ---

echo "Creating rocket for flight listing tests -> 201"
code="$(http_json POST "$BASE_URL/rockets" '{"name":"Falcon","capacity":3}' "$TMP_DIR/r1.hdr" "$TMP_DIR/r1.body")"
assert_status "$code" "201"

rocket_location="$(read_location_header "$TMP_DIR/r1.hdr")"
[[ -n "$rocket_location" ]] || fail "Expected Location header on 201 Created"
rocket_id="${rocket_location##*/}"
[[ -n "$rocket_id" ]] || fail "Could not parse rocket id from Location: $rocket_location"

# --- Create flights: far-future + near-future ---

far_launch="2099-01-01T00:00:00Z"
near_launch="$(date -u -d "+8 seconds" +"%Y-%m-%dT%H:%M:%SZ")"

echo "Creating far-future flight -> 201"
code="$(http_json POST "$BASE_URL/flights" "{\"rocketId\":\"$rocket_id\",\"launchDate\":\"$far_launch\",\"basePrice\":100}" "$TMP_DIR/f1.hdr" "$TMP_DIR/f1.body")"
assert_status "$code" "201"

flight1_location="$(read_location_header "$TMP_DIR/f1.hdr")"
[[ -n "$flight1_location" ]] || fail "Expected Location header on 201 Created"
flight1_id="${flight1_location##*/}"
[[ -n "$flight1_id" ]] || fail "Could not parse flight id from Location: $flight1_location"

echo "Creating near-future flight (expires soon) -> 201"
code="$(http_json POST "$BASE_URL/flights" "{\"rocketId\":\"$rocket_id\",\"launchDate\":\"$near_launch\",\"basePrice\":50}" "$TMP_DIR/f2.hdr" "$TMP_DIR/f2.body")"
assert_status "$code" "201"

flight2_location="$(read_location_header "$TMP_DIR/f2.hdr")"
[[ -n "$flight2_location" ]] || fail "Expected Location header on 201 Created"
flight2_id="${flight2_location##*/}"
[[ -n "$flight2_id" ]] || fail "Could not parse flight id from Location: $flight2_location"

# --- List future flights (should include both before sleep) ---

echo "Testing: GET /flights returns only future flights -> 200"
code="$(http_get "$BASE_URL/flights" "$TMP_DIR/t1.hdr" "$TMP_DIR/t1.body")"
assert_status "$code" "200"
assert_body_contains "$TMP_DIR/t1.body" "\"id\":\"$flight1_id\""
assert_body_contains "$TMP_DIR/t1.body" "\"id\":\"$flight2_id\""

# --- Prove strict future filtering by waiting until near-future flight becomes past ---

echo "Waiting for near-future flight to become past..."
sleep 10

echo "Testing: GET /flights excludes flights whose launchDate is not in the future -> 200"
code="$(http_get "$BASE_URL/flights" "$TMP_DIR/t2.hdr" "$TMP_DIR/t2.body")"
assert_status "$code" "200"
assert_body_contains "$TMP_DIR/t2.body" "\"id\":\"$flight1_id\""
assert_body_not_contains "$TMP_DIR/t2.body" "\"id\":\"$flight2_id\""

# --- Filter by state (case-insensitive) ---

cancel_launch="2099-01-03T00:00:00Z"

echo "Creating flight to cancel -> 201"
code="$(http_json POST "$BASE_URL/flights" "{\"rocketId\":\"$rocket_id\",\"launchDate\":\"$cancel_launch\",\"basePrice\":75}" "$TMP_DIR/f3.hdr" "$TMP_DIR/f3.body")"
assert_status "$code" "201"

flight3_location="$(read_location_header "$TMP_DIR/f3.hdr")"
[[ -n "$flight3_location" ]] || fail "Expected Location header on 201 Created"
flight3_id="${flight3_location##*/}"
[[ -n "$flight3_id" ]] || fail "Could not parse flight id from Location: $flight3_location"

echo "Cancelling flight -> 200"
code="$(http_post "$BASE_URL/flights/$flight3_id/cancel" "$TMP_DIR/c1.hdr" "$TMP_DIR/c1.body")"
assert_status "$code" "200"
assert_body_contains "$TMP_DIR/c1.body" "\"id\":\"$flight3_id\""
assert_body_contains "$TMP_DIR/c1.body" '"state":"CANCELLED"'

echo "Testing: GET /flights?state=cancelled filters by state -> 200"
code="$(http_get "$BASE_URL/flights?state=cancelled" "$TMP_DIR/t3.hdr" "$TMP_DIR/t3.body")"
assert_status "$code" "200"
assert_body_contains "$TMP_DIR/t3.body" "\"id\":\"$flight3_id\""
assert_body_not_contains "$TMP_DIR/t3.body" "\"id\":\"$flight1_id\""

# --- Invalid state filter -> 400 ---

echo "Testing: GET /flights?state=NOT_A_STATE -> 400"
code="$(http_get "$BASE_URL/flights?state=NOT_A_STATE" "$TMP_DIR/t4.hdr" "$TMP_DIR/t4.body")"
assert_status "$code" "400"
assert_body_contains "$TMP_DIR/t4.body" '"error":"state must be one of: SCHEDULED, CONFIRMED, SOLD_OUT, CANCELLED, DONE"'

echo "Testing: GET /flights?state= -> 400"
code="$(http_get "$BASE_URL/flights?state=" "$TMP_DIR/t5.hdr" "$TMP_DIR/t5.body")"
assert_status "$code" "400"
assert_body_contains "$TMP_DIR/t5.body" '"error":"state must be a valid flight state"'

echo "E2E OK (003-flight_querying_future_flights)"
