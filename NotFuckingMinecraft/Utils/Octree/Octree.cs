using OpenTK;
using System;
using System.Collections;

namespace NFM {
	// Everything octree releated shamelessly stolen from github

	[Serializable]
	public class Octree<T> : IOctree<T> where T : IEquatable<T> {
		protected internal OctreeNode<T> Top;

		public Octree()
			: this(1.0f, 0.0f, 1.0f, 0.0f, 1.0f, 0.0f, 20, OctreeNode<T>.NoMinSize) {
		}

		public Octree(float xMax, float xMin, float yMax, float yMin, float zMax, float zMin, int maxItems)
			: this(xMax, xMin, yMax, yMin, zMax, zMin, maxItems, OctreeNode<T>.NoMinSize) {
		}

		public Octree(int up, int left, int down, int right, int front, int back, int maxItems)
			: this(up, left, down, right, front, back, maxItems, OctreeNode<T>.DefaultMinSize) {
		}

		public Octree(float xMax, float xMin, float yMax, float yMin, float zMax, float zMin, int maxItems, float minSize) {
			Top = new OctreeNode<T>(xMax, xMin, yMax, yMin, zMax, zMin, maxItems, minSize);
		}

		public bool AddNode(float x, float y, float z, T obj) {
			return Top.AddNode(x, y, z, obj);
		}

		public bool AddNode(Vector3 vector, T obj) {
			return Top.AddNode(vector.X, vector.Y, vector.Z, obj);
		}

		public T RemoveNode(float x, float y, float z, T obj) {
			return Top.RemoveNode(x, y, z, obj);
		}

		public T RemoveNode(Vector3 vector, T obj) {
			return Top.RemoveNode(vector.X, vector.Y, vector.Z, obj);
		}

		public T GetNode(float x, float y, float z) {
			return Top.GetNode(x, y, z);
		}

		public T GetNode(Vector3 vector) {
			return Top.GetNode(vector.X, vector.Y, vector.Z);
		}

		public T GetNode(float x, float y, float z, double shortestDistance) {
			return Top.GetNode(x, y, z, shortestDistance);
		}

		public T GetNode(Vector3 vector, double shortestDistance) {
			return Top.GetNode(vector.X, vector.Y, vector.Z, shortestDistance);
		}

		public ArrayList GetNode(float xMax, float xMin, float yMax, float yMin, float zMax, float zMin) {
			return GetNode(xMax, xMin, yMax, yMin, zMax, zMin, ArrayList.Synchronized(new ArrayList(100)));
		}

		public ArrayList GetNode(float xMax, float xMin, float yMax, float yMin, float zMax, float zMin, ArrayList nodes) {
			if (nodes == null)
				nodes = ArrayList.Synchronized(new ArrayList(10));
			if (xMin > xMax || (Math.Abs(xMin - xMax) < 1e-6))
				return Top.GetNode(xMax, xMin, yMax, yMin, zMax, zMin, Top.GetNode(xMax, 0, yMax, yMin, zMax, zMin, nodes));
			return Top.GetNode(xMax, xMin, yMax, yMin, zMax, zMin, nodes);
		}

		public ArrayList GetNodes(float x, float y, float z, double radius) {
			return Top.GetNodes(x, y, z, radius);
		}

		public ArrayList GetNodes(Vector3 vector, double radius) {
			return Top.GetNodes(vector.X, vector.Y, vector.Z, radius);
		}

		public ArrayList GetNodes(float x, float y, float z, double minRadius, double maxRadius) {
			return Top.GetNodes(x, y, z, minRadius, maxRadius);
		}

		public ArrayList GetNodes(Vector3 vector, double minRadius, double maxRadius) {
			return Top.GetNodes(vector.X, vector.Y, vector.Z, minRadius, maxRadius);
		}

		public void Clear() {
			Top.Clear();
		}
	}

}