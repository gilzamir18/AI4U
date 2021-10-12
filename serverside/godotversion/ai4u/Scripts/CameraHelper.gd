extends Spatial


const INTERPOLATION = 0.1
const MINDISTANCE = 1

var originalCamZoom = null
var originalCamVect = null
var ray
var camera


func _ready():
	ray = get_node("CameraPivot/RayCamera")
	camera = get_node("CameraPivot/CameraOffset")
	originalCamVect = camera.translation.normalized()
	originalCamZoom = camera.translation.length()
	
	#Adds the player as an exception
	get_node("CameraPivot/RayCamera").add_exception(get_node(".."))


#warning-ignore:unused_argument
func _process(delta):
	var dist = camera.translation.length()
	var distTarget = originalCamZoom
	
	#If the ray collides overwrite the camera position
	if ray.is_colliding():
		distTarget = (ray.get_collision_point() - to_global(translation)).length() - MINDISTANCE
	
	dist += (distTarget - dist) * INTERPOLATION
	camera.translation = originalCamVect.normalized() * dist