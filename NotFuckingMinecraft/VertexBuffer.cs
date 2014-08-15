using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using Libraria;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;

//using Vector3 = OpenTK.Vector3;
//using Vector4 = OpenTK.Vector4;

namespace WorldPORTAL {
	class VertexArray : IRenderable, IBindable {
		bool Disposed = false;
		public static Random Rnd = new Random();

		public readonly int ID;
		public readonly int[] Buffs;
		public readonly Prog GLProg;

		public int GetID {
			get {
				return ID;
			}
		}

		// Data
		public Vector3[] VertData;
		public Vector3[] ColData;
		public uint[] IndData;
		public Vector2[] UVData;

		public Vector4[] BlockData;

		IBindable Tex;

		public bool VertDataDirty;
		public bool ColDataDirty;
		public bool IndDataDirty;
		public bool UVDataDirty;
		public bool BlockIndsDirty;

		public bool UseColorData, UseIndicesData, UseUVData, UseBlockIndexData;


		public PrimitiveType PType = PrimitiveType.Quads;
		public Matrix4 ModelMatrix;
		int attrib_data;
		int attrib_position;
		int attrib_color;
		int attrib_uv;

		int uniform_modelview;
		int uniform_view;
		int uniform_projection;
		int uniform_viewrot;
		int uniform_tex;
		int uniform_time;
		int uniform_resolution;
		int uniform_settings;

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
			Vector3[] Verts, uint[] Inds = null, Vector3[] Cols = null, Vector2[] UVs = null, Vector4[] BlockInds = null) {
			ID = GL.GenVertexArray();
			GL.BindVertexArray(ID);

			UseColorData = UseUVData = UseBlockIndexData = true;
			UseIndicesData = false;

			Buffs = new int[5]; // Indices?, Verts, Colors, UVs, BlockInds
			GL.GenBuffers(Buffs.Length, Buffs);

			Set(Verts, Inds, Cols, UVs, BlockInds);

			ModelMatrix = MViewMat;
			GLProg = GLProgram;
			attrib_data = GLProg.Attrib("Data");
			attrib_position = GLProg.Attrib("Position");
			attrib_color = GLProg.Attrib("Color");
			attrib_uv = GLProg.Attrib("UV");

			uniform_modelview = GLProg.Uniform("u_modelview");
			uniform_projection = GLProg.Uniform("u_projection");
			uniform_view = GLProg.Uniform("u_view");
			uniform_viewrot = GLProg.Uniform("u_viewrot");
			uniform_tex = GLProg.Uniform("TEX");
			uniform_time = GLProg.Uniform("Time");
			uniform_resolution = GLProg.Uniform("Resolution");
			uniform_settings = GLProg.Uniform("Settings");
		}

		public void SetIndices(uint[] Inds) {
			IndData = Inds;
			IndDataDirty = true;
		}

		public void SetVertices(Vector3[] Verts) {
			VertData = Verts;
			VertDataDirty = true;
		}

		public void SetColors(Vector3[] Colors) {
			ColData = Colors;
			ColDataDirty = true;
		}

		public void SetUVs(Vector2[] UVs) {
			UVData = UVs;
			UVDataDirty = true;
		}

		public void SetTexture(Texture T) {
			Tex = T;
		}

		public void SetTexture(GLTexture T) {
			Tex = T;
		}

		public void SetBlockInds(Vector4[] BlockInds) {
			BlockData = BlockInds;
			BlockIndsDirty = BlockInds != null;
		}

		public void Use(bool Indices = false, bool Colors = true, bool UVs = true, bool BlockData = true) {
			UseBlockIndexData = BlockData;
			UseColorData = Colors;
			UseIndicesData = Indices;
			UseUVData = UVs;
		}

		public void Set(Vector3[] Vertices, uint[] Indices = null,
			Vector3[] Colors = null, Vector2[] UVs = null, Vector4[] BlockInds = null) {

			SetVertices(Vertices);

			if (Indices == null)
				SetIndices(GenInds(Vertices));
			else
				SetIndices(Indices);

			if (Colors == null)
				SetColors(GenCols(Vertices));
			else
				SetColors(Colors);

			if (UVs == null)
				SetUVs(GenUVs(Vertices));
			else
				SetUVs(UVs);

			SetBlockInds(BlockInds);
		}

		public void Bind() {
			GLProg.Bind();
			GL.BindVertexArray(ID);

			if (Tex != null) {
				Tex.Bind();
				if (uniform_tex != -1)
					GL.Uniform1(uniform_tex, Tex.GetID);
			}

			if (uniform_modelview != -1)
				GL.UniformMatrix4(uniform_modelview, false, ref ModelMatrix);
			if (uniform_projection != -1)
				GL.UniformMatrix4(uniform_projection, false, ref Camera.Projection);
			if (uniform_view != -1)
				GL.UniformMatrix4(uniform_view, false, ref Camera.View);
			if (uniform_viewrot != -1)
				GL.UniformMatrix4(uniform_viewrot, false, ref Camera.Rotation);
			if (uniform_time != -1)
				GL.Uniform1(uniform_time, (float)Program.SWatch.ElapsedMilliseconds / 1000);
			if (uniform_resolution != -1)
				GL.Uniform2(uniform_resolution, Camera.ScreenRes);
			if (uniform_settings != -1)
				GL.Uniform4(uniform_settings, new Vector4(Settings.FXAA ? 1 : 0, 0, 0, 0));

			if (IndDataDirty && UseIndicesData) {
				IndDataDirty = false;
				GL.BindBuffer(BufferTarget.ElementArrayBuffer, Buffs[0]);
				GL.BufferData<uint>(BufferTarget.ElementArrayBuffer,
					new IntPtr(IndData.Length * sizeof(uint)), IndData, BufferUsageHint.StaticDraw);
			}

			if (VertDataDirty) {
				VertDataDirty = false;
				if (attrib_position != -1) {
					GL.BindBuffer(BufferTarget.ArrayBuffer, Buffs[1]);
					GL.BufferData<Vector3>(BufferTarget.ArrayBuffer,
						new IntPtr(VertData.Length * Vector3.SizeInBytes), VertData, BufferUsageHint.StaticDraw);
					GL.EnableVertexAttribArray(attrib_position);
					GL.VertexAttribPointer(attrib_position, 3, VertexAttribPointerType.Float, false, 0, 0);
				}
			}

			if (ColDataDirty && UseColorData) {
				ColDataDirty = false;
				if (attrib_color != -1) {
					if (ColData.Length != VertData.Length)
						throw new Exception("Block data and color data are not same length");
					GL.BindBuffer(BufferTarget.ArrayBuffer, Buffs[2]);
					GL.BufferData<Vector3>(BufferTarget.ArrayBuffer,
						(IntPtr)(ColData.Length * Vector3.SizeInBytes), ColData, BufferUsageHint.StaticDraw);
					GL.EnableVertexAttribArray(attrib_color);
					GL.VertexAttribPointer(attrib_color, 3, VertexAttribPointerType.Float, true, 0, 0);
				}
			}

			if (UVDataDirty && UseUVData) {
				UVDataDirty = false;
				if (attrib_uv != -1) {
					if (UVData.Length != VertData.Length)
						throw new Exception("Block data and uv data are not same length");
					GL.BindBuffer(BufferTarget.ArrayBuffer, Buffs[3]);
					GL.BufferData<Vector2>(BufferTarget.ArrayBuffer,
						new IntPtr(UVData.Length * Vector2.SizeInBytes), UVData, BufferUsageHint.StaticDraw);
					GL.EnableVertexAttribArray(attrib_uv);
					GL.VertexAttribPointer(attrib_uv, 2, VertexAttribPointerType.Float, false, 0, 0);
				}
			}

			if (BlockIndsDirty && UseBlockIndexData) {
				BlockIndsDirty = false;
				if (attrib_data != -1) {
					if (BlockData.Length != VertData.Length)
						throw new Exception("Block data and vert data are not same length");
					GL.BindBuffer(BufferTarget.ArrayBuffer, Buffs[4]);
					GL.BufferData<Vector4>(BufferTarget.ArrayBuffer,
						new IntPtr(BlockData.Length * Vector4.SizeInBytes), BlockData, BufferUsageHint.StaticDraw);
					GL.EnableVertexAttribArray(attrib_data);
					GL.VertexAttribPointer(attrib_data, 4, VertexAttribPointerType.Float, false, 0, 0);
				}
			}
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
			if (VertData.Length == 0)
				return;
			Bind();
			if (UseIndicesData)
				GL.DrawElements(PType, IndData.Length, DrawElementsType.UnsignedInt, IntPtr.Zero);
			else
				GL.DrawArrays(PType, 0, VertData.Length);
			Unbind();
		}

		~VertexArray() {
			Dispose();
		}

		public void Dispose() {
			if (!Disposed) {
				Disposed = true;
				if (Program.Running) {
					GL.DeleteVertexArray(ID);
					GL.DeleteBuffers(Buffs.Length, Buffs);
				}
			}
		}
	}
}