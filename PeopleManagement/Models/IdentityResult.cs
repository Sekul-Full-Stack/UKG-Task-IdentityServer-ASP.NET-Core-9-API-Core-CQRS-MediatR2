namespace PeopleManagement.Models
{
    public class IdentityResult<T>
    {
        public T Data { get; set; }
        public string ErrorMessage { get; set; }
        public bool Success => string.IsNullOrEmpty(ErrorMessage);
    }
}
