using OpenTK.Graphics.OpenGL;
using System;

namespace NFM {

	class Framebuffer : IBindable {
		public int ID;
		//public int Buffers; // Render buffer
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
				/*var B = new int[] { Buffers };
				GL.DeleteRenderbuffers(B.Length, B);*/
				GL.DeleteFramebuffer(ID);
			});
		}

		public Framebuffer(Renderer R, int W, int H) {
			ID = GL.GenFramebuffer();
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, ID);

			this.R = R;
			this.W = W;
			this.H = H;

			Color = new GLTexture(TextureTarget.Texture2D);
			Color.Bind(false);
			Color.Image2D(0, PixelInternalFormat.Rgb, W, H, PixelFormat.Rgb, PixelType.UnsignedByte);
			Color.TexParameter(TextureParameterName.TextureMinFilter, TextureMinFilter.Nearest);
			Color.TexParameter(TextureParameterName.TextureMagFilter, TextureMagFilter.Nearest);
			GL.FramebufferTexture2D(FramebufferTarget.Framebuffer,
				FramebufferAttachment.ColorAttachment0, Color.Target, Color.GetID, 0);

			DepthStencil = new GLTexture(TextureTarget.Texture2D);
			DepthStencil.Bind(false);
			DepthStencil.Image2D(0, PixelInternalFormat.Depth24Stencil8, W, H, PixelFormat.DepthStencil, PixelType.UnsignedInt248);
			DepthStencil.TexParameter(TextureParameterName.TextureMinFilter, TextureMinFilter.Nearest);
			DepthStencil.TexParameter(TextureParameterName.TextureMagFilter, TextureMagFilter.Nearest);
			GL.FramebufferTexture2D(FramebufferTarget.Framebuffer,
				FramebufferAttachment.DepthStencilAttachment, DepthStencil.Target, DepthStencil.GetID, 0);

			/*Buffers = GL.GenRenderbuffer();
			GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, Buffers);
			GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Depth24Stencil8, W, H);
			GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment,
				RenderbufferTarget.Renderbuffer, Buffers);

			DrawBuffersEnum[] DrawBuffs = new DrawBuffersEnum[] { 
				DrawBuffersEnum.ColorAttachment0,
			};
			GL.DrawBuffers(DrawBuffs.Length, DrawBuffs);*/

			Check();
			Unbind();
		}

		public void Check() {
			FramebufferErrorCode FEC;
			if ((FEC = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer)) != FramebufferErrorCode.FramebufferComplete)
				throw new Exception(FEC.ToString());

			ErrorCode EC;
			if ((EC = GL.GetError()) != ErrorCode.NoError)
				throw new Exception(EC.ToString());
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