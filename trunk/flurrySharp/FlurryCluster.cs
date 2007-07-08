/*
 * Created by SharpDevelop.
 * User: FurYy
 * Date: 7.07.2007
 * Time: 21:45
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using Tao.OpenGl;

namespace FlurrySharp
{
	public class FlurryCoreData:Types.GlobalInfo
	{
		
	}
	/// <summary>
	/// Description of FlurryCluster.
	/// </summary>
	/// 
	
	public class FlurryCluster
	{
		public FlurryCoreData flurryData;
		Gl_saver saver;//HACK this kinda complicates things as GL_saver depends on GlobalInfo and FlurryCluster init's GlobalInfo :P
		double oldFrameTime=0;
		
		
		/*
		 * Note: the Flurry base code keeps everything in a global variable named
		 * info.  We want to instance it, for multimon support (several separate
		 * Flurries), so we allocate several such structures, but to avoid changing
		 * the base code, we set info = current->globals before calling into it.
		 * Obviously, not thread safe.
		 */

		public FlurryCluster(FlurryClusterSpec spec)
		{
			saver=new Gl_saver();//HACK  I dont know if this is correct to initilize GL_saver for every cluster ? Doesn't OpenGL go nutties a little bit?
			saver.OTSetup();
			oldFrameTime = saver.TimeInSecondsSinceStart();

            flurryData = FlurryAlloc(spec);
			

			// specialize
			//flurryData.numStreams = spec.nStreams;
			//flurryData.currentColorMode = (Types.ColorModes)spec.color;
			//flurryData.streamExpansion = spec.thickness;
			//flurryData.star.rotSpeed = spec.speed;
		}


//		~FlurryCluster()
//		{
//			int i;
//			for (i = 0; i < MAXNUMPARTICLES; i++) {
//				free(flurryData->p[i]);
//			}
//			free(flurryData->s);
//			free(flurryData->star);
//			for (i = 0; i < 64; i++) {
//				free(flurryData->spark[i]);
//			}
//			free(flurryData);
//		}


		public void SetSize(int width, int height)
		{
			// make this flurry cluster current
			BecomeCurrent();
			// resize it
			saver.GLResize(width, height);
            foreach (Particle p in flurryData.particles)
                p.InitParticle();
		}


		public void PrepareToAnimate()
		{
			// make this flurry cluster current
			BecomeCurrent();
			// initialize it
			saver.GLSetupRC(flurryData);
            
		}

		double g_delay = 0.0;
		public void AnimateOneFrame()
		{
			// make this flurry cluster current
			BecomeCurrent();

			// Calculate the amount of progress made since the last frame
			// The Flurry core code does this itself, but we do our own calculation
			// here, and if we don't like the answer, we adjust our copy and then
			// tell the timer to lie so that when the core code reads it, it gets
			// the adjusted value.
			//double newFrameTime = saver.TimeInSecondsSinceStart();
			double newFrameTime = saver.TimeInSecondsSinceStart(g_delay);
			double deltaFrameTime = newFrameTime - oldFrameTime;
			if (Types.iMaxFrameProgressInMs > 0) {
				double maxFrameTime = Types.iMaxFrameProgressInMs / 1000.0;
				double overtime = deltaFrameTime - maxFrameTime;

				if (overtime > 0) {
					//_RPT3(_CRT_WARN, "Delay: hiding %g seconds (last=%g limit=%g)\n",
					//      overtime, deltaFrameTime, maxFrameTime);
					//TimeSupport_HideHostDelay(overtime);
					g_delay += overtime;
					deltaFrameTime -= overtime;
					newFrameTime -= overtime;
				}
			}
			oldFrameTime = newFrameTime;

			// dim the existing screen contents
			Gl.glBlendFunc(Gl.GL_SRC_ALPHA, Gl.GL_ONE_MINUS_SRC_ALPHA);
			Gl.glColor4d(0.0, 0.0, 0.0, 3.0 * deltaFrameTime);//HACK changed 5.0* deltaFrameTime to 1.0*delta...
			Gl.glRectd(0, 0, saver.info.sys_glWidth, saver.info.sys_glHeight);

			// then render the new frame blended over what's already there
			saver.GLRenderScene();
			Gl.glFlush();
		}


		FlurryCoreData FlurryAlloc(FlurryClusterSpec spec)
		{
			int i;
			FlurryCoreData flurry = new FlurryCoreData();
			flurry.flurryRandomSeed = Tools.RandFlt(0.0f, 300.0f);

            flurry.numStreams = spec.nStreams;
            flurry.currentColorMode = (Types.ColorModes)spec.color;
            flurry.streamExpansion = spec.thickness;
            

			//flurry.numStreams = 5;
			//flurry.streamExpansion = 100;
			//flurry.currentColorMode = Types.ColorModes.tiedyeColorMode;
			
			for (i = 0; i < Types.MAXNUMPARTICLES; i++) {
				flurry.particles[i] = new Particle(flurry);
			}
			
			flurry.smoke = new Smoke(flurry);
			//InitSmoke(flurry->s);
			
			
			flurry.star = new Star(flurry);
			//InitStar(flurry->star);
            flurry.star.rotSpeed = spec.speed;
			//flurry.star.rotSpeed = 1.0f;
			flurry.spark=new Spark[spec.nStreams];
			for (i = 0; i < flurry.spark.Length; i++) {
				flurry.spark[i] = new Spark(flurry);
				//InitSpark(flurry->spark[i]);
			}

			return flurry;
		}


		void BecomeCurrent()
		{
			saver.info = flurryData;
		}

	}
}
