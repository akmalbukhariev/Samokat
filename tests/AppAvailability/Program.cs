using Ninimum.Models;
var cases=new[]{
    (new ApplicationConfiguration{forceUpdateYn="Y",appVersion="1.0.0"},"1.0",Availability.Available),
    (new ApplicationConfiguration{forceUpdateYn="Y",appVersion="1.10"},"1.9.9",Availability.UpdateRequired),
    (new ApplicationConfiguration{forceUpdateYn="Y",appVersion="1.0"},"2.0",Availability.Available),
    (new ApplicationConfiguration{forceUpdateYn="N",appVersion="2.0"},"1.0",Availability.Available),
    (new ApplicationConfiguration{forceUpdateYn="Y",appVersion="garbage"},"1.0",Availability.Available),
    (new ApplicationConfiguration{forceUpdateYn="Y",appVersion="2.0",maintenanceYn="Y"},"1.0",Availability.Maintenance),
    (new ApplicationConfiguration{forceUpdateYn="Y",appVersion="1.0.0.1"},"1.0",Availability.UpdateRequired)
};
foreach(var (config,version,expected) in cases)if(AppAvailability.Evaluate(config,version)!=expected)throw new Exception($"Wrong availability for {version}/{config.appVersion}");
if(AppAvailability.Evaluate(null,"1.0")!=Availability.Available)throw new Exception("Missing config blocked startup");
Console.WriteLine("8 application availability cases passed.");
