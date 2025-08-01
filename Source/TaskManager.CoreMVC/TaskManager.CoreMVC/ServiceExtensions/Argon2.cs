using Konscious.Security.Cryptography;
using System.Security.Cryptography;
using System.Text;

namespace TaskManager.CoreMVC.ServiceExtensions
{
    /// <summary>
    /// Argon2 password hashing
    /// </summary>
    public class Argon2
    {
        /// <summary>
        /// Create random salt
        /// </summary>
        /// <returns></returns>
        public static byte[] CreateSalt()
        {
            var buffer = new byte[16];
            var rng = new RNGCryptoServiceProvider();
            rng.GetBytes(buffer);
            return buffer;
        }

        /// <summary>
        /// Hash password using salt
        /// </summary>
        /// <param name="password"></param>
        /// <param name="salt"></param>
        /// <returns></returns>
        public static byte[] HashPassword(string password, byte[] salt)
        {
            var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password));

            argon2.Salt = salt;
            argon2.DegreeOfParallelism = 4; // number of parallel instances of the Argon2 algorithm that are used to hash the password
            argon2.Iterations = 3; // number of algorithm applied to hash the password
            argon2.MemorySize = 1024; // the amount of memory that is used by the Argon2 algorithm to hash the password

            return argon2.GetBytes(16);
        }

        /// <summary>
        /// Verify password
        /// </summary>
        /// <param name="password"></param>
        /// <param name="salt"></param>
        /// <param name="hash"></param>
        /// <returns></returns>
        public static bool VerifyHash(string password, byte[] salt, byte[] hash)
        {
            var newHash = HashPassword(password, salt);
            return hash.SequenceEqual(newHash);
        }
    }
}
