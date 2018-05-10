import paho.mqtt.client as mqtt
import paho.mqtt.publish as publish
import time
import serial
import json


devList = { "DEV0": [ "LED", "OFF", "BUTTON", "LIFTED"], "DEV1": [ "LED", "OFF", "BUTTON", "LIFTED"], "DEV2": [ "LED", "OFF", "BUTTON", "LIFTED"]}

interfaceJSON = {
	"components" : [
		{"componentID":"LED", "componentType":"LED", "states":["ON", "OFF", "DISABLED"]},
		{"componentID":"BUTTON", "componentType":"BUTTON", "states":["LIFTED", "PRESSED", "DISABLED"]}
	]
}

#MQTT Information
#Topic stucture: Pettjo/Type/Environment/RPI/Dev ex: Pettjo/Action/House/RPI1/Dev0
mqttBrokerAddress = "YOUR_BROKER_ADDRESS" # Add your broker address here
listenTopic = "FtRD/#"


#RPI Information
rpiID = "RPI1"
Environment = "House"


# MQTT handling

def on_connect(client, userdata, flags, rc):
    print("Connected with result code " + str(rc))
    client.subscribe(listenTopic)


def on_message(client, userdata, msg):
	try:
	    print("Received data from " + msg.topic + ": " + msg.payload)
	    HandleMQTTMessage(msg)
	except (UnicodeDecodeError):
		print("Received faulty msg")


def HandleMQTTMessage(msg):
	topicSplit = msg.topic.split('/')
	msgType = "#"
	msgEnvironment = "#"
	msgRPI = "#"
	msgDev = "#"
	if (len(topicSplit) > 1):
		msgType = topicSplit[1]
	if (len(topicSplit) > 2):
		msgEnvironment = topicSplit[2]
	if (len(topicSplit) > 3):
		msgRPI = topicSplit[3]
	if (len(topicSplit) > 4):
		msgDev = topicSplit[4].upper()

	if (msgRPI != rpiID and msgRPI != "#"):
		return

	if (msgType == "Event"):
		if (msgDev == "#"):
			for dev in devList.keys():
				EventMsgRcvd(dev, msg.payload.rstrip())
		elif (msgDev in devList.keys()):
			EventMsgRcvd(msgDev, msg.payload.rstrip())

	elif (msgType == "StateReq"):
		if (msgDev == "#"):
			for dev in devList.keys():
				StateReqMsgRcvd(dev, msg.payload.rstrip())
		elif (msgDev in devList.keys()):
			StateReqMsgRcvd(msgDev, msg.payload.rstrip())

	elif (msgType == "InterfaceReq"):
		if (msgDev == "#"):
			for dev in devList.keys():
				InterfaceReqMsgRcvd(dev, msg.payload.rstrip())
		elif (msgDev in devList.keys()):
			InterfaceReqMsgRcvd(msgDev, msg.payload.rstrip())


def EventMsgRcvd(dev, msg):
	# MQTT msg format: "Comp:CompState"
	print("Recvd MQTTmsg: [" + dev + "] : [" + msg + "]")
	parsedMsg = msg.split(':')
	# Checks if device has component
	if (parsedMsg[0].upper() in devList[dev]):
		sendString = dev + ":#" + parsedMsg[0] + ":" + parsedMsg[1]
		SendSerialMsg(sendString)

def StateReqMsgRcvd(dev, msg):
	print("Recvd MQTTmsg: [" + dev + "] : [" + msg + "]")
	if (msg == "REQ"):
		sendString = dev + ":" + msg;
		SendSerialMsg(sendString)


def InterfaceReqMsgRcvd(dev, msg):
	print("Recvd MQTTmsg: [" + dev + "] : [" + msg + "]")
	if (msg == "REQ"):
		PublishMQTTMsg("InterFace", json.dumps(interfaceJSON), dev)
		

# Serial handling

def ActionSerialRcvd(dev, payload):	
	sendString = payload[0] + ":" payload[1]
	PublishMQTTMsg("Action", sendString, dev)

def StateSerialRcvd(dev, payload):
	stateJSON = {
    "connected":1,
    "components": [
        {"componentID":payload[0], "currentState":payload[1]},
        {"componentID":payload[2], "currentState":payload[3]}
        ]
    }
    PublishMQTTMsg("State", json.dumps(stateJSON), dev)


def HandleSerialMessage(msg):
	parsedMsg = msg.split(':')
	if (len(parsedMsg) != 3):
		return

	print("Received Serial message: [" + msg + "]")
	topic = parsedMsg[0]
	dev = parsedMsg[1].upper()
	payload = parsedMsg[2:]
	
	if (topic == "Action"):
		ActionSerialRcvd(dev, payload)

	elif (topic == "State"):
		StateSerialRcvd(dev, payload)


def SendSerialMsg(serialMsg):
	#print("Sent serial message: [" + serialMsg + "]")
	sendMsg = "<" + serialMsg + ">"
	sendMsg = sendMsg.encode()
	ser.write(sendMsg)
	print("Sent serial message: [" + sendMsg + "]")


def PublishMQTTMsg(msgType, payload, dev):
	if (dev in devList.keys()):
		topic = "FtRD/" + msgType + "/" + Environment + "/" + rpiID + "/" + dev		
		print("Published mqtt message: [" + topic + "]: " + "[" + payload + "]")
		publish.single(topic, payload, hostname=mqttBrokerAddress)




# System setup

def CleanUp():
	print("Ending and cleaning up")
	ser.close()
	client.disconnect()

try:
	#Serial Information
	print("Connecting Serial port")
	ser = serial.Serial(
		port='/dev/ttyUSB0',
		baudrate = 9600,
		parity=serial.PARITY_NONE,
		stopbits=serial.STOPBITS_ONE,
		bytesize=serial.EIGHTBITS,
		timeout=1
	)

except:
	print("Failed to connect serial")
	raise SystemExit

try:
	client = mqtt.Client()
	client.on_connect = on_connect
	client.on_message = on_message

	client.connect(mqttBrokerAddress, 1883, 60)

	client.loop_start()
	print("MQTT client connected!")


	while True:
		read_serial = ser.readline()
		HandleSerialMessage(read_serial.rstrip('\r\n'))

except (KeyboardInterrupt, SystemExit):
	print("Interrupt received")
	CleanUp()

except (RuntimeError):
	print("Run-Time Error")
	CleanUp()
