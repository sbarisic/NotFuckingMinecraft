using OpenTK;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace NFM {

	static class Rand {
		public static Random R = new Random();
	}

	static class Meth {
		public static float Pi {
			get {
				return (float)Math.PI;
			}
			set {
				throw new Exception("You can't change the definition of Pi, you faggot!");
			}
		}

		public static float Sin(this double D) {
			return ((float)D).Sin();
		}

		public static float Sin(this float F) {
			return (float)Math.Sin(F);
		}

		public static float Cos(this double D) {
			return ((float)D).Cos();
		}

		public static float Cos(this float F) {
			return (float)Math.Cos(F);
		}

		public static float Tan(this double D) {
			return ((float)D).Tan();
		}

		public static float Tan(this float F) {
			return (float)Math.Tan(F);
		}

		public static float Tanh(this double D) {
			return ((float)D).Tanh();
		}

		public static float Tanh(this float F) {
			return (float)Math.Tanh(F);
		}
	}

	static class VecMth {
		public static Vector3[] Bresenham(Vector3 Start, Vector3 End) {
			int startX = (int)Start.X, startY = (int)Start.Y, startZ = (int)Start.Z;
			int endX = (int)End.X, endY = (int)End.Y, endZ = (int)End.Z;
			List<Vector3> Lines = new List<Vector3>();

			int dx, dy, dz;
			int sx, sy, sz;
			int accum, accum2;

			dx = endX - startX;
			dy = endY - startY;
			dz = endZ - startZ;

			sx = ((dx) < 0 ? -1 : ((dx) > 0 ? 1 : 0));
			sy = ((dy) < 0 ? -1 : ((dy) > 0 ? 1 : 0));
			sz = ((dz) < 0 ? -1 : ((dz) > 0 ? 1 : 0));

			dx = Math.Abs(dx);
			dy = Math.Abs(dy);
			dz = Math.Abs(dz);

			endX += sx;
			endY += sy;
			endZ += sz;

			if (dx > dy) {
				if (dx > dz) {
					accum = dx >> 1;
					accum2 = accum;
					do {
						Lines.Add(new Vector3(startX, startY, startZ));

						accum -= dy;
						accum2 -= dz;
						if (accum < 0) {
							accum += dx;
							startY += sy;
						}
						if (accum2 < 0) {
							accum2 += dx;
							startZ += sz;
						}
						startX += sx;
					}
					while (startX != endX);
				} else {
					accum = dz >> 1;
					accum2 = accum;
					do {
						Lines.Add(new Vector3(startX, startY, startZ));

						accum -= dy;
						accum2 -= dx;
						if (accum < 0) {
							accum += dz;
							startY += sy;
						}
						if (accum2 < 0) {
							accum2 += dz;
							startX += sx;
						}
						startZ += sz;
					}
					while (startZ != endZ);
				}
			} else {
				if (dy > dz) {
					accum = dy >> 1;
					accum2 = accum;
					do {
						Lines.Add(new Vector3(startX, startY, startZ));

						accum -= dx;
						accum2 -= dz;
						if (accum < 0) {
							accum += dx;
							startX += sx;
						}
						if (accum2 < 0) {
							accum2 += dx;
							startZ += sz;
						}
						startY += sy;
					}
					while (startY != endY);
				} else {
					accum = dz >> 1;
					accum2 = accum;
					do {
						Lines.Add(new Vector3(startX, startY, startZ));

						accum -= dx;
						accum2 -= dy;
						if (accum < 0) {
							accum += dx;
							startX += sx;
						}
						if (accum2 < 0) {
							accum2 += dx;
							startY += sy;
						}
						startZ += sz;
					}
					while (startZ != endZ);
				}
			}

			return Lines.ToArray();
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

	static class Screen {
		public static int Wi {
			get {
				return System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
			}
		}

		public static int Hi {
			get {
				return System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;
			}
		}

		public static float Wf {
			get {
				return Wi;
			}
		}

		public static float Hf {
			get {
				return Hi;
			}
		}

		internal static Point? _Center;
		public static Point Center {
			get {
				if (_Center != null)
					return _Center.Value;
				return (_Center = new Point(Wi / 2, Hi / 2)).Value;
			}
		}

		public static T W<T>() {
			object R = Wi;
			return (T)R;
		}

		public static T H<T>() {
			object R = Hi;
			return (T)R;
		}
	}

}