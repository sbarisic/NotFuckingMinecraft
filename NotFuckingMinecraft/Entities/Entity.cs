using System;

namespace NFM.Entities {

	// TODO: REDO
	class Entity {
		internal Renderer R;

		public static T Create<T>(Renderer R) where T : Entity {
			T Ent = Activator.CreateInstance<T>();
			Ent.R = R;
			Ent.Init();
			return Ent;
		}

		public virtual void Init() {
			R.UpdateFrame += (S, E) => Update((float)E.Time);
			R.RenderFrame += (S, E) => Render((float)E.Time);
		}

		public virtual void Update(float T) {
		}

		public virtual void Render(float T) {
		}
	}

}