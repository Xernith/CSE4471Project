using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace WiresharkApp {
	public static class EncryptionHandler {
		static string k="";
		static int[] schedule;
		static bool keySet = false;

		public static void SetKey() {
			if (keySet) {
				return;
			}
			string randomKey = RandomString (15);
			SetKey(randomKey);
		}

		public static void SetKey(string key) {
			if (keySet) {
				//can't set the key more than once
				return;
			}
			keySet = true;
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

		public static string Encrypt(string data) {
			return Encrypt (k, data);
		}

		public static string Decrypt(string data) {
			return Decrypt (k, data);
		}

		public static void Test() {
			string key = "key";
			string text = "Plaintext";
			SetKey();
			string ciphertext = Encrypt(text);
			Console.WriteLine(ciphertext);
			Console.WriteLine(Decrypt(ciphertext));
		}

		public static string Encrypt(string key, string data) {
			Encoding unicode = Encoding.Unicode;
			return Convert.ToBase64String(RC4encrypt(unicode.GetBytes(key), unicode.GetBytes(data)));
		}

		public static string Decrypt(string key, string data) {
			Encoding unicode = Encoding.Unicode;
			return unicode.GetString(RC4encrypt(unicode.GetBytes(key), Convert.FromBase64String(data)));
		}

		public static byte[] RC4encrypt(byte[] key, byte[] text) {
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

		private static string RandomString(int length)
		{
			Random r = new Random();
			const string pool = "abcdefghijklmnopqrstuvwxyz0123456789";
			var builder = new StringBuilder();

			for (var i = 0; i < length; i++)
			{
				var c = pool[r.Next(0, pool.Length)];
				builder.Append(c);
			}

			return builder.ToString();
		}
	}
}
