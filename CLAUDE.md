Product Requirements Document. Dataverse Documentation CLI

Purpose
You need a command line tool to document Microsoft Dataverse solutions in a consistent, repeatable way. The tool targets architects, developers, and DevOps pipelines. It extracts metadata from a Dataverse environment and produces human readable and machine readable outputs. It supports local interactive use and automated pipeline execution.

Goals
You document Dataverse solutions quickly.
You remove manual documentation effort.
You generate diagrams directly from source metadata.
You enable use in CI and CD pipelines.

Non Goals
No data level reporting.
No runtime monitoring.
No modification of Dataverse data or metadata.

Target Users
Solution architects.
Power Platform developers.
DevOps engineers.

Supported Platforms
Windows.
macOS.
Linux.

Runtime
.NET global tool or standalone executable.

Authentication
You must support two authentication modes.

Interactive authentication
Used for local execution.
Uses OAuth device code or browser login.
Uses user delegated permissions.

Service principal authentication
Used for pipelines.
Uses client id, tenant id, and client secret or certificate.
Uses application permissions.

Authentication Requirements
You select auth mode via CLI arguments.
You can store config in environment variables.
You can override via command flags.
You fail fast on auth errors.

High Level Architecture
CLI entry point.
Command dispatcher.
Dataverse connector layer.
Metadata readers per feature.
Output renderers.

Core Commands
	1.	Get Environment Variables in a Solution
Command intent
You list all environment variables linked to a solution.

Inputs
Solution unique name.

Outputs
Variable display name.
Schema name.
Current value.
Default value.
Type.

Formats
Console table.
JSON.
Markdown.
	2.	Get Security Roles in a Solution
Command intent
You document security roles included in a solution.

Inputs
Solution unique name.

Outputs
Role name.
Business unit scope.
Privileges summary.

Formats
Console table.
JSON.
	3.	Get Queues in a Solution
Command intent
You list queues deployed via a solution.

Inputs
Solution unique name.

Outputs
Queue name.
Type.
Email enabled flag.

Formats
Console table.
JSON.
	4.	Entity Relationship Mermaid Diagram
Command intent
You generate a relationship diagram for a single entity.

Inputs
Entity logical name.
Optional depth parameter.

Processing
Read all relationships where entity is parent or child.

Outputs
Mermaid ER diagram text.

Formats
Mermaid only.
	5.	List Option Sets in a Solution
Command intent
You extract all option sets in scope of a solution.

Inputs
Solution unique name.

Outputs
Option set name.
Type global or local.
Option values and labels.

Formats
Console table.
JSON.
	6.	Get Processes in a Solution
Command intent
You document classic Dataverse processes.

Inputs
Solution unique name.

Outputs
Process name.
Type workflow or business process flow.
Status.

Formats
Console table.
JSON.
	7.	Get Cloud Flows in a Solution
Command intent
You list Power Automate cloud flows in a solution.

Inputs
Solution unique name.

Outputs
Flow name.
State.
Owner.

Formats
Console table.
JSON.
	8.	Cloud Flow Dependency Mermaid Diagram
Command intent
You visualise dependencies between cloud flows.

Inputs
Solution unique name or single flow name.

Processing
Resolve child flow references.
Resolve HTTP triggered dependencies where possible.

Outputs
Mermaid flow diagram text.

Formats
Mermaid only.

CLI Design

Command Structure
Single executable.
Subcommands per feature.

Example
dataverse-doc envvars –solution CoreSolution
dataverse-doc entity-diagram –entity account

Common Flags
–url
–auth-mode
–tenant-id
–client-id
–client-secret
–output

Error Handling
Clear error messages.
Non zero exit codes for failures.
Verbose mode for diagnostics.

Pipeline Usage
You can run headless using service principal auth.
You can export artifacts as files.
You can fail pipeline on missing access or invalid inputs.

Security
No secrets written to disk by default.
Supports secret injection via environment variables.

Extensibility
Each feature implemented as a module.
Shared Dataverse query layer.
Easy to add future documentation commands.

Acceptance Criteria
You can authenticate interactively and via service principal.
You can extract metadata for all listed features.
You can generate valid Mermaid diagrams.
You can run the tool in Azure DevOps.
You can export outputs in structured formats.

For powershell use this module  Microsoft.Xrm.Tooling.CrmConnector.PowerShell
If you need more like using WEBAPI use 
https://learn.microsoft.com/en-us/power-apps/developer/data-platform/webapi/use-web-api-metadata
https://learn.microsoft.com/en-us/power-apps/developer/data-platform/webapi/create-update-optionsets
https://learn.microsoft.com/en-us/power-apps/developer/data-platform/webapi/web-api-metadata-operations-sample
