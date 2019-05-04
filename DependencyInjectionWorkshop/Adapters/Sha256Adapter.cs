using System.Text;

namespace DependencyInjectionWorkshop.Adapters
{
    public class Sha256Adapter
    {
        public string GetHashedPassword(string password)
        {
            var hash = new StringBuilder();

            var crypt = new System.Security.Cryptography.SHA256Managed();
            var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(password));

            foreach (var theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }

            return hash.ToString();
        }
    }
}