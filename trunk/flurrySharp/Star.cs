/*
 * Created by SharpDevelop.
 * User: FurYy
 * Date: 7.07.2007
 * Time: 17:22
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;

namespace FlurrySharp
{
	/// <summary>
	/// Description of Star.
	/// </summary>
	public class Star
	{
//		typedef struct Star
		//{
		public float [] position=new float[3];
		public float mystery;
		public float rotSpeed;
		//} Star;
		
		const float BIGMYSTERY = 1800f;
		const int MAXANGLES = 16384;
		Types.GlobalInfo info;

		//Randomizer rand;
		
		public Star(Types.GlobalInfo a_info)
		{
			info=a_info;
			InitStar();
		
		}
		
		void InitStar()
		{
			
			int i;
			for (i = 0; i < 3; i++) {
				position[i] = Tools.RandFlt(-10000.0f, 10000.0f);
			}
			rotSpeed = Tools.RandFlt(0.4f, 0.9f);
			mystery = Tools.RandFlt(0.0f, 10.0f);
		}
		
		public void UpdateStar()
		{
			float rotationsPerSecond = (float) (2.0 * Math.PI * 12.0 / MAXANGLES) * rotSpeed; /* speed control */
			double thisPointInRadians = 2.0 * Math.PI * mystery / BIGMYSTERY;
			double thisAngle = info.fTime*rotationsPerSecond;
			double cf;
			double tmpX1,tmpY1,tmpZ1;
			double tmpX2,tmpY2,tmpZ2;
			double tmpX3,tmpY3,tmpZ3;
			double tmpX4,tmpY4,tmpZ4;
			double rotation;
			double cr;
			double sr;

			cf =  Math.Cos(7.0  * info.fTime * rotationsPerSecond)
				+ Math.Cos(3.0  * info.fTime * rotationsPerSecond)
				+ Math.Cos(13.0 * info.fTime * rotationsPerSecond);
			cf /= 6.0f;
			cf += 0.75f;
			
			position[0] = (float)(250.0f * cf * Math.Cos(11.0 * (thisPointInRadians + (3.0 *  thisAngle))));
			position[1] = (float)(250.0f * cf * Math.Sin(12.0 * (thisPointInRadians + (4.0 *  thisAngle))));
			position[2] = (float)(250.0f *      Math.Cos(23.0 * (thisPointInRadians + (12.0 * thisAngle))));
			
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
			
			position[0] = (float) tmpX4;
			position[1] = (float) tmpY4;
			position[2] = (float) tmpZ4;
		}

	}
}
