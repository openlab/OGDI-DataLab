
namespace Ogdi.InteractiveSdk.Mvc.Models
{
    public class AuthorInfo
    {
        public string Name { get; set; }
        public string Email { get; set; }

        public AuthorInfo(string name, string email)
        {
            Name = name;
            Email = email;
        }
    }
}
