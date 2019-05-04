using System.Text;

namespace DependencyInjectionWorkshop.Adapters
{
    public interface IHash
    {
        string GetHashed(string password);
    }

    public class Sha256Adapter : IHash
    {
        public string GetHashed(string password)
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