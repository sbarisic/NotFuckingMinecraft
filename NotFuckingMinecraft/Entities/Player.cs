using OpenTK;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;

namespace NFM.Entities {

	class Player : Entity {
		float Mouse_Yd = Mth.Pi / 2;
		float Mouse_Xd = 3 * (Mth.Pi / 4);

		Point Center;
		Point Delt;

		Dictionary<Key, bool> Keyboard;

		// TODO: Abstract later
		[DllImport("user32.dll")]
		static extern bool SetCursorPos(int X, int Y);
		static bool SetCursorPos(Point P) {
			return SetCursorPos(P.X, P.Y);
		}
		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool GetCursorPos(out Point lpPoint);
		static Point GetCursorPos() {
			Point R;
			GetCursorPos(out R);
			return R;
		}

		void AssureKey(Key K) {
			if (!Keyboard.ContainsKey(K))
				Keyboard.Add(K, false);
		}

		void AssureKey(params Key[] K) {
			foreach (var k in K)
				AssureKey(k);
		}

		public override void Init() {
			Camera.Projection = Matrix4.CreatePerspectiveFieldOfView(90 * Mth.Pi / 180,
				SizeMgr.SizeScale.X / SizeMgr.SizeScale.Y, 0.1f, 10000f);
			Camera.Move(0, 0, -Macroblock.ChunkSize * Block.Size * 6);
			Camera.ScreenRes = SizeMgr.SizeScale;

			Center = new Point(R.X + (int)SizeMgr.SizeScale.X / 2, R.Y + (int)SizeMgr.SizeScale.Y / 2);
			SetCursorPos(Center);
			R.Cursor = MouseCursor.Empty;

			Keyboard = new Dictionary<Key, bool>();

			AssureKey(Key.W, Key.A, Key.S, Key.D, Key.C, Key.Space, Key.Escape);

			R.KeyDown += KeyDown;
			R.KeyUp += KeyUp;
			R.MouseDown += MouseDown;
			base.Init();
		}

		void MouseDown(object sender, MouseButtonEventArgs e) {
			if (e.Button == MouseButton.Left) {
				Vector3 S = R.GameWorld.GetBlockPos(-Camera.GetPosition());
				Vector3 E = R.GameWorld.GetBlockPos(-Camera.GetPosition() + Camera.GetForward() * 20);
				Vector3[] P = VecMth.Bresenham(S, E);
				R.GameWorld.SetBlock(P[P.Length - 1], BlockDefs.Air);
			} else if (e.Button == MouseButton.Right) {
				Vector3 S = R.GameWorld.GetBlockPos(-Camera.GetPosition());
				Vector3 E = R.GameWorld.GetBlockPos(-Camera.GetPosition() + Camera.GetForward() * 20);
				Vector3[] P = VecMth.Bresenham(S, E);
				R.GameWorld.SetBlock(P[P.Length - 1], BlockDefs.Box);
			}
		}

		void KeyDown(object sender, KeyboardKeyEventArgs e) {
			if (e.IsRepeat)
				return;

			if (e.Key == Key.F1)
				Settings.Wireframe = !Settings.Wireframe;
			else if (e.Key == Key.F2)
				Settings.Colored = !Settings.Colored;
			else if (e.Key == Key.F3)
				Settings.FXAA = !Settings.FXAA;

			AssureKey(e.Key);
			Keyboard[e.Key] = true;
		}

		void KeyUp(object sender, KeyboardKeyEventArgs e) {
			AssureKey(e.Key);
			Keyboard[e.Key] = false;
		}

		public override void Update(float T) {
			if (!R.Focused)
				return;

			float Sens = 50 * T;

			Point MousePos = GetCursorPos();
			SetCursorPos(Center);
			Delt = new Point(-(Center.X - MousePos.X), -(Center.Y - MousePos.Y));
			if (Delt.X != 0 || Delt.Y != 0)
				UpdateMouse(new MouseMoveEventArgs(MousePos.X, MousePos.Y, Delt.X, Delt.Y));

			if (Keyboard[Key.Escape])
				R.Exit();

			if (Keyboard[Key.W])
				Camera.View = Matrix4.Mult(Camera.View, Matrix4.CreateTranslation(
					Sens * (Mouse_Xd + Mth.Pi / 2).Cos(), Sens * (Mouse_Xd + Mth.Pi / 2).Sin(), 0));
			if (Keyboard[Key.S])
				Camera.View = Matrix4.Mult(Camera.View, Matrix4.CreateTranslation(
					-Sens * (Mouse_Xd + Mth.Pi / 2).Cos(), -Sens * (Mouse_Xd + Mth.Pi / 2).Sin(), 0));
			if (Keyboard[Key.A])
				Camera.View = Matrix4.Mult(Camera.View, Matrix4.CreateTranslation(
					-Sens * Mouse_Xd.Cos(), -Sens * Mouse_Xd.Sin(), 0));
			if (Keyboard[Key.D])
				Camera.View = Matrix4.Mult(Camera.View, Matrix4.CreateTranslation(Sens * Mouse_Xd.Cos(),
					Sens * Mouse_Xd.Sin(), 0));
			if (Keyboard[Key.Space])
				Camera.View = Matrix4.Mult(Camera.View, Matrix4.CreateTranslation(0, 0, -Sens));
			if (Keyboard[Key.C])
				Camera.View = Matrix4.Mult(Camera.View, Matrix4.CreateTranslation(0, 0, Sens));

			Camera.Rotate(Mouse_Yd, (float)Math.PI, Mouse_Xd);
		}

		public void UpdateMouse(MouseMoveEventArgs e) {
			Mouse_Xd += -(float)e.XDelta / 100f;
			Mouse_Yd += (float)e.YDelta / 100f;

			Mouse_Xd = Mouse_Xd % (float)(2 * Math.PI);
			if (Mouse_Yd > Math.PI)
				Mouse_Yd = (float)Math.PI;
			if (Mouse_Yd < 0)
				Mouse_Yd = 0;
		}
	}

}