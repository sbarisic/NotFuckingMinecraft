using OpenTK;

namespace NFM {

	static class Camera {
		public static Matrix4 Projection = Matrix4.Identity;
		public static Matrix4 View = Matrix4.Identity;
		public static Matrix4 Rotation = Matrix4.Identity;
		public static Vector2 ScreenRes;

		public static void SetPos(float X, float Y, float Z) {
			SetPos(new Vector3(X, Y, Z));
		}

		public static void SetPos(Vector3 Pos) {
			View = Matrix4.CreateTranslation(Pos);
		}

		public static void Move(float X, float Y, float Z) {
			Move(new Vector3(X, Y, Z));
		}

		public static void Move(Vector3 Pos) {
			View = Matrix4.Mult(View, Matrix4.CreateTranslation(Pos));
		}

		public static void Rotate(float X, float Y, float Z) {
			Rotate(new Vector3(X, Y, Z));
		}

		public static void Rotate(Vector3 Rot) {
			Rotation = Matrix4.CreateRotationY(Rot.Y) * Matrix4.CreateRotationZ(Rot.Z) * Matrix4.CreateRotationX(Rot.X);
		}

		public static Vector3 GetRight() {
			return new Vector3(Rotation.M11, Rotation.M21, Rotation.M31);
		}

		public static Vector3 GetLeft() {
			return -GetRight();
		}

		public static Vector3 GetUp() {
			return new Vector3(Rotation.M12, Rotation.M22, Rotation.M33);
		}

		public static Vector3 GetDown() {
			return -GetUp();
		}

		public static Vector3 GetBackward() {
			return new Vector3(Rotation.M13, Rotation.M23, Rotation.M33);
		}

		public static Vector3 GetForward() {
			return -GetBackward();
		}

		public static Vector3 GetPosition() {
			return View.ExtractTranslation();
		}
	}

}