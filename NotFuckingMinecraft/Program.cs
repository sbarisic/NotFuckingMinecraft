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
using OpenTK.Graphics;

namespace NFM {

	static class Settings {
		public const string Version = "0.1.1.0";
		public const string Caption = "Not Fucking Minecraft " + Settings.Version;

		public static bool Flatlands = false;
		public static bool Colored = false;
		public static bool Wireframe = false;
		public static bool FXAA = true;
	}

	class Program {
		public static Stopwatch SWatch = new Stopwatch();

		public static void Indented(string Msg, Action A) {
			Indent(Msg);
			A();
			Indent(false);
		}

		public static void Indent(string Msg) {
			Debug.WriteLine(Msg + ":");
			Indent(true);
		}

		public static void Indent(bool DoIndent = true) {
			if (DoIndent)
				Debug.Indent();
			else
				Debug.Unindent();
		}

		public static void Msg(object O) {
			string S = O.ToString();
			if (S.Length > 0)
				Debug.WriteLine(S, "Msg");
		}

		public static void Msg(string[] M) {
			for (int i = 0; i < M.Length; i++)
				Msg(M[i]);
		}

		public static void MsgE(Exception E) {
			//MsgBox.Show(E.Message, "Not Fucking Minecraft Exception", MsgBoxButtons.OK);
			Debug.WriteLine(E, "MsgE");
		}

		static void DebugSetup() {
			Console.SetOut(new RWriter(WriterType.Out));
			Console.SetError(new RWriter(WriterType.Error));

			TraceListener L = new ConsoleTraceListener();
			Debug.Listeners.Add(L);

			AppDomain.CurrentDomain.UnhandledException += (S, E) => {
				MsgE((Exception)E.ExceptionObject);
			};
		}


		static void Main(string[] args) {
			DebugSetup();
			Console.WriteLine(Settings.Caption);

			Toolkit T = null;
			Renderer R = null;
			Indented("Renderer", () => {
				ToolkitOptions TO = new ToolkitOptions();
				TO.Backend = PlatformBackend.PreferNative;
				TO.EnableHighResolution = true;
				T = Toolkit.Init(TO);

				GraphicsMode GMode = new GraphicsMode(GraphicsMode.Default.ColorFormat, 24, 8, 0, 0, 2, false);
				R = new Renderer(GMode, 800, 600);
			});

			Indented("OpenGL", () => {
				Msg(string.Format("[{0}] Venor[{1}] GLSL[{2}] Renderer[{3}]",
					GL.GetString(StringName.Version),
					GL.GetString(StringName.Vendor),
					GL.GetString(StringName.ShadingLanguageVersion),
					GL.GetString(StringName.Renderer)));
			});

			Indented("Extensions", () => {
				Msg(GL.GetString(StringName.Extensions).Split(' '));
			});

			Indented("Run", () => {
				SWatch.Start();
				R.Run();
			});

			Indented("Disposal", () => {
				R.Dispose();
				T.Dispose();
			});

			Environment.Exit(0);
		}
	}

	class Renderer : GameWindow {
		public World GameWorld;

		Framebuffer Scr;
		VertexArray ScrQuad;

		public Renderer(GraphicsMode GMode, int W, int H) :
			base(W, H, GMode, Settings.Caption,
			GameWindowFlags.FixedWindow, DisplayDevice.Default, 1, 0, GraphicsContextFlags.Debug) {
			SizeMgr.SizeScale = new Vector2(W, H);
		}

		public Renderer(int W, int H)
			: base(W, H) {
			SizeMgr.SizeScale = new Vector2(W, H);
		}

		protected override void OnLoad(EventArgs e) {
			MakeCurrent();
			this.Icon = Icon.ExtractAssociatedIcon("Content/terminal.ico");

			X = Screen.Wi / 2 - Width / 2;
			Y = Screen.Hi / 2 - Height / 2;
			InitOpenGL();

			Prog ScrProg = new Prog(Shader.FF("Content/Shaders/screen.frag.glsl", ShaderType.FragmentShader),
							Shader.FF("Content/Shaders/screen.vert.glsl", ShaderType.VertexShader));
			ScrQuad = new VertexArray(ScrProg, Matrix4.Identity, new Vector3[] {
				new Vector3(-1, -1, -1),
				new Vector3(1, -1, -1),
				new Vector3(1, 1, -1),
				new Vector3(-1, 1, -1)
			}, null, null, new Vector2[] {
				new Vector2(0, 0),
				new Vector2(1, 0),
				new Vector2(1, 1),
				new Vector2(0, 1),
			});
			ScrQuad.Use(true, false, true, false);

			Scr = new Framebuffer(this, Width, Height);
			ScrQuad.SetTexture(Scr.Color);

			Macroblock.GlobalShaderProg = new Prog(
				Shader.FromFile("Content/Shaders/block.frag.glsl", ShaderType.FragmentShader),
				Shader.FromFile("Content/Shaders/block.vert.glsl", ShaderType.VertexShader));
			Macroblock.GlobalAtlas = new Texture("Content/Textures/block_atlas.png");

			GameWorld = new World(this);
			GameWorld.Load(0);

			Entity.Create<CaptionFPSCounter>(this);
			Entity.Create<Player>(this);
		}

		public void GLViewport() {
			Camera.ScreenRes = SizeMgr.SizeScale;
			GL.Viewport(0, 0, (int)SizeMgr.SizeScale.X, (int)SizeMgr.SizeScale.Y);
		}

		void Clear(float R = 255, float G = 255, float B = 255, float A = 255) {
			GL.ClearColor(R / 255, G / 255, B / 255, A / 255);
			GL.Clear(ClearBufferMask.ColorBufferBit);
			GL.Clear(ClearBufferMask.DepthBufferBit);
			GL.Clear(ClearBufferMask.StencilBufferBit);
		}

		void InitOpenGL() {
			GLViewport();

			GL.ClearColor(70f / 255, 70f / 255, 70f / 255, 1);
			GL.Enable(EnableCap.DepthTest);

			// Back face culling
			GL.Enable(EnableCap.CullFace);
			GL.CullFace(CullFaceMode.Back);

			// Blending
			GL.Enable(EnableCap.Blend);
			GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

			GL.Enable(EnableCap.Texture2D);
		}

		protected override void OnRenderFrame(FrameEventArgs e) {
			GLGarbage.Flush();
			Clear();

			Scr.Bind();
			{
				if (Settings.Wireframe)
					GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
				else
					GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

				Clear(70, 70, 70);
				GameWorld.Render();

				GL.CullFace(CullFaceMode.Front);
				GameWorld.RenderTransparent();
				GL.CullFace(CullFaceMode.Back);
				GameWorld.RenderTransparent();
			}
			Scr.Unbind();

			if (Settings.Wireframe)
				GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

			ScrQuad.Render();

			SwapBuffers();
			base.OnRenderFrame(e);
		}
	}

}