/*
 * Created by SharpDevelop.
 * User: FurYy
 * Date: 4.08.2007
 * Time: 19:42
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Diagnostics;
using Microsoft.Win32;

namespace FlurryLauncher
{
	class Program
	{
		public static void Main(string[] args)
		{
			try{
				RegistryKey key=Registry.LocalMachine.OpenSubKey(@"Software\FlurrySharp");
				if(key!=null){
					Console.WriteLine(key.GetValue("path"));
					if(key.GetValue("path")!=null)
						Process.Start(key.GetValue("path") as string,string.Join(" ",args));
				}
			}catch(Exception ex){
				Console.WriteLine(ex);
			}
		}
	}
}
