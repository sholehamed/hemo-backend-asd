// See https://aka.ms/new-console-template for more information

using Domain.Entities;
using Generator;
using Generator.Enums;

string s = Guid.Empty.ToString();

var tool = new GeneratorTool(
);
//tool.GeneratorDefaultClasses();
tool.GenerateCustomeCommandQuerys(CommandQuery.Command, CommandType.Create,
    QueryType.All, nameof(User),
    "NotificationLogin");

//

// AngularTableGenerator angularComponentGenerator=new AngularTableGenerator(
//     "C:\\Users\\v.vahid.ASD\\Desktop\\MyProjects\\vadood\\FrontIdentity\\src\\app\\Projects\\Identity\\Package", 
//     "GetDemoRequestWithPagination",
//     "GetDemoRequestWithPagination");
// angularComponentGenerator.CreateComponent();

// AngularComponentGenerator angularComponentGenerator=new AngularComponentGenerator(
//     "C:\\Users\\v.vahid.ASD\\Desktop\\MyProjects\\vadood\\FrontLog\\src\\app\\Projects\\Log\\SystemSetting\\GetPasswordBlockListWithPagination", 
//      "CreatePasswordBlockList",
//      "CreatePasswordBlockList");
//  angularComponentGenerator.CreateComponent();

//
