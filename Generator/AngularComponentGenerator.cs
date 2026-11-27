using System.Reflection;
using Domain.Attributes;
using Newtonsoft.Json.Linq;

namespace Generator;

public class AngularComponentGenerator
{
    private string AngularPath;
    private string operationId;
    private string ComponentName = "";
    private JObject Data;
    private JToken Api;
    private string DTOName = "";
    List<string>imports=new List<string>(){"PanelModule","FormsModule","ButtonModule"};
    public AngularComponentGenerator(string AngularPath, string operationId,string ComponentName)
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
                        Api= method.First;
                        return;
                    }
                }
            }
        }

        throw new Exception("operationId not found");
    }

    List<DTOField> GetDTO(JToken Api)
    {
         DTOName = Api["requestBody"]["content"]["application/json"]["schema"]["$ref"].ToString()
            .Replace("#/components/schemas/", "");

        JToken DTO = Data["components"]["schemas"][DTOName]["properties"];

        List<DTOField> fields = new List<DTOField>();
        foreach (JProperty property in DTO.Children<JProperty>())
        {
            string key = property.Name;
            string propName = property.Value["PropName"]?.ToString();
            string? type = property.Value["type"]?.ToString().ToLower();
            string? format= property.Value["format"]?.ToString().ToLower();
            long? MaxLenght = long.Parse(property.Value["MaxLenght"]?.ToString()??"0");
            long? MinLenght = long.Parse(property.Value["MinLenght"]?.ToString()??"0");
            long? MaxNumber = long.Parse(property.Value["MaxNumber"]?.ToString()??"0");
            long? MinNumber = long.Parse(property.Value["MinNumber"]?.ToString()??"0");
            fields.Add(new DTOField()
            {
                MaxLenght = MaxLenght,MinLenght = MinLenght,MaxNumber = MaxNumber,MinNumber = MinNumber,
                key = key, propName = propName, type = (string.IsNullOrWhiteSpace(format))?type:format
            });
        }

        return fields;
    }

    private string GetHTMLTag(DTOField field)
    {
        
        string tag = "";
        switch (field.type.ToLower())
        {
                
            case "string":
            {
                
                string MaxLenght = "";
                if (field.MaxLenght != 256)
                {
                    MaxLenght = @$"[maxlength]=""{field.MaxLenght}""";
                }
                string MinLenght = "";
                if (field.MinLenght > 0)
                {
                    MinLenght = @$"[minlength]=""{field.MinLenght}""";
                }
                imports.Add("InputStringComponent");
                tag = @$"            <div class=""col-span-12 md:col-span-6 lg:col-span-4 xl:col-span-3"">
                <app-input-string
                [Label]=""'{field.propName}'""
                [Placeholder]=""'{field.propName}'""
                [ReadOnly]=""false""
                [Required]=""true""
                [Data]=""{DTOName}.{field.key}""
                (DataValueChange)=""{DTOName}.{field.key}=$event""
                [Submitted]=""Form.submitted""
                {MaxLenght}
                {MinLenght}
                ></app-input-string>
            </div>";
                break;
            }
            case "int32":
            case "int64":
            case "integer":
            {
                string MaxNumber = "";
                if (field.MaxNumber != long.MaxValue)
                {
                    MaxNumber = @$"[maxlength]=""{field.MaxNumber}""";
                }
                string MinNumber = "";
                if (field.MinNumber > 0)
                {
                    MinNumber = @$"[minlength]=""{field.MinNumber}""";
                }
                imports.Add("InputNumberComponent");
                tag = $@"            <div class=""col-span-12 md:col-span-6 lg:col-span-4 xl:col-span-3"">
                <app-input-number
                    [Label]=""'{field.propName}'""
                    [Placeholder]=""'{field.propName}'""
                    [ReadOnly]=""false""
                    [Required]=""true""
                    [useGrouping]=""false""
                    [Data]=""{DTOName}.{field.key}""
                    (DataValueChange)=""{DTOName}.{field.key}=$event""
                    [Submitted]=""Form.submitted""
                    {MaxNumber}
                    {MinNumber}
                ></app-input-number>
            </div>";
                break;
            }

            case "boolean":
            {
                imports.Add("CheckboxComponent");
                tag = $@"            <div class=""col-span-12 md:col-span-6 lg:col-span-4 xl:col-span-3"">
                <app-checkbox
                    [Label]=""'{field.propName}'""
                    [Data]=""{DTOName}.{field.key}""
                    (DataValueChange)=""{DTOName}.{field.key}=$event""
                ></app-checkbox>
            </div>";
                break;
            }
            case "date-time":
            {
                imports.Add("DateInputComponent");
                tag = $@"            <div class=""col-span-12 md:col-span-6 lg:col-span-4 xl:col-span-3"">
                <app-date-input
                    [Label]=""'{field.propName}'""
                    [ReadOnly]=""false""
                    [Required]=""true""
                    [Data]=""{DTOName}.{field.key}""
                    (DataValueChange)=""{DTOName}.{field.key}=$event""
                    [Submitted]=""Form.submitted""
                ></app-date-input>
            </div>";
                break;
            }
            case "guid":
            {
                imports.Add("DropdownComponent");
                tag = $@"            <div class=""col-span-12 md:col-span-6 lg:col-span-4 xl:col-span-3"">
                <app-dropdown
                    [Label]=""'{field.propName}'""
                    [ReadOnly]=""false""
                    [Required]=""true""
                    [Options]=""[]""
                    [OptionsUrl]=""''""
                    (DataValueChange)=""{DTOName}.{field.key}=$event""
                    [Data]=""{DTOName}.{field.key}""
                    [DataIdName]=""'guid'""
                    [OptionName]=""'name'""
                    [Submitted]=""Form.submitted""
                ></app-dropdown>
            </div>";
                break;
            }
        }

        return tag;
    }

    public void Generate_scss()
    {
       
   
        List<string> css = new List<string>();
        

        string Folder=Path.Combine(AngularPath,ComponentName);
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
       
        string description = Api["description"].ToString();
        List<DTOField> DTOFields = GetDTO(Api);
        List<string> html = new List<string>();
        html.Add(@$"<p-panel [header]=""'{description}'"">
    <form #Form=""ngForm"">
        <div class=""grid grid-cols-12 gap-8"">");

        foreach (DTOField field in DTOFields)
        {
            string tag = GetHTMLTag(field);
            if (!string.IsNullOrWhiteSpace(tag))
            {
                html.Add(tag);
            }
           
        }


        html.Add(@$"        </div>
        <hr>
        <div class=""grid grid-cols-12 gap-8"">
            <div class=""col-span-6 text-center"">
                <p-button type=""Submit"" (onClick)=""Form.valid && (ChangePage?Update():Insert()) "" label=""ذخیره""
                    severity=""success"" />
            </div>
            <div class=""col-span-6 text-center"">
                <p-button type=""button"" (onClick)=""Back()"" routerLinkActive=""router-link-active"" label=""برگشت""
                    severity=""danger"" />
            </div>
        </div>
    </form>
</p-panel>");

        

        string Folder=Path.Combine(AngularPath,ComponentName);
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
        List<string> TS = new List<string>();
     
        TS.Add(@$"
import {{ MessageService }} from 'primeng/api';
import {{ Component }} from '@angular/core';
import {{ FormsModule }} from '@angular/forms';
import {{ PanelModule }} from 'primeng/panel';
import {{ ButtonModule }} from 'primeng/button';
import {{ ActivatedRoute, Router }} from '@angular/router';
@Component({{
    selector: '{ComponentName}',
    imports: [{(string.Join(",",imports))}],
    providers:[{tags}Client],
    templateUrl: './{ComponentName}.component.html',
    styleUrl: './{ComponentName}.component.scss',
    standalone:true
}})
export class {ComponentName}Component {{
    {DTOName}:{DTOName}=new {DTOName}();
    ChangePage: boolean = false;
    constructor(private {tags}Client:{tags}Client,
        private MessageService: MessageService,
        private ActivatedRoute: ActivatedRoute,
        private Router: Router) {{
        if (StaticFunctions.GetFormUrl() == '') {{
            this.ChangePage = true;
            this.ActivatedRoute.paramMap.subscribe(params => {{
                let guid = params.get('guid') || '';
                this.LoadData(guid)
            }});
        }}
    }}
    LoadData(guid: string) {{
        this.{tags}Client.get{tags}ById(guid).subscribe(Data => {{
            this.{DTOName} = Data;
        }})
    }}

    Insert() {{
        this.{tags}Client.create{tags}(this.{DTOName}).subscribe(Data => {{
            StaticFunctions.ShowSuccess(this.MessageService);
            this.Back();
        }})
    }}
    Update() {{
        this.{tags}Client.update{tags}(this.{DTOName}).subscribe(Data => {{
            StaticFunctions.ShowSuccess(this.MessageService);
            this.Back();
        }})
    }}

    Back() {{
        this.Router.navigate(['/'])
    }}
}}

");

        string Folder=Path.Combine(AngularPath,ComponentName);
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

