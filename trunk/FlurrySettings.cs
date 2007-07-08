/*
 * Created by SharpDevelop.
 * User: FurYy
 * Date: 7.07.2007
 * Time: 23:55
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Xml;
using System.Collections.Generic;

namespace FlurrySharp
{
	/// <summary>
	/// Description of FlurrySettings.
	/// </summary>
	public class FlurrySettings
	{
        public List<FlurrySpec> specs ;
        public FlurrySettings()
        {
            specs = new List<FlurrySpec>(FlurrySettings.MakeDefaultSpecs());
            foreach (string s in Properties.Settings.Default.ExtraFlurries)
            {
                specs.Add(new FlurrySpec(s));
            }
            //LoadFlurries();
        }

       /* void LoadFlurries()
        {
            XmlDocument xdoc = new XmlDocument();
            xdoc.Load("flurries.xml");
            XmlNodeList nodes = xdoc.GetElementsByTagName("flurry");
            foreach (XmlNode node in nodes)
            {
                Console.WriteLine(node.Attributes["settings"].InnerText);
                specs.Add(new FlurrySpec(node.Attributes["settings"].InnerText));
            }
        }*/


		public static FlurrySpec [] MakeDefaultSpecs()
		{
			//"Classic:{5,tiedye,100,1.0}",
			//"RGB:{3,red,100,0.8};{3,blue,100,0.8};{3,green,100,0.8}",
			//"Water:{1,blue,100.0,2.0};{1,blue,100.0,2.0};{1,blue,100.0,2.0};{1,blue,100.0,2.0};{1,blue,100.0,2.0};{1,blue,100.0,2.0};{1,blue,100.0,2.0};{1,blue,100.0,2.0};{1,blue,100.0,2.0}",
			//"Fire:{12,slowCyclic,10000.0,0.0}",
			//"Psychedelic:{10,rainbow,200.0,2.0}"
			FlurrySpec [] specs=new FlurrySpec[6];
			specs[0]=new FlurrySpec("Classic:{5,tiedye,100.0,1.0}");
			specs[1]=new FlurrySpec("RGB:{3,red,100,0.8};{3,blue,100,0.8};{3,green,100,0.8}");
			specs[2]=new FlurrySpec("Water:{1,blue,100.0,2.0};{1,blue,100.0,2.0};{1,blue,100.0,2.0};{1,blue,100.0,2.0};{1,blue,100.0,2.0};{1,blue,100.0,2.0};{1,blue,100.0,2.0};{1,blue,100.0,2.0};{1,blue,100.0,2.0}");
			specs[3]=new FlurrySpec("Fire:{12,slowCyclic,10000.0,0.0}");
			specs[4]=new FlurrySpec("Psychedelic:{10,rainbow,200.0,2.0}");
            specs[5]=new FlurrySpec("Crazy:{16,slowCyclic,200.0,0.7};{16,slowCyclic,200.0,0.5}");
			return specs;
		}
	}
}
