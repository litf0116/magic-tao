using System;
using System.Threading.Tasks;

namespace TtWork.Project.TestSample
{
    public class UserService
    {
        public async Task<UserDto> CreateUserAsync(CreateUserInput input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (string.IsNullOrEmpty(input.Name))
            {
                throw new ArgumentException("Name is required");
            }

            if (string.IsNullOrEmpty(input.Email))
            {
                throw new ArgumentException("Email is required");
            }

            var user = new User
            {
                Name = input.Name,
                Email = input.Email,
                CreatedAt = DateTime.Now
            };

            await _userRepository.InsertAsync(user);

            return ObjectMapper.Map<User, UserDto>(user);
        }

        public async Task<UserDto> GetUserByIdAsync(long id)
        {
            var user = await _userRepository.FindAsync(id);

            if (user == null)
            {
                return null;
            }

            return ObjectMapper.Map<User, UserDto>(user);
        }

        public async Task<bool> DeleteUserAsync(long id)
        {
            var user = await _userRepository.FindAsync(id);

            if (user == null)
            {
                return false;
            }

            await _userRepository.DeleteAsync(user);

            return true;
        }
    }

    public class CreateUserInput
    {
        public string Name { get; set; }
        public string Email { get; set; }
    }

    public class UserDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class User
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}