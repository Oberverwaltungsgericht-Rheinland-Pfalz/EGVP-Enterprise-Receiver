using OvgRlp.EgvpEpReceiver.Services;
using System;
using System.IO;
using Xunit;

namespace OvgRlp.EgvpEpReceiver.Test
{
  public class Configuration_Test
  {
    [Fact]
    public void ConfigService_Load()
    {
      var config = new ConfigurationService(Path.Combine(Helper.GetResourcesPath(), "Configuration\\Account_Settings.json"));
      Assert.NotNull(config);
    }

    [Fact]
    public void ConfigService_CheckPostboxes()
    {
      var config = new ConfigurationService(Path.Combine(Helper.GetResourcesPath(), "Configuration\\Account_Settings.json"));
      var boxes = config.GetAllPostboxes();
      Assert.NotNull(boxes);
      Assert.Equal(2, boxes.Count);
    }

    [Fact]
    public void ConfigService_CheckValues()
    {
      var config = new ConfigurationService(Path.Combine(Helper.GetResourcesPath(), "Configuration\\Account_Settings.json"));
      var boxes = config.GetAllPostboxes();

      Assert.Equal("Test Gericht 1", boxes[0].Name);
      Assert.Equal("DE.Justiz.233a8870-2319-4dbc-9f68-a4cc9da8f429.9999", boxes[0].Id);
      Assert.Equal(@"C:\dev\Temp\ExpPath", boxes[0].ExportPath[0]);
      Assert.Equal(@"C:\dev\Temp\ExportPath_EEB", boxes[0].ExportPath_EEB[0]);
      Assert.Equal(@"C:\dev\Temp\ArchivPath_1", boxes[0].ArchivPath[0]);
      Assert.Equal(@"C:\dev\Temp\ArchivPath_2", boxes[0].ArchivPath[1]);

      Assert.NotNull(boxes[0].ReceiveDepartments);
      Assert.Equal("Info", boxes[0].ReceiveDepartments.LogLevelDepartmentNotFound);
      Assert.Equal("//xjustiz:nachrichtenkopf/xjustiz:auswahl_empfaenger/xjustiz:empfaenger.gericht/code", boxes[0].ReceiveDepartments.XPathDepartmentId);
      Assert.NotNull(boxes[0].ReceiveDepartments.Departments);
      Assert.Equal("Department 1", boxes[0].ReceiveDepartments.Departments[0].Name);
      Assert.Equal("DepID1", boxes[0].ReceiveDepartments.Departments[0].Id);
      Assert.Equal(@"C:\dev\Temp\Department\ExpPath_1", boxes[0].ReceiveDepartments.Departments[0].ExportPath[0]);
      Assert.Equal(@"C:\dev\Temp\Department\ExpPath_2", boxes[0].ReceiveDepartments.Departments[0].ExportPath[1]);
      Assert.Equal(@"C:\dev\Temp\Department\ExportPath_EEB", boxes[0].ReceiveDepartments.Departments[0].ExportPath_EEB[0]);
      Assert.Equal(@"C:\dev\Temp\Department\ArchivPath", boxes[0].ReceiveDepartments.Departments[0].ArchivPath[0]);

      Assert.Equal("Test Gericht 2", boxes[1].Name);
      Assert.Equal("DE.Justiz.836cga01-ffff-2856-85ef-268df5e8b136.5555", boxes[1].Id);
      Assert.Equal(@"C:\dev\Temp\ArchivPath_Ger2", boxes[1].ExportPath[0]);
      Assert.Equal(@"C:\dev\Temp\ExportPath_EEB_Ger2", boxes[1].ExportPath_EEB[0]);
      Assert.Equal(@"C:\dev\Temp\ArchivPath\[yyyy]_[MM]", boxes[1].ArchivPath[0]);
      Assert.Null(boxes[1].ReceiveDepartments);
    }
  }
}