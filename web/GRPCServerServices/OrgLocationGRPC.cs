using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Repository.Services;
using Domain.Entities;
using FluentFTP.Helpers;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Newtonsoft.Json;
using System;
using System.IO;

namespace web.GRPCServerServices;

public class OrgLocationGRPC : OrgLocationGRPCService.OrgLocationGRPCServiceBase
{
     private IApplicationDbContext db;
    private readonly OrgLocationService orgLocationService;

    public OrgLocationGRPC(IApplicationDbContext db, OrgLocationService orgLocationService)
    {
        this.db = db;
        this.orgLocationService = orgLocationService;
    }



    public override async Task<GetOrgLocationInfoReply> GetOrgLocationInfo(GetOrgLocationInfoRequest request, ServerCallContext context)
    {
        Guid OrgLocationGUID = Guid.Parse(request.OrgLocationGUID);
        OrgLocation orgLocation = db.OrgLocation.First(a => a.GUID == OrgLocationGUID);
        OrgLocation Organization = new OrgLocation();

        var parts = orgLocation.Path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        long OrganizationGkey = 0;

        if (parts.Length >= 2)
        {
            OrganizationGkey = long.Parse(parts[1]);
            Organization = db.OrgLocation.First(a => a.Gkey == OrganizationGkey);
        }
        else
        {
            Organization = orgLocation;
        }

        GetOrgLocationInfoReply GetOrgLocationInfoReply = new GetOrgLocationInfoReply()
        {
            Address = orgLocation.Address ?? "",
            Email = orgLocation.Email ?? "",
            Fax = orgLocation.Fax ?? "",
            Gkey = orgLocation.Gkey,
            GUID = orgLocation.GUID.ToString(),
            Level = orgLocation.Level,
            Name = orgLocation.Name,
            OrgLocationPath = orgLocation.Path,
            POrgLocationGUID = orgLocation.POrgLocationGUID.ToString(),
            Tell = orgLocation.Tell ?? "",
            Url = orgLocation.Url ?? "",
            OrganizationGUID = Organization.GUID.ToString(),
            OrganizationName = orgLocation.Name,

        };
        return GetOrgLocationInfoReply;
    }

    public async override Task<HasAccessToChildReply> HasAccessToChild(HasAccessToChildRequest request, ServerCallContext context)
    {
        bool HasAccess = await orgLocationService.HasAccessToChild(request.ParentOrgLocationGUID, request.ChildOrgLocationGUID);

        return new HasAccessToChildReply() { HasAccess = HasAccess };
    }

    public async override Task<GetChildsReply> GetChilds(GetChildsRequest request, ServerCallContext context)
    {
        string JsonChilds = "[]";
        Guid OrgLocationGuid = Guid.Parse(request.OrgLocationGUID);
        OrgLocation? ParentOrgLocation = db.OrgLocation.FirstOrDefault(a => a.GUID == OrgLocationGuid);

        if (ParentOrgLocation != null)
        {
            List<Guid> Childs = db.OrgLocation.Where(a => a.Path.StartsWith(ParentOrgLocation.Path)).Select(a => a.GUID).ToList();
            JsonChilds = JsonConvert.SerializeObject(Childs);
        }

        return new GetChildsReply() { ChildOrgLocations = JsonChilds };
    }
}