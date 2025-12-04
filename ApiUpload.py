import requests
import json
import paho.mqtt.client as mqtt
import time

API_URL = "https://uclapi.com/workspaces/sensors/summary?survey_filter=student"
TOKEN = "uclapi-12c965c5cbaf15f-dec48bfc44de693-ab25562111f7394-52c71bd22af4fed"   #freeDesktop Api

MQTT_BROKER = "mqtt.cetools.org"
MQTT_TOPIC = "student/TripleBang/seats"  #  topic

def fetch_ucl_data():
    response = requests.get(API_URL, params={"token": TOKEN})
    data = response.json()

    # Filter One Pool Street and Marshgate
    pool = next((s for s in data["surveys"] if s["id"] == 111), None)
    marsh = next((s for s in data["surveys"] if s["id"] == 115), None)

    return {
        "pool": {
            "free": pool["sensors_absent"],
            "occupied": pool["sensors_occupied"]
        },
        "marshgate": {
            "free": marsh["sensors_absent"],
            "occupied": marsh["sensors_occupied"]
        }
    }

def main():
    client = mqtt.Client()
    client.connect(MQTT_BROKER, 1883, 60)

    while True:
        try:
            seats = fetch_ucl_data()
            payload = json.dumps(seats)
            client.publish(MQTT_TOPIC, payload)
            print("Published:", payload)

        except Exception as e:
            print("Error:", e)

        time.sleep(30)

if __name__ == "__main__":
    main()
