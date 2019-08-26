from unitynet import NetCon, get_state
from PIL import Image
import io
from tkinter import Canvas, PhotoImage, Tk, NW
import base64
import time

root = Tk()


canvas = Canvas(root, width=300, height=300)

img = PhotoImage(file="lastframe.png")

imageoncanvas = canvas.create_image(20, 20, anchor=NW, image=img)

root.update()

netcon = NetCon()

while True: 
	try:
		if netcon.open(0.1)==True:
			break
	except:
		time.sleep(0.1)

actions = {
	'w':("Up"),
	's':("Down"),
	'p':("PauseGame"),
	'c':("ResumeGame"),
	'r':("RestartGame")
}

def Up(event):
	global netcon
	netcon.send(actions['w'])


def Down(event):
	global netcon
	netcon.send(actions['s'])


def Restart(event):
	global netcon
	netcon.send(actions['r'])

canvas.bind('w', Up)
canvas.bind('s', Down)
canvas.bind('r', Restart)
canvas.bind("<1>", lambda event: canvas.focus_set())
canvas.pack()

Restart(None)
netcon.send("GetState")
fields = None

while True:
    fields = get_state(netcon, waiting=0.0)

    if fields:
        #print(fields['done'])
        content = fields['frame']
        imgdata = base64.b64decode(content)
        #bytesio = io.BytesIO(imgdata)
        #img = Image.open(bytesio)
        img = PhotoImage(data=imgdata)
        canvas.create_image(20, 20, anchor=NW, image=img)
        root.update()
        #image.save("lastframe.png")
        #print(fields['score'])
        if fields['done']==1:
            break
        netcon.send("GetState")

netcon.close()
