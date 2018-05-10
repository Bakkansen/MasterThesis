#include <RFduinoGZLL.h>
#include "PLAB_PushButton.h"

device_t role = HOST;

const int ledPin = 6;
const int buttonPin = 2;

// Operation mode of the RFduino
int mode = 0; // 0 = Start, 1 = GameMode, 2 = Completed

// PushButton setup
PLab_PushButton button(buttonPin);

const int connectedDevices = 3; // identify the number of devices that is connected to the game

// List of connected devices
device_t devList[8] = {DEVICE0, DEVICE1, DEVICE2, DEVICE3, DEVICE4, DEVICE5, DEVICE6, DEVICE7};
device_t currentDotHolder = HOST;


void setup() {
  // put your setup code here, to run once:  
  pinMode(ledPin, OUTPUT);
  Serial.begin(115200);
  RFduinoGZLL.begin(role);
}

void loop(){
  button.update();
  if (mode == 0) {
    if (button.pressed()) {
      mode = 1;
      Serial.println("Starting game mode");
      sendNewDot();
    } 
  } else if (mode == 1) {
    if (button.pressed()) {
      resetGame();
    }    
  }
  
  
}

void RFduinoGZLL_onReceive(device_t device, int rssi, char *data, int len) {
  if (device == currentDotHolder) {
    String str = data;
    if (str.startsWith("#BUTTON:P")) {
      RFduinoGZLL.sendToDevice(currentDotHolder, "#LED:OFF");      
      sendNewDot();
    }
  }    
}

void resetGame() {
  RFduinoGZLL.sendToDevice(currentDotHolder, "#LED:OFF");
  currentDotHolder = HOST;
  Serial.println("Game Reset, reconnect devices...");
  mode = 0;
}



void sendNewDot() {
  long randNumber = random(connectedDevices);
  if (devList[randNumber] == currentDotHolder) {
    sendNewDot();
  } else {
    currentDotHolder = devList[randNumber];
    RFduinoGZLL.sendToDevice(currentDotHolder, "#LED:ON");
    Serial.print("Sent dot to: DEVICE");
    Serial.println(randNumber);  
  }   
}

