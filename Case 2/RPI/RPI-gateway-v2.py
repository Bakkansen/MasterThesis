import paho.mqtt.client as mqtt
import paho.mqtt.publish as publish
import time
import serial

#RPI Information - Switch these for the other RPI Gateway: RPIID = otherRPID and otehrRPID = RPIID
RPIID = "RPI1"
otherRPIID = "RPI2"

#MQTT Information
#Topic stucture: FtRD/RPIID
mqttBrokerAddress = "YOUR_BROKER_ADDRESS" # Add your broker address here
subTopic = "FtRD/" + otherRPIID
pubTopic = "FtRD/" + RPIID


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
	if (len(topicSplit) > 1):
		if (topicSplit[1] == otherRPIID):
			text = msg.payload.rstrip()
			if (text.startsWith("#Beacon")):
				SendSerialMsg("#Beacon")
					

def HandleSerialMessage(msg):
	msg.rstrip('\r\n')
	if (msg.startsWith("#Beacon")):
		PublishMQTTMsg("#Beacon");


def SendSerialMsg(serialMsg):
	sendMsg = "<" + serialMsg + ">"
	sendMsg = sendMsg.encode()
	ser.write(sendMsg)
	print("Sent serial message: [" + sendMsg + "]")


def PublishMQTTMsg(payload):
	print("Published mqtt message: [" + pubTopic + "]: " + "[" + payload + "]")
	publish.single(pubTopic, payload, hostname=mqttBrokerAddress)


def CleanUp():
	print("Ending and cleaning up")
	ser.close()
	client.disconnect()


try:
	#Serial Information
	print("Connecting Serial port")
	ser = serial.Serial(
		port='/dev/ttyUSB0',
		baudrate = 115200,
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
		HandleSerialMessage(read_serial.rstrip())

except (KeyboardInterrupt, SystemExit):
	print("Interrupt received")
	CleanUp()

except (RuntimeError):
	print("Run-Time Error")
	CleanUp()
