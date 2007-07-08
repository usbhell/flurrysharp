/*
 * Created by SharpDevelop.
 * User: FurYy
 * Date: 7.07.2007
 * Time: 22:21
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using Tao.OpenGl;

namespace FlurrySharp
{
	
	
	/// <summary>
	/// Description of ScreenSaver.
	/// </summary>
	public class ScreenSaver
	{
		public ScreenSaver()
		{
		}
		
		/*
		 * Flurry for Windows.
		 *
		 * Screen saver host code, specific to Windows platform.
		 *
		 * Created 2/23/2003 by Matt Ginzton, magi@cs.stanford.edu
		 */


		/*
		 * Still to do:
		 * 2) visual configuration / customization
		 * 3) separate support for multiple monitors
		 *    XXX: double-buffered flip() without blocking the other monitor
		 */


		///////////////////////////////////////////////////////////////////////////
//
		// shared globals
//

		//CComModule _Module;


		///////////////////////////////////////////////////////////////////////////
//
		// module globals
//

		int g_nMonitors = 0;
		int g_iMonitor = 0;
		bool g_bPreviewMode = false;
		bool g_bThumbnailMode = false;


		///////////////////////////////////////////////////////////////////////////
//
		// data types
//

		/*
		 * frames-per-second calculation
		 */
		const int FPS_SAMPLES = 20;
		public class FPS{
			public int startTime;			// in ticks
			public int [] samples=new int[FPS_SAMPLES];	// in ticks
			public int nextSample;				// we use samples array as ring buffer
			public int nSamples;				// to tell if early/late, and running average
		} ;
		FPS fps=new FPS();

		/*
		 * Per-monitor flurry info.  In multimon mode, we have one of these for
		 * each monitor; in single-mon mode, we just have one (which might represent
		 * the entire desktop, and might represent just the primary monitor).
		 */
		public struct FlurryAnimateChildInfoStruct{
			public int id;				// ordinal just for debugging convenience
			public string device;		// name of display device, or NULL in single-mon mode
			public int updateInterval;	// time between refreshes
			public IntPtr hWnd;			// handle to child window for this monitor
			public Win32.RECT rc;			// child window rectangle in screen coordinates
			public IntPtr hglrc;		// handle to OpenGL rendering context for this window
			public IntPtr hdc;			// handle to DC used by hglrc
			public FPS fps;			// frames per second info
			public FlurryGroup flurry;// the data structure with info on the flurry clusters
		} ;

		FlurryAnimateChildInfoStruct FlurryAnimateChildInfo=new FlurryAnimateChildInfoStruct();


		///////////////////////////////////////////////////////////////////////////
		// public functions


		/*
		 * ScreensaverCommonInit
		 *
		 * Init code used by both personalities (configuration and screensaver)
		 * when invoked with /s.
		 */

		void ScreensaverCommonInit()
		{
			// count monitors
			g_nMonitors = Win32.User.GetSystemMetrics(Win32.User.SM_CMONITORS);
		}


		/*
		 * ScreensaverRuntimeInit
		 *
		 * Init code used only when running as screensaver, not during configuration
		 */

		/*void ScreensaverRuntimeInit(IntPtr hWnd)
		{
			// we're going to use rand(), so...
			//srand(GetTickCount());

			// register child window class
			WNDCLASS wc = { 0 };
			wc.lpszClassName = "FlurryAnimateChild";
			wc.lpfnWndProc = FlurryAnimateChildWindowProc;
			RegisterClass(&wc);

			// detect if we're in the desktop properties preview pane...
			// if so, use the window we're given (disable per-monitor behavior)
			RECT rc;
			Win32.User.GetWindowRect(hWnd, ref rc);
			//_RPT4(_CRT_WARN, "Init in window 0x%08x, parent 0x%08x: %d, %d\n",
			//      hWnd, GetParent(hWnd), rc.left, rc.top);
			if (Win32.User.GetParent(hWnd)!=0) {
				iMultiMonPosition = MULTIMON_ALLMONITORS;
				g_bThumbnailMode = true;
			}

			// initialize timer module
			TimeSupport_Init();

			#if _DEBUG
			// debugging behind toplevel windows is a pain :P
			SetWindowPos(hWnd, HWND_NOTOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
			#endif
		}*/


		/*
		 * ScreenSaverProc
		 *
		 * Scrnsave.lib entry point: called as WndProc for screen saver window
		 * when invoked with /s.
		 *
		 * We implement one big background window, spanning the whole desktop,
		 * which has 1 or more children (typically one for each monitor), each
		 * of which renders a FlurryGroup.
		 */

		
		int ScreenSaverProc(IntPtr hWnd, uint message, IntPtr wParam, IntPtr lParam)
		{
			switch (message) {
				case Win32.User.WM_CREATE:
					if (!g_bPreviewMode) {
						// initialization, since this is the first chunk of our code to run
						ScreensaverCommonInit();
						Settings_Read();
					} else {
						// we've already done the common init via the configuration dialog,
						// but we need to set a timer to disable the ignore-input code.
						SetTimer(hWnd, 0, 500, NULL);
					}
					ScreensaverRuntimeInit(hWnd);

					// prepare child windows for rendering
					ScreenSaverCreateChildren(hWnd);
					return 0;

				case Win32.User.WM_KEYDOWN:
					if (wParam == 'F') {
						iShowFPSIndicator = !iShowFPSIndicator;
					}
					break;

				case Win32.User.WM_TIMER:
					// cancel ignoring of input
					g_bPreviewMode = false;
					break;

				case Win32.User.WM_SETCURSOR:
					if (!g_bThumbnailMode) {
						SetCursor(NULL);
					}
					break;
			}

			// avoid bailing on input when right control key is down,
			// or in first 1/2 second of preview mode
			bool bIgnoreInput = (GetKeyState(VK_RCONTROL) >> 31) || g_bPreviewMode;

			return (bIgnoreInput ?
			        DefWindowProc :
			        DefScreenSaverProc)(hWnd, message, wParam, lParam);
		}

/*
		static void
			ScreenSaverCreateChildren(IntPtr hWndParent)
		{
			RECT rc;
			g_iMonitor = 0;

			switch (iMultiMonPosition) {
				case MULTIMON_PERMONITOR:
					EnumDisplayMonitors(NULL, NULL, ScreenSaverCreateChildrenCb,
					                    (LPARAM)hWndParent);
					return;

				case MULTIMON_PRIMARY:
					SetRect(&rc, 0, 0,
					        GetSystemMetrics(SM_CXSCREEN),
					        GetSystemMetrics(SM_CYSCREEN));
					break;

				case MULTIMON_ALLMONITORS:
				default:
					GetClientRect(hWndParent, &rc);
					break;
			}

			ScreenSaverCreateChild(hWndParent, &rc, 0, NULL);
		}


		int ScreenSaverCreateChildrenCb(IntPtr hMonitor, IntPtr hdcMonitor,
			                            Win32.RECT lprcMonitor, IntPtr dwData)
		{
			IntPtr hWndParent = dwData;
			Win32.RECT rcMonitorChild=new Win32.RECT();

			Win32.User.MONITORINFOEX mi;
			memset(&mi, 0, sizeof mi);
			mi.cbSize = sizeof mi;
			Win32.User.GetMonitorInfo(hMonitor, (MONITORINFO *)&mi);
			_RPT1(_CRT_WARN, "Found monitor: %s\n", mi.szDevice);

			rcMonitorChild = *lprcMonitor;
			ScreenToClient(hWndParent, ((POINT*)&rcMonitorChild) + 0);
			ScreenToClient(hWndParent, ((POINT*)&rcMonitorChild) + 1);

			ScreenSaverCreateChild(hWndParent, &rcMonitorChild, g_iMonitor++, mi.szDevice);
			return TRUE;
		}*/


		/*static void
			ScreenSaverCreateChild(HWND hWndParent, RECT *rc, int iMonitor, char *device)
		{
			char szName[20];
			FlurryAnimateChildInfo *child;

			wsprintf(szName, "flurryMon%d", iMonitor);
			_RPT2(_CRT_WARN, "Creating child %s on device %s:\n", szName, device);

			child = new FlurryAnimateChildInfo;
			memset(child, 0, sizeof *child);
			child->id = iMonitor;
			child->rc = *rc;
			child->device = device ? strdup(device) : NULL;
			child->fps.startTime = timeGetTime();

			#define RECTWIDTH(rc)  ((rc).right - (rc).left)
			#define RECTHEIGHT(rc) ((rc).bottom - (rc).top)

			// 200: n% / 100, and it counts on each size, so / 2 more
			InflateRect(&child->rc,
			            -(int)(iFlurryShrinkPercentage * RECTWIDTH(child->rc) / 200),
			            -(int)(iFlurryShrinkPercentage * RECTHEIGHT(child->rc) / 200));

			_RPT4(_CRT_WARN, "  position %d, %d, %d, %d\n",
			      child->rc.left, child->rc.top, RECTWIDTH(child->rc), RECTHEIGHT(child->rc));
			CreateWindow("FlurryAnimateChild", szName, WS_VISIBLE | WS_CHILD,
			             child->rc.left, child->rc.top, RECTWIDTH(child->rc), RECTHEIGHT(child->rc),
			             hWndParent, NULL, NULL, child);
		}*/


		/*
		 * FlurryAnimateChildWindowProc
		 *
		 * WndProc for the child windows that actually do the drawing.
		 * We handle WM_CREATE (setup), various painting and timer messages for
		 * animation, and forward mouse and keyboard messages to our parent.
		 */

		/*int FlurryAnimateChildWindowProc(HWND hWnd, UINT message,
			                             WPARAM wParam, LPARAM lParam)
		{
			FlurryAnimateChildInfo *child = (FlurryAnimateChildInfo *)
				GetWindowLong(hWnd, GWL_USERDATA);
			#ifdef _DEBUG
			static int iFrameCounter = 0;
			#endif

			switch (message) {
					case WM_CREATE: {
						CREATESTRUCT *create = (CREATESTRUCT *)lParam;
						// initialize per-child struct as window data
						child = (FlurryAnimateChildInfo *)create->lpCreateParams;
						child->hWnd = hWnd;
						SetWindowLong(hWnd, GWL_USERDATA, (LONG)child);
						// initialize flurry struct
						int preset = child->id < g_multiMonPreset.size() ?
							g_multiMonPreset[child->id] : iFlurryPreset;
						child->flurry = new FlurryGroup(preset);
						// prepare OpenGL context
						AttachGLToWindow(child);
						// prepare Flurry code --  must come after OpenGL initialization
						child->flurry->PrepareToAnimate();
						// set repaint timer
						SetTimer(hWnd, 1, child->updateInterval, NULL);
						// set up text parameters, in case we want to say anything
						SetTextColor(child->hdc, 0xFFFFFF);
						SetBkColor(child->hdc, 0x000000);
						//SetBkMode(child->hdc, TRANSPARENT);
						return 0;
					}

				case WM_ERASEBKGND:
					// Never erase, so we get that nice fade effect initially and
					// between frames.
					return 0;

				case WM_PAINT:
					_RPT1(_CRT_WARN, "Start render frame %d\n", iFrameCounter);
					if (wglMakeCurrent(child->hdc, child->hglrc)) {
						PAINTSTRUCT ps;
						BeginPaint(hWnd, &ps);
						CopyFrontBufferToBack(hWnd);	// always call; may do nothing
						child->flurry->AnimateOneFrame();
						if (iSettingBufferMode > BUFFER_MODE_SINGLE) {
							// ATI Radeon 9700s seem to get really upset if we call
							// SwapBuffers in a single-buffered context when invoked
							// with /p.  So be careful not to!
							SwapBuffers(ps.hdc);
						}
						EndPaint(hWnd, &ps);
					} else {
						_RPT1(_CRT_WARN, "OnPaint: wglMakeCurrent failed, error %d\n",
						      GetLastError());
					}
					ScreenSaverUpdateFpsIndicator(child);
					_RPT1(_CRT_WARN, "End render frame %d\n", iFrameCounter++);
					return 0;

				case WM_TIMER:
					InvalidateRect(hWnd, NULL, FALSE);
					UpdateWindow(hWnd);
					return 0;

				case WM_DESTROY:
					DetachGLFromWindow(child);
					delete child->flurry;
					free(child->device);
					free(child);
					SetWindowLong(hWnd, GWL_USERDATA, 0);
					return 0;

				case WM_MOUSEMOVE:
				case WM_LBUTTONDOWN:
				case WM_LBUTTONUP:
				case WM_RBUTTONDOWN:
				case WM_RBUTTONUP:
				case WM_KEYDOWN:
				case WM_KEYUP:
				case WM_SYSKEYDOWN:
				case WM_SYSKEYUP:
					// forward input messages to parent, which will probably dismiss us
					// note that the children don't actually get keyboard messages; the
					// main window has the focus.
					return SendMessage(GetParent(hWnd), message, wParam, lParam);
			}

			return DefWindowProc(hWnd, message, wParam, lParam);
		}*/


		/*
		 * RegisterDialogClasses
		 *
		 * Scrnsave.lib entry point: called before ScreenSaverConfigureDialog when
		 * invoked with /c.
		 */

		/*int RegisterDialogClasses(HANDLE hInst)
		{
			_Module.Init(NULL, (HINSTANCE)hInst);
			return TRUE;
		}*/


		/*
		 * ScreenSaverConfigureDialog
		 *
		 * Scrnsave.lib entry point: called when invoked with /c.
		 */

		/*int ScreenSaverConfigureDialog(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam)
		{
			switch (message) {
				case WM_INITDIALOG:
					ScreensaverCommonInit();
					Settings_Read();
					SettingsDialogInit(hWnd);
					SettingsDialogEnableControls(hWnd);
					SettingsToDialog(hWnd);
					return TRUE;
					break;

				case WM_COMMAND:
					switch (LOWORD(wParam)) {
							// command buttons:
						case IDOK:
							SettingsFromDialog(hWnd);
							Settings_Write();
							EndDialog(hWnd, 1);
							return TRUE;
						case IDCANCEL:
							EndDialog(hWnd, 0);
							return TRUE;
						case IDC_CREDITS:
							DialogBoxParam(NULL, MAKEINTRESOURCE(DLG_CREDITS), hWnd, CreditsDialog, 0);
							return TRUE;
						case IDC_ABOUT:
							CAboutBox::AutomaticDoModal();
							return TRUE;
						case IDC_TEST:
							SettingsFromDialog(hWnd);
							DoTestScreenSaver();
							return TRUE;

							// radio buttons:
						case IDC_POSITION_DESKTOP:
						case IDC_POSITION_PRIMARY:
						case IDC_POSITION_PER:
							SettingsDialogEnableControls(hWnd);
							break;
					}
					break; // from WM_COMMAND
				case WM_DESTROY:
					_Module.Term();
					break;
			}

			return FALSE;
		}


		static void
			SettingsDialogEnableControls(HWND hWnd)
		{
			// Configure button enabled only when relevant
			EnableWindow(GetDlgItem(hWnd, IDC_POSITION_PER_CONFIGURE),
			             IsDlgButtonChecked(hWnd, IDC_POSITION_PER));

			// but all multimon stuff enabled only on multimon system
			if (g_nMonitors <= 1) {
				EnableWindow(GetDlgItem(hWnd, IDC_POSITION_PRIMARY), FALSE);
				EnableWindow(GetDlgItem(hWnd, IDC_POSITION_PER), FALSE);
				EnableWindow(GetDlgItem(hWnd, IDC_POSITION_PER_CONFIGURE), FALSE);
			}

			// XXX and per-monitor flurry assignment isn't implemented yet anyway
			EnableWindow(GetDlgItem(hWnd, IDC_POSITION_PER_CONFIGURE), FALSE);
		}


		static void
			SettingsDialogInit(HWND hWnd)
		{
			HWND hPresetList = GetDlgItem(hWnd, IDC_VISUAL);
			for (int i = 0; i < g_visuals.size(); i++) {
				ComboBox_AddString(hPresetList, g_visuals[i]->name);
			}
		}


		static void
			SettingsToDialog(HWND hWnd)
		{
			// visual preset
			ComboBox_SetCurSel(GetDlgItem(hWnd, IDC_VISUAL), iFlurryPreset);

			// multimon options
			if (g_nMonitors <= 1) {
				iMultiMonPosition = MULTIMON_ALLMONITORS;
			}
			CheckRadioButton(hWnd, IDC_POSITION_DESKTOP, IDC_POSITION_PER,
			                 IDC_POSITION_DESKTOP + iMultiMonPosition);

			// buffering mode
			CheckRadioButton(hWnd, IDC_DOUBLE_BUFFER_NONE, IDC_DOUBLE_BUFFER_PARANOID,
			                 IDC_DOUBLE_BUFFER_NONE + iSettingBufferMode);
		}


		static void
			SettingsFromDialog(HWND hWnd)
		{
			// visual preset
			iFlurryPreset = ComboBox_GetCurSel(GetDlgItem(hWnd, IDC_VISUAL));

			// multimon options
			if (IsDlgButtonChecked(hWnd, IDC_POSITION_DESKTOP)) {
				iMultiMonPosition = MULTIMON_ALLMONITORS;
			} else if (IsDlgButtonChecked(hWnd, IDC_POSITION_PRIMARY)) {
				iMultiMonPosition = MULTIMON_PRIMARY;
			} else if (IsDlgButtonChecked(hWnd, IDC_POSITION_PER)) {
				iMultiMonPosition = MULTIMON_PERMONITOR;
			}

			// buffering mode
			if (IsDlgButtonChecked(hWnd, IDC_DOUBLE_BUFFER_NONE)) {
				iSettingBufferMode = BUFFER_MODE_SINGLE;
			} else if (IsDlgButtonChecked(hWnd, IDC_DOUBLE_BUFFER_OPTIMISTIC)) {
				iSettingBufferMode = BUFFER_MODE_FAST_DOUBLE;
			} else if (IsDlgButtonChecked(hWnd, IDC_DOUBLE_BUFFER_PARANOID)) {
				iSettingBufferMode = BUFFER_MODE_SAFE_DOUBLE;
			}
		}*/


		// WGL attach/detach code
/*
		static void
			AttachGLToWindow(FlurryAnimateChildInfo *child)
		{
			// find current display settings on this monitor
			DEVMODE mode;
			EnumDisplaySettings(child->device, ENUM_CURRENT_SETTINGS, &mode);
			_RPT4(_CRT_WARN, "  current display settings %dx%dx%dbpp@%dHz\n",
			      mode.dmPelsWidth, mode.dmPelsHeight, mode.dmBitsPerPel, mode.dmDisplayFrequency);
			if (mode.dmDisplayFrequency == 0) {	// query failed
				mode.dmDisplayFrequency = 60;	// default to sane value
			}
			_RPT1(_CRT_WARN, "  refresh time = %d ms\n", 1000 / mode.dmDisplayFrequency);
			child->updateInterval = 1000 / mode.dmDisplayFrequency;

			// build a pixel format
			int iPixelFormat;
			RECT rc;
			PIXELFORMATDESCRIPTOR pfd = {
				sizeof(PIXELFORMATDESCRIPTOR),    // structure size
				1,                                // version number
				PFD_DRAW_TO_WINDOW | PFD_SUPPORT_OPENGL, // flags: support OpenGL rendering to visible window
				PFD_TYPE_RGBA,                    // pixel format: RGBA
				mode.dmBitsPerPel,                // color depth, excluding alpha -- use whatever it's doing now
				0, 0, 0, 0, 0, 0, 8, 0,           // color bits ignored?
				0,                                // no accumulation buffer
				0, 0, 0, 0,                       // accum bits ignored
				16,                               // 16-bit z-buffer
				0,                                // no stencil buffer
				0,                                // no auxiliary buffer
				PFD_MAIN_PLANE,                   // main layer
				0,                                // reserved
				0, 0, 0                           // layer masks ignored
			};

			if (iSettingBufferMode > BUFFER_MODE_SINGLE) {
				pfd.dwFlags |= PFD_DOUBLEBUFFER;
			}

			// need a DC for the pixel format
			child->hdc = GetDC(child->hWnd);

			// apply pixel format to DC
			iPixelFormat = ChoosePixelFormat(child->hdc, &pfd);
			SetPixelFormat(child->hdc, iPixelFormat, &pfd);
			
			// then use this to create a rendering context
			child->hglrc = wglCreateContext(child->hdc);
			wglMakeCurrent(child->hdc, child->hglrc);

			// tell Flurry to use the whole window as viewport
			GetClientRect(child->hWnd, &rc);
			child->flurry->SetSize(rc.right - rc.left, rc.bottom - rc.top);

			// some nice debug output
			_RPT4(_CRT_WARN, "  child 0x%08x: hWnd 0x%08x, hdc 0x%08x, hglrc 0x%08x\n",
			      child, child->hWnd, child->hdc, child->hglrc);
			_RPT1(_CRT_WARN, "  GL vendor:     %s\n", glGetString(GL_VENDOR));
			_RPT1(_CRT_WARN, "  GL renderer:   %s\n", glGetString(GL_RENDERER));
			_RPT1(_CRT_WARN, "  GL version:    %s\n", glGetString(GL_VERSION));
			_RPT1(_CRT_WARN, "  GL extensions: %s\n", glGetString(GL_EXTENSIONS));
			_RPT0(_CRT_WARN, "\n");
		}


		static void
			DetachGLFromWindow(FlurryAnimateChildInfo *child)
		{
			if (child->hglrc == wglGetCurrentContext()) {
				_RPT1(_CRT_WARN, "Evicting context %d\n", child->id);
				wglMakeCurrent(NULL, NULL);
			}
			if (!wglDeleteContext(child->hglrc)) {
				_RPT2(_CRT_WARN, "Failed to delete context for %d: %d\n",
				      child->id, GetLastError());
			}
			ReleaseDC(child->hWnd, child->hdc);
		}
*/

		void CopyFrontBufferToBack(IntPtr hWnd)
		{
			bool bFirstTime = true;

			// copy front buffer to back buffer, to compensate for Windows'
			// possibly weird implementation of SwapBuffers().  As documented, it
			// reserves the right to leave the back buffer completely undefined
			// after each swap, but on both my ATI Radeon 8500 and NVidia GF4Ti4200
			// it works almost fine to just copy front to back once like this.

			if ((iSettingBufferMode == BUFFER.BUFFER_MODE_SAFE_DOUBLE) ||
			    (iSettingBufferMode == BUFFER.BUFFER_MODE_FAST_DOUBLE && bFirstTime)) {
				Win32.RECT rc=new Win32.RECT();
				Win32.User.GetClientRect(hWnd, ref rc);

				Gl.glDisable(Gl.GL_ALPHA_TEST);
				if (!iBugWhiteout) {
					// Found this by accident; Adam likes it.  Freakshow option #1.
					Gl.glDisable(Gl.GL_BLEND);
				}
				Gl.glReadBuffer(Gl.GL_FRONT);
				Gl.glDrawBuffer(Gl.GL_BACK);
				Gl.glRasterPos2i(0, 0);
				Gl.glCopyPixels(0, 0, rc.Right, rc.Bottom, Gl.GL_COLOR);
				if (!iBugWhiteout) {
					Gl.glEnable(Gl.GL_BLEND);
				}
				Gl.glEnable(Gl.GL_ALPHA_TEST);

				bFirstTime = false;
			}
		}


		/* BOOL WINAPI
			CreditsDialog(HWND hDlg, UINT message, WPARAM wParam, LPARAM lParam)
		{
			switch (message) {
				case WM_INITDIALOG:
					return TRUE;
					break;

				case WM_COMMAND:
					EndDialog(hDlg, 1);
					break;
			}

			return FALSE;
		}*/


		/*static void
			ScreenSaverUpdateFpsIndicator(FlurryAnimateChildInfo *child)
		{
			DWORD now = timeGetTime();
			FPS *fps = &child->fps;
			DWORD prevSample, prevRingSample = fps->samples[fps->nextSample];
			char buf[100];
			double last, recent, overall;

			// always gather data in case they turn on FPS later
			prevRingSample = fps->samples[fps->nextSample];
			prevSample = (fps->nSamples == 0) ? fps->startTime :
				((fps->nextSample == 0) ? fps->samples[FPS_SAMPLES - 1] :
				 fps->samples[fps->nextSample - 1]);

			fps->samples[fps->nextSample] = now;
			fps->nextSample = (fps->nextSample + 1) % FPS_SAMPLES;
			fps->nSamples++;

			_RPT3(_CRT_WARN, "Child %d: last render %d ms (target %d ms)\n",
			      child->id, now - prevSample, child->updateInterval);

			// but the rest of the work is only necessary if they want to see it
			if (!iShowFPSIndicator) {
				return;
			}

			// calculate overall, simple average
			overall = 1000.0 * fps->nSamples / (now - fps->startTime);

			// calculate last frame; in ring buffer if more than one, else same
			if (fps->nSamples == 1) {
				last = overall;
			} else {
				last = 1000.0 / (now - prevSample);
			}

			// calculate last 20; in ring buffer if more than 20, else same
			if (fps->nSamples < FPS_SAMPLES) {
				// ring buffer not full yet; just use from front till now
				recent = overall;
			} else {
				// ring buffer has full set of samples; use most recent set
				recent = 1000.0 / (now - prevRingSample) * FPS_SAMPLES;
			}

			sprintf(buf, "FPS: Overall %.1f / Recent %.1f / Last %.1f   ",
			        overall, recent, last);
			TextOut(child->hdc, 5, 5, buf, lstrlen(buf));
		}*/


		public void DoTestScreenSaver()
		{
			// calculate desktop rect
			int screenX = Win32.User.GetSystemMetrics(Win32.User.SM_XVIRTUALSCREEN);
			int screenY = Win32.User.GetSystemMetrics(Win32.User.SM_YVIRTUALSCREEN);
			int screenW = Win32.User.GetSystemMetrics(Win32.User.SM_CXVIRTUALSCREEN);
			int screenH = Win32.User.GetSystemMetrics(Win32.User.SM_CYVIRTUALSCREEN);

			// create fullscreen window
			/*WNDCLASS wc = { 0 };
			wc.lpszClassName = "FlurryTestWindow";
			wc.lpfnWndProc = ScreenSaverProc;
			wc.hbrBackground = (HBRUSH)GetStockObject(BLACK_BRUSH);
			RegisterClass(&wc);

			g_bPreviewMode = true;
			CreateWindowEx(WS_EX_TOPMOST, wc.lpszClassName, wc.lpszClassName, WS_VISIBLE | WS_POPUP,
			               screenX, screenY, screenW, screenH, NULL, NULL, _Module.m_hInst, NULL);
			               */
		}

	}
}
