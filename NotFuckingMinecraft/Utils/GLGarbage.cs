using System;
using System.Collections.Concurrent;

namespace NFM {

	static class GLGarbage {
		static ConcurrentQueue<Action> DeleteQueue = new ConcurrentQueue<Action>();

		public static void Enqueue(Action A) {
			DeleteQueue.Enqueue(A);
		}

		public static void Flush() {
			Action A;
			while (true)
				if (DeleteQueue.TryDequeue(out A))
					A();
				else
					break;
		}
	}

}