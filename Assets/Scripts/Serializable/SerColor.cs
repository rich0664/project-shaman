using UnityEngine;
using System.Collections;

[System.Serializable]
public class SerColor {

	public float[] colorFA = null;
	public SerColor(Color value){
		color = value;
	}

	[System.NonSerialized] public Color pColor;
	public Color color{
		get{			
			if(pColor == Color.clear){
				if(colorFA == null)
					return Color.clear;
				pColor = new Color(colorFA[0], colorFA[1], colorFA[2], colorFA[3]);
				return pColor;
			}else{
				return pColor;
			}
		}
		set{ pColor = value; colorFA = new float[]{value.r, value.g, value.b, value.a}; }
	}

	public static implicit operator SerColor (Color value){
		return new SerColor(value);
	}

	public static implicit operator Color (SerColor sc){
		return sc.color;
	}

}
