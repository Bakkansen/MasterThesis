#include <RFduinoGZLL.h>

device_t role = HOST;


// Serial parsing variables
const char startChar = '<';
const char stopChar = '>';

unsigned int index_pos = 0;

const unsigned int MAX_DEV_INPUT = 5;
const unsigned int MAX_MSG_INPUT = 30;

char msg[MAX_MSG_INPUT];    // Array to keep the component name

enum state_t {
  None,
  Msg
};
extern  state_t state = None;

// FtRD variables
const int connectedDevices = 3; // identify the number of devices that is connected to the Host

int mode = 0; // 0 = Starter host (must receive button press from DEV0 to start), 1 = Other hosts (all others need to have set to 1)

device_t devList[8] = {DEVICE0, DEVICE1, DEVICE2, DEVICE3, DEVICE4, DEVICE5, DEVICE6, DEVICE7};
device_t currentDotHolder = HOST;


void setup() {
  state = None;
  Serial.begin(115200);
  RFduinoGZLL.begin(role);  
}

void RFduinoGZLL_onReceive(device_t device, int rssi, char *data, int len) {
  if (device == DEVICE0 && mode == 0) {
    String str = data;
    if (str.startsWith("#BUTTON:P")) {
      mode = 1;
      delegateBeacon();
    }    
  } else if (device == currentDotHolder) {    
    String str = data;
    if (str.startsWith("#BUTTON:P")) {
      RFduinoGZLL.sendToDevice(currentDotHolder, "#LED:OFF");      
      delegateBeacon();
    }
  }
}

void delegateBeacon() {
  long randNumber = random(connectedDevices*2);
  if (randNumber < connectedDevices || currentDotHolder == HOST) {
      sendNewDot();    
  } else {
    currentDotHolder = HOST;
    Serial.println("#Beacon");
  }  
}

void sendNewDot() {
  long randNumber = random(connectedDevices);
  if (devList[randNumber] == currentDotHolder) {
    sendNewDot();
  } else {
    currentDotHolder = devList[randNumber];
    RFduinoGZLL.sendToDevice(currentDotHolder, "#LED:ON");
  } 
}


// Serial Reading Code


void handleIncomingData(const byte inByte) {
  switch (state) {
    case Msg:
      if (index_pos < (MAX_MSG_INPUT - 1)) {
        msg[index_pos++] = inByte;
      }
      break;
  }
}

void handleStopCharReceived() {
  String str = msg;
  Serial.println(str);
  if (str.startsWith("#Beacon") && currentDotHolder == HOST && mode == 1) {
    sendNewDot();
  }
  state = None;
}

void processIncomingByte(const byte inByte) {

  switch (inByte) {

    case startChar:
      state = Msg;
      index_pos = 0;
      break;

    case stopChar:
      handleStopCharReceived();
      break;

    default:
      handleIncomingData(inByte);
      break;
  }
}

void loop() {
  while (Serial.available() > 0) {
    processIncomingByte(Serial.read());
  }
}

