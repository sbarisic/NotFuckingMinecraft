using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using System.Threading;
using Libraria;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;

namespace WorldPORTAL {
	class Framebuffer : IBindable, IDisposable {
		bool Disposed;

		public int ID;
		public int RB; // Render buffers (depth and stencil)
		public int W, H;
		public GLTexture TEX;

		public int GetID {
			get {
				return ID;
			}
		}

		Renderer R;

		~Framebuffer() {
			Dispose();
		}

		public void Dispose() {
			if (!Disposed) {
				Disposed = true;
				if (Program.Running) {

				}
			}
		}

		public Framebuffer(Renderer R, int W, int H) {
			ID = GL.GenFramebuffer();
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, ID);

			this.R = R;
			this.W = W;
			this.H = H;

			TEX = new GLTexture(TextureTarget.Texture2D);
			TEX.Bind();
			TEX.Image2D(0, PixelInternalFormat.Rgb, W, H, PixelFormat.Rgb, PixelType.UnsignedByte);
			TEX.TexParameter(TextureParameterName.TextureMagFilter, TextureMagFilter.Nearest);
			TEX.TexParameter(TextureParameterName.TextureMinFilter, TextureMinFilter.Nearest);

			RB = GL.GenRenderbuffer();
			GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, RB);
			GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent, W, H);
			GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment,
				RenderbufferTarget.Renderbuffer, RB);
		
			GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TEX.ID, 0);

			DrawBuffersEnum[] DrawBuffs = new DrawBuffersEnum[] { 
				DrawBuffersEnum.ColorAttachment0
			};
			GL.DrawBuffers(DrawBuffs.Length, DrawBuffs);

			Check();
			Unbind();
		}

		public void Check() {
			FramebufferErrorCode FEC = FramebufferErrorCode.FramebufferComplete;
			if ((FEC = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer)) != FramebufferErrorCode.FramebufferComplete)
				throw new Exception(FEC.ToString());
		}

		public void Bind() {
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, ID);
			GL.Viewport(0, 0, W, H);
		}

		public void Unbind() {
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
			GL.Viewport(R.ClientRectangle.X, R.ClientRectangle.Y, R.ClientRectangle.Width, R.ClientRectangle.Height);
		}
	}
}