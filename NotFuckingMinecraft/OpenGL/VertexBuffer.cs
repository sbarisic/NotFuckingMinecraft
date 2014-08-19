using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Runtime.InteropServices;

//using Vector3 = OpenTK.Vector3;
//using Vector4 = OpenTK.Vector4;

namespace NFM {

	class VertexBuffer<T> : IBindable where T : struct {
		~VertexBuffer() {
			GLGarbage.Enqueue(() => {
				GL.DeleteBuffer(ID);
			});
		}

		bool Dirty;
		public readonly BufferTarget Target;
		public readonly BufferUsageHint Hint;

		T[] Data;
		int Attribute, SizeInBytes, Size, Stride, Offset;
		VertexAttribPointerType PointerType;
		//VertexAttribIntegerType IPointerType;
		bool Normalized, IntegralType;
		int ID;

		public int GetID {
			get {
				return ID;
			}
		}

		public int Length {
			get {
				if (Data == null)
					return 0;
				return Data.Length;
			}
		}

		public bool Active;

		public bool SetAttribute(int Attribute, bool Normalized = false, int Stride = 0, int Offset = 0) {
			this.Attribute = Attribute;
			this.Stride = Stride;
			this.Offset = Offset;
			this.Normalized = Normalized;
			if (Attribute == -1)
				return false;
			return true;
		}

		public VertexBuffer(BufferTarget BT, BufferUsageHint H, int AttribPointer = -1) {
			ID = GL.GenBuffer();
			Target = BT;
			Hint = H;
			Active = true;
			SetAttribute(AttribPointer);

			if (typeof(T) == typeof(Vector2)) {
				SizeInBytes = Vector2.SizeInBytes;
				Size = 2;
				PointerType = VertexAttribPointerType.Float;
			} else if (typeof(T) == typeof(Vector2d)) {
				SizeInBytes = Vector2d.SizeInBytes;
				Size = 2;
				PointerType = VertexAttribPointerType.Double;
			} else if (typeof(T) == typeof(Vector2h)) {
				SizeInBytes = Vector2h.SizeInBytes;
				Size = 2;
				PointerType = VertexAttribPointerType.HalfFloat;
			} else if (typeof(T) == typeof(Vector3)) {
				SizeInBytes = Vector3.SizeInBytes;
				Size = 3;
				PointerType = VertexAttribPointerType.Float;
			} else if (typeof(T) == typeof(Vector3d)) {
				SizeInBytes = Vector3d.SizeInBytes;
				Size = 3;
				PointerType = VertexAttribPointerType.Double;
			} else if (typeof(T) == typeof(Vector3h)) {
				SizeInBytes = Vector3h.SizeInBytes;
				Size = 3;
				PointerType = VertexAttribPointerType.HalfFloat;
			} else if (typeof(T) == typeof(Vector4)) {
				SizeInBytes = Vector4.SizeInBytes;
				Size = 4;
				PointerType = VertexAttribPointerType.Float;
			} else if (typeof(T) == typeof(Vector4d)) {
				SizeInBytes = Vector4d.SizeInBytes;
				Size = 4;
				PointerType = VertexAttribPointerType.Double;
			} else if (typeof(T) == typeof(Vector4h)) {
				SizeInBytes = Vector4h.SizeInBytes;
				Size = 4;
				PointerType = VertexAttribPointerType.HalfFloat;
			} else {
				SizeInBytes = Marshal.SizeOf(typeof(T));
				Size = 1;
				if (typeof(T) == typeof(int)) {
					PointerType = VertexAttribPointerType.Int;
					//IPointerType = VertexAttribIntegerType.Int;
					IntegralType = true;
				} else if (typeof(T) == typeof(uint)) {
					PointerType = VertexAttribPointerType.UnsignedInt;
					//IPointerType = VertexAttribIntegerType.UnsignedInt;
					IntegralType = true;
				} else if (typeof(T) == typeof(float))
					PointerType = VertexAttribPointerType.Float;
				else if (typeof(T) == typeof(byte)) {
					PointerType = VertexAttribPointerType.UnsignedByte;
					//IPointerType = VertexAttribIntegerType.UnsignedByte;
					IntegralType = true;
				} else if (typeof(T) == typeof(short)) {
					PointerType = VertexAttribPointerType.Short;
					//IPointerType = VertexAttribIntegerType.Short;
					IntegralType = true;
				} else if (typeof(T) == typeof(ushort)) {
					PointerType = VertexAttribPointerType.UnsignedShort;
					//IPointerType = VertexAttribIntegerType.UnsignedShort;
					IntegralType = true;
				} else
					throw new Exception("Unsupported vertex buffer type " + typeof(T).ToString());
			}
		}

		public void Set(T[] Dta) {
			Data = Dta;
			Dirty = Dta != null;
		}

		public void Bind() {
			if (!Active)
				return;
			GL.BindBuffer(Target, ID);
			if (Dirty) {
				Dirty = false;
				GL.BufferData<T>(Target, new IntPtr(Data.Length * SizeInBytes), Data, Hint);
				if (Attribute != -1) {
					GL.EnableVertexAttribArray(Attribute);
					if (IntegralType)
						GL.VertexAttribPointer(Attribute, Size, PointerType, Normalized, Stride, Offset);
					else
						GL.VertexAttribPointer(Attribute, Size, PointerType, Normalized, Stride, Offset);
				}
			}
		}

		public void BindOnAttrib(int Attribute, bool Normalized = false, int Stride = 0, int Offset = 0) {
			if (!Active)
				return;
			if (SetAttribute(Attribute, Normalized, Stride, Offset))
				Bind();
		}

		public void Unbind() {
			GL.BindBuffer(Target, 0);
		}
	}

	class VertexArray : IRenderable, IBindable {
		public static Random Rnd = new Random();

		public readonly int ID;
		public readonly Prog GLProg;

		public int GetID {
			get {
				return ID;
			}
		}

		VertexBuffer<uint> Indices;
		VertexBuffer<Vector2> UVs;
		VertexBuffer<Vector3> Verts, Colors;
		VertexBuffer<Vector4> BlockData;

		IBindable Tex, Tex2;

		public PrimitiveType PType = PrimitiveType.Quads;
		public Matrix4 ModelMatrix;

		internal static uint[] GenInds(Vector3[] Verts) {
			uint[] R = new uint[Verts.Length];
			for (uint i = 0; i < R.Length; i++)
				R[i] = i;
			return R;
		}

		internal static Vector3[] GenCols(Vector3[] Verts) {
			Vector3[] R = new Vector3[Verts.Length];
			for (int i = 0; i < R.Length; i++)
				R[i] = new Vector3(1, 1, 1);
			return R;
		}

		internal static Vector2[] GenUVs(Vector3[] Verts) {
			Vector2[] R = new Vector2[Verts.Length];
			for (int i = 0; i < R.Length; i++)
				R[i] = new Vector2(0, 0);
			return R;
		}

		public VertexArray(Prog GLProgram, Matrix4 MViewMat,
			Vector3[] Vert, uint[] Inds = null, Vector3[] Cols = null, Vector2[] UV = null, Vector4[] BlockInds = null) {
			ID = GL.GenVertexArray();
			GL.BindVertexArray(ID);

			ModelMatrix = MViewMat;
			GLProg = GLProgram;

			Indices = new VertexBuffer<uint>(BufferTarget.ElementArrayBuffer, BufferUsageHint.StaticDraw);
			Verts = new VertexBuffer<Vector3>(BufferTarget.ArrayBuffer, BufferUsageHint.StaticDraw);
			Verts.SetAttribute(GLProg.Attrib("Position"));
			Colors = new VertexBuffer<Vector3>(BufferTarget.ArrayBuffer, BufferUsageHint.StaticDraw);
			UVs = new VertexBuffer<Vector2>(BufferTarget.ArrayBuffer, BufferUsageHint.StaticDraw);
			BlockData = new VertexBuffer<Vector4>(BufferTarget.ArrayBuffer, BufferUsageHint.StaticDraw);

			Set(Vert, Inds, Cols, UV, BlockInds);
		}

		public void SetTexture(Texture T) {
			Tex = T;
		}

		public void SetTexture(GLTexture T) {
			Tex = T;
		}

		public void SetTexture2(GLTexture T) {
			Tex2 = T;
		}

		public void Use(bool Indice = false, bool Color = true, bool UV = true, bool BlockDta = true) {
			Indices.Active = Indice;
			Colors.Active = Color;
			UVs.Active = UV;
			BlockData.Active = BlockDta;
		}

		public void Set(Vector3[] Vertices, uint[] Indice = null,
			Vector3[] Color = null, Vector2[] UV = null, Vector4[] BlockInds = null) {
			Verts.Set(Vertices);
			Colors.Set(Color == null ? GenCols(Vertices) : Color);
			Indices.Set(Indice == null ? GenInds(Vertices) : Indice);
			UVs.Set(UV == null ? GenUVs(Vertices) : UV);
			BlockData.Set(BlockInds);
		}

		public void Bind() {
			GLProg.Bind();
			GL.BindVertexArray(ID);

			if (Tex != null)
				if (GLProg.SetUniform("TEX", Tex))
					Tex.Bind();
			if (Tex2 != null)
				if (GLProg.SetUniform("TEX2", Tex2))
					Tex2.Bind();

			GLProg.SetUniform("u_modelview", ref ModelMatrix);
			GLProg.SetUniform("u_projection", ref Camera.Projection);
			GLProg.SetUniform("u_view", ref Camera.View);
			GLProg.SetUniform("u_viewrot", ref Camera.Rotation);
			GLProg.SetUniform("Time", (float)Program.SWatch.ElapsedMilliseconds / 1000);
			GLProg.SetUniform("Resolution", Camera.ScreenRes);
			GLProg.SetUniform("Settings", new Vector4(Settings.FXAA ? 1 : 0, Settings.Wireframe ? 1 : 0, 0, 0));

			Indices.Bind();
			Verts.Bind();
			Colors.BindOnAttrib(GLProg.Attrib("Color"));
			UVs.BindOnAttrib(GLProg.Attrib("UV"));
			BlockData.BindOnAttrib(GLProg.Attrib("Data"));
		}

		public void Unbind() {
			Tex.Unbind();
			GL.BindVertexArray(0);
			GLProg.Unbind();
		}

		public void Render(Matrix4 Mat) {
			this.ModelMatrix = Mat;
			Render();
		}

		public void Render() {
			if (Verts.Length == 0)
				return;
			Bind();
			if (Indices.Active)
				GL.DrawElements(PType, Indices.Length, DrawElementsType.UnsignedInt, IntPtr.Zero);
			else
				GL.DrawArrays(PType, 0, Verts.Length);
			Unbind();
		}
	}

}