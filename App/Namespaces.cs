/*
 * Created by SharpDevelop.
 * User: andrewc
 * Date: 2/10/2009
 * Time: 11:35 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace sqlconsole
{
	/// <summary>
	/// Description of Namespaces.
	/// </summary>
	public sealed class Namespaces
	{
		private static Namespaces instance = new Namespaces();
		
		public static Namespaces Instance {
			get {
				return instance;
			}
		}
		
		private Namespaces()
		{
		}
		
		public void modifyMode() {
			Console.WriteLine("Namespace Modify Mode:");
			
		
		}
	}
}
