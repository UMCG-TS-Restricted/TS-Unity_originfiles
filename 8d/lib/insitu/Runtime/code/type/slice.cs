using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace insitu
{
	public struct slice
	{
		public byte[] data;
		public int offset;
		public int length;

		public void memset()
		{
			for (var i = 0; i < length; i++)
				data[offset + i] = 0;
		}

		public Span<byte> span => new Span<byte>(data, offset, length);
	}
}
