using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wandering.Helpers
{
	static class ArrayHelper
	{
		public static void ForEach<T>(this T[] array, Action<T> action)
		{
			for(int i=0; i<array.Length; ++i)
				action(array[i]);
		}
	}
}
