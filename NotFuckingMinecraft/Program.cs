#define FRAMEBUFFER // Enable screen framebffer

using NFM.Entities;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using MsgBox = System.Windows.Forms.MessageBox;
using MsgBoxButtons = System.Windows.Forms.MessageBoxButtons;
using MsgBoxIcon = System.Windows.Forms.MessageBoxIcon;
using System.Diagnostics;

namespace NFM {

	static class Settings {
		public const string Version = "0.1.0.2";

		public static bool Flatlands = false;
		public static bool Colored = false;
		public static bool Wireframe = false;
		public static bool FXAA = true;
	}

	class Program {
		public static Stopwatch SWatch = new Stopwatch();

		static void Msg(object O) {
			Debug.WriteLine(O, "Msg");
		}

		static void MsgE(Exception E) {
			//MsgBox.Show(E.Message, "Not Fucking Minecraft Exception", MsgBoxButtons.OK);
			Debug.WriteLine(E, "MsgE");
		}

		static void DebugSetup() {
			Console.SetOut(new RWriter(WriterType.Out));
			Console.SetError(new RWriter(WriterType.Error));

			TraceListener L = new ConsoleTraceListener();
			Debug.Listeners.Add(L);

			AppDomain.CurrentDomain.UnhandledException += (S, E) => {
				ShowWindow(ConsoleWindow, SW_SHOW);
				MsgE((Exception)E.ExceptionObject);
			};
		}

		[DllImport("kernel32.dll")]
		static extern IntPtr GetConsoleWindow();
		[DllImport("user32.dll")]
		static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
		const int SW_HIDE = 0;
		const int SW_SHOW = 5;
		static IntPtr ConsoleWindow;

		static void Main(string[] args) {
			//Console.Title = "Not Fucking Minecraft Console " + Settings.Version;
			Console.WriteLine("v{0}", Settings.Version);

			DebugSetup();
			ConsoleWindow = GetConsoleWindow();
			ShowWindow(ConsoleWindow, SW_HIDE);

			using (Renderer R = new Renderer()) {
				SWatch.Start();
				R.Run();
			}

			Environment.Exit(0);
		}
	}

	class Renderer : GameWindow {
		public string Caption = "Not Fucking Minecraft " + Settings.Version;

		public World GameWorld;

#if FRAMEBUFFER
		Framebuffer Scr;
		VertexArray ScrQuad;
#endif

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

				GL.CullFace(CullFaceMode.Front);
				GameWorld.RenderTransparent();
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