using System.Collections;
using System.Collections.Generic;
using System.Text;
using System;
using Godot;

namespace ai4u.ext;

[Serializable]
public class HomeostaticVariable: Variable
{
    private bool useDefinedCentroid = false;
    private float userCentroid = 0.0f;

    public void ResetCentroid()
    {
        this.userCentroid = 0.0f;
        this.useDefinedCentroid = false;
    }

    public void SetCentroid(float f)
    {
        this.userCentroid = f;
        this.useDefinedCentroid = true;
    }

    public bool CheckValue(float value)
    {
        return value >= minValue && value <= maxValue;
    }

    public void AddValue(float value)
    {
        this.mValue += value;

        if (this.mValue > rangeMax)
        {
            this.mValue = rangeMax;
        }
        else if (this.mValue < rangeMin)
        {
            this.mValue = rangeMin;
        }
    }

    public bool Check()
    {
        return mValue >= minValue && mValue <= maxValue;
    }

    public void Reset()
    {

    }

    public float GetNormalizedMinValue()
    {
        return (minValue - rangeMin)/(rangeMax-rangeMin);
    }

    public float GetNormalizedMaxValue()
    {
        return (minValue - rangeMin)/(rangeMax-rangeMin);
    }

    public float GetNormalizedValue()
    {
        return (mValue - rangeMin) / (rangeMax - rangeMin);
    }

    public float Centroid
    {
        get
        {
            if (this.useDefinedCentroid)
            {
                return this.userCentroid;
            }
            else
            {
                return 0.5f * (minValue+maxValue);
            }
        }
    }

    public float PositionToCentroid
    {
        get
        {
            return Value - Centroid;
        }
    }

    public float DistanceToCentroid
    {
        get
        {
            return Math.Abs(PositionToCentroid);
        }
    }

    public float NormalizedDistanceToCentroid
    {
        get
        {
            return DistanceToCentroid / (rangeMax - rangeMin);
        }
    }

    public float PositionToMin
    {
        get
        {
            return Value - minValue;
        }
    }

    public float PositionToMax
    {
        get
        {
            return Value - maxValue;
        }
    }

    public float DistanteToMin
    {
        get
        {
            return Math.Abs(PositionToMin);
        }
    }

    public float DistanceToMax
    {
        get
        {
            return Math.Abs(PositionToMax);
        }
    }
}
