using OpenTK;
using System;
using System.Collections;

namespace NFM {

	interface IRenderable {
		void Render();
	}

	interface IRenderableT : IRenderable {
		void RenderTransparent();
	}

	interface IUpdateable {
		void Update(float T);
	}

	interface IBindable {
		int GetID {
			get;
		}
		void Bind();
		void Unbind();
	}

	interface IOctree<T> where T : IEquatable<T> {
		bool AddNode(float x, float y, float z, T obj);
		bool AddNode(Vector3 vector, T obj);

		T RemoveNode(float x, float y, float z, T obj);
		T RemoveNode(Vector3 vector, T obj);

		void Clear();

		T GetNode(float x, float y, float z);
		T GetNode(Vector3 vector);
		T GetNode(float x, float y, float z, double shortestDistance);
		T GetNode(Vector3 vector, double shortestDistance);

		ArrayList GetNodes(float x, float y, float z, double radius);
		ArrayList GetNodes(Vector3 vector, double radius);
		ArrayList GetNodes(float x, float y, float z, double minRadius, double maxRadius);
		ArrayList GetNodes(Vector3 vector, double minRadius, double maxRadius);
		ArrayList GetNode(float xMax, float xMin, float yMax, float yMin, float zMax, float zMin);
	}

}