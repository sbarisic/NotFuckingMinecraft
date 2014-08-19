using OpenTK.Graphics.OpenGL;
using System;

using GLf = OpenTK.Graphics.OpenGL.GL.Ext;

namespace NFM {

	class Framebuffer : IBindable {
		public int ID;
		public int W, H;
		public GLTexture Color, DepthStencil;

		public int GetID {
			get {
				return ID;
			}
		}

		Renderer R;

		~Framebuffer() {
			GLGarbage.Enqueue(() => {
				GLf.DeleteFramebuffer(ID);
			});
		}

		public Framebuffer(Renderer R, int W, int H) {
			this.R = R;
			this.W = W;
			this.H = H;

			Color = new GLTexture(TextureTarget.Texture2D);
			Color.Bind(false);
			Color.Image2D(0, PixelInternalFormat.Rgba8, W, H, PixelFormat.Rgba, PixelType.UnsignedByte);
			Color.TexParameter(TextureParameterName.TextureMinFilter, TextureMinFilter.Nearest);
			Color.TexParameter(TextureParameterName.TextureMagFilter, TextureMagFilter.Nearest);

			DepthStencil = new GLTexture(TextureTarget.Texture2D);
			DepthStencil.Bind(false);
			DepthStencil.Image2D(0, PixelInternalFormat.DepthComponent32, W, H,
				PixelFormat.DepthComponent, PixelType.UnsignedInt);
			DepthStencil.TexParameter(TextureParameterName.TextureMinFilter, TextureMinFilter.Nearest);
			DepthStencil.TexParameter(TextureParameterName.TextureMagFilter, TextureMagFilter.Nearest);

			ID = GLf.GenFramebuffer();
			GLf.BindFramebuffer(FramebufferTarget.Framebuffer, ID);
			GLf.FramebufferTexture2D(FramebufferTarget.Framebuffer,
				FramebufferAttachment.ColorAttachment0, Color.Target, Color.GetID, 0);
			GLf.FramebufferTexture2D(FramebufferTarget.Framebuffer,
				FramebufferAttachment.DepthAttachment, DepthStencil.Target, DepthStencil.GetID, 0);

			Check();
			Unbind();
		}

		public void Check() {
			FramebufferErrorCode FEC;
			if ((FEC = GLf.CheckFramebufferStatus(FramebufferTarget.Framebuffer)) != FramebufferErrorCode.FramebufferComplete)
				throw new Exception(FEC.ToString());

			ErrorCode EC;
			if ((EC = GL.GetError()) != ErrorCode.NoError)
				throw new Exception(EC.ToString());
		}

		public void Bind() {
			GL.Viewport(0, 0, W, H);
			GLf.BindFramebuffer(FramebufferTarget.Framebuffer, ID);
		}

		public void Unbind() {
			R.GLViewport();
			GLf.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
		}
	}

}