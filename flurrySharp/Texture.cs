/*
 * Created by SharpDevelop.
 * User: FurYy
 * Date: 7.07.2007
 * Time: 18:44
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using Tao.OpenGl;

namespace FlurrySharp
{
	/// <summary>
	/// Description of Texture.
	/// </summary>
	public class Texture
	{
		
		//byte [][] smallTextureArray=new byte[][]{new byte[32],new byte[32]};
		static byte [,] smallTextureArray=new byte[32,32];
		static byte [,,] bigTextureArray=new byte[256,256,2];
		
		//IntPtr theTexture ;
		static int[] theTexture=new int[1];
		
		public Texture()
		{
			
		}
		
		
		public static  void SmoothTexture()
		{
			byte[,] filter=new byte[32,32];
			int i,j;
			float t;
			for (i = 1; i < 31; i++)
			{
				for (j = 1; j < 31; j++)
				{
					t = (float) smallTextureArray[i,j]*4;
					t += (float) smallTextureArray[i-1,j];
					t += (float) smallTextureArray[i+1,j];
					t += (float) smallTextureArray[i,j-1];
					t += (float) smallTextureArray[i,j+1];
					t /= 8.0f;
					filter[i,j] = (byte) t;
				}
			}
			for (i = 1; i < 31; i++)
			{
				for (j = 1; j < 31; j++)
				{
					smallTextureArray[i,j] = filter[i,j];
				}
			}
		}


		// add some randomness to texture data
		public static   void
			SpeckleTexture()
		{
			int i,j;
			int speck;
			float t;
			for (i = 2; i < 30; i++)
			{
				for (j = 2; j < 30; j++)
				{
					speck = 1;
					while ((speck <= 32) && (Tools.Rand() % 2)!=0)
					{
						t = (float) Math.Min(255,smallTextureArray[i,j]+speck);
						smallTextureArray[i,j] = (byte) t;
						speck+=speck;
					}
					speck = 1;
					while ((speck <= 32) && (Tools.Rand() % 2)!=0)
					{
						t = (float) Math.Max(0,smallTextureArray[i,j]-speck);
						smallTextureArray[i,j] = (byte) t;
						speck+=speck;
					}
				}
			}
		}

		static bool firstTime = true;
		public static  void MakeSmallTexture()
		{
			//bool firstTime = true;//HACK wtf? static
			int i, j;
			float r, t;
			if (firstTime)
			{
				firstTime = false;
				for (i = 0; i < 32; i++)
				{
					for (j = 0; j < 32; j++)
					{
						r = (float) Math.Sqrt((i-15.5)*(i-15.5)+(j-15.5)*(j-15.5));
						if (r > 15.0f)
						{
							smallTextureArray[i,j] = 0;
						}
						else
						{
							t = 255.0f * (float) Math.Cos(r*Math.PI/31.0);
							smallTextureArray[i,j] = (byte) t;
						}
					}
				}
			}
			else
			{
				for (i = 0; i < 32; i++)
				{
					for (j = 0; j < 32; j++)
					{
						r = (float) Math.Sqrt((i-15.5)*(i-15.5)+(j-15.5)*(j-15.5));
						if (r > 15.0f)
						{
							t = 0.0f;
						}
						else
						{
							t = 255.0f * (float) Math.Cos(r*Math.PI/31.0);
						}
						smallTextureArray[i,j] = (byte) Math.Min(255,(t+smallTextureArray[i,j]+smallTextureArray[i,j])/3);
					}
				}
			}
			SpeckleTexture();
			SmoothTexture();
			SmoothTexture();
		}


		public static  void CopySmallTextureToBigTexture(int k, int l)
		{
			int i, j;
			for (i = 0; i < 32; i++)
			{
				for (j = 0; j < 32; j++)
				{
					bigTextureArray[i+k,j+l,0] = smallTextureArray[i,j];
					bigTextureArray[i+k,j+l,1] = smallTextureArray[i,j];
				}
			}
		}


		public static  void AverageLastAndFirstTextures()
		{
			int i, j;
			int t;
			for (i = 0; i < 32; i++)
			{
				for (j = 0; j < 32; j++)
				{
					t = (smallTextureArray[i,j] + bigTextureArray[i,j,0]) / 2;
					smallTextureArray[i,j] = (byte) Math.Min(255,t);
				}
			}
		}


	
		static bool texturesBuilt = false;
		
		public  static  void MakeTexture()
		{
			int i, j;
			

			if (!texturesBuilt) {
				texturesBuilt = true;

				for (i = 0; i < 8; i++)
				{
					for (j = 0; j < 8; j++)
					{
						if (i==7 && j==7)
						{
							AverageLastAndFirstTextures();
						}
						else
						{
							MakeSmallTexture();
						}

						CopySmallTextureToBigTexture(i * 32,j * 32);
					}
				}
			}

			//example http://nemerle.org/svn/nemerle/trunk/snippets/opengl/sdlopengl4.n
			
			Gl.glPixelStorei(Gl.GL_UNPACK_ALIGNMENT,1);

			Gl.glGenTextures(1, theTexture);//TODO Check if it's alright
			Gl.glBindTexture(Gl.GL_TEXTURE_2D, theTexture[0]);

			// Set the tiling mode (this is generally always GL_REPEAT).
			Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_S, Gl.GL_REPEAT);
			Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_T, Gl.GL_REPEAT);

			// Set the filtering.
			Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_LINEAR);
			Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_LINEAR_MIPMAP_NEAREST);

			Glu.gluBuild2DMipmaps(Gl.GL_TEXTURE_2D, 2, 256, 256, Gl.GL_LUMINANCE_ALPHA, Gl.GL_UNSIGNED_BYTE, bigTextureArray);
			Gl.glTexEnvf(Gl.GL_TEXTURE_ENV, Gl.GL_TEXTURE_ENV_MODE, Gl.GL_MODULATE);
		}


	}
}
