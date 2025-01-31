using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;


namespace insitu
{
	/// <summary>
	///		Unity utility procedures.
	/// </summary>
	public static class Unity
	{
		/// <summary>
		///		Performs FindObjectsOfTypeAll and returns the first valid value.
		///		If there is no valid value, null will be returned.
		/// </summary>
		public static T FindResource<T>() where T : Object
		{
			var resources = Resources.FindObjectsOfTypeAll<T>();
			for (var i = 0; i < resources.Length; i++)
			{
				var resource = resources[i];
				if (resource is T result)
					return result;
			}

			return null;
		}

		/// <summary>
		///		Performs FindObjectsOfTypeAll and stores all valid results in <paramref name="list"/>.
		///		The procedure does not clear <paramref name="list"/> and will only append to the list.
		/// </summary>
		public static void FindResources<T>(List<T> list) where T : Object
		{
			var resources = Resources.FindObjectsOfTypeAll<T>();
			for (var i = 0; i < resources.Length; i++)
			{
				var resource = resources[i];
				if (resource is T result)
					list.Add(result);
			}
		}

		/// <see cref="double4.sqrmagnitude"/>
		public static float SqrMagnitude(Quaternion q) => q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w;

		/// <summary>
		///		Perform a ping to <paramref name="host"/>:<paramref name="port"/>.
		///		If trying to connect directly, the program will crash if it fails.
		///		This procedure makes sure a connection is available.
		/// </summary>
		public static async Task<string> Ping(string host, int port)
		{
			string result = null;
			Socket socket = null;

			try
			{
				socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				await socket.ConnectAsync(host, port);
				if (!socket.Connected)
					result = "Port not open";
			}
			catch (Exception e)
			{
				result = e.Message;
			}
			finally
			{
				socket?.Close();
				socket?.Dispose();
			}

			return result;
		}
	}
}
