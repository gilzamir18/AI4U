using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ai4u.ext
{

    public class MultTouchPrecondiction : MonoBehaviour
    {
        public TouchRewardFunc[] precondictions;
        private bool aNext;
        public bool atLeastOne = true;

        public bool allowNext
        {
            get
            {
                if (this.precondictions != null && this.precondictions.Length > 0)
                {
                    bool r = true;
                    foreach(TouchRewardFunc t in this.precondictions)
                    {
                        if (atLeastOne)
                        {
                            r = false;
                            if (t.allowNext)
                            {
                                return true;
                            }
                        } else {
                            if (t.allowNext)
                            {
                                if (!t.allowNext)
                                {
                                    return false;
                                }
                                r = r && t.allowNext;
                            }
                        }
                    }
                    return r;
                } else
                {
                    return true;
                }
            }
        }

        public bool wasTouched(RLAgent agent)
        {
            if (this.precondictions != null && this.precondictions.Length > 0)
            {
                bool r = true;
                foreach (TouchRewardFunc t in precondictions)
                {
                    if (atLeastOne)
                    {
                        r = false;
                        if (t.wasTouched(agent))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (!t.wasTouched(agent))
                        {
                            return false;
                        }
                    }
                }
                return r;
            } else
            {
                return true;
            }
        }
    }
}
