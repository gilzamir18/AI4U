using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ai4u
{
    public class SharedLinearRaycasting : MonoBehaviour, IAgentResetListener
    {
        [Tooltip("'noObjectCode' is the code produced when a ray does not hit any object.")]
        public int noObjectCode;
        [Tooltip("The 'eye' property refers to the position and forward direction of the casting rays.")]
        public GameObject eye;
        [Tooltip("The 'fieldOfView' property represents the angle between the two extreme rays that are cast from the position specified by the 'eye' property. This angle determines the extent of the field of view for the associated camera or sensor, and indicates the angular range within which objects can be detected or observed.")]
        public float fieldOfView = 180.0f;
        [Tooltip("A float value that determines the vertical shift of the casting rays.")]
        public float verticalShift = 0;
        [Tooltip("A float value that determines the horizontal shift of the casting rays.")]
        public float horizontalShift = 0;
        [Tooltip("A float value that determines the maximum distance that the casting rays can travel before being stopped.")]
        public float maxDistance = 500f;
        [Tooltip("A boolean value that indicates whether or not the sensor should return depth information.")]
        public bool returnDepthMatrix = false;
        [Tooltip("An integer value that determines how many rays will be cast from the sensor.")]
        public int numberOfRays = 5;
        [Tooltip("A boolean value that indicates whether or not the sensor should automatically map detected objects to integer codes based on their tags.")]
        public bool automaticTagMapping = false;
        [Tooltip("An integer value that is used to calculate the integer code for objects detected by the sensor when automaticTagMapping is enabled.")]
        public int tagCodeDistance = 10;
        [Tooltip("An array of ObjectMapping structs that define mappings between object tags and integer codes.")]
        public  ObjectMapping[] objectMapping;
        [Tooltip(" A boolean value that indicates whether or not debug information should be displayed while the sensor is active.")]
        public bool debugMode = false;
        [Tooltip("Color of the hitted debug rays.")]
        public Color rayHitColor;
        [Tooltip("Color of the unhitted debug rays.")]
        public Color rayNotHitColor;
        [Tooltip("The 'stackedObservation' property represents a collection of observations that have been stacked together in a specific format, where it allows multiple observations to be processed and analyzed as a single input.")]
        public int stackedObservations = 1;
        [Tooltip("The sensor owner.")]
        public BasicAgent agent;

        private HistoryStack<float> stack;
        private int depth = 1;
        private float angleStep;
        private Dictionary<string, int> mapping;
        private float halfFOV = 90;
        protected int[] Shape;
        protected SensorType Type;

        private bool initialized = false;

        public SensorType type
        {
            get 
            {
                return Type;
            }

            set
            {
                Type = value;
            }
        }
        
        public int[] shape 
        {
            get
            {
                return Shape;
            }

            set
            {
                Shape = value;
            }
        }

        void Awake()
        {
            initialize();
        }

        public void initialize()
        {
            if (initialized)
             {
                return;
             }
            depth = 1;
            type = SensorType.sfloatarray;
            mapping = new Dictionary<string, int>();
            if (returnDepthMatrix)
            {
                depth = 2;
            }

            shape = new int[1]{numberOfRays * depth};            
            stack = new HistoryStack<float>(stackedObservations * shape[0]);

            if (automaticTagMapping)
            {
                #if UNITY_EDITOR
                for (int i = 0; i < UnityEditorInternal.InternalEditorUtility.tags.Length; i++)
                {
                        string tag = UnityEditorInternal.InternalEditorUtility.tags[i];
                        mapping[tag] = i * tagCodeDistance;
                }
                #endif
            }
            else
            {
                foreach(ObjectMapping obj in objectMapping)
                {
                    mapping[obj.tag] = obj.code;
                }
            }

            agent.AddResetListener(this);
            initialized = true;
        }

        public void OnReset(Agent agent) 
        {
            stack = new HistoryStack<float>(stackedObservations * shape[0]);
            mapping = new Dictionary<string, int>();
            foreach(ObjectMapping obj in objectMapping)
            {
                mapping[obj.tag] = obj.code;
            }
            RayCasting();
        }

        void OnDrawGizmos()
        {

            if (numberOfRays > 1)
            {
                angleStep = fieldOfView/(numberOfRays - 1);
            }
            else
            {
                angleStep = 0;
            }

            halfFOV = fieldOfView/2;
            if (shape == null || stack == null)
            {
                type = SensorType.sfloatarray;
                shape = new int[1]{numberOfRays};
                stack = new HistoryStack<float>(stackedObservations * numberOfRays);
                mapping = new Dictionary<string, int>();
                foreach(ObjectMapping obj in objectMapping)
                {
                    mapping[obj.tag] = obj.code;
                }
            }
            RayCasting();
        }


        void RayCasting()
        {
            Vector3 pos = eye.transform.position;
            Vector3 fwd = eye.transform.forward;

            if (numberOfRays > 1)
            {
                angleStep = fieldOfView/(numberOfRays - 1);
            }
            else
            {
                angleStep = 0;
            }

            halfFOV = fieldOfView/2;
            
            for (uint i = 0; i < numberOfRays; i++)
            {
                float angle =  0;
                
                if (numberOfRays > 1)
                {
                    angle = i * angleStep - halfFOV;
                }
                
                Vector3 direction = Quaternion.Euler(verticalShift, angle + horizontalShift, 0) * fwd;
                RaycastHit hitinfo;
                if (Physics.Raycast(pos, direction, out hitinfo, maxDistance))
                {
                    GameObject gobj = hitinfo.collider.gameObject;
                    string objtag = gobj.tag;
                    if (mapping.ContainsKey(objtag))
                    {
                        int code = mapping[objtag];
                        stack.Push(code);
                        if (returnDepthMatrix)
                        {
                            stack.Push(hitinfo.distance);
                        }
                    }
                    else 
                    {
                        stack.Push(noObjectCode);
                        if (returnDepthMatrix)
                        {
                            stack.Push(hitinfo.distance);
                        }
                    }
                    if (debugMode)
                    {
                        Debug.DrawRay(pos, direction * hitinfo.distance, new Color(rayHitColor.r, rayHitColor.g, rayHitColor.b));
                    }
                }
                else
                {
                    if (debugMode)
                    {
                        Debug.DrawRay(pos, direction * hitinfo.distance, new Color(rayNotHitColor.r, rayNotHitColor.g, rayNotHitColor.b));
                    }
                    stack.Push(noObjectCode);
                    if (returnDepthMatrix)
                    {
                        stack.Push(-1);
                    }
                }
            }
        }

        void FixedUpdate()
        {
            RayCasting();
        }

        public float[] History
        {
            get
            {
                return stack.Values;
            }
        }
    }
}
