using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Domain.Attributes;
using Domain.Common;
using Generator.Enums;
using LibGit2Sharp;

namespace Generator;

public class GeneratorTool
{
    private readonly bool _overrideCreate = false;
    private readonly bool _overrideDataConfig = false;
    private readonly bool _overrideDelete = false;
    private readonly bool _overrideEndPoints = false;
    private readonly bool _overrideUpdate = false;
    private List<Type> _entityTypes;
    private bool _overrideBindPaging = false;
    private bool _overrideFindAll = false;
    private bool _overrideFindFirst = false;

    public GeneratorTool()
    {
        SetEntityType();
    }

    public void GenerateCustomeCommandQuerys(CommandQuery commandQuery, CommandType commandType, QueryType queryType,
        string TableName, string Name)
    {
        switch (commandQuery)
        {
            case CommandQuery.Command:
            {
                queryType = QueryType.None;
                break;
            }
            case CommandQuery.Query:
            {
                commandType = CommandType.None;
                break;
            }
        }

        var entityType = _entityTypes.First(a => a.Name == TableName);
        List<AppEntity> appEntities = GetAppEntity(entityType, false);

        switch (commandQuery)
        {
            case CommandQuery.Command:
            {
                var CommandsFolder = CreateSubFolder("Commands", TableName);
                switch (commandType)
                {
                    case CommandType.Create:
                    {
                        GenerateCreateCommand(CommandsFolder, TableName, appEntities, TableName + Name);
                        break;
                    }
                    case CommandType.Update:
                    {
                        GenerateUpdateCommand(CommandsFolder, TableName, appEntities, TableName + Name);
                        break;
                    }
                    case CommandType.Delete:
                    {
                        GenerateDeleteCommand(CommandsFolder, TableName, TableName + Name);
                        break;
                    }
                }

                break;
            }
            case CommandQuery.Query:
            {
                var QueriesFolder = CreateSubFolder("Queries", TableName);
                switch (queryType)
                {
                    case QueryType.Paging:
                    {
                        GeneratePaginationQueryCommand(QueriesFolder, TableName, appEntities, TableName + Name);
                        break;
                    }
                    case QueryType.All:
                    {
                        GenerateFindAllQuery(QueriesFolder, TableName, appEntities, TableName + Name);
                        break;
                    }
                    case QueryType.TreeFindAll:
                    {
                        GenerateFindAllTreeQuery(QueriesFolder, TableName, appEntities, TableName + Name);
                        break;
                    }
                    case QueryType.First:
                    {
                        GenerateFindByIdQuery(QueriesFolder, TableName, appEntities, TableName + Name);
                        break;
                    }
                }

                break;
            }
        }
    }

    public void GeneratorDefaultClasses()
    {
        GenerateEndPoints();
        GenerateCommandQuerys();
        GenerateDataConfigurations();
        GenerateIApplicationDbContext();
        GenerateRepositoryServices();
    }

    private bool AutoCommit()
    {
        var repoPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
            "../../../..");
        using (var repo = new Repository(repoPath))
        {
            var status = repo.RetrieveStatus();

            if (status.IsDirty)
            {
                Commands.Stage(repo, "*");
                var author = new Signature("AutoCommit", "AutoCommit@example.com", DateTimeOffset.Now);
                repo.Commit("AutoCommit " + DateTime.Now, author, author);
            }
        }

        return true;
    }

    private void SetEntityType()
    {
        var domainAssembly = Assembly.Load("Domain");

        var baseType = typeof(BaseEntity);
        _entityTypes = domainAssembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && baseType.IsAssignableFrom(t))
            .ToList();
    }

    private bool IsNullableProperty(PropertyInfo property)
    {
        // بررسی Nullable<T> برای Value Types
        if (Nullable.GetUnderlyingType(property.PropertyType) != null)
            return true;

        // بررسی Nullable Reference Types در C# 8+
        if (!property.PropertyType.IsValueType) // فقط برای کلاس‌ها
        {
            var nullableAttr = property.GetCustomAttribute<NullableAttribute>();
            if (nullableAttr != null && nullableAttr.NullableFlags.Length > 0 && nullableAttr.NullableFlags[0] == 2)
                return true;

            var contextAttr = property.DeclaringType?.GetCustomAttribute<NullableContextAttribute>();
            if (contextAttr != null && contextAttr.Flag == 2)
                return true;
        }

        return false;
    }

    private bool IsNumericType(Type type)
    {
        switch (Type.GetTypeCode(type))
        {
            case TypeCode.Byte:
            case TypeCode.SByte:
            case TypeCode.UInt16:
            case TypeCode.UInt32:
            case TypeCode.UInt64:
            case TypeCode.Int16:
            case TypeCode.Int32:
            case TypeCode.Int64:
            case TypeCode.Decimal:
            case TypeCode.Double:
            case TypeCode.Single:
                return true;
            default:
                return false;
        }
    }

    private string GetValidator(AppEntity item, PropConfig propConfig)
    {
        var DefualtPropConfig = new PropConfig("");
        var Validator =
            $"       RuleFor(v => v.{item.propertyName}){(item.propertyType.ToLower() == "string" ? $".MaximumLength({propConfig.MaxLenght}).WithMessage(ValidatorMassege.GetMaxLengthErrorMessage(\"{propConfig.Name}\",{propConfig.MaxLenght}))" : "")}" +
            $"{(item.propertyType.ToLower() == "string" && propConfig.MinLenght > 0 ? $".MinimumLength({propConfig.MinLenght}).WithMessage(ValidatorMassege.GetMinLengthErrorMessage(\"{propConfig.Name}\",{propConfig.MinLenght}))" : "")}" +
            $"{(item.isNumericType ? $".GreaterThan({propConfig.MinNumber}).WithMessage(ValidatorMassege.GetLessThanErrorMessage(\"{propConfig.Name}\", {propConfig.MinNumber}))" : "")}" +
            $"{(item.isNumericType && DefualtPropConfig.MaxNumber != propConfig.MaxNumber ? $".LessThan({propConfig.MaxNumber}).WithMessage(ValidatorMassege.GetGreaterThanErrorMessage(\"{propConfig.Name}\", {propConfig.MaxNumber}))" : "")}" +
            $"{(item.isNullable ? "" : $".NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage(\"{propConfig.Name}\"))")};";

        return Validator;
    }

    private string GetPropConfigString(PropConfig PropConfig)
    {
        var DefualtPropConfig = new PropConfig("");
        var value = $"[PropConfig(\"{PropConfig.Name}\"" +
                    $"{(DefualtPropConfig.MaxLenght == PropConfig.MaxLenght ? "" : $",MaxLenght:{PropConfig.MaxLenght}")}" +
                    $"{(DefualtPropConfig.MinLenght == PropConfig.MinLenght ? "" : $",MinLenght:{PropConfig.MinLenght}")}" +
                    $"{(DefualtPropConfig.MaxNumber == PropConfig.MaxNumber ? "" : $",MaxNumber:{PropConfig.MaxNumber}")}" +
                    $"{(DefualtPropConfig.MinNumber == PropConfig.MinNumber ? "" : $",MinNumber:{PropConfig.MinNumber}")}" +
                    $")]";

        return value;
    }

    public static string GetFriendlyTypeName(Type type)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            // اگر نوع Nullable باشد، نام نوع اصلی را همراه با ? برمی‌گردانیم
            return $"{GetFriendlyTypeName(Nullable.GetUnderlyingType(type))}";

        // اگر نوع آرایه باشد، نام نوع اصلی را با [] برمی‌گردانیم
        if (type.IsArray) return $"{GetFriendlyTypeName(type.GetElementType())}[]";

        // اگر نوع ژنریک باشد (مانند List<T>)، نام نوع را به شکل مناسب برمی‌گردانیم
        if (type.IsGenericType)
        {
            var genericTypeName = type.Name.Split('`')[0]; // نام نوع اصلی (مانند List)
            var genericArguments = type.GetGenericArguments() // آرگومان‌های ژنریک
                .Select(GetFriendlyTypeName) // نام نوع‌های ژنریک را به شکل مناسب دریافت می‌کنیم
                .ToArray();
            return $"{genericTypeName}<{string.Join(", ", genericArguments)}>";
        }

        // برای نوع‌های ساده، نام نوع را به شکل lowercase برمی‌گردانیم
        switch (type.Name)
        {
            case "String": return "string";
            case "Int32": return "int";
            case "Int64": return "long";
            case "Boolean": return "bool";
            case "Decimal": return "decimal";
            case "Double": return "double";
            case "Single": return "float";
            case "Guid": return "Guid";
            case "DateTime": return "DateTime";
            case "Object": return "object";
            default: return type.Name; // برای سایر نوع‌ها، نام اصلی را برمی‌گردانیم
        }
    }

    private List<AppEntity> GetAppEntity(Type entityType, bool FindAll)
    {
        List<AppEntity> AppEntitys = new();
        List<string> InValidFields = new()
        {
            "Created", "CreatedBy", "CreatedByIP", "LastModified", "LastModifiedBy", "DomainEvents", "LastModifiedByIP",
            "IsDelete","Deleted","DeletedBy","DeletedByIP","IsDelete","Deactivated","DeactivatedBy","DeactivatedByIP","IsDeactivated"
        };
        foreach (var property in entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            if (!property.GetGetMethod().IsVirtual)
            {
                var propConfig = GetPropConfig(property);
                var propertyName = property.Name;
                var propertyType = GetFriendlyTypeName(property.PropertyType);


                var isNullable = IsNullableProperty(property);
                var isNumericType = IsNumericType(property.PropertyType);
                if (!InValidFields.Any(a => a == propertyName) || FindAll)
                {
                    if (propertyName == "DomainEvents" || (!propConfig.AddToDTO && !FindAll)) continue;

                    AppEntitys.Add(new AppEntity
                    {
                        propertyName = propertyName,
                        propertyType = propertyType,
                        isNullable = isNullable,
                        PropertyInfo = property,
                        isNumericType = isNumericType
                    });
                }
            }

        return AppEntitys;
    }

    private string GetClassName(Type entityType)
    {
        var attribute = entityType.GetCustomAttribute<ClassName>();
        if (attribute != null) return attribute.Name;

        return "";
    }

    private PropConfig GetPropConfig(PropertyInfo Property)
    {
        var attribute = Property.GetCustomAttribute<PropConfig>();
        return attribute;
    }

    private void GenerateIApplicationDbContext()
    {
        var _Path = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
            "../../../../Application/Common/Interfaces/");
        List<string> Data = new();
        var DbSetContainer = new StringBuilder();

        foreach (var type in _entityTypes)
        {
            var TableName = type.Name.Replace("Domain.Entities.", "");
            DbSetContainer.AppendLine($"     public  DbSet<{TableName}> {TableName} {{ get; set;}}");
        }

        Data.Add(@$"
namespace Application.Common.Interfaces;
public interface  IApplicationDbContext
{{
{DbSetContainer}
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}}
");
        var FilePath = Path.Combine(_Path, "IApplicationDbContext.cs");
        File.WriteAllLines(FilePath, Data);
    }

    private void GenerateCommandQuerys()
    {
        foreach (var entityType in _entityTypes)
        {
            GenerateCommandQuerysByEntityType(entityType);
            GenerateEndPoints();
            GenerateDataConfigurations();
        }
    }

    private void GenerateCommandQuerysByEntityType(Type entityType)
    {
        List<AppEntity> appEntities = GetAppEntity(entityType, false);
        var TableName = entityType.Name.Replace("Domain.Entities.", "");

        var CommandsFolder = CreateSubFolder("Commands", TableName);
        var EventHandlersFolder = CreateSubFolder("EventHandlers", TableName);
        var QueriesFolder = CreateSubFolder("Queries", TableName);


        GenerateCreateCommand(CommandsFolder, TableName, appEntities, "Create" + TableName);
        GenerateUpdateCommand(CommandsFolder, TableName, appEntities, "Update" + TableName);
        GenerateDeleteCommand(CommandsFolder, TableName, "Delete" + TableName);
        GeneratePaginationQueryCommand(QueriesFolder, TableName, appEntities, TableName + "WithPagination");
        GenerateFindAllQuery(QueriesFolder, TableName, appEntities, TableName + "FindAll");
        GenerateFindByIdQuery(QueriesFolder, TableName, appEntities, TableName + "FindById");
    }

    private void GenerateEndPoints()
    {
        var _Path = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
            "../../../../web/Endpoints/");

        if (!Directory.Exists(_Path)) Directory.CreateDirectory(_Path);

        foreach (var entityType in _entityTypes)
        {
            var TableName = entityType.Name;
            var PersianName = GetClassName(entityType);
            List<string> Data = new();


            Data.Add(@$"
using Application.Common.Models;
using Application.Core.{TableName}_core.Commands.Create{TableName};
using Application.Core.{TableName}_core.Commands.Update{TableName};
using Application.Core.{TableName}_core.Commands.Delete{TableName};
using Application.Core.{TableName}_core.Queries;
using Microsoft.AspNetCore.Http.HttpResults;
using NSwag.Annotations;

namespace Web.Endpoints;
public class {TableName} : EndpointGroupBase
{{
    public override void Map(WebApplication app)
    {{
        app.MapGroup(this)
            //.RequireAuthorization()
            .MapGet({TableName}FindAll,""FindAll"")
            .MapPost(Get{TableName}WithPagination,""BindPaging"")
            .MapPost(Create{TableName})
            .MapPut(Update{TableName})
            .MapGet(Get{TableName}ById, ""{{Guid}}"")
            .MapDelete(Delete{TableName}, ""{{Guid}}"")
              ;
    }}

    [OpenApiOperation( """",""نمایش کامل لیست {PersianName} ها"")]
    public async Task<Ok<List<{TableName}FindAllDTO>>> {TableName}FindAll(ISender sender)
    {{
        var result = await sender.Send(new {TableName}FindAllQuery());
        return TypedResults.Ok(result);
    }}

    [OpenApiOperation( """",""نمایش صفحه بندی شده ی لیست {PersianName} ها"")]
    public async Task<Ok<PaginatedList<{TableName}WithPaginationDTO>>> Get{TableName}WithPagination(ISender sender,  {TableName}WithPaginationQuery query)
    {{
        var result = await sender.Send(query);

        return TypedResults.Ok(result);
    }}

    [OpenApiOperation( """",""واکشی {PersianName} با کلید"")]
    public async Task<Ok<{TableName}FindByIdDTO>> Get{TableName}ById(ISender sender, Guid Guid)
    {{
       var Response=  await sender.Send(new {TableName}FindByIdQuery(Guid));
       return TypedResults.Ok(Response);
    }}

    [OpenApiOperation( """",""ثبت {PersianName} جدید"")]
    public async Task<Created<Guid>> Create{TableName}(ISender sender, Create{TableName}Command command)
    {{
        var Guid = await sender.Send(command);

        return TypedResults.Created($""/{{nameof({TableName})}}/{{Guid}}"", Guid);
    }}

    [OpenApiOperation( """",""ویرایش {PersianName}"")]
    public async Task<Results<NoContent, BadRequest>> Update{TableName}(ISender sender, Update{TableName}Command command)
    {{
        await sender.Send(command);

        return TypedResults.NoContent();
    }}

    [OpenApiOperation( """",""حذف {PersianName}"")]
    public async Task<NoContent> Delete{TableName}(ISender sender, Guid Guid)
    {{
        await sender.Send(new Delete{TableName}Command(Guid));

        return TypedResults.NoContent();
    }}
}}
");

            var FilePath = Path.Combine(_Path, $"{TableName}Endpoints.cs");
            if (!File.Exists(FilePath) | _overrideEndPoints) File.WriteAllLines(FilePath, Data);
        }
    }

    private void GenerateDataConfigurations()
    {
        var _Path = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
            "../../../../Infrastructure/Data/Configurations/");

        if (!Directory.Exists(_Path)) Directory.CreateDirectory(_Path);

        foreach (var entityType in _entityTypes)
        {
            var TableName = entityType.Name;

            List<string> Data = new();

            if (TableName == "SubSystem")
            {
                var s = "";
            }
            
            bool IsAuditedEntity = typeof(AuditedEntity).IsAssignableFrom(entityType);
            Data.Add(@$"
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class {TableName}Configuration : IEntityTypeConfiguration<{TableName}>
{{
    public void Configure(EntityTypeBuilder<{TableName}> builder)
    {{
        builder.HasKey(u=>u.GUID);
        {((IsAuditedEntity)?"builder.HasQueryFilter(u => !u.IsDelete);":"")}
");

            List<AppEntity> appEntities = GetAppEntity(entityType, true);

            foreach (var property in appEntities)
            {
                var propConfig = GetPropConfig(property.PropertyInfo);
                if ((property.propertyType == "Guid" || propConfig.IsUnique || propConfig.HasIndex) &&
                    property.propertyName != "GUID" && property.propertyName != "LastModifiedBy" &&
                    property.propertyName != "CreatedBy")
                    Data.Add(@$"
        builder.HasIndex(u=>u.{property.propertyName}){(propConfig.IsUnique ? ".IsUnique()" : "")};
");
            }

            foreach (var property in appEntities)
                if (property.propertyName != "GUID")
                {
                    var propConfig = GetPropConfig(property.PropertyInfo);
                    Data.Add(@$"
        builder.Property(t => t.{property.propertyName}){(property.propertyType.ToLower() == "string" ? $".IsUnicode().HasMaxLength({propConfig.MaxLenght})" : "")}{(property.isNullable ? "" : ".IsRequired()")};
");
                }

            Data.Add(@"
    }
}
");


            var FilePath = Path.Combine(_Path, $"{TableName}Configuration.cs");
            if (!File.Exists(FilePath) || _overrideDataConfig) File.WriteAllLines(FilePath, Data);
        }
    }

    private string CreateSubFolder(string Name, string TableName)
    {
        var _Path = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
            "../../../../Application/Core/");

        if (!Directory.Exists(_Path)) Directory.CreateDirectory(_Path);

        var SubFolder = Path.Combine(_Path, TableName + "_core");
        if (!Directory.Exists(SubFolder)) Directory.CreateDirectory(SubFolder);

        var ResultPath = Path.Combine(SubFolder, Name);
        if (!Directory.Exists(ResultPath)) Directory.CreateDirectory(ResultPath);

        return ResultPath;
    }

    private void GenerateRepositoryServices()
    {
        foreach (var entityType in _entityTypes)
        {
            var TableName = entityType.Name.Replace("Domain.Entities.", "");
            var Path = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                "../../../../Application/Repository/Services");

            if (!Directory.Exists(Path)) Directory.CreateDirectory(Path);

            List<string> Data = new();
            Data.Add($@"
namespace Application.Repository.Services;

public class {TableName}Service:BaseRepository<{TableName}>
{{
    public {TableName}Service(IApplicationDbContext context, IMapper mapper) : base(context, mapper)
    {{
    }}
}}
");

            var FilePath2 = System.IO.Path.Combine(Path, TableName + "Service.cs");
            if (!File.Exists(FilePath2) | _overrideCreate) File.WriteAllLines(FilePath2, Data);
        }
    }

    private void GenerateAngularServices(string Path, string TableName, List<AppEntity> appEntities, string FileName)
    {
        var builder = new StringBuilder();

        List<string> Data = new();
        Data.Add(@"
");

        var FolderPath = System.IO.Path.Combine(Path, FileName);
        if (!Directory.Exists(FolderPath)) Directory.CreateDirectory(FolderPath);

        var FilePath = System.IO.Path.Combine(FolderPath, FileName + "Command.cs");
    }

    private void GenerateCreateCommand(string Path, string TableName, List<AppEntity> appEntities, string FileName)
    {
        var builder = new StringBuilder();
        var ValidatorBuilder = new StringBuilder();
        foreach (var item in appEntities)
            if (item.propertyName != "GUID")
            {
                var propConfig = GetPropConfig(item.PropertyInfo);
                builder.AppendLine(
                    @$"    
   {GetPropConfigString(propConfig)}
   public {item.propertyType}{(item.isNullable ? "?" : "")} {item.propertyName} {{ get; init; }}");
                ValidatorBuilder.AppendLine(GetValidator(item, propConfig));
            }


        List<string> Data = new();
        Data.Add($@"
namespace Application.Core.{TableName}_core.Commands.{FileName};

public record {FileName}Command : IRequest<Guid>, IMapFrom<{TableName}>
{{
 {builder}

    public void Mapping(Profile profile)
    {{
        profile.CreateMap<{FileName}Command, {TableName}>();
    }}
}}

public class {FileName}CommandHandler : IRequestHandler<{FileName}Command, Guid>
{{
    private {TableName}Service {TableName}Service;
    public {FileName}CommandHandler({TableName}Service  {TableName}Service)
    {{
        this.{TableName}Service  = {TableName}Service;
    }}

    public async Task<Guid> Handle({FileName}Command request, CancellationToken cancellationToken)
    {{
        {TableName} entity = {TableName}Service.mapper.Map<{TableName}>(request);

        {TableName}Service.Table.Add(entity);

        await {TableName}Service.SaveChangesAsync(cancellationToken);

        return entity.GUID;
    }}
}}

");


        var FolderPath = System.IO.Path.Combine(Path, FileName);
        if (!Directory.Exists(FolderPath)) Directory.CreateDirectory(FolderPath);

        var FilePath = System.IO.Path.Combine(FolderPath, FileName + "Command.cs");
        if (!File.Exists(FilePath) | _overrideCreate) File.WriteAllLines(FilePath, Data);


        List<string> Data2 = new();
        Data2.Add($@"
using Application.Common.Utility.Validator;
namespace Application.Core.{TableName}_core.Commands.{FileName};

public class {FileName}Validator : AbstractValidator<{FileName}Command>
{{
    public {FileName}Validator()
    {{
{ValidatorBuilder}
    }}
}}
");

        var FilePath2 = System.IO.Path.Combine(FolderPath, FileName + "Validator.cs");
        if (!File.Exists(FilePath2) | _overrideCreate) File.WriteAllLines(FilePath2, Data2);
    }

    private void GenerateUpdateCommand(string Path, string TableName, List<AppEntity> appEntities, string FileName)
    {
        var builder = new StringBuilder();
        var ValidatorBuilder = new StringBuilder();
        foreach (var item in appEntities)
        {
            var propConfig = GetPropConfig(item.PropertyInfo);
            builder.AppendLine(
                @$" 
   {GetPropConfigString(propConfig)}
   public {item.propertyType}{(item.isNullable ? "?" : "")} {item.propertyName} {{ get; init; }}");


            ValidatorBuilder.AppendLine(GetValidator(item, propConfig));
        }


        List<string> Data = new();
        Data.Add($@"
namespace Application.Core.{TableName}_core.Commands.{FileName};

public record {FileName}Command : IRequest, IMapFrom<{TableName}>
{{
{builder}
    public void Mapping(Profile profile)
    {{
        profile.CreateMap<{FileName}Command, {TableName}>();
    }}
}}

public class {FileName}CommandHandler : IRequestHandler<{FileName}Command>
{{
    private {TableName}Service {TableName}Service;

    public {FileName}CommandHandler({TableName}Service {TableName}Service)
    {{
        this.{TableName}Service = {TableName}Service;
    }}

    public async Task Handle({FileName}Command request, CancellationToken cancellationToken)
    {{
        {TableName}? OldEntity = await {TableName}Service.Table
            .FindAsync(request.GUID, cancellationToken);

        Guard.Against.NotFound(request.GUID, OldEntity);
        
        {TableName}Service.mapper.Map(request, OldEntity);
        
        await {TableName}Service.SaveChangesAsync(cancellationToken);
    }}
}}
");
        var FolderPath = System.IO.Path.Combine(Path, FileName);
        if (!Directory.Exists(FolderPath)) Directory.CreateDirectory(FolderPath);

        var FilePath = System.IO.Path.Combine(FolderPath, FileName + "Command.cs");
        if (!File.Exists(FilePath) | _overrideUpdate) File.WriteAllLines(FilePath, Data);

        List<string> Data2 = new();
        Data2.Add($@"
using Application.Common.Mappings;
using Domain.Entities;
using Application.Common.Utility.Validator;
namespace Application.Core.{TableName}_core.Commands.{FileName};

public class {FileName}Validator : AbstractValidator<{FileName}Command>
{{
    public {FileName}Validator()
    {{
{ValidatorBuilder}
    }}
}}
");
        var FilePath2 = System.IO.Path.Combine(FolderPath, FileName + "Validator.cs");
        if (!File.Exists(FilePath2) | _overrideUpdate) File.WriteAllLines(FilePath2, Data2);
    }

    private void GenerateDeleteCommand(string Path, string TableName, string FileName)
    {
        List<string> Data = new();
        Data.Add(@$"
namespace Application.Core.{TableName}_core.Commands.{FileName};

public record {FileName}Command(Guid GUID) : IRequest;

public class {FileName}CommandHandler : IRequestHandler<{FileName}Command>
{{
    private {TableName}Service {TableName}Service;

    public {FileName}CommandHandler({TableName}Service {TableName}Service)
    {{
        this.{TableName}Service = {TableName}Service;
    }}

    public async Task Handle({FileName}Command request, CancellationToken cancellationToken)
    {{
        var entity = await {TableName}Service.Table
            .FindAsync(new object[] {{ request.GUID }}, cancellationToken);

        Guard.Against.NotFound(request.GUID, entity);

        {TableName}Service.Table.Remove(entity);

        await {TableName}Service.SaveChangesAsync(cancellationToken);
    }}

}}
");
        var FolderPath = System.IO.Path.Combine(Path, FileName);
        if (!Directory.Exists(FolderPath)) Directory.CreateDirectory(FolderPath);

        var FilePath = System.IO.Path.Combine(FolderPath, FileName + "Command.cs");
        if (!File.Exists(FilePath) | _overrideDelete) File.WriteAllLines(FilePath, Data);
    }

    private void GeneratePaginationQueryCommand(string Path, string TableName, List<AppEntity> appEntities,
        string FileName)
    {
        var builder = new StringBuilder();
        foreach (var item in appEntities)
        {
            var propConfig = GetPropConfig(item.PropertyInfo);
            builder.AppendLine(
                @$"
    {GetPropConfigString(propConfig)}
    public {item.propertyType}{(item.isNullable ? "?" : "")} {item.propertyName} {{ get; init; }}");
        }

        List<string> Data = new();
        Data.Add(@$"
using Application.Common.Models;

namespace Application.Core.{TableName}_core.Queries;

public record {FileName}Query : PageFilter<{TableName}, {FileName}DTO>
{{

}}

public record {FileName}DTO : IMapFrom<{TableName}>
{{
{builder}

    public void Mapping(Profile profile)
    {{
        profile.CreateMap<{TableName}, {FileName}DTO>();
    }}
}}

public class
    {FileName}QueryHandler : IRequestHandler<{FileName}Query,
    PaginatedList<{FileName}DTO>>
{{
    private {TableName}Service {TableName}Service;
    public {FileName}QueryHandler({TableName}Service {TableName}Service)
    {{
        this.{TableName}Service = {TableName}Service;
    }}

    public async Task<PaginatedList<{FileName}DTO>> Handle({FileName}Query request,
        CancellationToken cancellationToken)
    {{
        IQueryable<{TableName}> queryable = {TableName}Service.Table;
        return await request.ToPaging( queryable,{TableName}Service.mapper);
    }}
}}
");
        var FilePath = System.IO.Path.Combine(Path, $"{FileName}.cs");
        if (!File.Exists(FilePath)) File.WriteAllLines(FilePath, Data);
    }

    private void GenerateFindAllQuery(string Path, string TableName, List<AppEntity> appEntities,
        string FileName)
    {
        var builder = new StringBuilder();
        foreach (var item in appEntities)
        {
            var propConfig = GetPropConfig(item.PropertyInfo);
            builder.AppendLine(
                @$"
    {GetPropConfigString(propConfig)}
    public {item.propertyType}{(item.isNullable ? "?" : "")} {item.propertyName} {{ get; init; }}");
        }

        List<string> Data = new();
        Data.Add(@$"

namespace Application.Core.{TableName}_core.Queries;

public record {FileName}Query : IRequest<List<{FileName}DTO>>
{{

}}

public record {FileName}DTO : IMapFrom<{TableName}>
{{
{builder}


    public void Mapping(Profile profile)
    {{
        profile.CreateMap<{TableName}, {FileName}DTO>();
    }}
}}

public class
    {FileName}QueryHandler : IRequestHandler<{FileName}Query,
    List<{FileName}DTO>>
{{
    private {TableName}Service {TableName}Service;
    public {FileName}QueryHandler({TableName}Service {TableName}Service)
    {{
        this.{TableName}Service = {TableName}Service;
    }}

    public async Task<List<{FileName}DTO>> Handle({FileName}Query request,
        CancellationToken cancellationToken)
    {{
        
        return await {TableName}Service.Table.AsNoTracking()
            .ProjectTo<{FileName}DTO>({TableName}Service.mapper.ConfigurationProvider).ToListAsync();
    }}
}}


");
        var FilePath = System.IO.Path.Combine(Path, $"{FileName}.cs");
        if (!File.Exists(FilePath)) File.WriteAllLines(FilePath, Data);
    }

    private void GenerateFindByIdQuery(string Path, string TableName, List<AppEntity> appEntities,
        string FileName)
    {
        var builder = new StringBuilder();
        foreach (var item in appEntities)
        {
            var propConfig = GetPropConfig(item.PropertyInfo);
            builder.AppendLine(
                @$"    
    {GetPropConfigString(propConfig)}
    public {item.propertyType}{(item.isNullable ? "?" : "")} {item.propertyName} {{ get; init; }}");
        }

        List<string> Data = new();
        Data.Add(@$"
namespace Application.Core.{TableName}_core.Queries;

public record {FileName}Query(Guid GUID) : IRequest<{FileName}DTO>;

public record {FileName}DTO : IMapFrom<{TableName}>
{{
{builder}


    public void Mapping(Profile profile)
    {{
        profile.CreateMap<{TableName}, {FileName}DTO>();
    }}
}}

public class
    {FileName}QueryHandler : IRequestHandler<{FileName}Query, {FileName}DTO>
{{
    private {TableName}Service {TableName}Service;

    public {FileName}QueryHandler({TableName}Service {TableName}Service)
    {{
        this.{TableName}Service = {TableName}Service;
    }}

    public async Task<{FileName}DTO> Handle({FileName}Query request,
        CancellationToken cancellationToken)
    {{
        {FileName}DTO? Data = {TableName}Service.Table.AsNoTracking().Where(a=>a.GUID==request.GUID)
            .ProjectTo<{FileName}DTO>({TableName}Service.mapper.ConfigurationProvider).FirstOrDefault();
        Guard.Against.NotFound(request.GUID, Data);
        return Data;
    }}
}}
");
        var FilePath = System.IO.Path.Combine(Path, $"{FileName}.cs");
        if (!File.Exists(FilePath)) File.WriteAllLines(FilePath, Data);
    }
    
    
    private void GenerateFindAllTreeQuery(string Path, string TableName, List<AppEntity> appEntities,
        string FileName)
    {
        var builder = new StringBuilder();
        foreach (var item in appEntities)
        {
            var propConfig = GetPropConfig(item.PropertyInfo);
            builder.AppendLine(
                @$"
    {GetPropConfigString(propConfig)}
    public {item.propertyType}{(item.isNullable ? "?" : "")} {item.propertyName} {{ get; init; }}");
        }

        List<string> Data = new();
        Data.Add(@$"
using Application.Common.Models;
namespace Application.Core.{TableName}_core.Queries;

public record {FileName}Query : IRequest<List<{FileName}DTO>>
{{

}}

public record {FileName}DTO : TreeNode<{FileName}Data, {FileName}DTO>, IMapFrom<{TableName}>
{{

    public void Mapping(Profile profile)
    {{
        profile.CreateMap<{TableName}, {FileName}DTO>()
            .ForMember(dest => dest.Label, opt => opt.MapFrom(src => src.))
            .ForMember(dest => dest.Key, opt => opt.MapFrom(src => src.GUID))
            .ForMember(dest => dest.Data, opt => opt.MapFrom(a => new {FileName}Data
            {{
                
            }}));
    }}
}}
public record {FileName}Data 
{{
{builder}

}}

public class
    {FileName}QueryHandler : IRequestHandler<{FileName}Query,
    List<{FileName}DTO>>
{{
    private {TableName}Service {TableName}Service;
    public {FileName}QueryHandler({TableName}Service {TableName}Service)
    {{
        this.{TableName}Service = {TableName}Service;
    }}

    public async Task<List<{FileName}DTO>> Handle({FileName}Query request,
        CancellationToken cancellationToken)
    {{
        
        IQueryable<{TableName}> queryable = {TableName}Service.Table.AsNoTracking();
        List<{FileName}DTO> Data = await queryable.Where(a=> a. == null)
            .ProjectTo<{FileName}DTO>({TableName}Service.mapper.ConfigurationProvider).ToListAsync();
        foreach (var Item in Data)
        {{
            await SetChild(queryable, Item);
        }}
        return Data;
    }}

    public async Task SetChild(IQueryable<{TableName}> queryable, {FileName}DTO Item)
    {{
        Item.Children = await queryable.Where(a =>
                a. == Item.Key
            ).OrderBy(a => a.Created).ProjectTo<{FileName}DTO>({TableName}Service.mapper.ConfigurationProvider)
            .ToListAsync();

        Item.Expanded = false;
        Item.Leaf=!Item.Children.Any();
        foreach (var child in Item.Children)
        {{
            await SetChild(queryable, child);
        }}
    }}
}}


");
        var FilePath = System.IO.Path.Combine(Path, $"{FileName}.cs");
        if (!File.Exists(FilePath)) File.WriteAllLines(FilePath, Data);
    }
}

public class AppEntity
{
    public string propertyName { get; set; }
    public string propertyType { get; set; }
    public bool isNullable { get; set; }
    public PropertyInfo PropertyInfo { get; set; }
    public bool isNumericType { get; set; }
}