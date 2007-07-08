using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using SdlDotNet.Graphics;
using SdlDotNet.Core;
using SdlDotNet.Graphics.Sprites;
using Tao.OpenGl;

/*
 * 
 * Uncomment the GLSetupRC() hack if this thing acts up.
 * 
 */

namespace FlurrySharp
{
#if USESDL
    public class MainClass
    {
        
        [STAThread]
        public static void Main(string[] args)
        {
            MainClass m = new MainClass();
            m.Run();
        }
        TextSprite txtSprite;
        Surface screen;
        int width, height;
        int preset = 0;
        FlurrySettings settings;
        FlurryGroup fgroup;
        int g_nMonitors;
        int framecount = 0;
        int showForFrameCount = Types.iMaxFrameProgressInMs * 5;//About 5 secs

        void Run()
        {
            SdlDotNet.Input.Mouse.ShowCursor = false;
            Video.WindowIcon(System.Reflection.Assembly.GetExecutingAssembly());
            Video.WindowCaption = "FlurrySharp Test Window";
            g_nMonitors = Win32.User.GetSystemMetrics(Win32.User.SM_CMONITORS);

            screen = Video.SetVideoMode(Win32.User.GetSystemMetrics(Win32.User.SM_CXVIRTUALSCREEN), Win32.User.GetSystemMetrics(Win32.User.SM_CYVIRTUALSCREEN), true, true, false, true, false);
            Win32.User.SetWindowPos(Video.WindowHandle, new IntPtr(0), 0, 0, screen.Width, screen.Height, 0);

            txtSprite = new TextSprite(new SdlDotNet.Graphics.Font("vera.ttf", 12));
            settings = new FlurrySettings();

            Events.Tick += new EventHandler<TickEventArgs>(Events_Tick);
            Events.VideoResize += new EventHandler<VideoResizeEventArgs>(Events_VideoResize);
            Events.Quit += new EventHandler<QuitEventArgs>(Events_Quit);
            Events.Fps = Types.iMaxFrameProgressInMs;
            Events.MouseMotion += new EventHandler<SdlDotNet.Input.MouseMotionEventArgs>(Events_MouseMotion);
            Events.KeyboardDown += new EventHandler<SdlDotNet.Input.KeyboardEventArgs>(Events_KeyboardDown);
            Init();
            Reshape();
            
            Events.Run();
        }



        #region Events
        void Events_KeyboardDown(object sender, SdlDotNet.Input.KeyboardEventArgs e)
        {
            if (e.Key == SdlDotNet.Input.Key.Q || e.Key == SdlDotNet.Input.Key.Escape)
            {
                Events.QuitApplication();
                SdlDotNet.Input.Mouse.ShowCursor = true;
            }

            if (e.Key == SdlDotNet.Input.Key.Space)
                NextPreset();
        }

        void Events_MouseMotion(object sender, SdlDotNet.Input.MouseMotionEventArgs e)
        {
            
        }

        void Events_Quit(object sender, QuitEventArgs e)
        {
            Events.QuitApplication();
        }

        void Events_VideoResize(object sender, VideoResizeEventArgs e)
        {
            screen = Video.SetVideoMode(e.Width, e.Height, true, true);
            Init();
            Reshape();
        }

        void Events_Tick(object sender, TickEventArgs e)
        {
            fgroup.AnimateOneFrame();
            txtSprite.X = 100;
            txtSprite.Y = 100;
            txtSprite.Color = System.Drawing.Color.Wheat;

            

            //if (framecount < showForFrameCount)
            {
                
                Tao.FreeGlut.Glut.glutBitmapString(Tao.FreeGlut.Glut.GLUT_BITMAP_TIMES_ROMAN_10, txtSprite.Text);
                framecount++;
            }
            Video.GLSwapBuffers();
            //CopyFrontBufferToBack();
        }
        #endregion


    

        void NextPreset()
        {
            preset++;
            if (preset >= settings.specs.Count)
                preset = 0;

            Init();
        }

    
        
        void CopyFrontBufferToBack( /*IntPtr hWnd*/)
        {
                Gl.glDisable(Gl.GL_ALPHA_TEST);
                if (!Types.iBugWhiteout)
                {
                    // Found this by accident; Adam likes it.  Freakshow option #1.
                    Gl.glDisable(Gl.GL_BLEND);
                }
                Win32.RECT rct=new Win32.RECT();
                Win32.User.GetWindowRect(Video.WindowHandle, ref rct);

                Gl.glReadBuffer(Gl.GL_FRONT);
                Gl.glDrawBuffer(Gl.GL_BACK);
                Gl.glRasterPos2i(0, 0);
                Gl.glCopyPixels(0, 0, rct.Right, rct.Bottom, Gl.GL_COLOR);
                if (!Types.iBugWhiteout)
                {
                    Gl.glEnable(Gl.GL_BLEND);
                }
                Gl.glEnable(Gl.GL_ALPHA_TEST);
        }



        void Reshape()
        {

            width = screen.Width;
            height = screen.Height;
            fgroup.SetSize(screen.Width, screen.Height);
            double h = (double)height / (double)width;
        }

        void Init()
        {
            //Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);
            fgroup = new FlurryGroup(settings.specs[preset]);
            fgroup.SetSize(screen.Width, screen.Height);
            fgroup.PrepareToAnimate();
            framecount = 0;
            txtSprite.Text = settings.specs[preset].name;
        }
    }
#endif
}
