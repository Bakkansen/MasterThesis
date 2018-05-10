#include <RFduinoGZLL.h>

device_t role = HOST;

const char startChar = '<';
const char stopChar = '>';
const char delimiter = ':';

unsigned int index_pos = 0;

const unsigned int MAX_DEV_INPUT = 5;
const unsigned int MAX_MSG_INPUT = 30;

char dev[MAX_DEV_INPUT];      // Array to keep the device info received
char msg[MAX_MSG_INPUT];    // Array to keep the component name

enum state_t {
  None,
  Devi,
  Msg
};
extern  state_t state = None;

void setup() {
  state = None;
  Serial.begin(115200);
  RFduinoGZLL.begin(role);
}

void RFduinoGZLL_onReceive(device_t device, int rssi, char *data, int len) {
  String str = data;
  if (len > 1) {
    String sender = "Action:" + getDeviceFromDevice_t(device) + str;
    Serial.println(sender);
  }
}

void handleDelimiter() {  
  if (state == Devi) {
    index_pos = 0;  
    state = Msg;
  } else if (state == Msg) {
    if (index_pos < (MAX_MSG_INPUT - 1)) {
      msg[index_pos++] = ':';
    }
  }
}

void handleIncomingData(const byte inByte) {
  switch (state) {
    case Devi:
      if (index_pos < (MAX_DEV_INPUT - 1)) {
        dev[index_pos++] = inByte;
      }
      break;

    case Msg:
      if (index_pos < (MAX_MSG_INPUT - 1)) {
        msg[index_pos++] = inByte;
      }
      break;

  }
}

void handleStopCharReceived() {
  device_t receiver = getDevice();

  String m = msg;
  if (m.startsWith("REQ")) {
    String d = dev;
    String sender = "State:" + d + "LED:OFF:BUTTON:Lifted";
    Serial.println(sender);
  } else if (receiver != HOST) {
    RFduinoGZLL.sendToDevice(receiver, msg);
  }
  state = None;
}

device_t getDevice() {
  device_t receiver = DEVICE0;
  int deviceInt = -1;
  if (isDigit(dev[3])) {
    deviceInt = dev[3] - '0';
  } 
  
  switch (deviceInt)  {
    case 0:
      receiver = DEVICE0;
      break;

    case 1:
      receiver = DEVICE1;
      break;

    case 2:
      receiver = DEVICE2;
      break;

    case 3:
      receiver = DEVICE3;
      break;

    case 4:
      receiver = DEVICE4;
      break;

    case 5:
      receiver = DEVICE5;
      break;

    case 6:
      receiver = DEVICE6;
      break;

    case 7:
      receiver = DEVICE7;
      break;
  }
  return receiver;
}

String getDeviceFromDevice_t(device_t dev) {
  String receiver = "DEV0";
  
  switch (dev)  {
    case 0:
      receiver = "DEV0";
      break;

    case 1:
      receiver = "DEV1";
      break;

    case 2:
      receiver = "DEV2";
      break;

    case 3:
      receiver = "DEV3";
      break;

    case 4:
      receiver = "DEV4";
      break;

    case 5:
      receiver = "DEV5";
      break;

    case 6:
      receiver = "DEV6";
      break;

    case 7:
      receiver = "DEV7";
      break;
  }
  return receiver;
}


void processIncomingByte(const byte inByte) {

  switch (inByte) {

    case startChar:
      state = Devi;
      index_pos = 0;
      break;

    case stopChar:
      handleStopCharReceived();
      break;

    case delimiter:
      handleDelimiter();
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

