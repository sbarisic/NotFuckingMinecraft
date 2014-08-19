using System;

namespace NFM.Entities {

	class CaptionFPSCounter : Entity {
		double CaptionTimer;
		public override void Update(float T) {
			CaptionTimer += T;
			if (CaptionTimer > .25) {
				CaptionTimer = 0;
				R.Title = Settings.Caption + " " + Math.Round(T, 4).ToString() + " ms; " + Math.Round(1f / T, 2) + " fps";
			}
		}
	}

}