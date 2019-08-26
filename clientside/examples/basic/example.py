from unitynet import NetCon

netcon = NetCon()

netcon.open()

actions = [
	("Left", [1]),
	("Right", [1]),
	("Up", [1]),
	("Down", [1])
]

netcon.send("GetState")
while True:
	fields = netcon.receive_data()
	if fields:
		print(fields)
		if fields['done']==1:
			break
		c = int(input("Comando [0=Left, 1=Right, 2=Up, 3=Down]"))
		netcon.send(actions[c][0], actions[c][1])
		netcon.send("GetState")
netcon.close()

