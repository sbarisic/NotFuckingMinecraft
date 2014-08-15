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

namespace WorldPORTAL {
	class Shader : IDisposable {
		bool Disposed = false;
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
			if (Status == 0) {
				StringBuilder Sb = new StringBuilder();
				Sb.AppendLine("Shader compilation failure in");
				Sb.AppendLine(Pth);
				Sb.AppendLine();
				Sb.AppendLine(GL.GetShaderInfoLog(ID));
				Dispose();
				throw new Exception(Sb.ToString());
			}
		}

		~Shader() {
			Dispose();
		}

		public void Dispose() {
			if (!Disposed) {
				Disposed = true;
				if (Program.Running)
					GL.DeleteShader(ID);
			}
		}

		public static Shader FF(string Pth, ShaderType ST) {
			return FromFile(Pth, ST);
		}

		public static Shader FromFile(string Pth, ShaderType ST) {
			return new Shader(Pth, File.ReadAllText(Pth), ST);
		}
	}

	class Prog : IBindable, IDisposable {
		bool Disposed = false;
		public readonly int ID;
		public readonly Shader[] Shaders;

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
			if (Status == 0) {
				StringBuilder Sb = new StringBuilder();
				Sb.AppendLine("Program linking failure:");
				Sb.AppendLine(GL.GetProgramInfoLog(ID));
				Dispose();
				throw new Exception(Sb.ToString());
			}
		}

		~Prog() {
			Dispose();
		}

		public void Dispose() {
			if (!Disposed) {
				Disposed = true;
				if (Program.Running)
					GL.DeleteProgram(ID);
			}
		}

		public int Attrib(string A) {
			return GL.GetAttribLocation(ID, A);
		}

		public int Uniform(string U) {
			return GL.GetUniformLocation(ID, U);
		}

		public void Bind() {
			GL.UseProgram(ID);
		}

		public void Unbind() {
			GL.UseProgram(0);
		}
	}
}