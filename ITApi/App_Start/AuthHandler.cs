using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;
using JWT;
using System.IdentityModel.Tokens.Jwt;
using System.IdentityModel.Tokens;
using System.ServiceModel.Security.Tokens;
using System.Text;
using ITApi.Models;
using InstaTransfer.ITResources.Api;
using InstaTransfer.ITResources.Enums;
using InstaTransfer.ITExceptions.Api;
using InstaTransfer.BLL.Models;
using JWT.Serializers;

namespace ITApi
{
    public class AuthHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
                    CancellationToken cancellationToken)
        {
            HttpResponseMessage errorResponse = null;
            BaseErrorResponse _baseErrorResponse;

            try
            {
                IEnumerable<string> authHeaderValues;
                request.Headers.TryGetValues("Authorization", out authHeaderValues);


                if (authHeaderValues == null)
                    return base.SendAsync(request, cancellationToken); // cross fingers

                var bearerToken = authHeaderValues.ElementAt(0);
                var token = bearerToken.StartsWith("Bearer ") ? bearerToken.Substring(7) : bearerToken;

                //var secret = ConfigurationManager.AppSettings.Get("jwtKey");
                var secret = "This is my shared, not so secret, secret!";



                // Validamos el token
                Thread.CurrentPrincipal = ValidateToken(
                    token,
                    secret,
                    true
                    );

                if (HttpContext.Current != null)
                {
                    HttpContext.Current.User = Thread.CurrentPrincipal;
                }
            }
            catch (InvalidUserTestModeException e)
            {
                _baseErrorResponse = new BaseErrorResponse(ApiErrors.InvalidUserTestModeExceptionMessage, e.ErrorCode);
                var badresult = new
                {
                    success = _baseErrorResponse.Success,
                    message = _baseErrorResponse.ResponseMessage,
                };
                errorResponse = request.CreateResponse(HttpStatusCode.Unauthorized, badresult);
            }
            catch (SignatureVerificationException e)
            {
                _baseErrorResponse = new BaseErrorResponse(ApiErrors.SignatureVerificationExceptionMessage, ApiErrors.SignatureVerificationExceptionCode);
                var badresult = new
                {
                    success = _baseErrorResponse.Success,
                    message = _baseErrorResponse.ResponseMessage
                };
                errorResponse = request.CreateResponse(HttpStatusCode.Unauthorized, badresult);
            }
            catch (Exception ex)
            {
                errorResponse = request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }


            return errorResponse != null
                ? Task.FromResult(errorResponse)
                : base.SendAsync(request, cancellationToken);
        }

        private static ClaimsPrincipal ValidateToken(string token, string secret, bool checkExpiration)
        {

            // Identity
            //var tokenHandler = new JwtSecurityTokenHandler();

            //var validationsParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            //{
            //    ValidateLifetime = false,
            //    ValidAudience = jwtAudience,
            //    ValidIssuer = jwtIssuer,
            //    ValidateIssuer = false,
            //    ValidateAudience = false,              
            //};
            //Microsoft.IdentityModel.Tokens.SecurityToken validatedToken;
            //var principal = tokenHandler.ValidateToken(token, validationsParameters, out validatedToken);


            IJsonSerializer serializer = new JsonNetSerializer();
            IDateTimeProvider provider = new UtcDateTimeProvider();
            IJwtValidator validator = new JwtValidator(serializer, provider);
            IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
            IJwtDecoder decoder = new JwtDecoder(serializer, validator, urlEncoder);

            //var payloadJson = decoder.Decode(token, secret, verify: true);
            var payloadData = decoder.DecodeToObject<IDictionary<string, object>>(token, secret, true);

            object exp;
            if (payloadData != null && (checkExpiration && payloadData.TryGetValue("exp", out exp)))
            {
                var validTo = FromUnixTime(long.Parse(exp.ToString()));
                if (DateTime.Compare(validTo, DateTime.UtcNow) <= 0)
                {
                    throw new Exception(
                        string.Format("Token is expired. Expiration: '{0}'. Current: '{1}'", validTo, DateTime.UtcNow));
                }
            }
            object isUserTestMode;
            object commerceStatus;
            if (payloadData.TryGetValue(ApiResources.CommerceStatusClaim, out commerceStatus) && payloadData.TryGetValue(ApiResources.CUserTestModeClaim, out isUserTestMode))
            {
                if (Convert.ToInt32(commerceStatus) == (int)CommerceStatus.Inactive && 
                   ((bool)isUserTestMode == false))
                {
                    throw new InvalidUserTestModeException(ApiErrors.InvalidUserTestModeExceptionMessage, ApiErrors.InvalidUserTestModeExceptionCode);
                }
            }
                

            var subject = new ClaimsIdentity("Federation", ClaimTypes.Name, ClaimTypes.Role);

            var claims = new List<Claim>();

            if (payloadData != null)
                foreach (var pair in payloadData)
                {
                    var claimType = pair.Key;

                    var source = pair.Value as ArrayList;

                    if (source != null)
                    {
                        claims.AddRange(from object item in source
                                        select new Claim(claimType, item.ToString(), ClaimValueTypes.String));

                        continue;
                    }

                    switch (pair.Key)
                    {
                        case "name":
                            claims.Add(new Claim(ClaimTypes.Name, pair.Value.ToString(), ClaimValueTypes.String));
                            break;
                        case "surname":
                            claims.Add(new Claim(ClaimTypes.Surname, pair.Value.ToString(), ClaimValueTypes.String));
                            break;
                        case "email":
                            claims.Add(new Claim(ClaimTypes.Email, pair.Value.ToString(), ClaimValueTypes.Email));
                            break;
                        case "role":
                            claims.Add(new Claim(ClaimTypes.Role, pair.Value.ToString(), ClaimValueTypes.String));
                            break;
                        case "userId":
                            claims.Add(new Claim(ClaimTypes.UserData, pair.Value.ToString(), ClaimValueTypes.Integer));
                            break;
                        default:
                            claims.Add(new Claim(claimType, pair.Value.ToString(), ClaimValueTypes.String));
                            break;
                    }
                }

            subject.AddClaims(claims);
            return new ClaimsPrincipal(subject);
        }

        private static DateTime FromUnixTime(long unixTime)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddSeconds(unixTime);
        }
    }
}