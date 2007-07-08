/*
 * Created by SharpDevelop.
 * User: FurYy
 * Date: 7.07.2007
 * Time: 15:25
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headersmokes.
 */

using System;
using Tao.OpenGl;
using System.Runtime;
using System.Runtime.InteropServices;

namespace FlurrySharp
{
	[StructLayout(LayoutKind.Sequential)]
	public struct floatToVector
	{
		public float[]f;
        public floatToVector(int size)
        {
            f = new float[4];
            for(int i=0;i<4;i++)
                f[i]=0;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct intToVector
	{
		public int[]i;
        public intToVector(int size)
        {
            i = new int[4];
            for (int a = 0; a < 4; a++)
                i[a] = 0;
        }
	}

	public class SmokeParticleV
	{
		public floatToVector[] color=new floatToVector[4];
		public floatToVector[] position=new floatToVector[3];
		public floatToVector[] oldposition=new floatToVector[3];
		public floatToVector[] delta=new floatToVector[3];
		public intToVector dead=new intToVector(4);
		public floatToVector time=new floatToVector(4);
		public intToVector animFrame=new intToVector(4);
        public SmokeParticleV()
        {
            for (int i = 0; i < delta.Length; i++)
                delta[i] = new floatToVector(4);

            for (int i = 0; i < oldposition.Length; i++)
                oldposition[i] = new floatToVector(4);

            for (int i = 0; i < position.Length; i++)
                position[i] = new floatToVector(4);

            for (int i = 0; i < color.Length; i++)
                color[i] = new floatToVector(4);
        }
    }

    public class SmokeV
	{
		public SmokeParticleV [] particles;
		public int nextParticle;
		public int nextSubParticle;
		public float lastParticleTime;
		public bool firstTime;
		public long frame;
		public float[] old;
        
		public floatToVector[] seraphimVertices;
        
        [MarshalAs(UnmanagedType.LPStruct)]
		public floatToVector[] seraphimColors;

		public float[] seraphimTextures;
		
		public SmokeV(int NUMSMOKEPARTICLES){
			particles=new SmokeParticleV[NUMSMOKEPARTICLES / 4];
            for (int i = 0; i < particles.Length; i++)
                particles[i] = new SmokeParticleV();

			nextParticle=0;
			nextSubParticle=0;
			lastParticleTime=0;
			firstTime=true;
			frame=0;
			old=new float[3];
			seraphimVertices=new floatToVector[NUMSMOKEPARTICLES * 2 + 1];
            
            for (int i = 0; i < seraphimVertices.Length; i++)
                seraphimVertices[i] = new floatToVector(4);

			seraphimColors=new floatToVector[NUMSMOKEPARTICLES * 4 + 1];
            for (int i = 0; i < seraphimColors.Length; i++)
                seraphimColors[i] = new floatToVector(4);

			seraphimTextures=new float[NUMSMOKEPARTICLES * 2 * 4];
		}
	} 
	
	/// <summary>
	/// Description of Smoke.
	/// </summary>
	public class Smoke
	{
		public const int NUMSMOKEPARTICLES = 3600;
		public const int MAXANGLES = 16384;
		public const int NOT_QUITE_DEAD = 3;
		public const float intensity = 75000.0f;
		public const int FALSE = 0;
		public const int TRUE = 1;
		
		Tools rand=new Tools();
		public SmokeV smokev;
		Types.GlobalInfo info;
		
		public Smoke(Types.GlobalInfo a_info)
		{
			info=a_info;
			smokev=new SmokeV(NUMSMOKEPARTICLES);
			
		}
		
		void InitSmoke()
		{
			int i;
			smokev.nextParticle = 0;
			smokev.nextSubParticle = 0;
			smokev.lastParticleTime = 0.25f;
			smokev.firstTime = true;
			smokev.frame = 0;
			for (i = 0; i < 3; i++) {
				smokev.old[i] = Tools.RandFlt(-100.0f, 100.0f);
			}
		}
		
		
		public void UpdateSmoke_ScalarBase()
		{
			int i,j,k;
			float sx = info.star.position[0];
			float sy = info.star.position[1];
			float sz = info.star.position[2];
			float frameRate;
			float frameRateModifier;

			smokev.frame++;

			if(!smokev.firstTime) {
				// release 12 puffs every frame
				if(info.fTime - smokev.lastParticleTime >= 1.0f / 121.0f) {
					float dx,dy,dz,deltax,deltay,deltaz;
					float f;
					float rsquared;
					float mag;

					dx = smokev.old[0] - sx;
					dy = smokev.old[1] - sy;
					dz = smokev.old[2] - sz;
					mag = 5.0f;
					deltax = (dx * mag);
					deltay = (dy * mag);
					deltaz = (dz * mag);
					for(i = 0; i < info.numStreams; i++) {
						float streamSpeedCoherenceFactor;
						
						smokev.particles[smokev.nextParticle].delta[0].f[smokev.nextSubParticle] = deltax;
						smokev.particles[smokev.nextParticle].delta[1].f[smokev.nextSubParticle] = deltay;
						smokev.particles[smokev.nextParticle].delta[2].f[smokev.nextSubParticle] = deltaz;
						smokev.particles[smokev.nextParticle].position[0].f[smokev.nextSubParticle] = sx;
						smokev.particles[smokev.nextParticle].position[1].f[smokev.nextSubParticle] = sy;
						smokev.particles[smokev.nextParticle].position[2].f[smokev.nextSubParticle] = sz;
						smokev.particles[smokev.nextParticle].oldposition[0].f[smokev.nextSubParticle] = sx;
						smokev.particles[smokev.nextParticle].oldposition[1].f[smokev.nextSubParticle] = sy;
						smokev.particles[smokev.nextParticle].oldposition[2].f[smokev.nextSubParticle] = sz;
						streamSpeedCoherenceFactor = Math.Max(0.0f,1.0f + Tools.RandBell(0.25f*Types.GlobalInfo.incohesion));
						dx = smokev.particles[smokev.nextParticle].position[0].f[smokev.nextSubParticle] - info.spark[i].position[0];
						dy = smokev.particles[smokev.nextParticle].position[1].f[smokev.nextSubParticle] - info.spark[i].position[1];
						dz = smokev.particles[smokev.nextParticle].position[2].f[smokev.nextSubParticle] - info.spark[i].position[2];
						rsquared = (dx*dx + dy*dy + dz*dz);
						f = Types.GlobalInfo.streamSpeed * streamSpeedCoherenceFactor;

						mag = f / (float) Math.Sqrt(rsquared);

						smokev.particles[smokev.nextParticle].delta[0].f[smokev.nextSubParticle] -= (dx * mag);
						smokev.particles[smokev.nextParticle].delta[1].f[smokev.nextSubParticle] -= (dy * mag);
						smokev.particles[smokev.nextParticle].delta[2].f[smokev.nextSubParticle] -= (dz * mag);
						smokev.particles[smokev.nextParticle].color[0].f[smokev.nextSubParticle] = info.spark[i].color[0] * (1.0f + Tools.RandBell(Types.GlobalInfo.colorIncoherence));
						smokev.particles[smokev.nextParticle].color[1].f[smokev.nextSubParticle] = info.spark[i].color[1] * (1.0f + Tools.RandBell(Types.GlobalInfo.colorIncoherence));
						smokev.particles[smokev.nextParticle].color[2].f[smokev.nextSubParticle] = info.spark[i].color[2] * (1.0f + Tools.RandBell(Types.GlobalInfo.colorIncoherence));
						smokev.particles[smokev.nextParticle].color[3].f[smokev.nextSubParticle] = 0.85f * (1.0f + Tools.RandBell(0.5f*Types.GlobalInfo.colorIncoherence));
						smokev.particles[smokev.nextParticle].time.f[smokev.nextSubParticle] = info.fTime;
						smokev.particles[smokev.nextParticle].dead.i[smokev.nextSubParticle] = FALSE;
						smokev.particles[smokev.nextParticle].animFrame.i[smokev.nextSubParticle] = Tools.Rand()&63;
						smokev.nextSubParticle++;
						if (smokev.nextSubParticle==4) {
							smokev.nextParticle++;
							smokev.nextSubParticle=0;
						}
						if (smokev.nextParticle >= NUMSMOKEPARTICLES/4) {
							smokev.nextParticle = 0;
							smokev.nextSubParticle = 0;
						}
					}

					smokev.lastParticleTime = info.fTime;
				}
			} else {
				smokev.lastParticleTime = info.fTime;
				smokev.firstTime = false;
			}

			for (i = 0; i < 3; i++) {
				smokev.old[i] = info.star.position[i];
			}
			
			frameRate = info.dframe / info.fTime;
			frameRateModifier = 42.5f / frameRate;

			for (i = 0; i < NUMSMOKEPARTICLES / 4; i++) {
				for (k = 0; k < 4; k++) {
					float dx,dy,dz;
					float f;
					float rsquared;
					float mag;
					float deltax;
					float deltay;
					float deltaz;
					
					if (smokev.particles[i].dead.i[k]==1) {
						continue;
					}
					
					deltax = smokev.particles[i].delta[0].f[k];
					deltay = smokev.particles[i].delta[1].f[k];
					deltaz = smokev.particles[i].delta[2].f[k];
					
					for(j=0;j<info.numStreams;j++) {
						dx = smokev.particles[i].position[0].f[k] - info.spark[j].position[0];
						dy = smokev.particles[i].position[1].f[k] - info.spark[j].position[1];
						dz = smokev.particles[i].position[2].f[k] - info.spark[j].position[2];
						rsquared = (dx*dx+dy*dy+dz*dz);

						f = (Types.GlobalInfo.gravity/rsquared) * frameRateModifier;

						if ((((i*4)+k) % info.numStreams) == j) {
							f *= 1.0f + Types.GlobalInfo.streamBias;
						}
						
						mag = f / (float) Math.Sqrt(rsquared);
						
						deltax -= (dx * mag);
						deltay -= (dy * mag);
						deltaz -= (dz * mag);
					}
					
					// slow this particle down by info.drag
					deltax *= info.drag;
					deltay *= info.drag;
					deltaz *= info.drag;
					
					if((deltax*deltax+deltay*deltay+deltaz*deltaz) >= 25000000.0f) {
						smokev.particles[i].dead.i[k] = TRUE;
						continue;
					}
					
					// update the position
					smokev.particles[i].delta[0].f[k] = deltax;
					smokev.particles[i].delta[1].f[k] = deltay;
					smokev.particles[i].delta[2].f[k] = deltaz;
					for (j = 0; j < 3; j++) {
						smokev.particles[i].oldposition[j].f[k] = smokev.particles[i].position[j].f[k];
						smokev.particles[i].position[j].f[k] += (smokev.particles[i].delta[j].f[k])*info.fDeltaTime;
					}
				}
			}
		}
		
		
        //TODO Release throws memory corrupt exception
		public void DrawSmoke_Scalar()
		{
			int svi = 0;
			int sci = 0;
			int sti = 0;
			int si = 0;
			float width;
			float sx,sy;
			float u0,v0,u1,v1;
			float w,z;
			float screenRatio = info.sys_glWidth / 1024.0f;
			float hslash2 = info.sys_glHeight * 0.5f;
			float wslash2 = info.sys_glWidth * 0.5f;
			int i,k;

			width = (Types.GlobalInfo.streamSize+2.5f*info.streamExpansion) * screenRatio;

			for (i = 0; i < NUMSMOKEPARTICLES / 4; i++) {
				for (k = 0; k < 4; k++) {
					float thisWidth;
					float oldz;
					
					if (smokev.particles[i].dead.i[k] == TRUE) {
						continue;
					}
					thisWidth = (Types.GlobalInfo.streamSize + (info.fTime - smokev.particles[i].time.f[k])*info.streamExpansion) * screenRatio;
					if (thisWidth >= width) {
						smokev.particles[i].dead.i[k] = TRUE;
						continue;
					}
					z = smokev.particles[i].position[2].f[k];
					sx = smokev.particles[i].position[0].f[k] * info.sys_glWidth / z + wslash2;
					sy = smokev.particles[i].position[1].f[k] * info.sys_glWidth / z + hslash2;
					oldz = smokev.particles[i].oldposition[2].f[k];
					if (sx > info.sys_glWidth  + 50.0f || sx < -50.0f ||
					    sy > info.sys_glHeight + 50.0f || sy < -50.0f ||
					    z < 25.0f || oldz < 25.0f) {
						continue;
					}

					w = Math.Max(1.0f,thisWidth/z);

					{
						float oldx = smokev.particles[i].oldposition[0].f[k];
						float oldy = smokev.particles[i].oldposition[1].f[k];
						float oldscreenx = (oldx * info.sys_glWidth / oldz) + wslash2;
						float oldscreeny = (oldy * info.sys_glWidth / oldz) + hslash2;
						float dx = (sx-oldscreenx);
						float dy = (sy-oldscreeny);
						
						float d = Tools.FastDistance2D(dx, dy);
						
						float sm, os, ow;
						if (d!=0f) {
							sm = w/d;
						} else {
							sm = 0.0f;
						}
						ow = Math.Max(1.0f,thisWidth/oldz);
						if (d!=0f) {
							os = ow/d;
						} else {
							os = 0.0f;
						}
						
						{
							floatToVector cmv=new floatToVector(4);
							float cm;
							float m = 1.0f + sm;
							
							float dxs = dx*sm;
							float dys = dy*sm;
							float dxos = dx*os;
							float dyos = dy*os;
							float dxm = dx*m;
							float dym = dy*m;
							
							smokev.particles[i].animFrame.i[k]++;
							if (smokev.particles[i].animFrame.i[k] >= 64) {
								smokev.particles[i].animFrame.i[k] = 0;
							}
							
							u0 = (smokev.particles[i].animFrame.i[k]&7) * 0.125f;
							v0 = (smokev.particles[i].animFrame.i[k]>>3) * 0.125f;
							u1 = u0 + 0.125f;
							v1 = v0 + 0.125f;
							u1 = u0 + 0.125f;
							v1 = v0 + 0.125f;
							cm = (1.375f - thisWidth/width);
							if (smokev.particles[i].dead.i[k] == 3) {
								cm *= 0.125f;
								smokev.particles[i].dead.i[k] = TRUE;
							}
							si++;
							cmv.f[0] = smokev.particles[i].color[0].f[k]*cm;
							cmv.f[1] = smokev.particles[i].color[1].f[k]*cm;
							cmv.f[2] = smokev.particles[i].color[2].f[k]*cm;
							cmv.f[3] = smokev.particles[i].color[3].f[k]*cm;
							
							{
								int ii, jj;
								for (jj = 0; jj < 4; jj++) {
									for (ii = 0; ii < 4; ii++) {
										smokev.seraphimColors[sci].f[ii] = cmv.f[ii];
									}
									sci += 1;
								}
							}
							
							smokev.seraphimTextures[sti++] = u0;
							smokev.seraphimTextures[sti++] = v0;
							smokev.seraphimTextures[sti++] = u0;
							smokev.seraphimTextures[sti++] = v1;
							
							smokev.seraphimTextures[sti++] = u1;
							smokev.seraphimTextures[sti++] = v1;
							smokev.seraphimTextures[sti++] = u1;
							smokev.seraphimTextures[sti++] = v0;
							
							smokev.seraphimVertices[svi].f[0] = sx+dxm-dys;
							smokev.seraphimVertices[svi].f[1] = sy+dym+dxs;
							smokev.seraphimVertices[svi].f[2] = sx+dxm+dys;
							smokev.seraphimVertices[svi].f[3] = sy+dym-dxs;
							svi++;
							
							smokev.seraphimVertices[svi].f[0] = oldscreenx-dxm+dyos;
							smokev.seraphimVertices[svi].f[1] = oldscreeny-dym-dxos;
							smokev.seraphimVertices[svi].f[2] = oldscreenx-dxm-dyos;
							smokev.seraphimVertices[svi].f[3] = oldscreeny-dym+dxos;
							svi++;
						}
					}
				}
			}

            //TODO Fix this nonsense
            float[,] fa = new float[smokev.seraphimColors.Length,4];//HACK non-blittable floatToVectors >:/
            for (i = 0; i < fa.Length/4; i++)
            {
                fa[i,0] = smokev.seraphimColors[i].f[0];
                fa[i, 1] = smokev.seraphimColors[i].f[1];
                fa[i, 2] = smokev.seraphimColors[i].f[2];
                fa[i, 3] = smokev.seraphimColors[i].f[3];
            }

            
            
            Gl.glColorPointer(4, Tao.OpenGl.Gl.GL_FLOAT, 0,fa/*smokev.seraphimColors*/);

            fa = new float[smokev.seraphimVertices.Length, 4];
            for (i = 0; i < fa.Length / 4; i++)
            {
                fa[i, 0] = smokev.seraphimVertices[i].f[0];
                fa[i, 1] = smokev.seraphimVertices[i].f[1];
                fa[i, 2] = smokev.seraphimVertices[i].f[2];
                fa[i, 3] = smokev.seraphimVertices[i].f[3];
            }

			Gl.glVertexPointer(2,Tao.OpenGl.Gl.GL_FLOAT,0,fa/*smokev.seraphimVertices*/);
       
			Gl.glTexCoordPointer(2,Tao.OpenGl.Gl.GL_FLOAT,0,smokev.seraphimTextures);
			Gl.glDrawArrays(Tao.OpenGl.Gl.GL_QUADS,0,si*4);

		}

		
		
	}
}
