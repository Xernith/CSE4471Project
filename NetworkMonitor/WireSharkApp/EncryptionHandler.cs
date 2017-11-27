using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace WiresharkApp {
	public class EncryptionHandler {
		string k="";
		int[] schedule;

		public EncryptionHandler(string key) {
			k = key;
			schedule = new int[256];

			int i=0;
			int j=0;
			for (i=0; i<schedule.Length; i++) {
				schedule[i] = i;
			}
			//Key scheduling
			j = 0;
			for (i=0; i<schedule.Length; i++) {
				j = (j + schedule[i] + key[i % key.Length]) % 256;
				int temp = schedule[i];
				schedule[i] = schedule[j];
				schedule[j] = temp;
			}
		}

		public string Encrypt(string data) {
			return Encrypt (k, data);
		}

		public string Decrypt(string data) {
			return Decrypt (k, data);
		}

		public void Test() {
			string key = k;
			string text = "Plaintext";
			string ciphertext = Encrypt(key,text);
			//Console.WriteLine(ciphertext);
			//Console.WriteLine(Decrypt(ciphertext));
		}

		public string Encrypt(string key, string data) {
			Encoding unicode = Encoding.Unicode;
			return Convert.ToBase64String(RC4encrypt(unicode.GetBytes(key), unicode.GetBytes(data)));
		}

		public string Decrypt(string key, string data) {
			Encoding unicode = Encoding.Unicode;
			return unicode.GetString(RC4encrypt(unicode.GetBytes(key), Convert.FromBase64String(data)));
		}

		public byte[] RC4encrypt(byte[] key, byte[] text) {
			int i=0;
			int j=0;

			int[] ray = new int[256];
			Array.Copy(schedule,ray,256);

			//Pseudo-random generation algorithm
			i=0;
			j=0;
			byte[] result = text.Select((b) => {
				i = (i + 1) & 255;
				j = (j + ray[i]) & 255;

				int temp = ray[i];
				ray[i] = ray[j];
				ray[j] = temp;

				return (byte)(b ^ ray[(ray[i] + ray[j]) % 255]);
			}).ToArray();

			return result;
		}
	}
}