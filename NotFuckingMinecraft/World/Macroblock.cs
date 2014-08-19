using OpenTK;
using System;
using System.Collections.Generic;
using System.Threading;

//using Vector4 = OpenTK.Vector4;

namespace NFM {

	class Macroblock : IRenderableT {
		byte Size;
		BlockID[, ,] Blocks;

		VertexArray VBO, VBOt;

		Prog ShaderProg;
		Texture Atlas;

		Vector3 Clr;
		List<Vector3> Mesh = new List<Vector3>();
		List<Vector2> UVs = new List<Vector2>();
		List<Vector3> Clrs = new List<Vector3>();
		List<Vector4> BlockData = new List<Vector4>();

		List<Vector3> MeshT = new List<Vector3>();
		List<Vector2> UVsT = new List<Vector2>();
		List<Vector3> ClrsT = new List<Vector3>();
		List<Vector4> BlockDataT = new List<Vector4>();

		World GW;

		public static Texture GlobalAtlas;
		public static Prog GlobalShaderProg;
		public static byte ChunkSize = 16;

		public Matrix4 ChunkMatrix;
		public Vector3 ChunkPos;

		public Macroblock(World GW) {
			this.Size = ChunkSize;
			this.ShaderProg = GlobalShaderProg;
			this.Atlas = GlobalAtlas;
			this.GW = GW;

			Clr = new Vector3((float)RND.Next(0, 100) / 100, (float)RND.Next(0, 100) / 100, (float)RND.Next(0, 100) / 100);

			ChunkMatrix = Matrix4.Identity;
		}

		public static Random RND = new Random();

		public int WorldX(int X) {
			return (int)ChunkPos.X * ChunkSize + X;
		}

		public int WorldY(int Y) {
			return (int)ChunkPos.Y * ChunkSize + Y;
		}

		public int WorldZ(int Z) {
			return (int)ChunkPos.Z * ChunkSize + Z;
		}

		public Vector3 WorldPos(Vector3 LocalPos) {
			return new Vector3(WorldX((int)LocalPos.X), WorldY((int)LocalPos.Y), WorldZ((int)LocalPos.Z));
		}

		public void Load() {
			Blocks = new BlockID[Size, Size, Size];

			int EX = WorldX(Size), EY = WorldY(Size), EZ = WorldZ(Size);
			//HashSet<Thread> Threads = new HashSet<Thread>();

			for (int x = WorldX(0), xx = 0; x < EX; x++, xx++) {
				/*Thread T = new Thread((IN) => {
					int x_ = (int)((Vector2)IN).X;
					int xx_ = (int)((Vector2)IN).Y;*/
				for (int y = WorldY(0), yy = 0; y < EY; y++, yy++)
					for (int z = WorldZ(0), zz = 0; z < EZ; z++, zz++) {

						double SN = 1;
						if (!Settings.Flatlands && (z < ChunkSize * 7) && (z > ChunkSize * 3))
							SN = GW.WorldNoise.GetValue(x, y, z) * 20;
						double Noise = SN + ChunkSize * 5;

						if (z == 0)
							Blocks[xx, yy, zz] = BlockDefs.Border;
						else if (z < Noise) // -(P.Z * 2.5 - 32)
							if (z < 5 * ChunkSize)
								Blocks[xx, yy, zz] = BlockDefs.Sand;
							else {
								Blocks[xx, yy, zz] = BlockDefs.Grass;
								if (zz != 0)
									Blocks[xx, yy, zz - 1] = BlockDefs.Dirt;
							}
						else if (z < 5 * ChunkSize)
							Blocks[xx, yy, zz] = BlockDefs.Water;
						else
							Blocks[xx, yy, zz] = BlockDefs.Air;


					}
				/*});
				T.Start(new Vector2(x, xx));
				Threads.Add(T);*/
			}

			/*foreach (var T in Threads)
				while (T.ThreadState == ThreadState.Running)
					;*/
		}

		public bool CheckPosition(int X, int Y, int Z) {
			if ((X < 0 || Y < 0 || Z < 0) || (X > (Size - 1) || Y > (Size - 1) || Z > (Size - 1)))
				return false;
			return true;
		}

		public BlockID GetBlock(int X, int Y, int Z) {
			if (!CheckPosition(X, Y, Z)) {
				Vector3 WP = WorldPos(new Vector3(X, Y, Z));
				return GW.GetBlock(this, (int)WP.X, (int)WP.Y, (int)WP.Z);
			}
			while (Blocks == null)
				;
			return Blocks[X, Y, Z];
		}

		public bool SetBlock(int X, int Y, int Z, BlockID B, bool Update = true) {
			if (Blocks != null && CheckPosition(X, Y, Z)) {
				if (Blocks[X, Y, Z] == B)
					return false;
				Blocks[X, Y, Z] = B;
				if (Update)
					Dirty = true;
				return true;
			}
			return false;
		}

		public BlockID GetID(int X, int Y, int Z) {
			return GetBlock(X, Y, Z);
		}

		int Sides;
		void GetBlockData(int x, int y, int z, BlockID B, BlockFace F) {
			B.GetFaceVerts(x, y, z, F, Mesh, MeshT);
			B.GetFaceUVs(F, Atlas, UVs, UVsT);

			if (B.IsTransparent())
				for (int i = 0; i < 4; i++)
					ClrsT.Add(new Vector3(1, 1, 1));
			else
				Sides++;

			Vector3 Normal;
			if (F == BlockFace.Forward)
				Normal = new Vector3(1, 0, 0);
			else if (F == BlockFace.Backward)
				Normal = new Vector3(-1, 0, 0);
			else if (F == BlockFace.Left)
				Normal = new Vector3(0, 1, 0);
			else if (F == BlockFace.Right)
				Normal = new Vector3(0, -1, 0);
			else if (F == BlockFace.Top)
				Normal = new Vector3(0, 0, 1);
			else
				Normal = new Vector3(0, 0, -1);

			List<Vector4> Lst = B.IsTransparent() == false ? BlockData : BlockDataT;
			Lst.Add(new Vector4(Normal, (int)B));
			Lst.Add(new Vector4(Normal, (int)B));
			Lst.Add(new Vector4(Normal, (int)B));
			Lst.Add(new Vector4(Normal, (int)B));
		}

		bool D, VDDirty;
		Thread RThread;
		public bool Dirty {
			get {
				return D;
			}
			set {
				D = value;
				if (D) {
					VDDirty = false;
					if (RThread != null)
						RThread.Abort();
					RThread = new Thread(Rebuild);
					RThread.Start();
				}
			}
		}

		// Thrown together fast, this REALLY REALLY needs a rewrite, not even i write this shitcode
		void Rebuild() {
			Mesh.Clear();
			UVs.Clear();
			Clrs.Clear();
			MeshT.Clear();
			UVsT.Clear();
			ClrsT.Clear();
			BlockData.Clear();
			BlockDataT.Clear();

			for (int x = 0; x < Size; x++)
				for (int y = 0; y < Size; y++)
					for (int z = 0; z < Size; z++) {
						BlockID CurBlock = GetBlock(x, y, z);
						if (CurBlock.IsTransparent()) {
							BlockID B;
							Sides = 0;

							if ((B = GetBlock(x, y, z - 1)) != CurBlock)
								if (B != BlockID.Air)
									GetBlockData(x, y, z - 1, B, BlockFace.Top);
							if ((B = GetBlock(x, y, z + 1)) != CurBlock)
								if (B != BlockID.Air)
									GetBlockData(x, y, z + 1, B, BlockFace.Bottom);
							if ((B = GetBlock(x - 1, y, z)) != CurBlock)
								if (B != BlockID.Air)
									GetBlockData(x - 1, y, z, B, BlockFace.Forward);
							if ((B = GetBlock(x + 1, y, z)) != CurBlock)
								if (B != BlockID.Air)
									GetBlockData(x + 1, y, z, B, BlockFace.Backward);
							if ((B = GetBlock(x, y - 1, z)) != CurBlock)
								if (B != BlockID.Air)
									GetBlockData(x, y - 1, z, B, BlockFace.Left);
							if ((B = GetBlock(x, y + 1, z)) != CurBlock) {
								if (B != BlockID.Air)
									GetBlockData(x, y + 1, z, B, BlockFace.Right);
							}

							float S = 1f - Sides * 0.095f;
							for (int i = 0; i < Sides * 4; i++) 
								Clrs.Add(new Vector3(S, S, S));
						}
					}

			VDDirty = true;
			Dirty = false;
		}

		bool ColoredLastFrame = Settings.Colored;

		public void Render() {
			if (ColoredLastFrame != Settings.Colored)
				Dirty = true;
			ColoredLastFrame = Settings.Colored;

			//Rebuild();
			if (VDDirty) {
				VDDirty = false;
				if (VBO == null) {
					VBO = new VertexArray(ShaderProg, ChunkMatrix, Mesh.ToArray(),
						null, Clrs.ToArray(), UVs.ToArray(), BlockData.ToArray());
					VBOt = new VertexArray(ShaderProg, ChunkMatrix, MeshT.ToArray(),
						null, ClrsT.ToArray(), UVsT.ToArray(), BlockDataT.ToArray());
				} else {
					VBO.Set(Mesh.ToArray(), null, Clrs.ToArray(), UVs.ToArray(), BlockData.ToArray());
					VBOt.Set(MeshT.ToArray(), null, ClrsT.ToArray(), UVsT.ToArray(), BlockDataT.ToArray());
				}
				VBO.SetTexture(Atlas);
				VBOt.SetTexture(Atlas);
			}

			if (VBO != null)
				VBO.Render(ChunkMatrix);
		}

		public void RenderTransparent() {
			if (VBOt != null)
				VBOt.Render(ChunkMatrix);
		}
	}

}