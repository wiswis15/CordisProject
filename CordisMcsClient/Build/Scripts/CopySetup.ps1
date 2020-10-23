param (
  [string]$Label
)
New-Item -path \\CORDISSVR01\Projects\MachineControlDashboard\$Label -itemType directory
Copy-item -path .\Delivery\Setup\*.* -destination \\CORDISSVR01\Projects\MachineControlDashboard\$Label
Rename-Item  -path \\CORDISSVR01\Projects\MachineControlDashboard\$Label\MachineControlDashboard32.msi -newname MachineControlDashboard32_$Label.msi