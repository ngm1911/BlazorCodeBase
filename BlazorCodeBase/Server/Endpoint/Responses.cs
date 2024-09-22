using BlazorCodeBase.Server.Model.Common;
using BlazorCodeBase.Shared;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Net;

namespace BlazorCodeBase.Server.Endpoint
{
    public static class Responses
    {
        public static BaseResponse OK => new();
        public static BaseResponse LockedOut => new(statusCode: HttpStatusCode.BadRequest, message: "User was locked");
        public static BaseResponse RequiresTwoFactor => new(statusCode: HttpStatusCode.OK, message: "Requires 2FA");
        public static BaseResponse UserNotFound => new(statusCode: HttpStatusCode.NotFound, message: "User is not found");
        public static BaseResponse UnAuthorized => new(statusCode: HttpStatusCode.Unauthorized, message: "UnAuthorized");
        public static BaseResponse NeedConfirmEmail => new(statusCode: HttpStatusCode.BadRequest, message: "Need confirm email");
    }
}
