using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace insitu
{
	public struct range
	{
		public int index;
		public int length;

		public range(int offset, int count)
		{
			index = offset;
			length = count;
		}
	}
}
