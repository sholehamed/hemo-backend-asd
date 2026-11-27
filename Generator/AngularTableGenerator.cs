using System.Reflection;
using System.Text;
using Domain.Attributes;
using Newtonsoft.Json.Linq;

namespace Generator;

public class AngularTableGenerator
{
    private string AngularPath;
    private string operationId;
    private string ComponentName = "";
    private JObject Data;
    private JToken Api;
    private string DTOName = "";
    private string ApiUrl = "";
    List<string> imports = new List<string>() { "BaseComponent", "ButtonModule" ,"RouterLink","TooltipModule"};

    public AngularTableGenerator(string AngularPath, string operationId, string ComponentName)
    {
        this.AngularPath = AngularPath;
        this.operationId = operationId;
        this.ComponentName = ComponentName;
    }

    public void CreateComponent()
    {
        GetJson();
        FindOperationById();
        GenerateInputHtml();
        GenerateTs();
        Generate_scss();
    }

    private void GetJson()
    {
        string JsonPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
            "../../../../web/wwwroot/api/specification.json");
        string Json = File.ReadAllText(JsonPath);
        JObject JObject = JObject.Parse(Json);
        Data = JObject;
    }

    void FindOperationById()
    {
        var paths = Data["paths"];

        if (paths != null)
        {
            foreach (var path in paths)
            {
                foreach (var method in path.First)
                {
                    if (method.First["operationId"]?.ToString() == operationId)
                    {
                        ApiUrl = path.Path.Replace("paths['/", "").Replace("']", "");
                        Api = method.First;
                        return;
                    }
                }
            }
        }

        throw new Exception("operationId not found");
    }

    List<DTOField> GetDTO(JToken Api)
    {
        string _DTOName = Api["responses"]["200"]["content"]["application/json"]["schema"]["$ref"].ToString()
            .Replace("#/components/schemas/", "");
        DTOName=Data["components"]["schemas"][_DTOName]["properties"]["data"]["items"]["$ref"].ToString().Replace("#/components/schemas/", "");
        JToken DTO = Data["components"]["schemas"][DTOName]["properties"];

        List<DTOField> fields = new List<DTOField>();
        foreach (JProperty property in DTO.Children<JProperty>())
        {
            string key = property.Name;
            string propName = property.Value["PropName"]?.ToString();
            string? type = property.Value["type"]?.ToString().ToLower();
            string? format = property.Value["format"]?.ToString().ToLower();
            long? MaxLenght = long.Parse(property.Value["MaxLenght"]?.ToString() ?? "0");
            long? MinLenght = long.Parse(property.Value["MinLenght"]?.ToString() ?? "0");
            long? MaxNumber = long.Parse(property.Value["MaxNumber"]?.ToString() ?? "0");
            long? MinNumber = long.Parse(property.Value["MinNumber"]?.ToString() ?? "0");
            fields.Add(new DTOField()
            {
                MaxLenght = MaxLenght, MinLenght = MinLenght, MaxNumber = MaxNumber, MinNumber = MinNumber,
                key = key, propName = propName, type = (string.IsNullOrWhiteSpace(format)) ? type : format
            });
        }

        return fields;
    }


    public void Generate_scss()
    {
        List<string> css = new List<string>();


        string Folder = Path.Combine(AngularPath, ComponentName);
        if (!Directory.Exists(Folder))
        {
            Directory.CreateDirectory(Folder);
        }

        string FileName = ComponentName + ".component.scss";
        string FilePath = Path.Combine(Folder, FileName);
        if (!File.Exists(FilePath))
        {
            File.WriteAllLines(FilePath, css);
        }
    }

    public void GenerateInputHtml()
    {
        string description = Api["description"].ToString().Replace("نمایش صفحه بندی شده ی","مدیریت");
       
        List<string> html = new List<string>();



        html.Add(@$"
<app-base
    [Name]=""'{description}'"" 
    [DataUrl]=""'{ApiUrl}'""
    [TableConfig]=""TableConfig"">
    <ng-template #Actions>
        <p-button [routerLink]=""['/']"" label=""ثبت جدید"" [raised]=""true""
            severity=""success"" />
    </ng-template>
    <ng-template #RowActions let-Item>
        <td>
           <p-button [routerLink]=""[''+Item.guid]"" pTooltip=""ویرایش"" icon=""pi pi-pencil"" [rounded]=""true"" severity=""warn"" />
           <p-button (onClick)=""Delete(Item)"" pTooltip=""حذف"" icon=""pi pi-trash"" [rounded]=""true"" severity=""danger"" />
        </td>

    </ng-template>
</app-base>

");


        string Folder = Path.Combine(AngularPath, ComponentName);
        if (!Directory.Exists(Folder))
        {
            Directory.CreateDirectory(Folder);
        }

        string FileName = ComponentName + ".component.html";
        string FilePath = Path.Combine(Folder, FileName);
        if (!File.Exists(FilePath))
        {
            File.WriteAllLines(FilePath, html);
        }
    }

    public void GenerateTs()
    {
        imports = imports.Distinct().ToList();
        string tags = Api["tags"].First().ToString();
        List<DTOField> DTOFields = GetDTO(Api);
        
        StringBuilder stringBuilder = new StringBuilder();
        foreach (DTOField dtoField in DTOFields)
        {
            if (dtoField.key == "guid")
            {
                continue;
            }
            int FilterType = 0;
            switch (dtoField.type.ToLower())
            {
                case "string":
                {
                    FilterType = 1;
                    break;
                }
                case "integer":
                case "int32":
                case "int64":
                {
                    FilterType = 2;
                    break;
                }

                case "boolean":
                {
                    FilterType = 4;
                    break;
                }
                case "date-time":
                {
                    FilterType = 3;
                    break;
                }
                case "guid":
                {
                    FilterType = 0;
                    break;
                }
            }

            stringBuilder.AppendLine(
                @$"    {{Name:'{dtoField.key}',PersianName:'{dtoField.propName}',FilterTypeBy:'',FilterType:{FilterType}}},");
        }
        
        List<string> TS = new List<string>();

        TS.Add(@$"
import {{ Component }} from '@angular/core';
import {{  ButtonModule }} from 'primeng/button';
import {{ RouterLink }} from '@angular/router';
import {{  TooltipModule }} from 'primeng/tooltip';
import {{ ConfirmationService, MessageService }} from 'primeng/api';
@Component({{
    selector: '{ComponentName}',
    imports: [{(string.Join(",", imports))}],
    providers:[{tags}Client],
    templateUrl: './{ComponentName}.component.html',
    styleUrl: './{ComponentName}.component.scss',
    standalone:true
}})
export class {ComponentName}Component {{
    @ViewChild(BaseComponent) BaseComponent: BaseComponent;
    TableConfig: TableConfig[] = [];
    constructor(private {tags}Client:{tags}Client
        ,private ConfirmationService:ConfirmationService,private MessageService:MessageService
) {{
        this.setTableConfig();
    }}

    setTableConfig() {{
        this.TableConfig = [
{stringBuilder.ToString()}
            {{ Name: '', PersianName: 'عملیات', FilterTypeBy: '', FilterType: 0 }},

        ].map(item => ({{
            ...new TableConfig(),
            ...item
        }}))
    }}

    Delete(item: any) {{

        StaticFunctions.confirm(this.ConfirmationService, () => {{
            this.{tags}Client.delete{tags}(item.guid).subscribe(Data => {{
                StaticFunctions.ShowSuccess(this.MessageService)
                this.BaseComponent.LoadData()
            }})
        }}, () => {{ }})

    }}

}}

");

        string Folder = Path.Combine(AngularPath, ComponentName);
        if (!Directory.Exists(Folder))
        {
            Directory.CreateDirectory(Folder);
        }

        string FileName = ComponentName + ".component.ts";
        string FilePath = Path.Combine(Folder, FileName);

        if (!File.Exists(FilePath))
        {
            File.WriteAllLines(FilePath, TS);
        }
    }
}