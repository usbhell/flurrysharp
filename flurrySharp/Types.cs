/*
 * Created by SharpDevelop.
 * User: FurYy
 * Date: 7.07.2007
 * Time: 15:47
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;

namespace FlurrySharp
{
	/// <summary>
	/// Description of Types.
	/// </summary>
	public class Types
	{
		
//			DECLARE_SETTINGS_VAR(DWORD, iSettingBufferMode, "Buffering mode", BUFFER_MODE_SINGLE)
//			DECLARE_SETTINGS_VAR(DWORD, iMultiMonPosition, "Multimon behavior", MULTIMON_ALLMONITORS)
//			DECLARE_SETTINGS_VAR(DWORD, iShowFPSIndicator, "FPS display", 0)
//			DECLARE_SETTINGS_VAR(DWORD, iBugBlockMode, "Freakshow: Block mode", 0)
//			DECLARE_SETTINGS_VAR(DWORD, iBugWhiteout, "Freakshow: Whiteout doublebuffer", 0)
//			DECLARE_SETTINGS_VAR(DWORD, iFlurryPreset, "Preset", 0)
//			DECLARE_SETTINGS_VAR(DWORD, iFlurryShrinkPercentage, "Shrink by %", 0)
//			DECLARE_SETTINGS_VAR(DWORD, iMaxFrameProgressInMs, "Max frame progress", 30)
		
		public enum BUFFER{ BUFFER_MODE_SINGLE, BUFFER_MODE_FAST_DOUBLE, BUFFER_MODE_SAFE_DOUBLE };
		public enum MULTIMON{ MULTIMON_ALLMONITORS, MULTIMON_PRIMARY, MULTIMON_PERMONITOR };

        public static int iMaxFrameProgressInMs = 30; //Side effect of the difference of this number and actual FPS determine how much blending there is : this<actual FPS -> more blending
        public static int iFlurryPreset = 0;
		public static BUFFER iSettingBufferMode=BUFFER.BUFFER_MODE_FAST_DOUBLE;
        public static MULTIMON iMultiMonPosition = MULTIMON.MULTIMON_ALLMONITORS;
        public static int iFlurryShrinkPercentage = 0;
        public static bool iBugBlockMode = false;
        public static bool iBugWhiteout = false;
		
		public const int OPT_MODE_SCALAR_BASE	=	0x0;
		public const int OPT_MODE_SCALAR_FRSQRTE=		0x1;
		public const int OPT_MODE_VECTOR_SIMPLE	=	0x2;
		public const int OPT_MODE_VECTOR_UNROLLED=	0x3;

		public enum ColorModes
		{
			redColorMode = 0,
			magentaColorMode,
			blueColorMode,
			cyanColorMode,
			greenColorMode,
			yellowColorMode,
			slowCyclicColorMode,
			cyclicColorMode,
			tiedyeColorMode,
			rainbowColorMode,
			whiteColorMode,
			multiColorMode,
			darkColorMode
		} ;

		public const int MAXNUMPARTICLES = 2500;

		/// <summary>
		/// Original C code has this static and does mambo-jambo to support multi-monitor.
		/// In C# code, that means, it makes a new "instance" of this global info, so leave only some main params const/static here
		/// and other shite as instance, so we can have different effects for different monitors/desktops.
		/// (WTF am i blabbering about...geez :D)
		/// </summary>
		public class GlobalInfo {
			public float flurryRandomSeed;
			public float fTime;
			public float fOldTime;
			public float fDeltaTime;
			public const float gravity = 1500000.0f;
			public int sys_glWidth;
			public int sys_glHeight;
			public float drag;
			public int MouseX = 0;
			public int MouseY = 0;
			public int MouseDown = 0;
			
			public ColorModes currentColorMode;
			public float streamExpansion;
			public int numStreams=0;
			
			public const float incohesion =0.07f;
			public const float colorIncoherence= 0.15f;
			public const float streamSpeed =450.0f;
			public const float fieldCoherence= 0;
			public const float fieldSpeed= 12.0f;
			public const int numParticles= 250;
			public const float starSpeed =50;
			public const float seraphDistance= 2000.0f;
			public const float streamSize =25000.0f;
			public const float fieldRange =1000.0f;
			public const float streamBias =7.0f;
			
			public int dframe;
			public float[] starfieldColor=new float[MAXNUMPARTICLES * 4 * 4];
			public float[] starfieldVertices=new float[MAXNUMPARTICLES * 2 * 4];
			public float[] starfieldTextures=new float[MAXNUMPARTICLES * 2 * 4];
			public int starfieldColorIndex;
			public int starfieldVerticesIndex;
			public int starfieldTexturesIndex;
			public Particle[] particles=new Particle[MAXNUMPARTICLES];
			public Smoke smoke;
			public Star star;
			public Spark[] spark;//=new Spark[64];

			public int optMode;
		} ;
		
	}
	
	
}
