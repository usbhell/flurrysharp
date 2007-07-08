/*
 * Created by SharpDevelop.
 * User: FurYy
 * Date: 7.07.2007
 * Time: 17:23
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;

namespace FlurrySharp
{
	/// <summary>
	/// Description of Spark.
	/// </summary>
	public class Spark
	{
		public float[] position=new float[3];
		public int mystery;
		public float[] delta=new float[3];
		public float[] color=new float[4];
		
		const float BIGMYSTERY = 1800.0f;
		const int MAXANGLES = 16384;
		
		Types.GlobalInfo info;
		
		public Spark(Types.GlobalInfo a_info)
		{
			info=a_info;
			InitSpark();
		}
		
		void InitSpark()
		{
			int i;
			for (i = 0; i < 3; i++) {
				position[i] = Tools.RandFlt(-100.0f, 100.0f);
			}
		}
		
		
		public void UpdateSparkColour()
		{
			const float rotationsPerSecond = (float) (2.0 * Math.PI * Types.GlobalInfo.fieldSpeed / MAXANGLES);
			double thisPointInRadians = 2.0 * Math.PI * mystery / BIGMYSTERY;
			double thisAngle = info.fTime * rotationsPerSecond;
			float cycleTime = 20.0f;
			float colorRot;
			float redPhaseShift;
			float greenPhaseShift;
			float bluePhaseShift;
			float baseRed;
			float baseGreen;
			float baseBlue;
			float colorTime;
			
			switch (info.currentColorMode) {
					case Types.ColorModes.rainbowColorMode: cycleTime = 1.5f; break;
					case Types.ColorModes.tiedyeColorMode: cycleTime = 4.5f; break;
					case Types.ColorModes.cyclicColorMode: cycleTime = 20.0f; break;
					case Types.ColorModes.slowCyclicColorMode: cycleTime = 120.0f; break;
			}

			colorRot = (float) (2.0 * Math.PI/cycleTime);
			redPhaseShift = 0.0f;
			greenPhaseShift = cycleTime / 3.0f;
			bluePhaseShift = cycleTime * 2.0f / 3.0f;
			colorTime = info.fTime;
			
			switch (info.currentColorMode) {
				case Types.ColorModes.whiteColorMode:
					baseRed = 0.1875f;
					baseGreen = 0.1875f;
					baseBlue = 0.1875f;
					break;
				case Types.ColorModes.multiColorMode:
					baseRed = 0.0625f;
					baseGreen = 0.0625f;
					baseBlue = 0.0625f;
					break;
				case Types.ColorModes.darkColorMode:
					baseRed = 0.0f;
					baseGreen = 0.0f;
					baseBlue = 0.0f;
					break;
				default:
					if ((int)info.currentColorMode < (int)Types.ColorModes.slowCyclicColorMode) {
						colorTime = ((float)info.currentColorMode / 6.0f) * cycleTime;
					} else {
						colorTime = info.fTime + info.flurryRandomSeed;
					}
					baseRed   = 0.109375f * ((float) Math.Cos((colorTime+redPhaseShift) * colorRot) + 1.0f);
					baseGreen = 0.109375f * ((float) Math.Cos((colorTime+greenPhaseShift) * colorRot) + 1.0f);
					baseBlue  = 0.109375f * ((float) Math.Cos((colorTime+bluePhaseShift) * colorRot) + 1.0f);
					break;
			}
			
			color[0] = baseRed   + 0.0625f * (0.5f + (float) Math.Cos((15.0 * (thisPointInRadians + 3.0 * thisAngle)))
			                                  + (float) Math.Sin((7.0  * (thisPointInRadians + thisAngle))));
			color[1] = baseGreen + 0.0625f * (0.5f + (float) Math.Sin(((thisPointInRadians) + thisAngle)));
			color[2] = baseBlue  + 0.0625f * (0.5f + (float) Math.Cos((37.0 * (thisPointInRadians + thisAngle))));
		}
		
		
		
		public void UpdateSpark()
		{
			float rotationsPerSecond = (float) (2.0 * Math.PI * Types.GlobalInfo.fieldSpeed / MAXANGLES);
			double thisPointInRadians = 2.0 * Math.PI * mystery / BIGMYSTERY;
			double thisAngle = info.fTime * rotationsPerSecond;
			double cf;
			int i;
			double tmpX1,tmpY1,tmpZ1;
			double tmpX2,tmpY2,tmpZ2;
			double tmpX3,tmpY3,tmpZ3;
			double tmpX4,tmpY4,tmpZ4;
			double rotation;
			double cr;
			double sr;
			float cycleTime = 20.0f;
			float [] old=new float[3];

			UpdateSparkColour();
			
			for (i = 0; i < 3; i++) {
				old[i] = position[i];
			}
			
			cf = Math.Cos(7.0 * thisAngle) + Math.Cos(3.0 * thisAngle) + Math.Cos(13.0 * thisAngle);
			cf /= 6.0f;
			cf += 2.0f;

			position[0] = (float)(Types.GlobalInfo.fieldRange * cf * Math.Cos(11.0 * (thisPointInRadians + (3.0  * thisAngle))));
			position[1] = (float)(Types.GlobalInfo.fieldRange * cf * Math.Sin(12.0 * (thisPointInRadians + (4.0  * thisAngle))));
			position[2] = (float)(Types.GlobalInfo.fieldRange *      Math.Cos(23.0 * (thisPointInRadians + (12.0 * thisAngle))));
			
			rotation = thisAngle * 0.501 + 5.01 * (double) mystery / (double) BIGMYSTERY;
			cr = Math.Cos(rotation);
			sr = Math.Sin(rotation);
			tmpX1 = position[0] * cr - position[1] * sr;
			tmpY1 = position[1] * cr + position[0] * sr;
			tmpZ1 = position[2];
			
			tmpX2 = tmpX1 * cr - tmpZ1 * sr;
			tmpY2 = tmpY1;
			tmpZ2 = tmpZ1 * cr + tmpX1 * sr;
			
			tmpX3 = tmpX2;
			tmpY3 = tmpY2 * cr - tmpZ2 * sr;
			tmpZ3 = tmpZ2 * cr + tmpY2 * sr + Types.GlobalInfo.seraphDistance;
			
			rotation = thisAngle * 2.501 + 85.01 * (double) mystery / (double) BIGMYSTERY;
			cr = Math.Cos(rotation);
			sr = Math.Sin(rotation);
			tmpX4 = tmpX3 * cr - tmpY3 * sr;
			tmpY4 = tmpY3 * cr + tmpX3 * sr;
			tmpZ4 = tmpZ3;
			
			position[0] = (float) tmpX4 + Tools.RandBell(5.0f * Types.GlobalInfo.fieldCoherence);
			position[1] = (float) tmpY4 + Tools.RandBell(5.0f * Types.GlobalInfo.fieldCoherence);
			position[2] = (float) tmpZ4 + Tools.RandBell(5.0f * Types.GlobalInfo.fieldCoherence);

			for (i = 0; i < 3; i++) {
				delta[i] = (position[i] - old[i]) / info.fDeltaTime;
			}
		}
	}
}
