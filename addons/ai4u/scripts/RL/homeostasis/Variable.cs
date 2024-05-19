using System.Collections;
using System.Collections.Generic;

namespace ai4u.ext;


[System.Serializable]
public class Variable
{
    public string name;
    public float rangeMin;
    public float rangeMax;
    public float minValue;
    public float maxValue;
 
    protected float mValue;


    public Variable(string name="")
    {
        this.name = name;
    }

    public float Value
    {
        get
        {
            return mValue;
        }

        set
        {
            this.mValue = value;
        }
    }
}
