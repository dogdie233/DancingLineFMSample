using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Level.Skins
{
	public class SkinManager
	{
		public static Type defaultSkin = typeof(NormalSkin);
		private static Type[] _skins;
		public static Type[] Skins
		{
			get
			{
				if (_skins == null) { UpdateSkinList(); }
				return _skins;
			}
			private set => _skins = value;
		}

		public static void UpdateSkinList()
		{
			_skins = Assembly.GetCallingAssembly()
						.GetTypes()
						.Where(t => typeof(SkinBase).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
						.ToArray();
			Debug.Log($"Found {_skins.Length} skins");
		}

		public static SkinBase[] InstantiateSkins(Line line)
		{
			List<SkinBase> skins = new List<SkinBase>();
			foreach (Type type in Skins)
			{
				ConstructorInfo constructor = type.GetConstructor(new Type[] { typeof(Line) });  // 获取 new (Line)的构造器
				skins.Add((SkinBase)constructor.Invoke(new object[] { line }));
			}
			return skins.Count != 0 ? skins.ToArray() : null;
		}

		public static void ChangeSkin(Line line, Type newSkin) => line.ChangeSkin(newSkin);

		public static void ChangeAllLineSkin(Type newSkin)
		{
			foreach (Line line in GameController.lines)
			{
				line.ChangeSkin(newSkin);
			}
		}
	}
}
