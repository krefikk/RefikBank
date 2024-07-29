using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RefikBank.Models;
using System.Security.Claims;
using System.Text;
using RefikBank.Operations;
using RefikBank.Database;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Principal;
using System.Linq.Expressions;

namespace RefikBank.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public UsersController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("register")]
        // Creates a user and user ID, also creates a TR account for that user
        public IActionResult Register(string _mail, string _password)
        {
            // Check if there's a user with the same mail address
            var userCheck = MemoryDatabase.Users.FirstOrDefault(u => u.GetMail() == _mail);
            if (userCheck != null) 
            {
                return BadRequest("A user with this email address already exists.");
            }

            Operations.Operations operations = new Operations.Operations();
            string _userID = operations.CreateUniqueUserID();
            var user = new User
            {
                userID = _userID,
                accounts = new List<Account>()
            };

            user.SetMail(_mail);
            user.SetPassword(_password);
            user.CreateNewAccount(0);
            MemoryDatabase.Users.Add(user);

            return Ok(new { _userID });
        }

        [HttpPost("login")]
        public IActionResult Login(string _userID, string _password)
        {
            var user = MemoryDatabase.Users.FirstOrDefault(u => u.userID == _userID && u.GetPassword() == _password);

            if (user == null)
            {
                return Unauthorized();
            }

            var token = GenerateJwtToken(user);
            return Ok(new { token });
        }

        [Authorize]
        [HttpGet("accounts")]
        public IActionResult GetAccounts()
        {
            var userIDClaim = User.Claims.FirstOrDefault(c => c.Type == "userID");
            if (userIDClaim == null)
            {
                return Unauthorized("userID claim not found.");
            }

            var userID = userIDClaim.Value;
            var user = MemoryDatabase.Users.FirstOrDefault(u => u.userID == userID);

            if (user == null)
            {
                return NotFound();
            }

            var accounts = user.GetAccounts();

            if (accounts.Count == 0)
            {
                return NotFound();
            }
            else
            {
                List<string> accountLog = new List<string>();
                foreach (var account in accounts)
                {
                    string log = account.accountType switch
                    {
                        0 => $"AN: {account.accountNumber}, Balance: {account.GetBalance()} Turkish Liras",
                        1 => $"AN: {account.accountNumber}, Balance: {account.GetBalance()} Dollars",
                        2 => $"AN: {account.accountNumber}, Balance: {account.GetBalance()} Euros",
                        _ => "Unknown account type"
                    };
                    accountLog.Add(log);
                }
                return Ok(new { message = accountLog });
            }
        }

        [Authorize]
        [HttpGet("balance")]
        public IActionResult GetBalance()
        {
            var userID = User.Claims.First(c => c.Type == "userID").Value;
            var user = MemoryDatabase.Users.FirstOrDefault(u => u.userID == userID);

            if (user == null)
            {
                return NotFound();
            }

            var accounts = user.GetAccounts();

            if (accounts.Count == 0) 
            {
                // Bunun gerçekleşmemesi gerekli çünkü kullanıcı yeni bir kayıt oluşturduğunda otomatik olarak adına bir TR hesabı açılıyor.
                return NotFound();
            }
            else if (accounts.Count == 1)
            {
                // Tek hesap varsa, bu hesabın bakiyesini göster
                var account = accounts.First();
                return Ok(new { account.accountNumber, V = account.GetBalance() });
            }
            else if (accounts.Count > 1)
            {
                // Birden fazla hesap varsa, kullanıcıdan seçim yapmasını iste
                return Ok(new
                {
                    message = "Multiple accounts found. Please select an account.",
                    accounts = accounts.Select(a => new { a.accountNumber, a.accountType }).ToList()
                });
            }

            return NotFound();
        }

        [Authorize]
        [HttpGet("balance/{accountNumber}")]
        public IActionResult GetBalanceForAccount(string accountNumber)
        {
            var userID = User.Claims.First(c => c.Type == "userID").Value;
            var user = MemoryDatabase.Users.FirstOrDefault(u => u.userID == userID);

            if (user == null)
            {
                return NotFound();
            }

            var account = user.GetAccounts().FirstOrDefault(a => a.accountNumber == accountNumber);

            if (account == null)
            {
                return NotFound();
            }

            return Ok(new { account.accountNumber, V = account.GetBalance() });
        }

        [Authorize]
        [HttpGet("deposit")]
        public IActionResult Deposit(string accountNumber, float amount)
        {
            var userID = User.Claims.First(c => c.Type == "userID").Value;
            var user = MemoryDatabase.Users.FirstOrDefault(u => u.userID == userID);
            if (user == null)
            {
                return Unauthorized();
            }

            var account = user.GetAccounts().FirstOrDefault(a => a.accountNumber == accountNumber);
            if (account == null)
            {
                return NotFound();
            }

            account.IncreaseBalance(amount);
            string log = account.accountType switch
            {
                0 => $"Balance increased by {amount} Turkish Liras. New Balance: {account.GetBalance()} Turkish Liras",
                1 => $"Balance increased by {amount} Dollars. New Balance: {account.GetBalance()} Dollars",
                2 => $"Balance increased by {amount} Euros. New Balance: {account.GetBalance()} Euros",
                _ => $"Balance increased by {amount} (unknown currency type). New Balance: {account.GetBalance()}",
            };
            return Ok(new { message = log });
        }

        [Authorize]
        [HttpPost("transfer")]
        public IActionResult Transfer(string sourceAccountNumber, string targetAccountNumber, float amount)
        {
            Operations.Operations operations = new Operations.Operations();
            TransferRequest request = new TransferRequest
            {
                SourceAccountNumber = sourceAccountNumber,
                SourceAccountType = operations.GetAccountTypeFromAccountNumber(sourceAccountNumber),
                TargetAccountNumber = targetAccountNumber,
                TargetAccountType = operations.GetAccountTypeFromAccountNumber(targetAccountNumber),
                Amount = amount
            };

            var userID = User.Claims.First(c => c.Type == "userID").Value;
            var user = MemoryDatabase.Users.FirstOrDefault(u => u.userID == userID);

            if (user == null)
            {
                return NotFound();
            }

            var sourceAccount = user.GetAccounts().FirstOrDefault(a => a.accountNumber == request.SourceAccountNumber && a.accountType == request.SourceAccountType);
            if (sourceAccount == null)
            {
                return NotFound("Source account not found");
            }

            var targetUser = MemoryDatabase.Users.FirstOrDefault(u => u.GetAccounts().Any(a => a.accountNumber == request.TargetAccountNumber && a.accountType == request.TargetAccountType));
            if (targetUser == null)
            {
                return NotFound("Target user not found");
            }

            var targetAccount = targetUser.GetAccounts().FirstOrDefault(a => a.accountNumber == request.TargetAccountNumber && a.accountType == request.TargetAccountType);
            if (targetAccount == null)
            {
                return NotFound("Target account not found");
            }

            if (sourceAccount.GetBalance() < request.Amount)
            {
                return BadRequest("Insufficient funds");
            }

            float convertedAmount = request.Amount;
            if (request.SourceAccountType != request.TargetAccountType)
            {
                convertedAmount = operations.ConvertCurrency(request.Amount, request.SourceAccountType, request.TargetAccountType);
            }

            sourceAccount.DecreaseBalance(request.Amount);
            targetAccount.IncreaseBalance(convertedAmount);

            string log = targetAccount.accountType switch
            {
                0 => $"Your {amount} Turkish Liras transfer completed successfully. New Balance: {targetAccount.GetBalance()} Turkish Liras",
                1 => $"Your {amount} Dollars transfer completed successfully. New Balance: {targetAccount.GetBalance()} Dollars",
                2 => $"Your {amount} Euros transfer completed successfully. New Balance: {targetAccount.GetBalance()} Euros",
                _ => $"Your {amount} (unknown currency type) transfer completed successfully. New Balance: {targetAccount.GetBalance()}",
            };
            return Ok(new { message = log }); 
        }

        private string GenerateJwtToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim("userID", user.userID),
                new Claim("mail", user.GetMail()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
