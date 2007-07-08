/*
 *
 * Using USEFADEHACK define, the Gl_saver.GLSetupRC() hack, to get the fading startup to work.
 * Something is messed up with double-buffering in here.
 * 
 * Define USESDL to compile to the SDL test window
 * 
 * 
 * NOTE: This is totally pre-alpha code, so bugs and feature loss is normal ;)
 */


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Tao.Platform.Windows;
using Tao.OpenGl;

namespace FlurrySharp
{
	public partial class MainForm : Form
	{
		FlurrySettings settings;
		FlurryGroup flgroup;
		IntPtr hglrc, hdc;
		bool preview = false;
		int preset = 0;
		int mouseThreshold = 0;
		bool config = false;

		public MainForm(string[] args)
		{
			InitializeComponent();
			settings = new FlurrySettings();
			preset = Properties.Settings.Default.SelectedFlurry;
			
			IntPtr topmost = new IntPtr(-1);

			if (args.Length > 0)
			{
				if (args[0].ToLower().Equals("/p"))
				{
					SetParent((IntPtr)int.Parse(args[1]));
					preview = true;
				}
				if (args[0].ToLower().StartsWith("/c") )
				{
					config = true;
					
					SettingsForm sf = new SettingsForm();
					sf.SpecItems=this.settings.specs.ToArray();
					sf.SelectedItem = preset;
					if (sf.ShowDialog() == DialogResult.OK)
					{
						preset = sf.SelectedItem;
						Properties.Settings.Default.SelectedFlurry = preset;
						Properties.Settings.Default.Save();
					}
					Close();
				}
			}
			
			if(!preview && !config)
			{
				int width = Win32.User.GetSystemMetrics(Win32.User.SM_CXVIRTUALSCREEN);
				int height = Win32.User.GetSystemMetrics(Win32.User.SM_CYVIRTUALSCREEN);
				Win32.User.SetWindowPos(this.Handle, topmost, 0, 0, width, height, 0);
				Win32.User.ShowCursor(0);
			}


			if (!config)
			{
				if (preset > settings.specs.Count)
					preset = 0;
				timer1.Interval = (int)(1000.0 / Types.iMaxFrameProgressInMs);
				timer1.Tick += new EventHandler(timer1_Tick);
				Init();

				timer1.Start();
			}
		}

		void SetParent(IntPtr hWnd)
		{
			Win32.RECT rc = new Win32.RECT();
			Win32.User.GetWindowRect(hWnd, ref rc);
			this.Width = rc.Right-rc.Left;
			this.Height = rc.Bottom-rc.Top;
			//int style=Win32.User.GetWindowLong(this.Handle, Win32.User.GWL_STYLE);
			Win32.User.SetParent(this.Handle, hWnd);
			Win32.User.SetWindowLong(this.Handle, Win32.User.GWL_STYLE, Win32.User.WS_VISIBLE | Win32.User.WS_CHILDWINDOW);
			Win32.User.SetWindowPos(this.Handle, IntPtr.Zero, 0, 0, this.Width, this.Height, 0/*Win32.User.SWP_NOMOVE | Win32.User.SWP_NOSIZE*/);
		}

		void timer1_Tick(object sender, EventArgs e)
		{
			this.Invalidate();
			this.Update();
			
			
		}


		#if !USESDL
		public static void Main(string[] args)
		{
			//for x monitors, spawn new window
			//if(args.Length>0 && args[0].Equals("/c"))
			//Application.Run(new SettingsForm());
			//else
			MainForm m = new MainForm(args);
			if(!m.config)//total hack, find nicer way :P
				Application.Run(m);
		}
		#endif

		protected override void OnPaint(PaintEventArgs e)
		{
			if (DesignMode)
				base.OnPaint(e);
			else
			{
				if (flgroup != null)
				{

					if (Wgl.wglMakeCurrent(hdc, hglrc))
					{
						flgroup.AnimateOneFrame();
						CopyFrontBufferToBack();
						if (Types.iSettingBufferMode == Types.BUFFER.BUFFER_MODE_SINGLE)
						{
							Gdi.SwapBuffers(hdc);
						}

					}
				}
			}
		}


		protected override void OnPaintBackground(PaintEventArgs e)
		{
			if (DesignMode)
				base.OnPaintBackground(e);
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			DetachGLFromWindow();
			base.OnClosing(e);
		}

		void nextPreset()
		{
			preset++;
			if (preset >= settings.specs.Count)
				preset = 0;
			Init();
		}

		void Init()
		{
			flgroup = new FlurryGroup(settings.specs[preset]);
			//flgroup.SetSize(this.Width, this.Height);
			AttachGLToWindow();
			flgroup.PrepareToAnimate();

		}

		bool bFirstTime = true;
		void CopyFrontBufferToBack(/*IntPtr hWnd*/)
		{


			// copy front buffer to back buffer, to compensate for Windows'
			// possibly weird implementation of SwapBuffers().  As documented, it
			// reserves the right to leave the back buffer completely undefined
			// after each swap, but on both my ATI Radeon 8500 and NVidia GF4Ti4200
			// it works almost fine to just copy front to back once like this.

			if ((Types.iSettingBufferMode == Types.BUFFER.BUFFER_MODE_SAFE_DOUBLE) ||
			    (Types.iSettingBufferMode == Types.BUFFER.BUFFER_MODE_FAST_DOUBLE && bFirstTime))
			{
				//Win32.RECT rc = new Win32.RECT();
				//Win32.User.GetClientRect(hWnd, ref rc);

				Gl.glDisable(Gl.GL_ALPHA_TEST);
				if (!Types.iBugWhiteout)
				{
					// Found this by accident; Adam likes it.  Freakshow option #1.
					Gl.glDisable(Gl.GL_BLEND);
				}
				Gl.glReadBuffer(Gl.GL_FRONT);
				Gl.glDrawBuffer(Gl.GL_BACK);
				Gl.glRasterPos2i(0, 0);
				Gl.glCopyPixels(0, 0, this.Width, this.Height, Gl.GL_COLOR);
				if (!Types.iBugWhiteout)
				{
					Gl.glEnable(Gl.GL_BLEND);
				}
				Gl.glEnable(Gl.GL_ALPHA_TEST);

				bFirstTime = false;
			}
		}


		void AttachGLToWindow(/*FlurryAnimateChildInfo *child*/)
		{

			Gdi.PIXELFORMATDESCRIPTOR pfd = new Gdi.PIXELFORMATDESCRIPTOR();//HACK
			pfd.nSize = (short)System.Runtime.InteropServices.Marshal.SizeOf(pfd);
			pfd.nVersion = 1;
			pfd.dwFlags = Gdi.PFD_DRAW_TO_WINDOW | Gdi.PFD_SUPPORT_OPENGL;
			pfd.iPixelType = Gdi.PFD_TYPE_RGBA;
			pfd.iLayerType = Gdi.PFD_MAIN_PLANE;
			pfd.cDepthBits = 16;
			pfd.cColorBits = 24;
			#if !USEFADEHACK
			if (Types.iSettingBufferMode != Types.BUFFER.BUFFER_MODE_SINGLE)
				pfd.dwFlags |= Gdi.PFD_DOUBLEBUFFER;
			#endif
			hdc = User.GetDC(this.Handle);
			//Win32.GDI.SetBkColor(hdc, 0);
			int iPixelFormat = Gdi.ChoosePixelFormat(hdc, ref pfd);

			Gdi.SetPixelFormat(hdc, iPixelFormat, ref pfd);

			// then use this to create a rendering context
			hglrc = Wgl.wglCreateContext(hdc);
			Wgl.wglMakeCurrent(hdc, hglrc);

			// tell Flurry to use the whole window as viewport
			//GetClientRect(child->hWnd, &rc);
			flgroup.SetSize(this.Width, this.Height);

			// some nice debug output
			//_RPT4(_CRT_WARN, "  child 0x%08x: hWnd 0x%08x, hdc 0x%08x, hglrc 0x%08x\n",
			//    child, child->hWnd, child->hdc, child->hglrc);
			//_RPT1(_CRT_WARN, "  GL vendor:     %s\n", glGetString(GL_VENDOR));
			//_RPT1(_CRT_WARN, "  GL renderer:   %s\n", glGetString(GL_RENDERER));
			//_RPT1(_CRT_WARN, "  GL version:    %s\n", glGetString(GL_VERSION));
			//_RPT1(_CRT_WARN, "  GL extensions: %s\n", glGetString(GL_EXTENSIONS));
			//_RPT0(_CRT_WARN, "\n");
		}


		void DetachGLFromWindow(/*FlurryAnimateChildInfo *child*/)
		{
			if (hglrc == Wgl.wglGetCurrentContext())
			{
				//_RPT1(_CRT_WARN, "Evicting context %d\n", child->id);
				Wgl.wglMakeCurrent(IntPtr.Zero, IntPtr.Zero);
			}
			if (!Wgl.wglDeleteContext(hglrc))
			{
				//_RPT2(_CRT_WARN, "Failed to delete context for %d: %d\n",
				//    child->id, GetLastError());
			}
			User.ReleaseDC(this.Handle, hdc);
		}

		int x = 0, y=0;
		private void mouseMove(object sender, MouseEventArgs e)
		{
			if (!preview && (x!=e.X || y!=e.Y ))
				mouseThreshold++;

			x=e.X;y = e.Y;

			if (!preview && mouseThreshold >= 10)
				Close();
			
		}

		private void keyDown(object sender, KeyEventArgs e)
		{
			if (!preview && e.KeyData != Keys.Space)
				Close();

			if (e.KeyData == Keys.Space)//or just else :P
				nextPreset();
		}
	}
}
