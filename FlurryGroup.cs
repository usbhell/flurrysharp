/*
 * Created by SharpDevelop.
 * User: FurYy
 * Date: 7.07.2007
 * Time: 22:53
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections.Generic;

namespace FlurrySharp
{
	/// <summary>
	/// Description of FlurryGroup.
	/// </summary>
	public class FlurryGroup
	{
		
		
		List<FlurryCluster> clusters;
		
		/*
		 * Note: the Flurry base code keeps everything in a global variable named
		 * info.  We want to instance it, for multimon support (several separate
		 * Flurries), so we allocate several such structures, but to avoid changing
		 * the base code, we set info = current->globals before calling into it.
		 * Obviously, not thread safe.
		 */

		public FlurryGroup(FlurrySpec preset)
		{
			clusters=new List<FlurryCluster>();
			
			//if (preset > g_visuals.size()) {
				//_RPT2(_CRT_WARN, "Invalid preset %d (max %d); using default\n",
				//      preset, g_visuals.size());
			//	preset = 0;
			//}
			//FlurrySpec visual = g_visuals[preset];

			for (int i = 0; i < preset.clusters.Count; i++) {
				clusters.Add(new FlurryCluster(preset.clusters[i]));
			}
		}


		//	~FlurryGroup(void)
//		{
//			for (int i = 0; i < clusters.size(); i++) {
//				delete clusters[i];
//			}
//		}


		public void SetSize(int width, int height)
		{
			for (int i = 0; i < clusters.Count; i++) {
				clusters[i].SetSize(width, height);
			}
		}


		public void PrepareToAnimate()
		{
			if (!Types.iBugBlockMode) {
				// Found this by accident; looks cool.  Freakshow option #2.
				Texture.MakeTexture();
			}

			for (int i = 0; i < clusters.Count; i++) {
				clusters[i].PrepareToAnimate();
			}
		}


		public void AnimateOneFrame()
		{
			for (int i = 0; i < clusters.Count; i++) {
				clusters[i].AnimateOneFrame();
			}
		}

	}
}
