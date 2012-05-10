# First load object models to use
[void][System.Reflection.Assembly]::LoadWithPartialName(“Microsoft.Practices.SharePoint.Common”)

# call the registration method to ensure all registered diagnostic areas are also registered as event sources
[Microsoft.Practices.SharePoint.Common.Logging.DiagnosticsAreaEventSource]::EnsureConfiguredAreasRegistered()

