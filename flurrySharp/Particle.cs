/*
 * Created by SharpDevelop
 * User: FurYy
 * Date: 7.07.2007
 * Time: 15:56
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;

namespace FlurrySharp
{
	/// <summary>
	/// Description of Particles.
	/// </summary>
	public class Particle
	{
		//public struct Particle
		//{
		public float charge;
		public float x;
		public float y;
		public float z;
		public float oldx;
		public float oldy;
		public float oldz;
		public float deltax;
		public float deltay;
		public float deltaz;
		public float r;
		public float g;
		public float b;
		public int animFrame;
		//} ;
		
		Types.GlobalInfo info;
		
		public Particle(Types.GlobalInfo a_info)
		{
			info=a_info;
			InitParticle();
		}
		
		
		public void DrawParticle()//(Particle p) // the math was easier in 2D - so 2D it is
		{
			int i;
			
			float screenx = (x * info.sys_glWidth / z) + info.sys_glWidth * 0.5f;
			float screeny = (y * info.sys_glWidth / z) + info.sys_glHeight * 0.5f;
			float oldscreenx = (oldx * info.sys_glWidth / oldz) + info.sys_glWidth * 0.5f;
			float oldscreeny = (oldy * info.sys_glWidth / oldz) + info.sys_glHeight * 0.5f;

			if (z < 100.0f ||
			    screenx > info.sys_glWidth + 100.0f || screenx < -100.0f ||
			    screeny > info.sys_glHeight + 100.0f || screeny < -100.0f) {
				// clipping tests; if clipped, reset to sane state
				InitParticle();//(p);
			}

			for (i = 0; i < 4; i++) {
				info.starfieldColor[info.starfieldColorIndex++] = r;
				info.starfieldColor[info.starfieldColorIndex++] = g;
				info.starfieldColor[info.starfieldColorIndex++] = b;
				info.starfieldColor[info.starfieldColorIndex++] = 1.0f;
			}
			
			if (++animFrame == 64) {
				animFrame = 0;
			}
			
			
			{
				float dx = (screenx-oldscreenx);
				float dy = (screeny-oldscreeny);
				float m = Tools.FastDistance2D(dx, dy);
				float u0 = (animFrame&7) * 0.125f;
				float v0 = (animFrame>>3) * 0.125f;
				float u1 = u0 + 0.125f;
				float v1 = v0 + 0.125f;
				float size = (3500.0f*(info.sys_glWidth/1024.0f));
				float w = Math.Max(1.5f,size/z);
				float ow = Math.Max(1.5f,size/oldz);
				float d = Tools.FastDistance2D(dx, dy);
				float s, os, dxs, dys, dxos, dyos, dxm, dym;
				
				if(d!=0f) {
					s = w/d;
				} else {
					s = 0.0f;
				}
				
				if(d!=0f) {
					os = ow/d;
				} else {
					os = 0.0f;
				}
				
				m = 2.0f + s;
				
				dxs = dx*s;
				dys = dy*s;
				dxos = dx*os;
				dyos = dy*os;
				dxm = dx*m;
				dym = dy*m;
				
				// ok info is a ref to global info so tests so that global instance of info
				// should be updated with these new values.
				info.starfieldTextures[info.starfieldTexturesIndex++] = u0;
				info.starfieldTextures[info.starfieldTexturesIndex++] = v0;
				info.starfieldVertices[info.starfieldVerticesIndex++] = screenx+dxm-dys;
				info.starfieldVertices[info.starfieldVerticesIndex++] = screeny+dym+dxs;
				info.starfieldTextures[info.starfieldTexturesIndex++] = u0;
				info.starfieldTextures[info.starfieldTexturesIndex++] = v1;
				info.starfieldVertices[info.starfieldVerticesIndex++] = screenx+dxm+dys;
				info.starfieldVertices[info.starfieldVerticesIndex++] = screeny+dym-dxs;
				info.starfieldTextures[info.starfieldTexturesIndex++] = u1;
				info.starfieldTextures[info.starfieldTexturesIndex++] = v1;
				info.starfieldVertices[info.starfieldVerticesIndex++] = oldscreenx-dxm+dyos;
				info.starfieldVertices[info.starfieldVerticesIndex++] = oldscreeny-dym-dxos;
				info.starfieldTextures[info.starfieldTexturesIndex++] = u1;
				info.starfieldTextures[info.starfieldTexturesIndex++] = v0;
				info.starfieldVertices[info.starfieldVerticesIndex++] = oldscreenx-dxm-dyos;
				info.starfieldVertices[info.starfieldVerticesIndex++] = oldscreeny-dym+dxos;
			}
		}


		public void UpdateParticle()//(Particle *p)
		{
			this.oldx = this.x;
			this.oldy = this.y;
			this.oldz = this.z;

			this.x += this.deltax * info.fDeltaTime;
			this.y += this.deltay * info.fDeltaTime;
			this.z += this.deltaz * info.fDeltaTime;
		}


		public void InitParticle()//(Particle *p)
		{
            if (info.sys_glWidth == 0 || info.sys_glHeight == 0)
                return;

			int r1, r2;
			Particle p = this;
			oldz = Tools.RandFlt(2500.0f,22500.0f);
			r1 = Tools.Rand();
			r2 = Tools.Rand();
			oldx = ((float) (r1 % (int) info.sys_glWidth) - info.sys_glWidth * 0.5f) /
				(info.sys_glWidth / oldz);
			oldy = (info.sys_glHeight * 0.5f - (float) (r2 % (int) info.sys_glHeight)) /
				(info.sys_glWidth / oldz);
			deltax = 0.0f;
			deltay = 0.0f;
			deltaz = -Types.GlobalInfo.starSpeed;
			x = oldx + deltax;
			y = oldy + deltay;
			z = oldz + deltaz;
			r = Tools.RandFlt(0.125f, 1.0f);
			g = Tools.RandFlt(0.125f, 1.0f);
			b = Tools.RandFlt(0.125f, 1.0f);
			animFrame = 0;
		}

	}
}
