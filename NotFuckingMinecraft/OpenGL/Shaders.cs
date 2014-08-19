using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NFM {

	class Shader {
		public readonly int ID;
		public string Pth;

		public Shader(string Pth, string Src, ShaderType ST) {
			this.Pth = Pth;
			ID = GL.CreateShader(ST);
			if (ID == 0)
				throw new Exception("GL.CreateShader failed");

			GL.ShaderSource(ID, Src);
			GL.CompileShader(ID);

			int Status;
			GL.GetShader(ID, ShaderParameter.CompileStatus, out Status);
			string InfoLog = GL.GetShaderInfoLog(ID);

			if (Status == 0) {
				StringBuilder Sb = new StringBuilder();
				Sb.AppendLine("Shader compilation failure in");
				Sb.AppendLine(Pth);
				Sb.AppendLine();
				Sb.AppendLine(GL.GetShaderInfoLog(ID));
				throw new Exception(Sb.ToString());
			}

			Program.Msg(InfoLog);
		}

		~Shader() {
			GLGarbage.Enqueue(() => {
				GL.DeleteShader(ID);
			});
		}

		public static Shader FF(string Pth, ShaderType ST) {
			return FromFile(Pth, ST);
		}

		public static Shader FromFile(string Pth, ShaderType ST) {
			return new Shader(Pth, File.ReadAllText(Pth), ST);
		}
	}

	class Prog : IBindable {
		public readonly int ID;
		public readonly Shader[] Shaders;

		Dictionary<string, int> Uniforms = new Dictionary<string, int>();
		Dictionary<string, int> Attributes = new Dictionary<string, int>();

		public int GetID {
			get {
				return ID;
			}
		}

		public Prog(params Shader[] Shaders) {
			if (Shaders.Length < 1)
				throw new Exception("No shaders to attach");

			ID = GL.CreateProgram();
			if (ID == 0)
				throw new Exception("GL.CreateProgram failed");

			this.Shaders = new Shader[Shaders.Length];
			for (int i = 0; i < Shaders.Length; i++) {
				GL.AttachShader(ID, Shaders[i].ID);
				this.Shaders[i] = Shaders[i];
			}

			GL.LinkProgram(ID);

			foreach (var Sh in Shaders)
				GL.DetachShader(ID, Sh.ID);

			int Status;
			GL.GetProgram(ID, GetProgramParameterName.LinkStatus, out Status);
			string PInfo = GL.GetProgramInfoLog(ID);

			if (Status == 0) {
				StringBuilder Sb = new StringBuilder();
				Sb.AppendLine("Program linking failure:");
				Sb.AppendLine(PInfo);
				throw new Exception(Sb.ToString());
			}

			Program.Msg(PInfo);
		}

		~Prog() {
			GLGarbage.Enqueue(() => {
				GL.DeleteProgram(ID);
			});
		}

		public int Attrib(string A) {
			if (Attributes.ContainsKey(A))
				return Attributes[A];
			Attributes.Add(A, GL.GetAttribLocation(ID, A));
			return Attrib(A);
		}

		public int Uniform(string U) {
			if (Uniforms.ContainsKey(U))
				return Uniforms[U];
			Uniforms.Add(U, GL.GetUniformLocation(ID, U));
			return Uniform(U);
		}

		public bool SetUniform(string K, float V) {
			int U = Uniform(K);
			if (U == -1)
				return false;
			GL.Uniform1(U, V);
			return true;
		}

		public bool SetUniform(string K, int V) {
			int U = Uniform(K);
			if (U == -1)
				return false;
			GL.Uniform1(U, V);
			return true;
		}

		public bool SetUniform(string K, IBindable V) {
			int U = Uniform(K);
			if (U == -1)
				return false;
			V.Bind();
			GL.Uniform1(U, V.GetID);
			return true;
		}

		public bool SetUniform(string K, Vector2 V) {
			int U = Uniform(K);
			if (U == -1)
				return false;
			GL.Uniform2(U, V);
			return true;
		}

		public bool SetUniform(string K, Vector4 V) {
			int U = Uniform(K);
			if (U == -1)
				return false;
			GL.Uniform4(U, V);
			return true;
		}

		public bool SetUniform(string K, ref Matrix4 V, bool Transpose = false) {
			int U = Uniform(K);
			if (U == -1)
				return false;
			GL.UniformMatrix4(U, Transpose, ref V);
			return true;
		}

		public void Bind() {
			GL.UseProgram(ID);
		}

		public void Unbind() {
			GL.UseProgram(0);
		}
	}

}