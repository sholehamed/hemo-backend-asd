using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Repository.Services;
using Domain.Entities;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Newtonsoft.Json;
using Role = Web.Endpoints.Role;

namespace web.GRPCServerServices;

public class TokenGRPC : TokenGRPCService.TokenGRPCServiceBase
{
    private UserService UserService;

    public TokenGRPC(UserService userService)
    {
        UserService = userService;
    }

    public async override Task<CheckTokenReply> CheckToken(CheckTokenRequest request, ServerCallContext context)
    {
        return new CheckTokenReply()
        {
            IsValid = UserService.CheckToken(request.Token)
        };
    }
}