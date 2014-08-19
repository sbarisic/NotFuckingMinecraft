using LibNoise;
using LibNoise.Filter;
using LibNoise.Primitive;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Threading;

namespace NFM {

	class World : IRenderableT {
		Macroblock[, ,] Chunks;
		byte WorldSize;
		byte WorldHeight;

		public SumFractal WorldNoise;
		public Renderer R;
		VertexArray Skybox;

		public World(Renderer R) {
			this.R = R;
			WorldSize = 10; // 24
			WorldHeight = 16; // 16

			Chunks = new Macroblock[WorldSize, WorldSize, WorldHeight];

			Prog SkyboxShader = new Prog(Shader.FF("Content/Shaders/skybox.vert.glsl", ShaderType.VertexShader),
				Shader.FF("Content/Shaders/skybox.frag.glsl", ShaderType.FragmentShader));

			float UW = 1f / 6;
			float SS = 10;

			Skybox = new VertexArray(SkyboxShader, Matrix4.Identity, new Vector3[] {
				// Top
				new Vector3(-SS, -SS, SS),
				new Vector3(-SS, SS, SS),
				new Vector3(SS, SS, SS),
				new Vector3(SS, -SS, SS),
				// Bottom
				new Vector3(-SS, -SS, -SS),
				new Vector3(SS, -SS, -SS),
				new Vector3(SS, SS, -SS),
				new Vector3(-SS, SS, -SS),
				// Forward
				new Vector3(-SS, -SS, -SS),
				new Vector3(-SS, -SS, SS),
				new Vector3(SS, -SS, SS),
				new Vector3(SS, -SS, -SS),
				// Right
				new Vector3(-SS, SS, -SS),
				new Vector3(-SS, SS, SS),
				new Vector3(-SS, -SS, SS),
				new Vector3(-SS, -SS, -SS),
				// Back
				new Vector3(SS, SS, -SS),
				new Vector3(SS, SS, SS),
				new Vector3(-SS, SS, SS),
				new Vector3(-SS, SS, -SS),
				// Left
				new Vector3(SS, -SS, -SS),
				new Vector3(SS, -SS, SS),
				new Vector3(SS, SS, SS),
				new Vector3(SS, SS, -SS),

			}, UV: new Vector2[] {
				//1
				new Vector2(UW * 1, 1),
				new Vector2(UW * 1, 0),
				new Vector2(UW * 0, 0),
				new Vector2(UW * 0, 1),
				//2
				new Vector2(UW * 2, 0),
				new Vector2(UW * 1, 0),
				new Vector2(UW * 1, 1),
				new Vector2(UW * 2, 1),
				//3
				new Vector2(UW * 3, 1),
				new Vector2(UW * 3, 0),
				new Vector2(UW * 2, 0),
				new Vector2(UW * 2, 1),
				//4	
				new Vector2(UW * 4, 1),
				new Vector2(UW * 4, 0),
				new Vector2(UW * 3, 0),
				new Vector2(UW * 3, 1),
				//5
				new Vector2(UW * 5, 1),
				new Vector2(UW * 5, 0),
				new Vector2(UW * 4, 0),
				new Vector2(UW * 4, 1),
				//6
				new Vector2(UW * 6, 1),
				new Vector2(UW * 6, 0),
				new Vector2(UW * 5, 0),
				new Vector2(UW * 5, 1),
			});

			Skybox.SetTexture(new Texture("Content/Textures/skybox1.png"));
		}

		private void ReloadNbChunks(Vector3 P) {
			ReloadChunks(P + new Vector3(1, 0, 0),
				P + new Vector3(-1, 0, 0),
				P + new Vector3(0, 1, 0),
				P + new Vector3(0, -1, 0),
				P + new Vector3(0, 0, 1),
				P + new Vector3(0, 0, -1));
		}
		private void ReloadChunks(params Vector3[] C) {
			foreach (var Ch in C)
				ReloadChunk(Ch);
		}
		private void ReloadChunk(Vector3 V) {
			ReloadChunk((int)V.X, (int)V.Y, (int)V.Z);
		}
		private void ReloadChunk(float X, float Y, float Z) {
			ReloadChunk((int)X, (int)Y, (int)Z);
		}
		private void ReloadChunk(int X, int Y, int Z) {
			Macroblock C = GetChunk(X, Y, Z);
			if (C != null)
				C.Dirty = true;
		}

		public void Load(int Seed) {
			new Thread(() => {
				WorldNoise = new SumFractal();
				WorldNoise.OctaveCount = 1;
				WorldNoise.Frequency = 0.012f;
				WorldNoise.SpectralExponent = 1.1f;
				WorldNoise.Lacunarity = 1f;
				WorldNoise.Primitive3D = new SimplexPerlin(Seed, NoiseQuality.Standard);

				Chunks = new Macroblock[WorldSize, WorldSize, WorldHeight];
				for (int x = 0; x < WorldSize; x++)
					for (int y = 0; y < WorldSize; y++)
						for (int z = 0; z < WorldHeight; z++) {
							Vector3 P = new Vector3(x, y, z);
							var C = Chunks[(int)P.X, (int)P.Y, (int)P.Z] = new Macroblock(this);
							C.ChunkPos = P;
							C.Load();
							C.Dirty = true;
							ReloadNbChunks(P);
						}
			}).Start();
		}

		public Macroblock GetChunk(int x, int y, int z) {
			if ((x < 0 || y < 0 || z < 0) || (x > (WorldSize - 1) || y > (WorldSize - 1) || z > (WorldHeight - 1)))
				return null;
			return Chunks[x, y, z];
		}

		public BlockID GetBlock(Macroblock Src, int x, int y, int z) {
			var Chnk = GetChunk(x / Macroblock.ChunkSize, y / Macroblock.ChunkSize, z / Macroblock.ChunkSize);
			if (Chnk != null && Chnk != Src)
				return Chnk.GetBlock(x % Macroblock.ChunkSize, y % Macroblock.ChunkSize, z % Macroblock.ChunkSize);
			else
				return BlockDefs.Air;
		}

		public Vector3 GetBlockPos(Vector3 Pos) {
			return new Vector3(((int)Pos.X) / Block.Size, ((int)Pos.Y) / Block.Size, ((int)Pos.Z) / Block.Size);
		}

		public bool SetBlock(Vector3 Pos, BlockID B) {
			return SetBlock((int)Pos.X, (int)Pos.Y, (int)Pos.Z, B);
		}

		public bool SetBlock(int x, int y, int z, BlockID B) {
			var Chnk = GetChunk(x / Macroblock.ChunkSize, y / Macroblock.ChunkSize, z / Macroblock.ChunkSize);
			if (Chnk != null)
				if (Chnk.SetBlock(x % Macroblock.ChunkSize, y % Macroblock.ChunkSize, z % Macroblock.ChunkSize, B)) {
					ReloadNbChunks(new Vector3(x / Macroblock.ChunkSize, y / Macroblock.ChunkSize, z / Macroblock.ChunkSize));
					return true;
				}
			return false;
		}

		public void RenderSkybox() {
			GL.DepthMask(false);
			Skybox.Render();
			GL.DepthMask(true);
		}

		public void Render() {
			for (int x = 0; x < WorldSize; x++)
				for (int y = 0; y < WorldSize; y++)
					for (int z = 0; z < WorldHeight; z++) {
						Macroblock Chnk = Chunks[x, y, z];
						if (Chnk != null) {
							Chnk.ChunkMatrix = Matrix4.CreateTranslation(
								x * Macroblock.ChunkSize * Block.Size,
								y * Macroblock.ChunkSize * Block.Size,
								z * Macroblock.ChunkSize * Block.Size);
							Chnk.Render();
						}
					}


		}

		public void RenderTransparent() {
			for (int x = 0; x < WorldSize; x++)
				for (int y = 0; y < WorldSize; y++)
					for (int z = 0; z < WorldHeight; z++)
						if (Chunks[x, y, z] != null)
							Chunks[x, y, z].RenderTransparent();
		}
	}

}