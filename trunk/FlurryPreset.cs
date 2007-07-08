/*
 * Created by SharpDevelop.
 * User: FurYy
 * Date: 7.07.2007
 * Time: 21:52
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections.Generic;

namespace FlurrySharp
{
	/// <summary>
	/// Description of FlurryClusterSpec.
	/// </summary>
	
	public class FlurryClusterSpec
	{
		public int nStreams;
		public Types.ColorModes color;
		public float thickness;
		public float speed;
		
		public FlurryClusterSpec(int a_nStreams,Types.ColorModes a_color,float a_thickness,float a_speed){
			nStreams=a_nStreams;
			color=a_color;
			thickness=a_thickness;
			speed=a_speed;
		}
	}
	
	public class FlurrySpec {

		public bool valid;
		public string name;
		/*vector<FlurryClusterSpec>*/
		public List<FlurryClusterSpec> clusters;
		public FlurrySpec(string format)
		{
			clusters=new List<FlurryClusterSpec>();
			valid=ParseFromString(format);
		}

		//bool WriteToString(char *format, int formatLen);
		bool ParseFromString(string format)
		{
			int nStreams;
			Types.ColorModes color=Types.ColorModes.cyclicColorMode;
			float thickness;
			float speed;
			string [] splitVals;
			
			string[] split=format.Split(':');
			name=split[0];
			//try{
				split=split[1].Split(';');
				foreach(string s in split)
				{
					splitVals=s.Trim(new char[]{'{','}'}).Split(',');
					nStreams=int.Parse(splitVals[0]);
					color=GetColor(splitVals[1]);
					thickness=float.Parse(splitVals[2],System.Globalization.NumberStyles.Any); //HACK WTF is the matter with this throwing exceptions :S
                    speed = float.Parse(splitVals[3], System.Globalization.NumberStyles.Any);
					clusters.Add(new FlurryClusterSpec(nStreams,color,thickness,speed));
				}
				
				if(clusters.Count>0)
					return true;
			//}catch{}
			
			return false;
		}
		
		Types.ColorModes GetColor(string format)//Seems to work on .NET 2.0
		{
            Types.ColorModes mode = Types.ColorModes.blueColorMode;

            System.Reflection.FieldInfo[] fields = mode.GetType().GetFields();
            foreach (System.Reflection.FieldInfo f in fields)
            {
                if (f.Name.Contains(format))
                    mode = (Types.ColorModes)f.GetValue(f);
            }

			return mode;
		}
	}
}
