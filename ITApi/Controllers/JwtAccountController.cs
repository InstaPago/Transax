using InstaTransfer.BLL.Concrete;
using InstaTransfer.DataAccess;
using ITApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Security.Cryptography;
using Microsoft.AspNet.Identity.Owin;
using System.Threading.Tasks;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using InstaTransfer.ITExceptions.Api;
using InstaTransfer.ITResources.Api;
using InstaTransfer.ITResources.Constants;
using InstaTransfer.ITResources.Enums;
using System.Web.Http.ModelBinding;
using System.Configuration;
using JWT;
using InstaTransfer.BLL.Models;
using JWT.Algorithms;
using JWT.Serializers;
using System.Web.Http.Description;

namespace ITApi.Controllers
{
    /// <summary>
    /// Controlador de las cuentas autenticadas por Json Web Tokens
    /// </summary>
    [Authorize(Roles = UserRoleConstant.CommerceApiUser)]
    [RoutePrefix("api")]
    public class JwtAccountController : ApiController
    {
        URepository<AspNetUser> ANUserRepo = new URepository<AspNetUser>();
        private ApplicationUserManager _userManager;
        BaseErrorResponse _baseErrorResponse;
        BaseSuccessResponse _baseSuccessResponse;

        /// <summary>
        /// User Manager del controlador
        /// </summary>
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        // POST api/login
        /// <summary>
        /// Inicia sesion y retorna el token de acceso
        /// </summary>
        /// <param name="model">Credenciales del usuario</param>
        /// <returns>Respuesta que incluye el token de acceso</returns>
        [AllowAnonymous]
        [Route("login")]
        [ResponseType(typeof(LoginPostResponse))]
        [HttpPost]
        public async Task<HttpResponseMessage> Login(LoginModel model)
        {
            HttpResponseMessage response = null;
            string token = null;
            var errorList = new List<string>();

            // Validar si el model es null

            if (!ModelState.IsValid)
            {
                foreach (ModelState modelState in ModelState.Values)
                {
                    foreach (ModelError error in modelState.Errors)
                    {
                        if (error.ErrorMessage != string.Empty)
                        {
                            errorList.Add(error.ErrorMessage);
                        }
                    }
                }
                _baseErrorResponse = new BaseErrorResponse(errorList, null);
                var badresult = new
                {
                    success = _baseErrorResponse.Success,
                    message = _baseErrorResponse.Message,
                    token = _baseErrorResponse.ResponseObject
                };
                return Request.CreateResponse(HttpStatusCode.BadRequest, badresult);
            }

            try
            {
                // Obtenemos los valores para las validaciones respectivas
                var existingUser = ANUserRepo.GetAllRecords(u => u.Email == model.email).FirstOrDefault();
                var dbUser = await UserManager.FindAsync(model.email, model.password);
                var cUser = GetCUser(existingUser);
                var cUserTestMode = cUser.TestMode;
                var commerceStatus = cUser.Commerce.CommerceStatus.Id;
                var cUserStatus = cUser.CUserStatus.Id;

                #region Validaciones

                // Validamos que exista un usuario en la base de datos
                if (existingUser == null)
                {
                    response = Request.CreateResponse(HttpStatusCode.NotFound);
                }
                // Validamos que las credenciales devuelvan a un usuario existente
                if (dbUser == null)
                {
                    throw new InvalidUserException(ApiErrors.InvalidUserExceptionMessage, ApiErrors.InvalidUserExceptionCode);
                }
                // Validamos el rol del usuario
                var isApiUser = await UserManager.IsInRoleAsync(dbUser.Id, UserRoleConstant.CommerceApiUser);
                if (!isApiUser)
                {
                    throw new InvalidUserRoleException(ApiErrors.InvalidUserRoleExceptionMessage, ApiErrors.InvalidUserRoleExceptionCode);
                }
                // Validamos que el usuario este activo
                else if (cUserStatus == (int)CommerceUserStatus.Inactive)
                {
                    throw new InactiveUserException(ApiErrors.InactiveUserExceptionMessage, ApiErrors.InactiveUserExceptionCode);
                }
                // Validamos el estado del comercio
                else if (commerceStatus == (int)InstaTransfer.ITResources.Enums.CommerceStatus.Inactive)
                {
                    if (cUserTestMode == false)
                    {
                        throw new InvalidUserTestModeException(ApiErrors.InvalidUserTestModeExceptionMessage, ApiErrors.InvalidUserTestModeExceptionCode);
                    }

                }
                #endregion

                var oUser = (object)dbUser;
                token = CreateTokenIdentity(existingUser, out oUser);
                // Construimos la respuesta base
                _baseSuccessResponse = new BaseSuccessResponse(ApiResources.OperationSuccessMessage);
                // Construimos el formato del resultado
                var result = new
                {
                    success = _baseSuccessResponse.Success,
                    message = _baseSuccessResponse.Message,
                    token
                };
                response = Request.CreateResponse(HttpStatusCode.OK, result);
                return response;
            }
            catch (InvalidUserException e)
            {
                _baseErrorResponse = new BaseErrorResponse(ApiErrors.InvalidUserExceptionMessage, e.ErrorCode);
                var badresult = new
                {
                    success = _baseErrorResponse.Success,
                    message = _baseErrorResponse.ResponseMessage,
                    token
                };
                return Request.CreateResponse(HttpStatusCode.Unauthorized, badresult);
            }
            catch (InvalidUserRoleException e)
            {
                _baseErrorResponse = new BaseErrorResponse(ApiErrors.InvalidUserRoleExceptionMessage, e.ErrorCode);
                var badresult = new
                {
                    success = _baseErrorResponse.Success,
                    message = _baseErrorResponse.ResponseMessage,
                    token
                };
                return Request.CreateResponse(HttpStatusCode.Unauthorized, badresult);
            }
            catch (InactiveUserException e)
            {
                _baseErrorResponse = new BaseErrorResponse(ApiErrors.InactiveUserExceptionMessage, e.ErrorCode);
                var badresult = new
                {
                    success = _baseErrorResponse.Success,
                    message = _baseErrorResponse.ResponseMessage,
                    token
                };
                return Request.CreateResponse(HttpStatusCode.Unauthorized, badresult);
            }
            catch (InvalidUserTestModeException e)
            {
                _baseErrorResponse = new BaseErrorResponse(ApiErrors.InvalidUserTestModeExceptionMessage, e.ErrorCode);
                var badresult = new
                {
                    success = _baseErrorResponse.Success,
                    message = _baseErrorResponse.ResponseMessage,
                    token
                };
                return Request.CreateResponse(HttpStatusCode.Unauthorized, badresult);
            }
            catch (Exception)
            {
                _baseErrorResponse = new BaseErrorResponse(ApiErrors.OperationErrorMessage, ApiErrors.OperationErrorCode);
                var badresult = new
                {
                    success = _baseErrorResponse.Success,
                    message = _baseErrorResponse.ResponseMessage,
                    token
                };
                return Request.CreateResponse(HttpStatusCode.InternalServerError, badresult);
            }
        }

        private static CUser GetCUser(AspNetUser user)
        {
            // Inicializamos las variables
            URepository<CUser> CURepo = new URepository<CUser>();
            // Obtenemos el registro de la base de datos
            var cUser = CURepo.GetCUser(user.Id);
            // Retornamos el usuario
            return cUser;
        }

        /// <summary>
        /// Crea un <see cref="SecurityToken"/> a partir de un usuario.
        /// </summary>
        /// <param name="user">Usuario</param>
        /// <param name="dbUser">Usuario de la base de datos</param>
        /// <returns>Token y usuario del token</returns>
        private static string CreateTokenIdentity(AspNetUser user, out object dbUser)
        {
            // SecurityKey
            var plainTextSecurityKey = "This is my shared, not so secret, secret!";
            // SignInCredentials
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(plainTextSecurityKey));
            var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256Signature);
            // Probar con varios roles
            var roleList = user.AspNetUserRoles.ToList();
            var roleNames = roleList.Select(r => r.AspNetRole.Name).ToList();
            // Expiration
            var now = DateTime.UtcNow;
            var minutes = int.Parse(ConfigurationManager.AppSettings[ApiResources.AppSettingsKeyJWTExpiration]);
            // Subject
            var claimsIdentity = new ClaimsIdentity(new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Email, ClaimValueTypes.Email),
                new Claim(ApiResources.RifClaim,  user.CUsers.FirstOrDefault().RifCommerce.Trim()),
                new Claim(ApiResources.CUserClaim, (user.CUsers.FirstOrDefault().Id).ToString()),
                new Claim(ApiResources.CUserTestModeClaim, (user.CUsers.FirstOrDefault().TestMode).ToString(), ClaimValueTypes.Boolean),
                new Claim(ApiResources.CommerceStatusClaim, user.CUsers.FirstOrDefault().Commerce.CommerceStatus.Id.ToString(), ClaimValueTypes.Integer)
            });

            // Roles
            foreach (var roleName in roleNames)
            {
                claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, roleName));
            }

            // Descriptor
            var securityTokenDescriptor = new SecurityTokenDescriptor()
            {
                Audience = "http://my.website.com",
                Issuer = "http://my.tokenissuer.com",
                Subject = claimsIdentity,
                Expires = now.AddMinutes(minutes),
                SigningCredentials = signingCredentials
            };

            // Create Token
            var tokenHandler = new JwtSecurityTokenHandler();
            var plainToken = tokenHandler.CreateToken(securityTokenDescriptor);
            var signedAndEncodedToken = tokenHandler.WriteToken(plainToken);

            // Usuario asignado al token
            dbUser = new { user.Email, user.Id };

            return signedAndEncodedToken;
        }


        #region Obsolete

        [Obsolete("Se utilizan las librerias de microsoft")]
        /// <summary>
        /// Create a Jwt with user information
        /// </summary>
        /// <param name="user"></param>
        /// <param name="dbUser"></param>
        /// <returns></returns>
        private static string CreateToken(AspNetUser user, out object dbUser)
        {
            var unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            // Todo (CreateToken): Tomar desde el web.config
            var minutes = int.Parse(ConfigurationManager.AppSettings[ApiResources.AppSettingsKeyJWTExpiration]);
            //var expiry = Math.Round((DateTime.UtcNow.AddHours(2) - unixEpoch).TotalSeconds);
            var expiry = Math.Round((DateTime.UtcNow.AddMinutes(minutes) - unixEpoch).TotalSeconds);
            var issuedAt = Math.Round((DateTime.UtcNow - unixEpoch).TotalSeconds);
            var notBefore = Math.Round((DateTime.UtcNow.AddMonths(6) - unixEpoch).TotalSeconds);


            var payload = new Dictionary<string, object>
            {
                {"email", user.Email},
                {"userId", user.Id},
                {"role", "Admin"  },
                {"sub", user.Id},
                {"nbf", notBefore},
                {"iat", issuedAt},
                {"exp", expiry}
            };

            //var secret = ConfigurationManager.AppSettings.Get("jwtKey");
            const string apikey = "secretKey";


            IJwtAlgorithm algorithm = new HMACSHA256Algorithm();
            IJsonSerializer serializer = new JsonNetSerializer();
            IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
            IJwtEncoder encoder = new JwtEncoder(algorithm, serializer, urlEncoder);

            var token = encoder.Encode(payload, apikey);

            dbUser = new { user.Email, user.Id };
            return token;
        }

        [Obsolete("No hace falta")]
        /// <summary>
        ///     Creates a random salt to be used for encrypting a password
        /// </summary>
        /// <returns></returns>
        public static string CreateSalt()
        {
            var data = new byte[0x10];
            using (var cryptoServiceProvider = new RNGCryptoServiceProvider())
            {
                cryptoServiceProvider.GetBytes(data);
                return Convert.ToBase64String(data);
            }
        }

        [Obsolete("No hace falta tampoco")]
        /// <summary>
        ///     Encrypts a password using the given salt
        /// </summary>
        /// <param name="password"></param>
        /// <param name="salt"></param>
        /// <returns></returns>
        public static string EncryptPassword(string password, string salt)
        {
            using (var sha256 = SHA256.Create())
            {
                var saltedPassword = string.Format("{0}{1}", salt, password);
                var saltedPasswordAsBytes = Encoding.UTF8.GetBytes(saltedPassword);
                return Convert.ToBase64String(sha256.ComputeHash(saltedPasswordAsBytes));
            }
        }

        #endregion


    }
}
