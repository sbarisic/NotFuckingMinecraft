using OpenTK.Graphics.OpenGL;
using System;

namespace NFM {

	class Framebuffer : IBindable {
		public int ID;
		public int Buffers; // Render buffer
		public int W, H;
		public GLTexture TEX;

		public int GetID {
			get {
				return ID;
			}
		}

		Renderer R;

		~Framebuffer() {
			GLGarbage.Enqueue(() => {
				var B = new int[] { Buffers };
				GL.DeleteRenderbuffers(B.Length, B);
				GL.DeleteFramebuffer(ID);
			});
		}

		public Framebuffer(Renderer R, int W, int H) {
			ID = GL.GenFramebuffer();
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, ID);

			this.R = R;
			this.W = W;
			this.H = H;

			TEX = new GLTexture(TextureTarget.Texture2D);
			TEX.Bind();	
			TEX.Image2D(0, PixelInternalFormat.Rgba, W, H, PixelFormat.Rgba, PixelType.UnsignedByte);
			TEX.TexParameter(TextureParameterName.TextureMagFilter, TextureMagFilter.Linear);
			TEX.TexParameter(TextureParameterName.TextureMinFilter, TextureMinFilter.Linear);
			GL.FramebufferTexture2D(FramebufferTarget.Framebuffer,
				FramebufferAttachment.ColorAttachment0, TEX.TexT, TEX.GetID, 0);

			/*DTEX = new GLTexture(TextureTarget.Texture2D);
			DTEX.Bind();
			DTEX.TexParameter(TextureParameterName.TextureWrapS, TextureWrapMode.ClampToEdge);
			DTEX.TexParameter(TextureParameterName.TextureWrapT, TextureWrapMode.ClampToEdge);
			DTEX.TexParameter(TextureParameterName.TextureMagFilter, TextureMagFilter.Nearest);
			DTEX.TexParameter(TextureParameterName.TextureMinFilter, TextureMinFilter.Nearest);
			DTEX.TexParameter(TextureParameterName.DepthTextureMode, InternalFormat.Intensity);
			DTEX.TexParameter(TextureParameterName.TextureCompareMode, TextureCompareMode.CompareRToTexture);
			DTEX.TexParameter(TextureParameterName.TextureCompareFunc, DepthFunction.Lequal);
			DTEX.Image2D(0, PixelInternalFormat.DepthComponent32, W, H, PixelFormat.DepthComponent, PixelType.UnsignedInt);*/

			Buffers = GL.GenRenderbuffer();
			GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, Buffers);
			GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Depth24Stencil8, W, H);
			GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment,
				RenderbufferTarget.Renderbuffer, Buffers);
			
			/*GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.StencilAttachment,
				RenderbufferTarget.Renderbuffer, Buffers);
			//GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TEX.GetID, 0);
			//GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment1, DTEX.GetID, 0);//*/

			DrawBuffersEnum[] DrawBuffs = new DrawBuffersEnum[] { 
				DrawBuffersEnum.ColorAttachment0,
				//DrawBuffersEnum.None,
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