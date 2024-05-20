class_name Brain
extends Node

enum data_type {FLOAT, INT, BOOL, STR, BYTE_ARRAY, FLOAT_ARRAY, INT_ARRAY, STRING_ARRAY}

var agent: BasicAgent = null
var received_cmd: String
var received_args: Array[String]
var command_fields = {}

func setup(agent: BasicAgent) -> void:
	pass

func close() -> void:
	pass
	
func set_cmd_fields(fields):
	self.command_fields = fields

func get_field(name: String) -> Array[String]:
	return self.command_fields[name]

func set_received_cmd_name(cmdname : String):
	self.received_cmd = cmdname

func contains_cmd_field(cmd: String) -> bool:
	if self.command_fields != null:
		return cmd in self.command_fields
	else:
		return false

func set_received_cmd_args(args: Array[String]):
	self.received_args = args

func get_received_cmd():
	return self.received_cmd
	
func on_step_reward(step: int, reward: float):
	pass

func get_received_args(cmd: String) -> Array[String]:
	if cmd == null:
		return received_args
	else:
		return self.command_fields[cmd]

	

	
	
	



