/*
  NodeMCU v3 (ESP8266) + UCL API seat gauge
  - Fetch live OPS / MSG seat data from UCL API
  - MG90S servo needle shows vacancy rate 
  - Waveshare LCD1602 RGB display shows building name / occupied / free seats
  - KY-040 rotary encoder:
    One detent = toggle preview between OPS / MSG
    Press = confirm selection and update gauge
*/

#include <Arduino.h>
#include <ESP8266WiFi.h>
#include <WiFiClientSecure.h>
#include <Servo.h>
#include <Wire.h>
#include "Waveshare_LCD1602_RGB.h"

// WiFi & API CONFIG 
const char* ssid     = "CE-Hub-Student";
const char* password = "casa-ce-gagarin-public-service";

// UCL API token
const char* UCLAPI_TOKEN = "uclapi-12c965c5cbaf15f-dec48bfc44de693-ab25562111f7394-52c71bd22af4fed";

// Update interval (ms)
const unsigned long UPDATE_INTERVAL = 120000;  // refresh every 120 seconds


#define SERVO_PIN D8    


#define ENC_CLK  D5      
#define ENC_DT   D6      
#define ENC_SW   D7      
#define I2C_SDA  D2      
#define I2C_SCL  D1      

// Servo mapping (0% ~ 100% -> 163°)
// Tune these angles after mechanical installation
const float SERVO_MIN_ANGLE = 0.0;   // servo angle for 0% on gauge
const float SERVO_MAX_ANGLE = 163.0;  // servo angle for 100% on gauge

// GLOBAL OBJECTS 
Servo gaugeServo;

Waveshare_LCD1602_RGB lcd(16, 2);

unsigned long lastUpdateTime = 0;

// Building data structure
struct BuildingData {
  int totalSeats;
  int occupiedSeats;
  int absentSeats;
  int otherSeats;
  bool valid;
};

BuildingData opsData  = {0, 0, 0, 0, false};
BuildingData msgData  = {0, 0, 0, 0, false};

// Encoder: preview & confirmed selection
int previewBuilding = 0;   // 0=OPS, 1=MSG (changes when rotating)
int activeBuilding  = 0;   // 0=OPS, 1=MSG (changes when pressing)

// Encoder debouncing
int lastCLK = HIGH;
int lastSW  = HIGH;
unsigned long lastStepTime   = 0;
unsigned long lastButtonTime = 0;
const unsigned long STEP_DEBOUNCE   = 3;    // ms
const unsigned long BUTTON_DEBOUNCE = 200;  // ms

//  FUNCTION DECLARATIONS 
void connectWiFi();
void fetchSeatData();
void updateDisplayAndServo();
float calcVacancyRate(const BuildingData& data);
void moveServoToVacancy(float vacancyPercent);
void showErrorOnLCD(const char* msg);

void setupEncoder();
void handleEncoder();
void showPreviewOnLCD();

//  STRING-BASED JSON PARSING HELPERS 

// From position of a key in body, find the integer after ':'.
int readIntAfterKey(const String& body, int keyPos) {
  if (keyPos < 0) return -1;
  int colonPos = body.indexOf(':', keyPos);
  if (colonPos < 0) return -1;
  int i = colonPos + 1;
  // Skip spaces
  while (i < (int)body.length() && (body[i] == ' ' || body[i] == '\t')) i++;

  int val = 0;
  bool hasDigit = false;
  while (i < (int)body.length() && body[i] >= '0' && body[i] <= '9') {
    val = val * 10 + (body[i] - '0');
    hasDigit = true;
    i++;
  }
  if (!hasDigit) return -1;
  return val;
}

// Find the given building name in JSON body, then parse its total sensors_* fields
bool extractBuildingData(const String& body, const char* buildingName, BuildingData& out) {
  int namePos = body.indexOf(buildingName);
  if (namePos < 0) {
    Serial.print("Building not found in JSON: ");
    Serial.println(buildingName);
    return false;
  }

  // First find "staff_survey" that precedes the building total section
  int staffPos = body.indexOf("\"staff_survey\"", namePos);
  if (staffPos < 0) {
   
    staffPos = namePos;
  }

  // Then from that point find the three total sensor fields
  int absentPos   = body.indexOf("\"sensors_absent\"",   staffPos);
  int occupiedPos = body.indexOf("\"sensors_occupied\"", staffPos);
  int otherPos    = body.indexOf("\"sensors_other\"",    staffPos);

  if (absentPos < 0 || occupiedPos < 0 || otherPos < 0) {
    Serial.print("Sensors fields missing for: ");
    Serial.println(buildingName);
    return false;
  }

  int absent   = readIntAfterKey(body, absentPos);
  int occupied = readIntAfterKey(body, occupiedPos);
  int other    = readIntAfterKey(body, otherPos);

  if (absent < 0 || occupied < 0 || other < 0) {
    Serial.print("Failed to parse ints for: ");
    Serial.println(buildingName);
    return false;
  }

  out.absentSeats   = absent;
  out.occupiedSeats = occupied;
  out.otherSeats    = other;
  out.totalSeats    = absent + occupied + other;
  out.valid         = (out.totalSeats > 0);

  Serial.print("Parsed TOTAL ");
  Serial.print(buildingName);
  Serial.print(" -> absent=");
  Serial.print(absent);
  Serial.print(", occupied=");
  Serial.print(occupied);
  Serial.print(", other=");
  Serial.println(other);

  return true;
}


// setup

void setup() {
  Serial.begin(74880);
  delay(100);

  
  Wire.begin(I2C_SDA, I2C_SCL);

  
  lcd.init();
  lcd.setRGB(0, 128, 255);   
  lcd.setCursor(0, 0);
  lcd.send_string("Seat Gauge");
  lcd.setCursor(0, 1);
  lcd.send_string("Booting...");


  gaugeServo.attach(SERVO_PIN);
  gaugeServo.write(SERVO_MIN_ANGLE);

  
  setupEncoder();

 
  connectWiFi();

  
  fetchSeatData();

  
  previewBuilding = 0;
  activeBuilding  = 0;

  updateDisplayAndServo();
}


// loop

void loop() {
  unsigned long now = millis();

  // Periodic API refresh
  if (now - lastUpdateTime > UPDATE_INTERVAL) {
    lastUpdateTime = now;
    fetchSeatData();
  }

  // Handle encoder input
  handleEncoder();

  delay(5);
}


// Connect to WiFi

void connectWiFi() {
  WiFi.mode(WIFI_STA);
  WiFi.begin(ssid, password);

  Serial.print("Connecting to WiFi");
  lcd.clear();
  lcd.setCursor(0, 0);
  lcd.send_string("Connecting WiFi");

  int dot = 0;
  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
    Serial.print(".");
    lcd.setCursor(0, 1);
    lcd.send_string("Status ");
    for (int i = 0; i < dot; i++) lcd.send_string(".");
    dot = (dot + 1) % 4;
  }

  Serial.println();
  Serial.println("WiFi connected.");
  Serial.print("IP: ");
  Serial.println(WiFi.localIP());

  lcd.clear();
  lcd.setCursor(0, 0);
  lcd.send_string("WiFi Connected");
  lcd.setCursor(0, 1);
  lcd.send_string(WiFi.localIP().toString().c_str());
  delay(1000);
}


// Fetch OPS / MSG totals from UCL API (string-based parsing)

void fetchSeatData() {
  if (WiFi.status() != WL_CONNECTED) {
    connectWiFi();
  }

  const char* host = "uclapi.com";
  const int httpsPort = 443;

  WiFiClientSecure client;
  client.setInsecure();
  client.setTimeout(15000);

  Serial.println("Connecting to API server...");
  if (!client.connect(host, httpsPort)) {
    Serial.println("Connection failed!");
    showErrorOnLCD("API conn fail");
    return;
  }

  String url = "/workspaces/sensors/summary?token=" + String(UCLAPI_TOKEN);
  Serial.print("Requesting URL: ");
  Serial.println(url);

  client.print(String("GET ") + url + " HTTP/1.1\r\n" +
               "Host: uclapi.com\r\n" +
               "User-Agent: ESP8266\r\n" +
               "Connection: close\r\n\r\n");

  Serial.println("Waiting for response...");
  unsigned long start = millis();
  while (client.connected() && !client.available()) {
    if (millis() - start > 15000) {
      Serial.println("Timeout waiting for response");
      showErrorOnLCD("API timeout");
      client.stop();
      return;
    }
    delay(10);
  }

  // 1. Read HTTP header
  String header = "";
  while (client.connected()) {
    String line = client.readStringUntil('\n');
    if (line == "\r" || line == "") break;   // blank line → header end
    header += line + "\n";
  }

  Serial.println("===== HTTP HEADER START =====");
  Serial.println(header);
  Serial.println("===== HTTP HEADER END =====");

  // 2. Read body (blocking until connection closes)
  String body = "";
  while (client.connected() || client.available()) {
    if (client.available()) {
      char c = client.read();
      body += c;
    } else {
      delay(5);
    }
  }
  client.stop();

  body.trim();
  Serial.print("Body length: ");
  Serial.println(body.length());
  Serial.println("===== HTTP BODY (FIRST 300 CHARS) =====");
  Serial.println(body.substring(0, 300));
  Serial.println("... (truncated)");
  Serial.println("=======================================");

  if (body.length() == 0) {
    Serial.println("Empty body, cannot parse.");
    showErrorOnLCD("Empty JSON");
    return;
  }

  // Parse just the two buildings we care about
  opsData.valid = false;
  msgData.valid = false;

  bool okOPS = extractBuildingData(body, "East Campus - Pool St",   opsData);
  bool okMSG = extractBuildingData(body, "East Campus - Marshgate", msgData);

  if (okOPS) {
    Serial.println("Updated OPS data from live API.");
  }
  if (okMSG) {
    Serial.println("Updated MSG data from live API.");
  }

  if (!okOPS || !okMSG) {
    showErrorOnLCD("Bldg not found");
  }
}


// Update LCD and servo based on activeBuilding

void updateDisplayAndServo() {
  bool selectOPS = (activeBuilding == 0);
  BuildingData current = selectOPS ? opsData : msgData;
  const char* buildingShortName = selectOPS ? "OPS" : "MSG";

  if (!current.valid) {
    showErrorOnLCD("No data");
    return;
  }

  float vacancy = calcVacancyRate(current);
  if (vacancy < 0) vacancy = 0;
  if (vacancy > 100) vacancy = 100;

  moveServoToVacancy(vacancy);

  int freeSeats = current.absentSeats;

  lcd.clear();
  lcd.setCursor(0, 0);
  String line1 = String(buildingShortName) + " Vac:" + String((int)vacancy) + "%";
  lcd.send_string(line1.c_str());

  lcd.setCursor(0, 1);
  String line2 = "Occ:" + String(current.occupiedSeats) + " Free:" + String(freeSeats);
  lcd.send_string(line2.c_str());

  Serial.print("Active building: ");
  Serial.print(buildingShortName);
  Serial.print(" vacancy=");
  Serial.print(vacancy);
  Serial.print("%  Occ=");
  Serial.print(current.occupiedSeats);
  Serial.print(" Free=");
  Serial.println(freeSeats);
}


// Compute vacancy rate

float calcVacancyRate(const BuildingData& data) {
  if (data.totalSeats <= 0) return -1.0;
  return (float)data.absentSeats * 100.0 / (float)data.totalSeats;
}


// Servo angle control

void moveServoToVacancy(float vacancyPercent) {
  float angleRange  = SERVO_MAX_ANGLE - SERVO_MIN_ANGLE;
  float targetAngle = SERVO_MIN_ANGLE + (vacancyPercent / 100.0) * angleRange;
  gaugeServo.write((int)targetAngle);

  Serial.print("Servo angle -> ");
  Serial.println(targetAngle);
}


// Show error message on LCD

void showErrorOnLCD(const char* msg) {
  lcd.clear();
  lcd.setCursor(0, 0);
  lcd.send_string("Error:");
  lcd.setCursor(0, 1);
  lcd.send_string(msg);
}


// Encoder setup

void setupEncoder() {
  pinMode(ENC_CLK, INPUT_PULLUP);
  pinMode(ENC_DT,  INPUT_PULLUP);
  pinMode(ENC_SW,  INPUT_PULLUP);

  lastCLK = digitalRead(ENC_CLK);
  lastSW  = digitalRead(ENC_SW);
}


// Encoder logic
//  - Rotate one detent: previewBuilding ^= 1
//  - Press: activeBuilding = previewBuilding; update LCD + servo

void handleEncoder() {
  unsigned long now = millis();

  // 1. Rotation detection (using CLK falling edge)
  int clkState = digitalRead(ENC_CLK);
  if (clkState != lastCLK && clkState == LOW) {
    if (now - lastStepTime > STEP_DEBOUNCE) {
      previewBuilding ^= 1;         // toggle between OPS and MSG
      lastStepTime = now;

      Serial.print("Preview building: ");
      Serial.println(previewBuilding == 0 ? "OPS" : "MSG");

      showPreviewOnLCD();
    }
  }
  lastCLK = clkState;

  // 2. Button press detection
  int swState = digitalRead(ENC_SW);
  if (swState != lastSW && swState == LOW) {
    if (now - lastButtonTime > BUTTON_DEBOUNCE) {
      activeBuilding = previewBuilding;
      lastButtonTime = now;

      Serial.print("Confirmed building: ");
      Serial.println(activeBuilding == 0 ? "OPS" : "MSG");

      updateDisplayAndServo();
    }
  }
  lastSW = swState;
}


// Encoder preview display: only change first line
// keep last confirmed numbers on second line.

void showPreviewOnLCD() {
  lcd.setCursor(0, 0);
  if (previewBuilding == 0) {
    lcd.send_string(">OPS           ");
  } else {
    lcd.send_string(">MSG           ");
  }
}
