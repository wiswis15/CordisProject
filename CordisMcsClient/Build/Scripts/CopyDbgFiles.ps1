param (
  [string]$Label
)

& 'c:\Program Files\7-zip\7z.exe' a "\\CORDISSVR01\Projects\MachineControlDashboard\$Label\DbgFiles.zip" .\Delivery\Bin\Release\*.nrmap .\Delivery\Bin\Release\*.pdb .\Delivery\Bin\Release\*.hash
