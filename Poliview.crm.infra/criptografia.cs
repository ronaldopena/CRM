using System.Security.Cryptography;
using System.Text;

namespace Poliview.crm.infra
{
    public class Criptografia
    {
        public static string? Criptografar(string? _senha)
        {
            if (_senha == null)
                return null;

            using (SHA1 sha = SHA1.Create())
            {
                ASCIIEncoding ae = new ASCIIEncoding();
                byte[] senhaCripto = sha.ComputeHash(ae.GetBytes(_senha));
                return Convert.ToBase64String(senhaCripto);
            }
        }
    }
}
