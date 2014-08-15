using System;

namespace NFM {

	[Serializable]
	public class OctreeLeaf<T> {
		private float fx, fy, fz;
		private T objectValue;

		public OctreeLeaf(float x, float y, float z, T obj) {
			fx = x;
			fy = y;
			fz = z;
			objectValue = obj;
		}

		public OctreeLeaf(double x, double y, double z, T obj)
			: this((float)x, (float)y, (float)z, obj) {
		}

		public T LeafObject {
			get {
				return objectValue;
			}
		}

		public float X {
			get {
				return fx;
			}
			set {
				fx = value;
				;
			}
		}

		public float Y {
			get {
				return fy;
			}
			set {
				fy = value;
				;
			}
		}

		public float Z {
			get {
				return fz;
			}
			set {
				fz = value;
				;
			}
		}
	}

}