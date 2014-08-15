using OpenTK;
using System;
using System.Collections.Generic;

namespace NFM {

	enum BlockID : ushort {
		Air = 0,
		Grass,
		Stone,
		Water,
		Sand,
		Dirt,

		Border
	}

	class BlockDef {
		//Block B;

		public BlockID ID;
		public ushort TexID;
		public bool Transparent;

		public BlockDef(BlockID ID, bool Transparent) {
			this.ID = ID;
			TexID = (ushort)ID;
			this.Transparent = Transparent;
			BDefs.Add(ID, this);

			//B = new Block(ID);
		}

		static Dictionary<BlockID, BlockDef> BDefs = new Dictionary<BlockID, BlockDef>();
		public static BlockDef Get(BlockID ID) {
			if (BDefs.ContainsKey(ID))
				return BDefs[ID];
			return null;
		}

		public static implicit operator BlockID(BlockDef D) {
			return D.ID;
		}
	}

	static class BlockDefs {
		public static bool IsTransparent(this BlockID B) {
			return BlockDef.Get(B).Transparent;
		}

		public static ushort GetTexID(this BlockID B) {
			return BlockDef.Get(B).TexID;
		}

		public static BlockDef Border = new BlockDef(BlockID.Border, false);
		public static BlockDef Air = new BlockDef(BlockID.Air, true);
		public static BlockDef Grass = new BlockDef(BlockID.Grass, false);
		public static BlockDef Stone = new BlockDef(BlockID.Stone, false);
		public static BlockDef Water = new BlockDef(BlockID.Water, true);
		public static BlockDef Sand = new BlockDef(BlockID.Sand, false);
		public static BlockDef Dirt = new BlockDef(BlockID.Dirt, false);
	}

	enum BlockFace {
		Forward = 0,
		Backward,
		Left,
		Right,
		Top,
		Bottom
	}

	static class Block {
		public static int Size = 10;

		public static void GetFaceVerts(this BlockID B, int lx, int ly, int lz, BlockFace F, List<Vector3> Opaque,
			List<Vector3> Trans) {
			float x = lx * Block.Size;
			float y = ly * Block.Size;
			float z = lz * Block.Size;

			List<Vector3> L = B.IsTransparent() ? Trans : Opaque;

			switch (F) {
				case BlockFace.Forward:
					L.AddRange(new Vector3[] {
						new Vector3(x + Block.Size, y, z),
						new Vector3(x + Block.Size, y + Block.Size, z),
						new Vector3(x + Block.Size, y + Block.Size, z + Block.Size),
						new Vector3(x + Block.Size, y, z + Block.Size)
					});
					return;
				case BlockFace.Backward:
					L.AddRange(new Vector3[] {
						new Vector3(x, y, z + Block.Size),
						new Vector3(x, y + Block.Size, z + Block.Size),
						new Vector3(x, y + Block.Size, z),
						new Vector3(x, y, z)
					});
					return;
				case BlockFace.Left:
					L.AddRange(new Vector3[] {
						new Vector3(x, y + Block.Size, z + Block.Size),
						new Vector3(x + Block.Size, y + Block.Size, z + Block.Size),
						new Vector3(x + Block.Size, y + Block.Size, z),
						new Vector3(x, y + Block.Size, z)
					});
					return;
				case BlockFace.Right:
					L.AddRange(new Vector3[] {
						new Vector3(x, y, z),
						new Vector3(x + Block.Size, y, z),
						new Vector3(x + Block.Size, y, z + Block.Size),
						new Vector3(x, y, z + Block.Size)
					});
					return;
				case BlockFace.Top:
					L.AddRange(new Vector3[] {
						new Vector3(x, y, z + Block.Size),
						new Vector3(x + Block.Size, y, z + Block.Size),
						new Vector3(x + Block.Size, y + Block.Size, z + Block.Size),
						new Vector3(x, y + Block.Size, z + Block.Size)
					});
					return;
				case BlockFace.Bottom:
					L.AddRange(new Vector3[] {
						new Vector3(x, y + Block.Size, z),
						new Vector3(x + Block.Size, y + Block.Size, z),
						new Vector3(x + Block.Size, y, z),
						new Vector3(x, y, z)
					});
					return;
			}

			throw new Exception("Unreachable code reached");
		}

		public static void GetFaceUVs(this BlockID B, BlockFace F, Texture Tx, List<Vector2> Opaque, List<Vector2> Trans) {
			float TexSize = 1f / 6f * Tx.W;
			float w = 1f / ((float)Tx.W / TexSize);
			float h = 1f / ((float)Tx.H / TexSize);

			float x = 0;
			float y = h * B.GetTexID();

			Vector2[] R;
			List<Vector2> L = B.IsTransparent() ? Trans : Opaque;

			switch (F) {
				case BlockFace.Forward:
					R = new Vector2[] {
						new Vector2(x + w * 2, y + h),
						new Vector2(x + w * 3, y + h),
						new Vector2(x + w * 3, y),
						new Vector2(x + w * 2, y),
					};
					break;
				case BlockFace.Backward:
					R = new Vector2[] {
						new Vector2(x + w * 5, y),
						new Vector2(x + w * 4, y),
						new Vector2(x + w * 4, y + h),
						new Vector2(x + w * 5, y + h),
					};
					break;
				case BlockFace.Left:
					R = new Vector2[] {
						new Vector2(x + w * 6, y),
						new Vector2(x + w * 5, y),
						new Vector2(x + w * 5, y + h),
						new Vector2(x + w * 6, y + h),
					};
					break;
				case BlockFace.Right:
					R = new Vector2[] {
						new Vector2(x + w * 3, y + h),
						new Vector2(x + w * 4, y + h),
						new Vector2(x + w * 4, y),
						new Vector2(x + w * 3, y),
					};
					break;
				case BlockFace.Top:
					R = new Vector2[] {
						new Vector2(x, y),
						new Vector2(x, y + h),
						new Vector2(x + w, y + h),
						new Vector2(x + w, y),
					};
					break;
				case BlockFace.Bottom:
					R = new Vector2[] {
						new Vector2(x + w, y),
						new Vector2(x + w, y + h),
						new Vector2(x + w * 2, y + h),
						new Vector2(x + w * 2, y),
					};
					break;
				default:
					throw new Exception("Unknown block face");
			}
			L.AddRange(R);
		}
	}

}