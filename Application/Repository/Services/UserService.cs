using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading;
using Application.Common.Exceptions;
using Application.Common.Models;
using Application.Common.Utility.Security;
using Application.Repository.Webservice;
using BotDetect.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Tool_Utility.Encryption;
using Role = Domain.Entities.Role;

namespace Application.Repository.Services;

public class UserService : BaseRepository<User>
{
    private AuthService AuthService;
    MenuService MenuService;
    public static List<UserToken> _userTokens = new List<UserToken>();
    public static readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
    private readonly OrgLocationService orgLocationService;
    private SystemSettingService SystemSettingService;
    private IServiceScopeFactory IServiceScopeFactory;
    private Shahkar Shahkar;

    public UserService(IApplicationDbContext context, IMapper mapper, AuthService authService, MenuService menuService, OrgLocationService orgLocationService, SystemSettingService systemSettingService, IServiceScopeFactory serviceScopeFactory, Shahkar shahkar) : base(context, mapper)
    {
        AuthService = authService;
        MenuService = menuService;
        this.orgLocationService = orgLocationService;
        SystemSettingService = systemSettingService;
        IServiceScopeFactory = serviceScopeFactory;
        Shahkar = shahkar;
    }

    public bool CheckToken(string Token)
    {
        Guid TokenKey = Guid.Parse(Token);

        _lock.WaitAsync().Wait();
        bool IsValid = false;
        try
        {
            IsValid = _userTokens.Any(a => a.TokenKey == TokenKey && a.ExpireDate > DateTime.Now);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        finally
        {
            _lock.Release();
        }

        return IsValid;
    }

    public (Guid TokenKey, bool MultiLogin) SetTokenKey(SystemSetting systemSetting, Guid UserGUID,
        bool MultiLoginAlowed)
    {
        int OldTokenCount = 0;
        if (!MultiLoginAlowed)
        {
            OldTokenCount = Logout(UserGUID,false);
        }

        DateTime ExpireDate =
            DateTime.Now.AddSeconds(systemSetting.TokenExpireSeconds + systemSetting.RefreshTokenExpireSeconds);
        Guid TokenKey = Guid.NewGuid();
        _lock.WaitAsync().Wait();
        try
        {
            _userTokens.Add(new UserToken() { UserGUID = UserGUID, TokenKey = TokenKey, ExpireDate = ExpireDate });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        finally
        {
            _lock.Release();
        }

        return (TokenKey, OldTokenCount > 0);
    }

    public int Logout(Guid UserGUID,bool ForceLogout)
    {
        SystemSetting systemSetting = SystemSettingService.GetSystemSetting().Result;
        _lock.WaitAsync().Wait();
        int CurrentUserTokensCount = 0;
        try
        {
            DateTime Now = DateTime.Now;

            if (!systemSetting.MultiLoginAlowed||ForceLogout==true)
            {
                List<UserToken> CurrentUserTokens =
              _userTokens.Where(a => a.UserGUID == UserGUID && a.ExpireDate > Now).ToList();
                CurrentUserTokensCount = CurrentUserTokens.Count;
                foreach (var RemoveToken in CurrentUserTokens)
                {
                    _userTokens.Remove(RemoveToken);
                }
            }


            List<UserToken> RemoveTokens =
                _userTokens.Where(a => a.ExpireDate <= Now).ToList();
            foreach (var RemoveToken in RemoveTokens)
            {
                _userTokens.Remove(RemoveToken);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        finally
        {
            _lock.Release();
        }

        return CurrentUserTokensCount;



    }

    public void DetectBot(string UserEnteredCaptchaCode, string CaptchaId)
    {
        SimpleCaptcha yourFirstCaptcha = new SimpleCaptcha();

        bool isHuman = yourFirstCaptcha.Validate(UserEnteredCaptchaCode, CaptchaId);

        if (isHuman == false)
        {
            throw new ServiceExeption("کد امنیتی وارد شده اشتباه است");
        }
    }


    public async Task<Token> GetToken(User user, Guid? PackageGUID = null, Guid? OrgLocationGUID = null,
        bool? AdminLogin = null, bool MultiLoginAlowed = false)
    {
        Package? package = db.Package.Include(hh => hh.MainMenu).FirstOrDefault(a => a.GUID == PackageGUID);

        //string IdentityPath = await MenuService.GetIdentityPath();
        var Now = DateTime.Now;

        List<UserRole> UserRole = db.UserRole.Include(a => a.Role)
            .Where(a =>
                    a.UserGUID == user.GUID &&
                (
                    a.Role.Gkey == 1 ||
                    (OrgLocationGUID.HasValue == false && a.Role.Gkey == 2)
                    ||
                    (
                        package != null &&
                        AdminLogin == true &&
                        a.OrgLocationGUID == OrgLocationGUID &&
                        a.RoleGUID == package.RoleGUID
                    )
                    ||
                    (
                        package != null &&
                        AdminLogin == false &&
                        a.OrgLocationGUID == OrgLocationGUID &&
                        a.Role.RoleAccess.Any(RoleAccess => package.MainMenuGuid != null && RoleAccess.Menu.Path.StartsWith(package.MainMenu.Path))
                        //a.Role.RoleAccess.Any(RoleAccess => db.RoleAccess.Any(PackageRoleAccess =>
                        //    !RoleAccess.Menu.Path.Contains(IdentityPath) &&
                        //    PackageRoleAccess.RoleGUID == package.RoleGUID &&
                        //    RoleAccess.MenuGUID == PackageRoleAccess.MenuGUID))
                    )
                )
                &&
                a.ExpireDate > Now).ToList();


        var key = await CreateToken(user, UserRole, MultiLoginAlowed);

        var token = new Token()
        {
            ForceChangePassword = user.ForceChangePassword,
            Key = key.Token,
            MultiLogin = key.MultiLogin,
            FirstName = user.FirstName,
            LastName = user.LastName,
            ExpireDate = key.ExpireDate,
            UserRole = UserRole.OrderBy(a => a.Role.Level).ThenBy(a => a.Gkey).First().Gkey
        };
        return token;
    }

    public async Task CheckUserLoginAccess(User user)
    {
        if (user.Blocked)
        {
            throw new ServiceExeption("حساب کاربری شما مسدود شده است.");
        }

        if (user.Locked && user.LockoutEndDate != null && user.LockoutEndDate.Value > DateTime.Now)
        {
            throw new ServiceExeption(
                $"به دلیل تلاش ناموفق جهت ورود به سیستم حساب کاربری شما تا تاریخ {user.LockoutEndDate.Value.ToShamsiWhitTime()} مسدود شده است.");
        }
    }

    public async Task OnFaildLogin(Guid UserGUID)
    {
        SystemSetting systemSetting = await SystemSettingService.GetSystemSetting();
        using (var scope = IServiceScopeFactory.CreateScope())
        {
            IApplicationDbContext db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
            User user = db.User.First(a => a.GUID == UserGUID);
            user.FailedLoginCount++;
            if (user.FailedLoginCount >= systemSetting.LoginAttempts)
            {
                user.LockoutEndDate = DateTime.Now.AddSeconds(systemSetting.LockUserSeconds);
                user.Locked = true;
            }

            await db.SaveChangesAsync(CancellationToken.None);
            ;
        }
    }

    public async Task OnSuccessLogin(Guid UserGUID)
    {
        using (var scope = IServiceScopeFactory.CreateScope())
        {
            IApplicationDbContext db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
            User user = db.User.First(a => a.GUID == UserGUID);
            user.LockoutEndDate = null;
            user.Locked = false;
            user.FailedLoginCount = 0;
            user.MobileIsConfirm = true;
            await db.SaveChangesAsync(CancellationToken.None);
            ;
        }
    }

       public async Task<(string Token, bool MultiLogin, DateTime ExpireDate)> CreateToken(User user,
        List<UserRole> UserRole, bool MultiLoginAlowed)
    {
        await CheckUserLoginAccess(user);

        //foreach (var _UserRole in UserRole)
        //{
        //    bool SignIsValid = await _UserRole.SignIsValid();
        //    if (!SignIsValid)
        //    {
        //        Role Role=db.Role.First(a=>a.GUID==_UserRole.RoleGUID);
        //        throw new ServiceExeption($"خطای امنیتی در نقش {Role.Name} رخ داده است. ");
        //    }
        //}
        SystemSetting systemSetting = await SystemSettingService.GetSystemSetting();

        var issuer = Environment.GetEnvironmentVariable("ValidIssuer");
        var audience = Environment.GetEnvironmentVariable("ValidAudience");
        var key = Encoding.ASCII.GetBytes(Environment.GetEnvironmentVariable("IssuerSigningKey"));

        List<Claim> claims = new();

        var RoleGkeys = UserRole.Select(a => a.Role.Gkey).ToList();
        var UserRoleGkeys = UserRole.Select(a => a.Gkey).ToList();
        var UserRoleOrglocations = UserRole.Select(a => a.Gkey + "/" + a.OrgLocationGUID).ToList();
        foreach (var RoleGkey in RoleGkeys) claims.Add(new Claim("scope", RoleGkey.ToString()));
        foreach (var UserRoleGkey in UserRoleGkeys) claims.Add(new Claim("UserRoles", UserRoleGkey.ToString()));
        foreach (var UserRoleOrglocation in UserRoleOrglocations)
            claims.Add(new Claim("UserRoleOrglocation", UserRoleOrglocation.ToString()));

        DateTime ExpireDate = DateTime.Now.AddSeconds(systemSetting.TokenExpireSeconds);
        var TokenKeyInfo = SetTokenKey(systemSetting, user.GUID, MultiLoginAlowed);
        claims.Add(new Claim("TokenKey", TokenKeyInfo.TokenKey.ToString()));
        claims.Add(new Claim("Id", user.GUID.ToString()));
        claims.Add(new Claim("ExpireDate", ExpireDate.ToString()));
        claims.Add(new Claim("UserAgent", AuthService.GetUserAgent()));
        claims.Add(new Claim("IpAddress", AuthService.GetIpAddress()));
        claims.Add(new Claim("Username", user.Username));
        claims.Add(new Claim("UserFullname", user.FirstName + " " + user.LastName));
        claims.Add(new Claim("Email", user.Email ?? ""));
        claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddSeconds(systemSetting.TokenExpireSeconds),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials
            (new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha512Signature)
        };
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var jwtToken = tokenHandler.WriteToken(token);

        return (jwtToken, TokenKeyInfo.MultiLogin, ExpireDate);
    }
    
 
    public static string HashPassword(Guid Userid, string Password)
    {
        return (Userid + Password + "dzl6QrdRHd7AZU1oeycDqTr1oU").SHA256Hash();
    }

    public async Task InserDefaults(CancellationToken cancellationToken)
    {
        if (!Table.Any())
        {
            var RoleGUID = db.Role.FirstOrDefault(a => a.Gkey == 1)?.GUID;
            if (RoleGUID == null)
            {
                try
                {
                    Role role = new Role();
                    role.Gkey = 1;
                    role.Name = "سوپر ادمین";
                    role.Path = "/1/";
                    db.Role.Add(role);
                    await db.SaveChangesAsync(cancellationToken: cancellationToken);
                    RoleGUID = role.GUID;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }

            var OrgLocationGUID = db.OrgLocation.FirstOrDefault(a => a.Gkey == 1)?.GUID;
            if (OrgLocationGUID == null)
            {
                try
                {
                    OrgLocation OrgLocation = new OrgLocation();
                    OrgLocation.Gkey = 1;
                    OrgLocation.Name = "بولوت";
                    OrgLocation.Path = "/1/";
                    db.OrgLocation.Add(OrgLocation);
                    await db.SaveChangesAsync(cancellationToken: cancellationToken);
                    OrgLocationGUID = OrgLocation.GUID;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }

            var UserGUID = Guid.NewGuid();
            var AdminUser = new User
            {
                GUID = UserGUID,
                Email = "vadood.vahid21@gmail.com",
                Mobile = "09148492785",
                Username = "admin",
                Password = HashPassword(UserGUID, "159753vV!@#"),
                FirstName = "ودود",
                LastName = "وحید",
            };
            AdminUser.UserRoles.Add(new UserRole
            {
                RoleGUID = (Guid)RoleGUID,
                UserGUID = UserGUID,
                OrgLocationGUID = (Guid)OrgLocationGUID,
                Gkey = 1,
                ExpireDate = DateTime.Now.AddYears(5)
            });
            Table.Add(AdminUser);

            await db.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<(Guid UserGUID, Guid OrglocationGUID)> CreateUser(string Username, string Password,
        string FirstName,
        string LastName, string Email, string Mobile, Guid OrgLocationTypeGUID, string OrgLocationName,
        string OrgLocationAddress, string OrgLocationTell, string OrgLocationFax, string OrgLocationEmail,
        string OrgLocationUrl, DateTime ExpireDate, bool ValidateUserExist, string Nationalcode
        , CancellationToken cancellationToken,string FatherName="",bool? IsMan=null)
    {

        bool ShahkarResult = await Shahkar.CheckMobile(Nationalcode, Mobile);

        if (ShahkarResult == false)
        {
            throw new ServiceExeption("کد ملی و شماره موبایل مطابقت ندارد");
        }
        
        #region InsertUser

        User? user = db.User.FirstOrDefault(a => a.Username == Username);
        if (user == null)
        {
            if (Password.Length < 12)
            {
                throw new ServiceExeption("رمز عبور باید حداقل 12 کاراکتر باشد");
            }

            bool PasswordBlock = db.PasswordBlockList.Any(a => a.Password == Password);

            if (PasswordBlock)
            {
                throw new ServiceExeption("رمز عبور دارای امنیت کافی نمی باشد");
            }
            user = new User();
            user.Username = Username?.ToLower().Trim();
            user.Email = Email?.ToLower().Trim();
            user.Mobile = Mobile?.ToLower().Trim();
            user.Password = Password;
            user.FirstName = FirstName?.ToLower().Trim();
            user.LastName = LastName?.ToLower().Trim();
            user.Password = HashPassword(user.GUID, user.Password);
            user.ForceChangePassword = true;
            user.Nationalcode = Nationalcode;
            user.FatherName = FatherName;
            user.IsMan = IsMan;
            bool UsernameExist = Table.Any(a => a.Username == user.Username);
            if (UsernameExist)
            {
                throw new ServiceExeption("نام کاربری وارد شده قبلا ثبت شده است.");
            }

           
            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                bool EmailExist = Table.Any(a => a.Email == user.Email);
                if (EmailExist)
                {
                    throw new ServiceExeption("ایمیل وارد شده قبلا ثبت شده است.");
                }
            }

            Table.Add(user);
        }
        else if (ValidateUserExist)
        {
            throw new ServiceExeption("نام کاربری وارد شده قبلا ثبت شده است.");
        }

        #endregion

        #region InsertOrglocation

        OrgLocation? OrgLocation = db.OrgLocation.FirstOrDefault(a => a.OrgLocationTypeGUIO == OrgLocationTypeGUID &&
                                                                      a.UserGUID == user.GUID
        );
        if (OrgLocation == null)
        {
            OrgLocation = new OrgLocation()
            {
                POrgLocationGUID = db.OrgLocation.First(a => a.Gkey == 1).GUID,
                Name = OrgLocationName,
                Address = OrgLocationAddress,
                Email = OrgLocationEmail,
                Fax = OrgLocationFax,
                Url = OrgLocationUrl,
                Tell = OrgLocationTell,
                UserGUID = user.GUID,
                OrgLocationTypeGUIO = OrgLocationTypeGUID
            };
            db.OrgLocation.Add(OrgLocation);
        }

        #endregion

        #region InsertUserRole

        List<Role> Roles = db.Role.Where(a => a.OrgLocationTypeGUID == OrgLocationTypeGUID).ToList();

        foreach (var role in Roles)
        {
            bool Exist = db.UserRole.Any(a =>
                a.UserGUID == user.GUID && a.RoleGUID == role.GUID && a.OrgLocationGUID == OrgLocation.GUID);
            if (!Exist)
            {
                UserRole UserRole = new UserRole()
                {
                    UserGUID = user.GUID,
                    ExpireDate = ExpireDate,
                    RoleGUID = role.GUID,
                    OrgLocationGUID = OrgLocation.GUID
                };
                db.UserRole.Add(UserRole);
            }
        }

        #endregion

        await SaveChangesAsync(cancellationToken);

        return (user.GUID, OrgLocation.GUID);
        ;
    }

    public async Task<Guid> GetOrInsertUserWithUserRole(string Username, string Password,
        string FirstName, string LastName, string Email, string Mobile,
        Guid OrgLocationId, long RoleGkey, DateTime ExpireDate, CancellationToken cancellationToken)
    {
        User? user = db.User.FirstOrDefault(a => a.Username == Username);
        if (user == null)
        {
            bool MobileExist = Table.Any(a => a.Mobile == Mobile);
            if (MobileExist)
            {
                throw new ServiceExeption("موبایل وارد شده قبلا ثبت شده است.");
            }

            if (!string.IsNullOrWhiteSpace(Email))
            {
                bool EmailExist = Table.Any(a => a.Email == Email);
                if (EmailExist)
                {
                    throw new ServiceExeption("ایمیل وارد شده قبلا ثبت شده است.");
                }
            }

            user = new User();
            user.Username = Username?.ToLower().Trim();
            user.Email = Email?.ToLower().Trim();
            user.Mobile = Mobile?.ToLower().Trim();
            user.Password = Password;
            user.FirstName = FirstName?.ToLower().Trim();
            user.LastName = LastName?.ToLower().Trim();
            user.Password = HashPassword(user.GUID, user.Password);

            Table.Add(user);
        }

        var Role = await db.Role.FirstOrDefaultAsync(hh => hh.Gkey == RoleGkey);

        if (Role == null)
        {
            throw new ServiceExeption("نقش مشخص شده یافت نشد!");
        }
        else
        {
            if (Role.OrgLocationGUID != null &&
                !await orgLocationService.HasAccessToChild(Role.OrgLocationGUID.Value, OrgLocationId))
            {
                throw new ServiceExeption("نقش انتخابی در دسترسی نیست!");
            }

            if (db.UserRole.Any(hh =>
                    hh.UserGUID == user.GUID && hh.RoleGUID == Role.GUID && hh.OrgLocationGUID == OrgLocationId))
            {
                var existedUserRole = await db.UserRole.FirstOrDefaultAsync(hh =>
                    hh.UserGUID == user.GUID && hh.RoleGUID == Role.GUID && hh.OrgLocationGUID == OrgLocationId);
                existedUserRole.ExpireDate = ExpireDate;
                db.UserRole.Update(existedUserRole);
            }
            else
            {
                var userRole = new UserRole()
                {
                    UserGUID = user.GUID,
                    OrgLocationGUID = OrgLocationId,
                    RoleGUID = Role.GUID,
                    ExpireDate = ExpireDate
                };

                db.UserRole.Add(userRole);
            }
        }

        await SaveChangesAsync(cancellationToken);

        return user.GUID;
    }

    public async Task<bool> ChangePassword(Guid UserGUID, string OldPassword, string Password, bool CheckOldPassword)
    {
        if (Password.Length < 12)
        {
            throw new ServiceExeption("رمز عبور باید حداقل 12 کاراکتر باشد");
        }

        bool PasswordBlock = db.PasswordBlockList.Any(a => a.Password == Password);

        if (PasswordBlock)
        {
            throw new ServiceExeption("رمز عبور دارای امنیت کافی نمی باشد");
        }

        User user = db.User.First(a => a.GUID == UserGUID);

        if (CheckOldPassword == true && user.Password != HashPassword(UserGUID, OldPassword))
        {
            throw new ServiceExeption("رمز عبور فعلی معتبر نمی باشد");
        }

        user.Password = HashPassword(user.GUID, Password);
        user.ForceChangePassword = false;
        await db.SaveChangesAsync(cancellationToken: CancellationToken.None);

        return true;
    }
}