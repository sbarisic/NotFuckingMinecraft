using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace WorldPORTAL {
	static class Mth {
		public static float Pi {
			get {
				return (float)Math.PI;
			}
		}

		public static float Sin(this double D) {
			return Sin((float)D);
		}

		public static float Sin(this float F) {
			return (float)Math.Sin(F);
		}

		public static float Cos(this double D) {
			return Cos((float)D);
		}

		public static float Cos(this float F) {
			return (float)Math.Cos(F);
		}
	}

	static class SizeMgr {
		public static Vector2 SizeScale;

		public static float GetX(float Sz) {
			return SizeScale.X * Sz;
		}

		public static float GetY(float Sz) {
			return SizeScale.Y * Sz;
		}
	}
}