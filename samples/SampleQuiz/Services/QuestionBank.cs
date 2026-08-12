using SampleQuiz.Models;

namespace SampleQuiz.Services;

public static class QuestionBank
{
    public static readonly string[] Categories = [".NET", "Azure", "Windows"];

    public static List<QuizQuestion> GetQuestions(string category) =>
        AllQuestions.Where(q => q.Category == category).ToList();

    public static readonly List<QuizQuestion> AllQuestions =
    [
        // ===== .NET - Easy =====
        new()
        {
            Question = "What language is most commonly associated with .NET development?",
            Options = ["Java", "C#", "Python", "Ruby"],
            CorrectIndex = 1,
            Category = ".NET",
            Difficulty = "Easy"
        },
        new()
        {
            Question = "What does CLR stand for in .NET?",
            Options = ["Common Language Runtime", "Core Language Runtime", "Compiled Language Runtime", "Central Library Resource"],
            CorrectIndex = 0,
            Category = ".NET",
            Difficulty = "Easy"
        },
        new()
        {
            Question = "Which file extension is used for C# source files?",
            Options = [".vb", ".cs", ".fs", ".cpp"],
            CorrectIndex = 1,
            Category = ".NET",
            Difficulty = "Easy"
        },
        new()
        {
            Question = "What is the base class for all .NET types?",
            Options = ["System.Base", "System.Object", "System.Type", "System.Root"],
            CorrectIndex = 1,
            Category = ".NET",
            Difficulty = "Easy"
        },
        new()
        {
            Question = "Which keyword is used to define a class in C#?",
            Options = ["struct", "module", "class", "define"],
            CorrectIndex = 2,
            Category = ".NET",
            Difficulty = "Easy"
        },
        new()
        {
            Question = "What tool is used to manage .NET projects from the command line?",
            Options = ["npm", "dotnet CLI", "nuget.exe", "msbuild only"],
            CorrectIndex = 1,
            Category = ".NET",
            Difficulty = "Easy"
        },

        // ===== .NET - Medium =====
        new()
        {
            Question = "Which .NET type is used for immutable, thread-safe string building?",
            Options = ["StringBuilder", "StringWriter", "string", "ReadOnlySpan<char>"],
            CorrectIndex = 3,
            Category = ".NET",
            Difficulty = "Medium"
        },
        new()
        {
            Question = "What is the purpose of the 'async' keyword in C#?",
            Options = ["Makes a method run on a background thread", "Marks a method that contains await expressions", "Forces parallel execution", "Enables multi-threading"],
            CorrectIndex = 1,
            Category = ".NET",
            Difficulty = "Medium"
        },
        new()
        {
            Question = "Which collection type guarantees unique elements in .NET?",
            Options = ["List<T>", "Queue<T>", "HashSet<T>", "LinkedList<T>"],
            CorrectIndex = 2,
            Category = ".NET",
            Difficulty = "Medium"
        },
        new()
        {
            Question = "What attribute marks a test method in MSTest?",
            Options = ["[Fact]", "[Test]", "[TestMethod]", "[TestCase]"],
            CorrectIndex = 2,
            Category = ".NET",
            Difficulty = "Medium"
        },
        new()
        {
            Question = "In .NET dependency injection, what is the default lifetime for services registered with AddScoped?",
            Options = ["Singleton", "Per HTTP request", "Transient", "Per application domain"],
            CorrectIndex = 1,
            Category = ".NET",
            Difficulty = "Medium"
        },
        new()
        {
            Question = "Which interface must a class implement to be used in a 'foreach' loop?",
            Options = ["IComparable", "IEnumerable", "IDisposable", "IFormattable"],
            CorrectIndex = 1,
            Category = ".NET",
            Difficulty = "Medium"
        },
        new()
        {
            Question = "What does the 'record' keyword introduce in C# 9?",
            Options = ["A mutable reference type", "An immutable reference type with value equality", "A database model", "A logging mechanism"],
            CorrectIndex = 1,
            Category = ".NET",
            Difficulty = "Medium"
        },
        new()
        {
            Question = "Which NuGet package provides the System.Text.Json serializer?",
            Options = ["Newtonsoft.Json", "System.Text.Json", "System.Runtime.Serialization", "System.Xml.Linq"],
            CorrectIndex = 1,
            Category = ".NET",
            Difficulty = "Medium"
        },

        // ===== .NET - Hard =====
        new()
        {
            Question = "What is the purpose of Span<T> in .NET?",
            Options = ["Managed heap allocation for arrays", "Type-safe memory access without heap allocation", "Thread synchronization primitive", "Generic constraint for value types"],
            CorrectIndex = 1,
            Category = ".NET",
            Difficulty = "Hard"
        },
        new()
        {
            Question = "In .NET's garbage collector, what triggers a Gen 2 collection?",
            Options = ["Every 10 seconds", "When Gen 0 and Gen 1 are insufficient to free memory", "When Dispose is called", "On application shutdown only"],
            CorrectIndex = 1,
            Category = ".NET",
            Difficulty = "Hard"
        },
        new()
        {
            Question = "What does the [CallerMemberName] attribute do?",
            Options = ["Restricts method access to specific callers", "Automatically captures the calling method's name as a parameter", "Validates caller permissions at runtime", "Logs the call stack to diagnostics"],
            CorrectIndex = 1,
            Category = ".NET",
            Difficulty = "Hard"
        },
        new()
        {
            Question = "Which pattern does the C# 'await using' syntax implement?",
            Options = ["IDisposable", "IAsyncDisposable", "IAsyncEnumerable", "IObservable"],
            CorrectIndex = 1,
            Category = ".NET",
            Difficulty = "Hard"
        },
        new()
        {
            Question = "What is the difference between 'ValueTask<T>' and 'Task<T>'?",
            Options = ["ValueTask is always faster", "ValueTask avoids heap allocation when the result is available synchronously", "Task supports cancellation but ValueTask does not", "There is no difference"],
            CorrectIndex = 1,
            Category = ".NET",
            Difficulty = "Hard"
        },
        new()
        {
            Question = "In Blazor WebAssembly, which interop mechanism is used to call JavaScript?",
            Options = ["P/Invoke", "COM Interop", "IJSRuntime", "HttpClient"],
            CorrectIndex = 2,
            Category = ".NET",
            Difficulty = "Hard"
        },

        // ===== Azure - Easy =====
        new()
        {
            Question = "What does Azure App Service primarily provide?",
            Options = ["Virtual machine management", "Web application hosting", "Database administration", "Network routing"],
            CorrectIndex = 1,
            Category = "Azure",
            Difficulty = "Easy"
        },
        new()
        {
            Question = "Which Azure service is used for object/blob storage?",
            Options = ["Azure SQL", "Azure Blob Storage", "Azure Redis Cache", "Azure Service Bus"],
            CorrectIndex = 1,
            Category = "Azure",
            Difficulty = "Easy"
        },
        new()
        {
            Question = "What is the Azure CLI command prefix?",
            Options = ["azure", "az", "azcli", "cloud"],
            CorrectIndex = 1,
            Category = "Azure",
            Difficulty = "Easy"
        },
        new()
        {
            Question = "Which Azure service provides serverless compute for running code on-demand?",
            Options = ["Azure Virtual Machines", "Azure Functions", "Azure Kubernetes Service", "Azure Batch"],
            CorrectIndex = 1,
            Category = "Azure",
            Difficulty = "Easy"
        },
        new()
        {
            Question = "What does a Resource Group in Azure represent?",
            Options = ["A billing account", "A logical container for related Azure resources", "A virtual network", "An availability zone"],
            CorrectIndex = 1,
            Category = "Azure",
            Difficulty = "Easy"
        },

        // ===== Azure - Medium =====
        new()
        {
            Question = "What is the maximum size of a single block blob in Azure Blob Storage?",
            Options = ["5 TB", "190.7 TiB", "1 PB", "256 GB"],
            CorrectIndex = 1,
            Category = "Azure",
            Difficulty = "Medium"
        },
        new()
        {
            Question = "Which Azure service provides a fully managed NoSQL database?",
            Options = ["Azure SQL Database", "Azure Cosmos DB", "Azure Table Storage", "Azure Database for MySQL"],
            CorrectIndex = 1,
            Category = "Azure",
            Difficulty = "Medium"
        },
        new()
        {
            Question = "What is Azure DevOps Pipelines used for?",
            Options = ["Monitoring application health", "Continuous integration and delivery", "Managing DNS records", "Virtual network peering"],
            CorrectIndex = 1,
            Category = "Azure",
            Difficulty = "Medium"
        },
        new()
        {
            Question = "Which Azure service allows you to manage secrets, keys, and certificates?",
            Options = ["Azure Active Directory", "Azure Key Vault", "Azure Policy", "Azure Monitor"],
            CorrectIndex = 1,
            Category = "Azure",
            Difficulty = "Medium"
        },
        new()
        {
            Question = "What does Azure Application Insights provide?",
            Options = ["Blob storage analytics", "Application performance monitoring and diagnostics", "Virtual machine scaling", "Container orchestration"],
            CorrectIndex = 1,
            Category = "Azure",
            Difficulty = "Medium"
        },
        new()
        {
            Question = "In Azure Functions, what is a 'binding'?",
            Options = ["A security constraint", "A declarative way to connect to data sources without manual code", "A load balancing strategy", "A deployment configuration"],
            CorrectIndex = 1,
            Category = "Azure",
            Difficulty = "Medium"
        },
        new()
        {
            Question = "Which consistency level in Cosmos DB offers the strongest guarantees?",
            Options = ["Eventual", "Session", "Bounded Staleness", "Strong"],
            CorrectIndex = 3,
            Category = "Azure",
            Difficulty = "Medium"
        },

        // ===== Azure - Hard =====
        new()
        {
            Question = "What is the Azure Well-Architected Framework's reliability pillar primarily concerned with?",
            Options = ["Minimizing cost", "Ensuring workloads perform their intended function consistently", "Encrypting data at rest", "Automating deployments"],
            CorrectIndex = 1,
            Category = "Azure",
            Difficulty = "Hard"
        },
        new()
        {
            Question = "In Azure Kubernetes Service, what is a DaemonSet?",
            Options = ["A pod that runs on every node in the cluster", "A horizontal pod autoscaler", "A network policy", "A storage class"],
            CorrectIndex = 0,
            Category = "Azure",
            Difficulty = "Hard"
        },
        new()
        {
            Question = "What does Azure Front Door provide that a standard Load Balancer does not?",
            Options = ["TCP load balancing", "Global HTTP/HTTPS load balancing with WAF and CDN", "Database connection pooling", "DNS zone management"],
            CorrectIndex = 1,
            Category = "Azure",
            Difficulty = "Hard"
        },
        new()
        {
            Question = "Which Azure service uses the Raft consensus algorithm for leader election?",
            Options = ["Azure Service Fabric", "Azure Functions", "Azure Logic Apps", "Azure Event Grid"],
            CorrectIndex = 0,
            Category = "Azure",
            Difficulty = "Hard"
        },
        new()
        {
            Question = "What is the purpose of Azure Private Link?",
            Options = ["Public DNS resolution", "Secure access to Azure services over a private endpoint in your VNet", "Cross-region VM migration", "Automated certificate renewal"],
            CorrectIndex = 1,
            Category = "Azure",
            Difficulty = "Hard"
        },

        // ===== Windows - Easy =====
        new()
        {
            Question = "What is the Windows Registry used for?",
            Options = ["File compression", "Storing system and application configuration settings", "Network packet routing", "Disk defragmentation"],
            CorrectIndex = 1,
            Category = "Windows",
            Difficulty = "Easy"
        },
        new()
        {
            Question = "Which keyboard shortcut opens Task Manager in Windows?",
            Options = ["Ctrl+Alt+Delete", "Ctrl+Shift+Esc", "Alt+F4", "Win+R"],
            CorrectIndex = 1,
            Category = "Windows",
            Difficulty = "Easy"
        },
        new()
        {
            Question = "What file system does modern Windows primarily use?",
            Options = ["ext4", "FAT32", "NTFS", "HFS+"],
            CorrectIndex = 2,
            Category = "Windows",
            Difficulty = "Easy"
        },
        new()
        {
            Question = "What is PowerShell?",
            Options = ["A graphics editor", "A task automation and configuration management framework", "A web browser", "A disk utility"],
            CorrectIndex = 1,
            Category = "Windows",
            Difficulty = "Easy"
        },
        new()
        {
            Question = "Which Windows feature allows running Linux distributions natively?",
            Options = ["Hyper-V", "Windows Subsystem for Linux (WSL)", "Windows Sandbox", "Remote Desktop"],
            CorrectIndex = 1,
            Category = "Windows",
            Difficulty = "Easy"
        },
        new()
        {
            Question = "What does the 'ipconfig' command display?",
            Options = ["Running processes", "Network configuration details", "Disk usage", "Installed programs"],
            CorrectIndex = 1,
            Category = "Windows",
            Difficulty = "Easy"
        },

        // ===== Windows - Medium =====
        new()
        {
            Question = "What is the purpose of the Windows Event Viewer?",
            Options = ["Managing scheduled tasks", "Viewing system, security, and application logs", "Configuring network adapters", "Managing user accounts"],
            CorrectIndex = 1,
            Category = "Windows",
            Difficulty = "Medium"
        },
        new()
        {
            Question = "Which Windows tool is used to manage Group Policy settings?",
            Options = ["regedit", "gpedit.msc", "devmgmt.msc", "compmgmt.msc"],
            CorrectIndex = 1,
            Category = "Windows",
            Difficulty = "Medium"
        },
        new()
        {
            Question = "What is a Windows Service?",
            Options = ["A desktop application with a GUI", "A long-running background process managed by the Service Control Manager", "A browser extension", "A type of DLL file"],
            CorrectIndex = 1,
            Category = "Windows",
            Difficulty = "Medium"
        },
        new()
        {
            Question = "Which command-line tool is used to query DNS records on Windows?",
            Options = ["ping", "tracert", "nslookup", "netstat"],
            CorrectIndex = 2,
            Category = "Windows",
            Difficulty = "Medium"
        },
        new()
        {
            Question = "What does DISM stand for in Windows administration?",
            Options = ["Disk Image Service Manager", "Deployment Image Servicing and Management", "Device Installation System Module", "Dynamic Integrated Security Manager"],
            CorrectIndex = 1,
            Category = "Windows",
            Difficulty = "Medium"
        },

        // ===== Windows - Hard =====
        new()
        {
            Question = "What is the Windows Filtering Platform (WFP)?",
            Options = ["A file indexing service", "A set of APIs for creating network filtering applications", "A display driver framework", "A memory management subsystem"],
            CorrectIndex = 1,
            Category = "Windows",
            Difficulty = "Hard"
        },
        new()
        {
            Question = "In NTFS, what is an Alternate Data Stream (ADS)?",
            Options = ["A backup copy of a file", "A hidden data fork attached to a file that can store metadata or additional content", "An encrypted file system extension", "A network file share protocol"],
            CorrectIndex = 1,
            Category = "Windows",
            Difficulty = "Hard"
        },
        new()
        {
            Question = "What Windows mechanism does Address Space Layout Randomization (ASLR) provide?",
            Options = ["Faster memory allocation", "Randomizes memory addresses of executables to prevent exploit attacks", "Compresses virtual memory pages", "Manages thread scheduling priority"],
            CorrectIndex = 1,
            Category = "Windows",
            Difficulty = "Hard"
        },
        new()
        {
            Question = "Which Windows API is used for asynchronous I/O operations?",
            Options = ["ReadFile with OVERLAPPED structure", "CreateProcess", "RegOpenKey", "SetWindowsHookEx"],
            CorrectIndex = 0,
            Category = "Windows",
            Difficulty = "Hard"
        },
    ];
}
