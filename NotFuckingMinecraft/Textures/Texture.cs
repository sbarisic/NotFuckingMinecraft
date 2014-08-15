using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using System.Runtime.InteropServices;
using PF = System.Drawing.Imaging.PixelFormat;
using ILM = System.Drawing.Imaging.ImageLockMode;

using OpenTK;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;
using OpenTK.Platform;
using OpenTK.Platform.Windows;

namespace WorldPORTAL {
	public class GLTexture : IDisposable, IBindable {
		bool Disposed;

		public int ID;
		public TextureTarget TexT;

		public int GetID {
			get {
				return ID;
			}
		}

		public GLTexture(TextureTarget TT) {
			ID = GL.GenTexture();
			TexT = TT;
		}

		public void Bind() {
			GL.ActiveTexture(TextureUnit.Texture0 + ID);
			GL.BindTexture(TexT, ID);
		}

		public void Unbind() {
			GL.BindTexture(TexT, 0);
		}

		public void Image2D(int Lvl, PixelInternalFormat PIF, int W, int H, PixelType PT) {
			Image2D(Lvl, PIF, W, H, 0, PT);
		}
		public void Image2D(int Lvl, PixelInternalFormat PIF, int W, int H, PixelFormat PF, PixelType PT) {
			Image2D(Lvl, PIF, W, H, PF, PT, IntPtr.Zero);
		}
		public void Image2D(int Lvl, PixelInternalFormat PIF, int W, int H, PixelFormat PF, PixelType PT, IntPtr Data) {
			GL.TexImage2D(TexT, Lvl, PIF, W, H, 0, PF, PT, Data);
		}
		public void Image2D(int Lvl, PixelInternalFormat PIF, int W, int H, PixelFormat PF, PixelType PT, byte[] Data) {
			GL.TexImage2D(TexT, Lvl, PIF, W, H, 0, PF, PT, Data);
		}

		public void TexParameter(TextureParameterName TPN, TextureMinFilter Param) {
			TexParameter(TPN, (int)Param);
		}
		public void TexParameter(TextureParameterName TPN, TextureMagFilter Param) {
			TexParameter(TPN, (int)Param);
		}
		public void TexParameter(TextureParameterName TPN, TextureWrapMode Param) {
			TexParameter(TPN, (int)Param);
		}
		public void TexParameter(TextureParameterName TPN, int Param) {
			GL.TexParameter(TexT, TPN, Param);
		}

		~GLTexture() {
			Dispose();
		}

		public void Dispose() {
			if (!Disposed) {
				Disposed = true;
				if (Program.Running)
					GL.DeleteTexture(ID);
			}
		}
	}

	class Texture : IBindable, IDisposable {
		bool Disposed = false;

		//internal int ID;
		public GLTexture TEX;
		public int GetID {
			get {
				return TEX.ID;
			}
		}
		internal bool Dirty;

		public int W, H;
		public byte[] Data;
		public bool Transparent;

		public TextureTarget TextureType = TextureTarget.Texture2D,
			TexImageType = TextureTarget.Texture2D;

		internal virtual void Init(int W, int H) {
			if (TEX == null) {
				TEX = new GLTexture(TextureType);
				TEX.Bind();

				TEX.TexParameter(TextureParameterName.TextureMagFilter, TextureMagFilter.Nearest);
				TEX.TexParameter(TextureParameterName.TextureMinFilter, TextureMinFilter.Nearest);
				TEX.TexParameter(TextureParameterName.TextureWrapS, TextureWrapMode.MirroredRepeat);
				TEX.TexParameter(TextureParameterName.TextureWrapT, TextureWrapMode.MirroredRepeat);
				TEX.TexParameter(TextureParameterName.TextureWrapR, TextureWrapMode.MirroredRepeat);
			}

			this.W = W;
			this.H = H;
			Data = new byte[W * H * 4];
		}

		public Texture(Framebuffer FB) {
			TEX = FB.TEX;
			Init(FB.W, FB.H);
			Dirty = true;
		}

		public Texture(GLTexture T, int W, int H) {
			TEX = T;
			Init(W, H);
			Dirty = true;
		}

		public Texture(int W, int H) {
			Init(W, H);
			Dirty = true;
		}

		public Texture(string Filename) {
			if (!File.Exists(Filename))
				throw new Exception("Texture file " + Filename + " not found!");

			Bitmap Bmp = new Bitmap(Filename);
			Init(Bmp.Width, Bmp.Height);

			var BLock = Bmp.LockBits(new Rectangle(0, 0, Bmp.Width, Bmp.Height), ILM.ReadOnly, PF.Format32bppArgb);
			Marshal.Copy(BLock.Scan0, Data, 0, W * H * 4);
			Bmp.UnlockBits(BLock);
			Bmp.Dispose();

			Dirty = true;
		}

		public virtual byte[] Get() {
			return Data;
		}

		public virtual Color Get(int X, int Y) {
			int i = (Y * W + X) * 4;
			return Color.FromArgb(Data[i + 3], Data[i], Data[i + 1], Data[i + 2]);
		}

		public virtual void Set(byte[] Data) {
			if (Data.Length != this.Data.Length)
				throw new Exception("Data length either too long or too short");

			lock (this.Data) {
				this.Data = Data;
				Dirty = true;
			}
		}

		public virtual void Set(int X, int Y, byte R = 255, byte G = 255, byte B = 255, byte A = 255) {
			int i = (Y * W + X) * 4;
			Data[i] = R;
			Data[i + 1] = G;
			Data[i + 2] = B;
			Data[i + 3] = A;
			Dirty = true;
		}

		public virtual void Bind() {
			TEX.Bind();
			if (Dirty) {
				Dirty = false;
				lock (Data)
					TEX.Image2D(0, PixelInternalFormat.Rgba, W, H, PixelFormat.Bgra, PixelType.UnsignedByte, Data);
			}
		}

		public virtual void Unbind() {
			TEX.Unbind();
		}

		~Texture() {
			Dispose();
		}

		public void Dispose() {
			if (!Disposed) {
				Disposed = true;
				if (Program.Running)
					TEX.Dispose();
			}
		}
	}
}