﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace cn.vimfung.luascriptcore
{
	/// <summary>
	/// Lua方法
	/// </summary>
	public class LuaFunction : LuaBaseObject
	{
		private string _index;
		private LuaContext _context;

		/// <summary>
		/// 初始化
		/// </summary>
		/// <param name="decoder">对象解码器</param>
		public LuaFunction (LuaObjectDecoder decoder)
			: base(decoder)
		{
			int contextId = decoder.readInt32 ();
			_context = LuaContext.getContext (contextId);
			_index = decoder.readString ();
		}

		/// <summary>
		/// 序列化对象
		/// </summary>
		/// <param name="encoder">对象编码器.</param>
		public override void serialization(LuaObjectEncoder encoder)
		{
			base.serialization (encoder);

			encoder.writeInt32 (_context.objectId);
			encoder.writeString (_index);
		}

		/// <summary>
		/// 调用
		/// </summary>
		/// <param name="arguments">Arguments.</param>
		public LuaValue invoke(List<LuaValue> arguments)
		{
			IntPtr funcPtr = IntPtr.Zero;
			IntPtr argsPtr = IntPtr.Zero;
			IntPtr resultPtr = IntPtr.Zero;

			LuaObjectEncoder funcEncoder = new LuaObjectEncoder ();
			funcEncoder.writeObject (this);

			byte[] bytes = funcEncoder.bytes;
			funcPtr = Marshal.AllocHGlobal (bytes.Length);
			Marshal.Copy (bytes, 0, funcPtr, bytes.Length);

			if (arguments != null)
			{
				LuaObjectEncoder argEncoder = new LuaObjectEncoder ();
				argEncoder.writeInt32 (arguments.Count);
				foreach (LuaValue value in arguments)
				{
					argEncoder.writeObject (value);
				}

				bytes = argEncoder.bytes;
				argsPtr = Marshal.AllocHGlobal (bytes.Length);
				Marshal.Copy (bytes, 0, argsPtr, bytes.Length);
			}

			int size = NativeUtils.invokeLuaFunction (_context.objectId, funcPtr, argsPtr, out resultPtr);

			if (argsPtr != IntPtr.Zero)
			{
				Marshal.FreeHGlobal (argsPtr);
			}
			if (funcPtr != IntPtr.Zero)
			{
				Marshal.FreeHGlobal (funcPtr);
			}

			return LuaObjectDecoder.DecodeObject (resultPtr, size) as LuaValue;
		}
	}
}

