using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace RDSUT
{
	//[TestClass]
	public class UnitTest1
	{
		[TestMethod]
		public void TestMethod1()
		{
		}

		//[TestMethod]
		public int[] GetReagentItems()
		{
			var usedReagents = new string[1] { "MG" };
			var Reagent = "UU,CT";
			var result = new int[4];
			var reagentItems = new System.Collections.Generic.List<string>(Reagent.Split(','));
			for (int i = 0; i < usedReagents.Length; i++)
			{
				var currentItem = usedReagents.ToArray()[i];
				var test = reagentItems.FirstOrDefault(o => o == currentItem);
				result[i] = test != null ? 1 : 0;
			}
			return result;
		}
	}
}
