class_name BasicAgent
extends Node

signal reset

class command:
	var name:String
	var args:Array[String]
	
	func create(name: String, args: Array[String]):
		self.name = name
		self.args = args

class agent_control_info:
	var paused:bool = false
	var stopped:bool  = true
	var applying_action:bool = false
	var frame_counter:int = -1
	var lastCmd: command = null
	var skip_frame:int = 4;
	var repeat_action: bool = true
	var lastResetId: String = ""
	var lastEnvResetId: String = ""
	var env_mode:bool = true


