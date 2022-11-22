using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ai4u
{

    public class MultTouchPrecondition : RewardFunc
    {
        public TouchRewardFunc[] preconditions;
        public bool atLeastOne = true;

        public bool allowNext
        {
            get
            {
                if (this.preconditions != null && this.preconditions.Length > 0)
                {
                    bool r = true;
                    foreach(TouchRewardFunc t in this.preconditions)
                    {
                        if (atLeastOne)
                        {
                            r = false;
                            if (t.allowNext)
                            {
                                return true;
                            }
                        } else {
                            if (!t.allowNext) {
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

        public bool wasTouched(BasicAgent agent)
        {
            if (this.preconditions != null && this.preconditions.Length > 0)
            {
                bool r = true;
                foreach (TouchRewardFunc t in preconditions)
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
