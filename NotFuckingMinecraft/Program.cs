#define FRAMEBUFFER // Enable screen framebffer

using NFM.Entities;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using MsgBox = System.Windows.Forms.MessageBox;
using MsgBoxButtons = System.Windows.Forms.MessageBoxButtons;
using MsgBoxIcon = System.Windows.Forms.MessageBoxIcon;

namespace NFM {

	static class Settings {
		public static bool Flatlands = false;
		public static bool Colored = false;
		public static bool Wireframe = false;
		public static bool FXAA = true;
	}

	class Program {
		public static bool Running;
		public static Stopwatch SWatch = new Stopwatch();

		static void Msg(object O) {
			MsgBox.Show(O.ToString(), "Not Fucking Minecraft", MsgBoxButtons.OK);
		}

		static void CrashDump(Exception E) {
			File.WriteAllText("CRASHDUMP.txt", string.Format("This fucking exception was not handled\n\n{0}", E));
			MsgBox.Show("CRASHDUMP.txt has been created!", "MOTHERFUCKING SHIT", MsgBoxButtons.OK, MsgBoxIcon.Error);
		}

		static void Main(string[] args) {
			Running = true;

			// Enable this crash handler when releasing, else it doesn't dump any crash info
			//*
			bool Crashed = false;
			AppDomain.CurrentDomain.UnhandledException += (S, E) => {
				if (Crashed)
					return;
				Running = !(Crashed = true);
				CrashDump((Exception)E.ExceptionObject);
				Environment.Exit(1);
			};//*/

			Renderer R = new Renderer();
			SWatch.Start();
			R.Run();

			Environment.Exit(0);
		}
	}

	class Renderer : GameWindow {
		public string Caption = "Not Fucking Minecraft";

		public World GameWorld;

#if FRAMEBUFFER
		Framebuffer Scr;
		VertexArray ScrQuad;
#endif

		protected override void OnClosing(System.ComponentModel.CancelEventArgs e) {
			Program.Running = false;
			base.OnClosing(e);
		}

		protected override void OnResize(EventArgs e) {
			Camera.ScreenRes = SizeMgr.SizeScale = new Vector2(ClientRectangle.Width, ClientRectangle.Height);
			GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);
			base.OnResize(e);
		}

		protected override void OnLoad(EventArgs e) {
			base.OnLoad(e);

			ToolkitOptions.Default.Backend = PlatformBackend.PreferNative;

			VSync = VSyncMode.On;
			WindowBorder = OpenTK.WindowBorder.Fixed;
			Title = Caption;

			this.Icon = Icon.ExtractAssociatedIcon("Content/terminal.ico");

			Width = 800;
			Height = 600;
			X = Screen.Wi / 2 - Width / 2;
			Y = Screen.Hi / 2 - Height / 2;
			OnResize(null);

			GL.ClearColor(70f / 255, 70f / 255, 70f / 255, 1);
			GL.Enable(EnableCap.DepthTest);
			GL.Enable(EnableCap.Texture2D);

			// Back face culling
			GL.Enable(EnableCap.CullFace);
			GL.CullFace(CullFaceMode.Back);

			// Blending
			GL.Enable(EnableCap.Blend);
			GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

#if FRAMEBUFFER
			Scr = new Framebuffer(this, Width, Height);
			Prog ScrProg = new Prog(Shader.FF("Content/Shaders/screen.frag.glsl", ShaderType.FragmentShader),
				Shader.FF("Content/Shaders/screen.vert.glsl", ShaderType.VertexShader));
			ScrQuad = new VertexArray(ScrProg, Matrix4.Identity, new Vector3[] {
				new Vector3(-1, -1, 0),
				new Vector3(1, -1, 0),
				new Vector3(1, 1, 0),
				new Vector3(-1, 1, 0)
			}, null, null, new Vector2[] {
				new Vector2(0, 0),
				new Vector2(1, 0),
				new Vector2(1, 1),
				new Vector2(0, 1),
			});
			ScrQuad.Use(true, false, true, false);
			ScrQuad.SetTexture(Scr.TEX);
#endif

			Macroblock.GlobalShaderProg = new Prog(
				Shader.FromFile("Content/Shaders/block.frag.glsl", ShaderType.FragmentShader),
				Shader.FromFile("Content/Shaders/block.vert.glsl", ShaderType.VertexShader));
			Macroblock.GlobalAtlas = new Texture("Content/Textures/block_atlas.png");

			GameWorld = new World(this);
			GameWorld.Load(0);

			Entity.Create<CaptionFPSCounter>(this);
			Entity.Create<Player>(this);
		}

		protected override void OnRenderFrame(FrameEventArgs e) {
			GLGarbage.Flush();

			ClearBufferMask CBM = ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit;
			GL.Clear(CBM);

			// TODO: Fix these gay framebuffers, they obviously don't work on nVidia cards
#if FRAMEBUFFER
			Scr.Bind();
			{
#endif

				if (Settings.Wireframe)
					GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
				else
					GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

				GL.Clear(CBM);
				GameWorld.Render();

				/*GL.CullFace(CullFaceMode.Front);
				GameWorld.RenderTransparent();*/
				GL.CullFace(CullFaceMode.Back);
				GameWorld.RenderTransparent();
#if FRAMEBUFFER
			}
			Scr.Unbind();

			if (Settings.Wireframe)
				GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
			ScrQuad.Render();
#endif
			SwapBuffers();
			base.OnRenderFrame(e);


		}
	}

}