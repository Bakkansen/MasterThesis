#include <RFduinoGZLL.h>
#include "PLAB_PushButton.h"

device_t role = DEVICE0; // Set specific device ID here

const int ledPin = 6;
const int buttonPin = 2;

unsigned long pollTime = 0;

// PushButton setup
PLab_PushButton button(buttonPin);


boolean pollInterval(unsigned long &since, unsigned long interval) {
  unsigned long currentmillis = millis();
  if (currentmillis - since >= interval) {
    since = currentmillis;
    return true;
  }
  return false;
}


void setup() {
  // put your setup code here, to run once:  
	pinMode(ledPin, OUTPUT);
	Serial.begin(115200);
	RFduinoGZLL.begin(role);
}

void loop(){
	button.update();
	if (button.pressed()){
		RFduinoGZLL.sendToHost("#BUTTON:P");
	}	
  if (pollInterval(pollTime, 100)) {
    RFduinoGZLL.sendToHost(NULL, 0);
  }
}

void RFduinoGZLL_onReceive(device_t device, int rssi, char *data, int len) {
	String str = data;  
  if (len > 2) {
    Serial.println(str);  
  }  
	if (str.startsWith("#LED:ON")) {
		digitalWrite(ledPin, HIGH);
	}
	if (str.startsWith("#LED:OFF")) {
		digitalWrite(ledPin, LOW);
	}
}

