using M2Lib.m2;
using MDXLib.MDX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDXParser
{
	public static class Tests
	{
		static string DESKTOP = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
		static string[] known = new string[] { "InnerRage_Impact_Chest.mdx", "Purge_New_Impact_Chest.mdx", "FireBlast_Impact_Chest.mdx", "RibbonTrail.mdx", "Steam04.mdx", "SteamGeyser.mdx", "Waterbreathing_Impact_Base.mdx" };

		public static void Compare()
		{

			M2 comparison = new M2();
			using (var reader = new BinaryReader(new FileStream(@"Files\Boar.m2", FileMode.Open)))
				comparison.Load(reader);

			var mdxnsfsdfew = new Model(Path.Combine(DESKTOP, "models2", "Boar.mdx"));
			File.WriteAllText($"2SidedPickAxe.mdx.json", Newtonsoft.Json.JsonConvert.SerializeObject(mdxnsfsdfew.Chunks, Newtonsoft.Json.Formatting.Indented));


			foreach (var file in known)
			{
				var mdxold = new Model(@"C:\Users\TomSpearman\Desktop\models\" + file);
				var mdxnew = new Model(@"C:\Users\TomSpearman\Desktop\models2\" + file);

				var format = Newtonsoft.Json.Formatting.Indented;
				File.WriteAllText($"{file}_old.json", Newtonsoft.Json.JsonConvert.SerializeObject(mdxold.Chunks, format));
				File.WriteAllText($"{file}_new.json", Newtonsoft.Json.JsonConvert.SerializeObject(mdxnew.Chunks, format));

			}



		}

		public static void BulkParse()
		{
			Compare();

			new Model(@"C:\Users\TomSpearman\Desktop\models2\2SidedPickAxe.mdx");

			List<string[]> hierachy = new List<string[]>();
			var files = Directory.EnumerateFiles(Path.Combine(DESKTOP, "models2"), "*.mdx", SearchOption.AllDirectories);

			

			HashSet<uint> flags = new HashSet<uint>();
			foreach (var f in files)
			{
				var mdx = new Model(f);

				//if (mdx.Has<GEOS>() && mdx.Get<GEOS>().Any(y => y.SelectionGroup != 0))
				//{
				//	Console.WriteLine(f);
				//	Console.WriteLine(string.Join("|", mdx.Get<GEOS>().Where(y => y.SelectionGroup != 0).Select(y => y.SelectionGroup).Distinct()));
				//}

				//var c = x.Hierachy.Where(y => (y.Flags & AlphaLib.GENOBJECTFLAGS.GENOBJECT_MDLBONESECTION) != 0);
				//if (c.Any())
				//{
				//	foreach (var t in c)
				//		if (t.GetType() != typeof(Bone))
				//			throw new Exception("");

				//	//Console.WriteLine(f);
				//	//foreach (var cx in c)
				//	//	Console.WriteLine(cx.Name + "|" + cx.GetType().Name);
				//}
			}

			Console.ReadLine();
		}
	}
}
