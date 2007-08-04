/*
 * Created by SharpDevelop.
 * User: FurYy
 * Date: 7.07.2007
 * Time: 20:51
 * 
 * Currently stripping all AltiVec etc.,so called "special accelerated modes".
 */

using System;
using Tao.OpenGl;

namespace FlurrySharp
{
	/// <summary>
	/// Description of Gl_saver.
	/// </summary>
	public class Gl_saver
	{
		public Gl_saver(/*FlurryCoreData a_info*/)//a_info is FlurryCoreData derived from GlobalInfo
		{
			//info=(Types.GlobalInfo)a_info;
		}
		
		const int TRUE=1;
		const int FALSE=0;
		
		// some globals
		public Types.GlobalInfo info=null; //Is getter better? static?
		double gStartTime = 0.0;
		TimeSpan startTime=new TimeSpan(DateTime.Now.Ticks);


		public void OTSetup()
		{
			//if (gStartTime == 0.0) {
			//	gStartTime = CurrentTime();
			//}
			startTime=new TimeSpan(DateTime.Now.Ticks);
			gStartTime=startTime.TotalSeconds;
		}

		public double CurrentTime()
		{
			TimeSpan span=new TimeSpan(DateTime.Now.Ticks);
			return span.TotalSeconds;
		}

		public double TimeInSecondsSinceStart()
		{
			return (CurrentTime() - gStartTime);
		}

		public double TimeInSecondsSinceStart(double delay)
		{
			return (CurrentTime() - gStartTime) - delay;
		}

		
		///<summary>
		/// Do any initialization of the rendering context here, such as
		/// setting background colors, setting up lighting, or performing
		/// preliminary calculations.
		/// </summary>
		public void GLSetupRC(Types.GlobalInfo a_info)
		{
			int i,k;
			info=a_info;

			// timing setup
			OTSetup();
			info.fTime = (float)TimeInSecondsSinceStart() + info.flurryRandomSeed;
			info.fOldTime = info.fTime;
			info.optMode = Types.OPT_MODE_SCALAR_BASE;

			// initialize particles
			
			for (i = 0; i < info.smoke.smokev.particles.Length; i++) {
				for (k = 0; k < info.smoke.smokev.particles[i].dead.i.Length/*4*/; k++) {
					info.smoke.smokev.particles[i].dead.i[k] = TRUE;
				}
			}
			
			for (i = 0; i < /*12*/info.spark.Length; i++) {
				info.spark[i].mystery = 1800 * (i + 1) / /*13*/(info.spark.Length+1);//HACK
				info.spark[i].UpdateSpark();
			}
			
			//foreach(Spark s in info.spark)
			//{
			//	s.mystery = 1800 * (i + 1) / 13;
			//	s.UpdateSpark();
			//}
			
			// setup the defaults for OpenGL
			Gl.glDisable(Gl.GL_DEPTH_TEST);
			Gl.glAlphaFunc(Gl.GL_GREATER, 0.0f);
			Gl.glEnable(Gl.GL_ALPHA_TEST);
			Gl.glShadeModel(Gl.GL_FLAT);
			Gl.glDisable(Gl.GL_LIGHTING);
			Gl.glDisable(Gl.GL_CULL_FACE);
			Gl.glEnable(Gl.GL_BLEND);
			
			Gl.glViewport(0, 0, (int)info.sys_glWidth, (int)info.sys_glHeight);
			Gl.glMatrixMode(Gl.GL_PROJECTION);
			Gl.glLoadIdentity();
			Glu.gluOrtho2D(0, info.sys_glWidth, 0, info.sys_glHeight);
			Gl.glMatrixMode(Gl.GL_MODELVIEW);
			Gl.glLoadIdentity();
			
			Gl.glClearColor(0.0f, 0.0f, 0.0f, 1.0f);
			#if !USEFADEHACK
			Gl.glClear(Gl.GL_COLOR_BUFFER_BIT); //HACK GLsetupRC() hack for fading startup
			#endif
			
			Gl.glEnableClientState(Gl.GL_COLOR_ARRAY);
			Gl.glEnableClientState(Gl.GL_VERTEX_ARRAY);
			Gl.glEnableClientState(Gl.GL_TEXTURE_COORD_ARRAY);
		}


		//////////////////////////////////////////////////////////////////
		//////////////////////////////////////////////////////////////////
		// Render the OpenGL Scene here. Called by the WM_PAINT message
		// handler.
		//#include <crtdbg.h>
		public void GLRenderScene()
		{
			//int i;
			
			info.dframe++;
			
			info.fOldTime = info.fTime;
			info.fTime = (float)TimeInSecondsSinceStart() + info.flurryRandomSeed;
			info.fDeltaTime = info.fTime - info.fOldTime;
			//_RPT1(_CRT_WARN, "base code thinks last frame took %g sec\n", info.fDeltaTime);
			Console.Error.WriteLine("base code thinks last frame took {0} sec", info.fDeltaTime);
			
			info.drag = (float) Math.Pow(0.9965, info.fDeltaTime * 85.0);
			
			//for (i = 0; i < numParticles; i++) {
			//	UpdateParticle(info.particles[i]);
			//}
			foreach(Particle p in info.particles)
				p.UpdateParticle();
			
			//UpdateStar(info.star);
			info.star.UpdateStar();
			
			//for (i = 0; i <info.numStreams; i++)
			//{
			//    //UpdateSpark(info.spark[i]);
			//    info.spark[i].UpdateSpark();
			//}
			
			foreach(Spark s in info.spark){
				s.UpdateSpark();
			}
			
//			switch(info.optMode) {
//				case Types.OPT_MODE_SCALAR_BASE:
//					//UpdateSmoke_ScalarBase(info.smoke);
//					info.smoke.UpdateSmoke_ScalarBase();
//					break;
//				default:
//					break;
//			}
			info.smoke.UpdateSmoke_ScalarBase();
			
			Gl.glBlendFunc(Gl.GL_SRC_ALPHA,Gl.GL_ONE);
			Gl.glEnable(Gl.GL_TEXTURE_2D);
			
//			switch(info.optMode) {
//				case Types.OPT_MODE_SCALAR_BASE:
//				case Types.OPT_MODE_SCALAR_FRSQRTE:
//					//DrawSmoke_Scalar(info.smoke);
//					info.smoke.DrawSmoke_Scalar();
//					break;
//				default:
//					break;
//			}
			info.smoke.DrawSmoke_Scalar();
			
			Gl.glDisable(Gl.GL_TEXTURE_2D);
		}


		public void GLResize(int w, int h)
		{
			info.sys_glWidth = w;
			info.sys_glHeight = h;
		}

		
	}
}
