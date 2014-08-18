using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace NFM {
	enum WriterType {
		Out,
		Error
	}

	class RWriter : TextWriter {
		WriterType T;
		TextWriter B;
		string OutPath;

		StreamWriter SW;

		public override Encoding Encoding {
			get {
				return Encoding.Default;
			}
		}

		public RWriter(WriterType T, TextWriter Base = null)
			: base() {
			B = Base;
			this.T = T;
			OutPath = T.ToString() + ".txt";

			if (File.Exists(OutPath))
				File.Delete(OutPath);
			SW = File.AppendText(OutPath);
			SW.AutoFlush = true;
		}

		public override void Write(char value) {
			SW.Write(value);
			if (B != null)
				B.Write(value);
		}

		/*
		public override void Write(bool value) {
			SW.Write(value);
			if (B != null)
				B.Write(value);
		}

		public override void Write(char[] buffer) {
			SW.Write(buffer);
			if (B != null)
				B.Write(buffer);
		}

		public override void Write(char[] buffer, int index, int count) {
			SW.Write(buffer, index, count);
			if (B != null)
				B.Write(buffer, index, count);
		}

		public override void Write(string value) {
			SW.Write(value);
			if (B != null)
				B.Write(value);
		}

		public override void WriteLine(string value) {
			Write(value + '\n');
		}
		//*/

	}
}